using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeetFlow_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailAndCalendarNameToGoogleIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CalendarName",
                table: "GoogleIntegrations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "GoogleIntegrations",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CalendarName",
                table: "GoogleIntegrations");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "GoogleIntegrations");
        }
    }
}
