using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaBooking.Infrastructure.Migrations
{
    public partial class AddSeatHold : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SeatHolds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShowTimeId = table.Column<int>(type: "int", nullable: false),
                    SeatId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    HoldToken = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeatHolds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeatHolds_Seats_SeatId",
                        column: x => x.SeatId,
                        principalTable: "Seats",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SeatHolds_ShowTimes_ShowTimeId",
                        column: x => x.ShowTimeId,
                        principalTable: "ShowTimes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SeatHolds_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SeatHolds_HoldToken",
                table: "SeatHolds",
                column: "HoldToken");

            migrationBuilder.CreateIndex(
                name: "IX_SeatHolds_SeatId",
                table: "SeatHolds",
                column: "SeatId");

            migrationBuilder.CreateIndex(
                name: "IX_SeatHolds_ShowTimeId_SeatId",
                table: "SeatHolds",
                columns: new[] { "ShowTimeId", "SeatId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SeatHolds_UserId",
                table: "SeatHolds",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeatHolds");
        }
    }
}
