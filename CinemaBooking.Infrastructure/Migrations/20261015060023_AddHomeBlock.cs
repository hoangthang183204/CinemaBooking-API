using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaBooking.Infrastructure.Migrations
{
    public partial class AddHomeBlock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HomeBlocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kind = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Subtitle = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Tag = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DateLabel = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AccentColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MovieId = table.Column<int>(type: "int", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeBlocks", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "HomeBlocks",
                columns: new[] { "Id", "AccentColor", "DateLabel", "ImageUrl", "IsActive", "Kind", "MovieId", "SortOrder", "Subtitle", "Tag", "Title" },
                values: new object[,]
                {
                    { 1, null, "25.09.2026", "https://images.unsplash.com/photo-1489599849927-2ee91cede3ba?w=1600", true, "hero", 1, 1, "THE JOURNEY OF HEROES", "A FILM BY TONI DƯƠNG BẢO ANH & MÈO BÍCH TRANG", "TRẠI BUÔN NGƯỜI" },
                    { 2, null, "25.09.2026", "https://images.unsplash.com/photo-1478720568477-152d9b164e26?w=1600", true, "hero", 1, 2, "PHIM HOẠT HÌNH VIỆT NAM", "SUẤT CHIẾU ĐẶC BIỆT", "HỒN ĐÁO QUÊN LÀNG" },
                    { 3, "#e63946", null, null, true, "snack", null, 1, "Đồng giá 79K", null, "COMBO BẮP NƯỚC" },
                    { 4, "#3b82f6", null, null, true, "snack", null, 2, "Chọn combo yêu thích", null, "THIS OR THAT" },
                    { 5, "#f59e0b", null, null, true, "snack", null, 3, "Đồng giá 50.000đ/vé 2D", null, "SPECIAL MONDAY" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HomeBlocks");
        }
    }
}
