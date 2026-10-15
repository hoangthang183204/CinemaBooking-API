using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaBooking.Infrastructure.Migrations
{
    public partial class AddConcessionStock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                table: "ConcessionItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "ConcessionItems",
                keyColumn: "Id",
                keyValue: 1,
                column: "StockQuantity",
                value: 40);

            migrationBuilder.UpdateData(
                table: "ConcessionItems",
                keyColumn: "Id",
                keyValue: 2,
                column: "StockQuantity",
                value: 40);

            migrationBuilder.UpdateData(
                table: "ConcessionItems",
                keyColumn: "Id",
                keyValue: 3,
                column: "StockQuantity",
                value: 40);

            migrationBuilder.UpdateData(
                table: "ConcessionItems",
                keyColumn: "Id",
                keyValue: 4,
                column: "StockQuantity",
                value: 40);

            migrationBuilder.UpdateData(
                table: "ConcessionItems",
                keyColumn: "Id",
                keyValue: 5,
                column: "StockQuantity",
                value: 40);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockQuantity",
                table: "ConcessionItems");
        }
    }
}
