# Cinema Booking System

Hệ thống đặt vé xem phim (Web API) xây dựng bằng .NET 6, kiến trúc Clean Architecture.

## Yêu cầu

- .NET 6 SDK
- SQL Server (LocalDB hoặc SQL Server Express)

## Cấu trúc Solution

- **CinemaBooking.Domain**: Entity, Enum
- **CinemaBooking.Application**: Service, Use Case, DTO, Interface Repository
- **CinemaBooking.Infrastructure**: EF Core, Repository, DbContext, Seed
- **CinemaBooking.API**: ASP.NET Core Web API, Controller

## Cài đặt & Chạy

1. Cấu hình chuỗi kết nối trong `WebApplication1/appsettings.json` hoặc `appsettings.Development.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=.;Database=CinemaBookingDb;Trusted_Connection=True;TrustServerCertificate=True"
   }
   ```
   (Có thể sao chép từ `appsettings.Sample.json`.)

2. Áp dụng migration và chạy API:
   ```bash
   cd d:\HocDotNet\CinemaBookingSystem
   dotnet ef database update --project CinemaBooking.Infrastructure --startup-project WebApplication1
   dotnet run --project WebApplication1
   ```
   (Lần đầu đã có migration `AddShowTimeSeed` để seed 3 suất chiếu mẫu.)

3. Mở Swagger: `https://localhost:7xxx/swagger` (hoặc cổng được in trong terminal).

## API chính

| Method | Endpoint | Mô tả |
|--------|----------|--------|
| GET | /api/Movies | Danh sách phim |
| GET | /api/Movies/{id} | Chi tiết phim |
| GET | /api/Rooms | Danh sách phòng chiếu |
| GET | /api/Rooms/{id} | Chi tiết phòng |
| GET | /api/showtimes | Danh sách suất chiếu |
| GET | /api/showtimes/{id}/room-seats | Ghế trong phòng theo suất chiếu |
| POST | /api/tickets | Đặt vé (body: ShowTimeId, SeatId) |

## Đặt vé (POST /api/tickets)

Request body:
```json
{
  "showTimeId": 1,
  "seatId": 1
}
```

Response thành công:
```json
{
  "success": true,
  "message": "Đặt vé thành công",
  "data": {
    "ticketId": 1,
    "price": 75000,
    "message": "Đặt vé thành công"
  }
}
```

## Ghi chú

- Seed data: 2 phim (Avatar, Inception), 2 phòng (Room 1, Room 2), ghế A1–A3 (phòng 1), B1–B2 (phòng 2). Cần thêm dữ liệu suất chiếu (ShowTimes) qua migration seed hoặc thêm API quản lý suất chiếu để có thể đặt vé.
