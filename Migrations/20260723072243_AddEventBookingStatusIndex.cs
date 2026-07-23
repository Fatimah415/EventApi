using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventApi.Migrations
{
    /// <inheritdoc />
    public partial class AddEventBookingStatusIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_EventBookings_Status",
                table: "EventBookings",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EventBookings_Status",
                table: "EventBookings");
        }
    }
}
