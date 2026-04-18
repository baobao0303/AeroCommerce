-- =======================================================================
-- MIGRATION: Auth Stored Procedures (Login by Email/Phone, OTP handling)
-- RUN ON: Supabase / PostgreSQL
-- =======================================================================

-- 1. SP: Tìm User bằng Email hoặc SĐT
-- Dùng cho luồng đăng nhập linh hoạt.
CREATE OR REPLACE FUNCTION sp_get_user_by_email_or_phone(p_identifier VARCHAR)
RETURNS SETOF "Users"
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT * FROM "Users"
    WHERE 
        ("Email" = p_identifier) OR 
        ("PhoneNumber" = p_identifier AND "PhoneNumberVerified" = true)
    LIMIT 1;
END;
$$;


-- 2. SP: Cấp mã OTP mới và vô hiệu hóa (revoke) tất cả mã cũ của luồng đó
CREATE OR REPLACE PROCEDURE sp_create_otp(
    p_receiver VARCHAR,
    p_code VARCHAR,
    p_type VARCHAR,
    p_expired_in_minutes INT
)
LANGUAGE plpgsql
AS $$
BEGIN
    -- Vô hiệu hóa OTP cũ của cùng loại (Register/ForgotPassword)
    UPDATE "Otps"
    SET "IsUsed" = true
    WHERE "Receiver" = p_receiver AND "Type" = p_type AND "IsUsed" = false;

    -- Thêm OTP mới
    INSERT INTO "Otps" ("Id", "Receiver", "Code", "Type", "IsUsed", "CreatedAt", "ExpiresAt")
    VALUES (
        gen_random_uuid(), 
        p_receiver, 
        p_code, 
        p_type, 
        false, 
        now(), 
        now() + (p_expired_in_minutes || ' minutes')::INTERVAL
    );
END;
$$;


-- 3. SP: Xác thực và tiêu thụ (Consume) mã OTP
-- Dùng Transaction (Procedure) để tránh lỗi Race Condition
CREATE OR REPLACE PROCEDURE sp_verify_and_consume_otp(
    p_receiver VARCHAR,
    p_code VARCHAR,
    p_type VARCHAR
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_otp_id UUID;
    v_expires_at TIMESTAMP;
BEGIN
    -- Tìm mã OTP hợp lệ
    SELECT "Id", "ExpiresAt" INTO v_otp_id, v_expires_at
    FROM "Otps"
    WHERE "Receiver" = p_receiver 
      AND "Code" = p_code 
      AND "Type" = p_type 
      AND "IsUsed" = false
    LIMIT 1 FOR UPDATE; -- Block khóa row để chống request đồng thời

    IF v_otp_id IS NULL THEN
        RAISE EXCEPTION 'Mã OTP không hợp lệ hoặc đã được sử dụng.';
    END IF;

    IF v_expires_at < now() THEN
        RAISE EXCEPTION 'Mã OTP đã hết hạn.';
    END IF;

    -- Đánh dấu đã dùng
    UPDATE "Otps" SET "IsUsed" = true WHERE "Id" = v_otp_id;

    -- Nếu là OTP đăng ký -> Mở khóa PhoneNumber
    IF p_type = 'Register' THEN
        UPDATE "Users" 
        SET "PhoneNumberVerified" = true 
        WHERE "PhoneNumber" = p_receiver OR "Email" = p_receiver;
    END IF;
END;
$$;
