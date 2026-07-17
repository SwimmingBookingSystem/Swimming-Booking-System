# Phân tích luồng và Sequence Diagram - Swimming Booking System

## 1. Phạm vi phân tích

Bộ PlantUML này được xây dựng từ implementation hiện tại trong các project:

- `SBS.WebApp`: Razor Pages và JavaScript phía client; mọi dữ liệu nghiệp vụ được gọi qua HTTP API.
- `SBS.Api`: REST controllers, JWT authorization, FluentValidation và chuyển request vào MediatR.
- `SBS.Application`: các use case theo CQRS/Vertical Slice (Command, Query, Handler, Consumer).
- `SBS.Infrastructure`: EF Core, SQL Server, ASP.NET Identity, Redis, PayOS, Cloudinary, RabbitMQ/MassTransit, email và QR code.
- `SBS.Domain`: các entity chính như `Pool`, `PoolSlot`, `Booking`, `BookingDetail`, `Payment`, `CheckIn`, `PoolStaffAssignment`.

## 2. Các diagram

1. `01-login.puml`: đăng nhập, phát JWT/refresh token, lưu refresh token trong Redis, ghi cookie và điều hướng theo role.
2. `02-booking-payment.puml`: tạo booking, khóa chống overbooking, PayOS, webhook, tạo payment/QR và gửi email bất đồng bộ.
3. `03-checkin-checkout.puml`: check-in bằng QR/thủ công và check-out, bao gồm kiểm tra phân công bể bơi và thời gian ca.
4. `04-manager-create-pool.puml`: Manager upload ảnh, tạo bể, tính sức chứa chuẩn và cập nhật ảnh.
5. `05-admin-assign-staff.puml`: Admin tạo mới hoặc cập nhật nhân viên và gán/bỏ gán bể bơi.

## 3. Trạng thái nghiệp vụ chính

- Booking: `PendingPayment` -> `Paid` -> `CheckIn` -> `Completed`.
- Booking quá hạn hoặc bị hủy khi đang chờ thanh toán: `PendingPayment` -> `Cancelled`.
- Payment thành công: tạo bản ghi `Payment(Status = Success)` và sinh `QrCodeData` duy nhất.
- Check-in tạo một bản ghi `CheckIn`; quan hệ Booking - CheckIn là 1-1.
- Check-out cập nhật `CheckIn.CheckOutTime`, chuyển Booking sang `Completed` và phát `SlotCapacityFreedEvent`.

## 4. Kết luận quan trọng từ code hiện tại

- **Tạo bể bơi thuộc quyền Manager, không phải Admin.** Endpoint `POST /api/manager/pools` chỉ có `[Authorize(Roles = "Manager")]`. Vì vậy diagram 04 dùng actor Manager để phản ánh đúng hệ thống đang chạy.
- Admin quản lý nhân sự và phân công bể qua `POST /api/admin/users/create-staff` hoặc `PUT /api/admin/users/{userId}`.
- Mỗi bể chỉ có tối đa một staff vì database đặt unique index trên `PoolStaffAssignments.PoolId` và service cũng kiểm tra bể đã được phân công hay chưa.
- Check-in/check-out kiểm tra `PoolStaffAssignments`; staff chỉ thao tác được booking thuộc bể mình được gán.
- Controller check-in cho phép role `Staff,Manager,Admin`, nhưng handler vẫn yêu cầu người dùng có bản ghi phân công bể. Do đó Manager/Admin không được gán trong `PoolStaffAssignments` sẽ bị từ chối ở tầng nghiệp vụ.
- Luồng tạo staff chưa có transaction bao trùm: tài khoản và role được tạo trước khi kiểm tra/gán bể. Nếu bể không tồn tại hoặc đã có người phụ trách, API trả lỗi nhưng tài khoản staff có thể đã tồn tại.
- Luồng cập nhật staff cũng cập nhật hồ sơ Identity trước khi kiểm tra phân công bể, nên lỗi phân công có thể không hoàn tác phần hồ sơ đã cập nhật.
- `AssignedByAdminId` tồn tại trong entity nhưng service hiện chưa ghi giá trị này.
- UI tạo bể đang gọi hai bước: `POST /pools` để tạo bể (không truyền mảng ảnh trong payload hiện tại), sau đó `PUT /pools/{id}/images` để lưu ảnh đã upload lên Cloudinary.

## 5. Cách render

Mỗi file có thể mở bằng PlantUML extension trong VS Code/IntelliJ hoặc render bằng PlantUML CLI:

```powershell
plantuml docs/uml/*.puml
```

