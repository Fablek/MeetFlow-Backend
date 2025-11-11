using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeetFlow_Backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGoogleIntegrationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GoogleIntegrations_GoogleEmail",
                table: "GoogleIntegrations");

            migrationBuilder.DropColumn(
                name: "GoogleEmail",
                table: "GoogleIntegrations");

            migrationBuilder.AlterColumn<string>(
                name: "RefreshToken",
                table: "GoogleIntegrations",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "CalendarId",
                table: "GoogleIntegrations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccessToken",
                table: "GoogleIntegrations",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.CreateIndex(
                name: "IX_GoogleIntegrations_Email",
                table: "GoogleIntegrations",
                column: "Email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GoogleIntegrations_Email",
                table: "GoogleIntegrations");

            migrationBuilder.AlterColumn<string>(
                name: "RefreshToken",
                table: "GoogleIntegrations",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CalendarId",
                table: "GoogleIntegrations",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccessToken",
                table: "GoogleIntegrations",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "GoogleEmail",
                table: "GoogleIntegrations",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_GoogleIntegrations_GoogleEmail",
                table: "GoogleIntegrations",
                column: "GoogleEmail");
        }
    }
}
