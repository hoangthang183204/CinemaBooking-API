using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaBooking.Infrastructure.Migrations
{
    public partial class AddSiteArticle : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SiteArticles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Page = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Slot = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Meta = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    Body = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Extra = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteArticles", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SiteArticles",
                columns: new[] { "Id", "Body", "Extra", "IsActive", "Meta", "Page", "Slot", "SortOrder", "Title" },
                values: new object[,]
                {
                    { 1, "Thông báo vận hành rạp và các thay đổi trên hệ thống đặt vé.", null, true, null, "news", "intro", 0, "Mở đầu" },
                    { 2, "Từ thứ Sáu, phòng 1 và phòng 2 tăng suất tối để phục vụ khách đặt vé online. Lịch chi tiết nằm ở mục Lịch chiếu.", null, true, "12.10.2026", "news", "card", 1, "National Cinema mở thêm suất cuối tuần" },
                    { 3, "Sau khi thanh toán, vé nằm trong mục Vé của tôi. Nhân viên quét QR tại cửa phòng chiếu, không cần in giấy.", null, true, "05.10.2026", "news", "card", 2, "Vé điện tử có mã QR check-in" },
                    { 4, "Hàng ghế đôi được chọn theo cặp. Nếu một ghế trong cặp đã bán, cả cặp sẽ không chọn được.", null, true, "28.09.2026", "news", "card", 3, "Ghế đôi đã có trên sơ đồ phòng" },
                    { 5, "Ưu đãi đang công bố. Chỉ mức giá ghi trên màn thanh toán mới là số tiền của đơn online.", null, true, null, "promotion", "intro", 0, "Mở đầu" },
                    { 6, "Mọi ghế trên web đang tính 75.000đ. Giá này đã áp dụng khi bạn xác nhận đặt vé.", null, true, "ONLINE", "promotion", "card", 1, "Đồng giá vé 75.000đ" },
                    { 7, "Mua kèm vé tại quầy. Combo không trừ vào đơn thanh toán online.", null, true, "QUẦY", "promotion", "card", 2, "Combo bắp nước 79.000đ" },
                    { 8, "Một số suất 2D bán tại quầy có giá ưu đãi. Nhân viên thông báo trước khi thu tiền.", null, true, "THỨ HAI", "promotion", "card", 3, "Suất ngày thường tại quầy" },
                    { 9, "Giá trên website là giá đã gồm ghế bạn chọn. Đơn online chưa áp dụng mã giảm giá.", null, true, null, "price", "intro", 0, "Mở đầu" },
                    { 10, null, "75.000đ / ghế", true, "Đặt online", "price", "row", 1, "Thường, VIP, ghế đôi" },
                    { 11, null, "Nhân viên xác nhận khi thu tiền", true, "Tiền mặt, chuyển khoản, thẻ", "price", "row", 2, "Bán tại quầy" },
                    { 12, "Sau khi chọn ghế, hệ thống giữ trong 10 phút. Hết giờ ghế được mở lại cho khách khác.", null, true, null, "price", "card", 3, "Giữ ghế" },
                    { 13, "Tối đa 8 ghế cho một lần đặt, kể cả ghế đôi. Ghế đôi tính theo từng chỗ ngồi.", null, true, null, "price", "card", 4, "Số ghế mỗi đơn" },
                    { 14, "Lịch sự kiện của National Cinema Center. Suất nào mở bán online sẽ xuất hiện trong Lịch chiếu.", null, true, null, "festival", "intro", 0, "Mở đầu" },
                    { 15, "Các suất tối trong tuần được xếp thêm tại phòng 1. Xem giờ cụ thể ở Lịch chiếu.", null, true, "20–26.10.2026", "festival", "card", 1, "Tuần phim cuối tháng" },
                    { 16, "Suất sáng dành cho gia đình. Vé vẫn đặt trên web theo giá niêm yết.", null, true, "02–08.11.2026", "festival", "card", 2, "Tuần phim hoạt hình" },
                    { 17, "Một số buổi giới thiệu phim chỉ bán tại quầy. Nhân viên sẽ thông báo khi suất không mở online.", null, true, "Cả năm", "festival", "card", 3, "Suất đặc biệt tại rạp" },
                    { 18, "National Cinema Center là rạp đặt vé trực tuyến và bán vé tại quầy. Khách xem lịch, chọn ghế, thanh toán và nhận vé QR. Nhân viên check-in bằng mã trên vé.", null, true, null, "about", "intro", 0, "Mở đầu" },
                    { 19, "Chọn phim, suất chiếu và ghế. Ghế được giữ 10 phút trước khi thanh toán.", null, true, null, "about", "card", 1, "Đặt vé online" },
                    { 20, "Nhân viên bán vé tại quầy, thu tiền và xuất lại mã QR nếu khách cần.", null, true, null, "about", "card", 2, "Quầy vé" },
                    { 21, "Hotline 1900 1234. Quầy mở trước suất chiếu đầu tiên trong ngày 30 phút.", null, true, null, "about", "card", 3, "Liên hệ" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteArticles");
        }
    }
}
