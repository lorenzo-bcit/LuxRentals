using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuxRentals.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingAvailabilityIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Booking_fkCarId",
                table: "Booking");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_Car_ActiveWindow",
                table: "Booking",
                columns: new[] { "fkCarId", "cancelledAt", "startDateTime", "endDateTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Booking_Car_ActiveWindow",
                table: "Booking");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_fkCarId",
                table: "Booking",
                column: "fkCarId");
        }
    }
}
