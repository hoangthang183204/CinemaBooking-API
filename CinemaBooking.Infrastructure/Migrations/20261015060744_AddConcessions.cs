using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaBooking.Infrastructure.Migrations
{
    public partial class AddConcessions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConcessionItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConcessionItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BookingConcessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    ConcessionItemId = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingConcessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingConcessions_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingConcessions_ConcessionItems_ConcessionItemId",
                        column: x => x.ConcessionItemId,
                        principalTable: "ConcessionItems",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "ConcessionItems",
                columns: new[] { "Id", "Category", "Description", "ImageUrl", "IsActive", "Name", "Price", "SortOrder" },
                values: new object[,]
                {
                    { 1, "Bắp", "Bắp ngọt, phần vừa", "https://images.unsplash.com/photo-1578849278619-e73505e9610f?auto=format&fit=crop&w=600&q=80", true, "Bắp rang bơ", 45000m, 1 },
                    { 2, "Bắp", "Bắp caramel giòn", "https://images.unsplash.com/photo-1585647347483-22b66260dfff?auto=format&fit=crop&w=600&q=80", true, "Bắp caramel", 55000m, 2 },
                    { 3, "Nước", "Ly lớn, đá riêng", "https://images.unsplash.com/photo-1629203851122-3726ecdf080e?auto=format&fit=crop&w=600&q=80", true, "Nước ngọt", 25000m, 3 },
                    { 4, "Nước", "Chai 500ml", "https://images.unsplash.com/photo-1548839140-29a749e1cf4d?auto=format&fit=crop&w=600&q=80", true, "Nước suối", 15000m, 4 },
                    { 5, "Combo", "1 bắp rang bơ và 1 nước ngọt", "https://images.unsplash.com/photo-1513558161293-cdaf765ed2fd?auto=format&fit=crop&w=600&q=80", true, "Combo bắp + nước", 79000m, 5 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingConcessions_BookingId",
                table: "BookingConcessions",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingConcessions_ConcessionItemId",
                table: "BookingConcessions",
                column: "ConcessionItemId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingConcessions");

            migrationBuilder.DropTable(
                name: "ConcessionItems");
        }
    }
}
