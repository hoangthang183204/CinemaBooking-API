# Cinema Booking System

Hệ thống đặt vé xem phim **National Cinema Center**. Khách chọn phim, ghế và thanh toán trên web. Nhân viên bán vé tại quầy, soát vé và bàn giao ca. Quản trị viên quản lý phim, phòng, suất chiếu, nhân sự và doanh thu.

Giao diện nằm ở thư mục riêng: `D:\HocDotNet\CinemaBookingVue`.

## Công nghệ

- Backend: ASP.NET Core, .NET 6, Entity Framework Core 6, SQL Server, JWT.
- Frontend: Vue 3, Vue Router, Vite.
- Kiến trúc backend chia bốn tầng: Domain, Application, Infrastructure, API.

## Vai trò

- **Khách:** xem phim, đặt ghế, thanh toán, xem vé của mình.
- **Nhân viên:** bán vé tại quầy, soát vé, quản lý tồn bắp nước, mở và bàn giao ca.
- **Quản trị:** toàn bộ danh mục, đơn hàng, nhân sự, ca trực, báo cáo và cấu hình.

Đăng nhập xong, hệ thống đưa mỗi vai trò về đúng trang của họ.

## Chức năng

### Khách

- Trang chủ có banner, combo, danh sách phim và ô tìm phim.
- Chi tiết phim: poster, quốc gia, ngôn ngữ, diễn viên, trailer, điểm đánh giá và suất chiếu của đúng phim đó.
- Các trang nội dung: lịch chiếu, tin tức, khuyến mãi, giá vé, liên hoan phim, giới thiệu.
- Chọn ghế trên sơ đồ phòng. Ghế được giữ 10 phút, tối đa 8 ghế. Có thể thêm bắp nước và nhập mã giảm giá.
- Thanh toán hiện tiền ghế, bắp nước, phụ thu, số tiền mã giảm trừ và tổng phải trả.
- Cổng thanh toán: VNPay, MoMo (môi trường test) và QR chuyển khoản.
- Vé của tôi có phân trang. Vé chưa trả thì hủy được. Vé đã trả thì hoàn được nếu còn hơn 2 giờ trước giờ chiếu. Hủy xong, ghế và bắp nước được trả lại. Vé có mã QR từng ghế.
- Giao diện sáng và tối.

### Nhân viên

- Ca trực: mở ca, xem doanh thu toàn rạp và số đã thu hôm nay, bàn giao ca, in và lưu biên bản. Nhân viên kế tiếp đăng nhập sẽ thấy số tiền mặt đầu ca và bấm nhận ca. Ca còn mở khi sang ngày mới sẽ tự đóng.
- Bán vé tại quầy: chọn phim, suất còn giờ, ghế, bắp nước, thu tiền mặt có tính tiền thối, hoặc nhận chuyển khoản. In hóa đơn kèm QR.
- Bắp nước: xem và sửa tồn kho.
- Soát vé bằng QR. Vé hợp lệ, vé đã dùng và vé sai ngày chiếu được phân biệt bằng màu.
- Đơn đặt vé: tìm khách theo điện thoại hoặc email, in lại vé, hủy, hoàn tiền, đổi suất, đánh dấu đã trả tiền.
- Tạo QR thu tiền theo số tiền.

### Quản trị

- Dashboard: doanh thu 14 ngày, tỷ lệ lấp đầy ghế, cảnh báo, đơn gần đây và phim bán chạy.
- Phim, phòng và sơ đồ ghế.
- Suất chiếu. Suất đã qua giờ không tạo được. Cùng một phòng, suất sau phải cách lúc phim trước kết thúc ít nhất 30 phút.
- Người dùng: đổi vai trò, khóa hoặc mở tài khoản.
- Nhân viên: ảnh, họ tên, điện thoại, CCCD, chức vụ, trạng thái làm việc, lịch sử ca, và dải cho biết ai đang trực.
- Ca trực: danh sách, gán ca, lịch sử bán trong ca, nhập tiền đếm và duyệt ca.
- Đơn đặt vé, bắp nước, banner, combo, mã khuyến mãi, nội dung trang.
- Báo cáo doanh thu theo phim, phòng hoặc khung giờ, xuất Excel.
- Cấu hình giá vé, giờ mở cửa, giờ đóng cửa, phụ thu cuối tuần, phụ thu suất tối và mức cảnh báo bắp nước sắp hết.

## Cấu trúc backend

| Thư mục | Việc của tầng |
| --- | --- |
| `CinemaBooking.Domain` | Thực thể và enum |
| `CinemaBooking.Application` | DTO, interface, quy tắc dùng chung |
| `CinemaBooking.Infrastructure` | EF Core, DbContext, dịch vụ |
| `WebApplication1` | API. Tên assembly là `CinemaBooking.API` |
| `scripts` | Script SQL bổ sung cột và dữ liệu sau các migration |

Solution chạy bằng Visual Studio là `CinemaBooking.API.sln`. Project khởi động là `WebApplication1`.

## Yêu cầu

- .NET 6 SDK
- SQL Server
- Node.js 18 trở lên, để chạy giao diện

## Chạy dự án

1. Tạo database và cập nhật schema:

```bash
dotnet ef database update --project CinemaBooking.Infrastructure --startup-project WebApplication1
```

2. Chạy các file trong `scripts` trên database `CinemaBookingDb` nếu bảng còn thiếu cột của ca trực, nhân viên hoặc nội dung trang. Các cột này được thêm bằng SQL, chưa gom hết vào migration.

3. Mở `CinemaBooking.API.sln` và nhấn Run. API lắng nghe:

- HTTP: `http://localhost:5065`
- HTTPS và Swagger: `https://localhost:7106/swagger`

Khi chạy ở môi trường Development, API không chuyển HTTP sang HTTPS. Giao diện gọi API qua HTTP `5065`.

4. Chạy giao diện:

```bash
cd D:\HocDotNet\CinemaBookingVue
npm install
npm run dev
```

Mở `http://localhost:5173`. Vite chuyển `/api` và `/uploads` sang `http://localhost:5065`.

## Cấu hình

Sửa `WebApplication1/appsettings.json`. Có thể đối chiếu `appsettings.Sample.json`.

- `ConnectionStrings:DefaultConnection`: chuỗi kết nối SQL Server.
- `Jwt`: khóa ký token, issuer, audience, thời gian hết hạn.
- `Frontend:BaseUrl`: địa chỉ giao diện, mặc định `http://localhost:5173`.
- `Api:PublicUrl`: địa chỉ API để cổng thanh toán gọi lại, mặc định `http://localhost:5065`.
- `Payment:VNPay`: mã cửa hàng và khóa bí mật sandbox của bạn.
- `Payment:MoMo`: đang để bộ khóa test công khai của MoMo. Muốn thu tiền thật thì thay bằng tài khoản merchant của bạn.
- `VietQR`: mã ngân hàng, số tài khoản và tên hiển thị trên QR chuyển khoản.

Không đưa file chứa khóa thật lên kho công khai.

## Quy ước nghiệp vụ

- Giá vé online lấy từ cấu hình, mặc định 75.000 đồng một ghế, cộng phụ thu nếu có.
- Suất chiếu lưu theo giờ Việt Nam.
- Một mã giảm giá dùng một lần cho mỗi khách, và cũng chịu giới hạn số lượt của chương trình.
- Mã nhân viên chính là mã người dùng. Mã này được gán vào ca trực.
