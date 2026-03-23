# Kế hoạch phát triển: ApiGateway (Cổng 5000)

## Mục tiêu của ApiGateway
Đóng vai trò là "Cửa Bảo Vệ" duy nhất (Single Point of Entry) cho toàn bộ Client (Frontend Next.js, Mobile App).
Client thay vì gọi trực tiếp cổng 5001 (User) hay 5002 (Post), Client sẽ chỉ gọi đúng cổng 5000 (Gateway). Gateway sẽ có nhiệm vụ định tuyến (Route) request đó về đúng Microservice phía sau.

Nên sử dụng: **YARP (Yet Another Reverse Proxy)** của Microsoft thay cho Ocelot (vì YARP được hỗ trợ natively trên .NET 8, siêu nhanh và dễ setup).

---

## 📌 Task 1: Setup YARP (hoặc Ocelot)
- [ ] Nếu dùng YARP:
  - Cài đặt package `Yarp.ReverseProxy`.
  - Cấu hình middleware `app.MapReverseProxy()` ở file `Program.cs`.
- [ ] Nếu dùng Ocelot:
  - Cài đặt package `Ocelot`.
  - Sửa `Program.cs` cấu hình Ocelot.

## 📌 Task 2: Khai báo File Cấu Hình Kịch Bản Định Tuyến (Routing)
Cấu hình trong `appsettings.json` cho Gateway để nó tự nhận biết cách chuyển hướng:

- [ ] **Route 1 (User Auth):** 
  - Khách gọi: `http://localhost:5000/users/{anything}`
  - Gateway đẩy đi: `http://localhost:5001/api/{anything}`
- [ ] **Route 2 (User Profile/Logic):** 
  - Khách gọi: `http://localhost:5000/auth/{anything}`
  - Gateway đẩy đi: `http://localhost:5001/api/auth/{anything}`
- [ ] **Route 3 (Posts):** 
  - Khách gọi: `http://localhost:5000/posts/{anything}`
  - Gateway đẩy đi: `http://localhost:5002/api/posts/{anything}`

## 📌 Task 3: Cấu hình CORS (Cross-Origin Resource Sharing)
- [ ] Do Next.js chạy ở Frontend (`localhost:3000`), Gateway phải là thằng chịu trách nhiệm mở CORS config để cho phép NextJS gọi API vào hệ thống mà không bị trình duyệt chặt lại vì lỗi Cross-Domain.
- [ ] Setup `builder.Services.AddCors(...)` áp dụng trên toàn Gateway.

## 📌 Task 4: Phân quyền & Giữ cửa (Authentication ở Gateway - Nâng cao)
- [ ] Cài đặt gói xác thực JWT Bearer tại chính ApiGateway.
- [ ] Yêu cầu Gateway "kiểm tra vé" (validate token JWT xem có hợp lệ, không hết hạn) **trước khi** cho phép các Request lọt qua để đi xuống các Microservice (như PostService). Nếu vé giả, Gateway sẽ từ chối thẳng mặt trả về HTTP 401 Unauthorized mà không tốn công gọi xuống service dưới.
