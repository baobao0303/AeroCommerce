# Kế hoạch phát triển: UserService (Cổng 5001)

## Mục tiêu của UserService
UserService chịu trách nhiệm quản lý người dùng, định danh (Authentication) và phân quyền (Authorization) cho toàn bộ hệ thống. Dữ liệu lưu tại Supabase (Schema: `user_service`).

---

## 📌 Task 1: Cấu hình JWT (JSON Web Token)
- [ ] Mở file `appsettings.Development.json` kiểm tra cấu hình secret key cho JWT.
- [ ] Viết một Service (VD: `ITokenService` và `TokenService`) để chuyên chịu trách nhiệm sinh chuỗi JWT.
  - Chuỗi JWT cần chứa Payload: `UserId`, `Username`, `Role` (Admin/User).
  - Cấu hình thời gian hết hạn (VD: 7 ngày).

## 📌 Task 2: Hiện thực Service Logic (IUserService)
- [ ] Chỉnh sửa `IUserRepository` (nếu cần) hoặc dùng chung Generic Repository để thao tác DB.
- [ ] Viết hàm `RegisterAsync`:
  - **Check trùng lặp:** Kiểm tra xem Email hoặc Username đã tồn tại trong database chưa (tránh lỗi duplicate schema).
  - **Bảo mật:** Băm mật khẩu (Hashing Password) bằng thuât toán chuẩn (VD: `BCrypt.Net-Next`). **KHÔNG ĐƯỢC LƯU PLAIN TEXT.**
  - **Lưu DB:** Tạo Entity `User` mới và lưu thành công xuống DB.
- [ ] Viết hàm `LoginAsync`:
  - Tìm kiếm User theo Email hoặc Username.
  - Kiểm tra (Verify) chuỗi PasswordHash trong DB.
  - Gọi `TokenService` để sinh mã JWT trả về cho client.

## 📌 Task 3: API Endpoints (AuthController)
- [ ] Tạo (hoặc cập nhật) file `Controllers/AuthController.cs`.
- [ ] API `POST /api/auth/register`: Nhận tham số (Email, Username, Password) qua DTO, gọi logic.
- [ ] API `POST /api/auth/login`: Nhận tham số (Email/Username, Password), gửi lại HTTP 200 kèm Token.
- [ ] API `GET /api/auth/me`: Nhận Bearer Token ở Header, bóc tách ra `UserId` để lấy toàn bộ thông tin User từ DB trả về API (Profile người dùng).

## 📌 Task 4: Tương tác Microservices (Optional/Future)
- Khi một user mới được tạo ra thành công, bắn một Event (ví dụ dùng RabbitMQ/MassTransit) mang tên `UserCreatedEvent`. 
- Service bài viết (PostService) có thể lắng nghe Event này để nhân bản một bảng `User` thu gọn bên đó (chứa Id và Name) để dùng làm hiển thị tác giả bài viết tránh phải JOIN trực tiếp chéo database.
