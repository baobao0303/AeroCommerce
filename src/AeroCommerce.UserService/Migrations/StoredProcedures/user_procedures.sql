-- ════════════════════════════════════════════════════════
--  UserService — Stored Procedures / Functions
--  Dùng cho: Supabase SQL Editor hoặc Migration seed
-- ════════════════════════════════════════════════════════

-- ── SP: Tìm user theo email (case-insensitive) ──────────
CREATE OR REPLACE FUNCTION sp_get_user_by_email(p_email TEXT)
RETURNS SETOF users AS $$
BEGIN
    RETURN QUERY
    SELECT *
    FROM users
    WHERE LOWER(email) = LOWER(p_email)
      AND is_active = TRUE
    LIMIT 1;
END;
$$ LANGUAGE plpgsql;

-- ── SP: Lấy danh sách active users với phân trang ───────
CREATE OR REPLACE FUNCTION sp_get_active_users_paged(p_offset INT, p_limit INT)
RETURNS SETOF users AS $$
BEGIN
    RETURN QUERY
    SELECT *
    FROM users
    WHERE is_active = TRUE
    ORDER BY created_at DESC
    OFFSET p_offset
    LIMIT p_limit;
END;
$$ LANGUAGE plpgsql;

-- ── SP: Soft delete user ────────────────────────────────
CREATE OR REPLACE PROCEDURE sp_soft_delete_user(p_user_id UUID)
LANGUAGE plpgsql AS $$
BEGIN
    UPDATE users
    SET is_active  = FALSE,
        updated_at = NOW()
    WHERE id = p_user_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'User % not found', p_user_id;
    END IF;
END;
$$;

-- ── SP: Thống kê users ──────────────────────────────────
CREATE OR REPLACE FUNCTION sp_get_user_stats()
RETURNS TABLE(total_users BIGINT, active_users BIGINT, admin_count BIGINT) AS $$
BEGIN
    RETURN QUERY
    SELECT
        COUNT(*)                                    AS total_users,
        COUNT(*) FILTER (WHERE is_active = TRUE)    AS active_users,
        COUNT(*) FILTER (WHERE role = 'Admin')      AS admin_count
    FROM users;
END;
$$ LANGUAGE plpgsql;
