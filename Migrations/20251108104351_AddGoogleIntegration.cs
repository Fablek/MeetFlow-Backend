using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeetFlow_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GoogleIntegrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessToken = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    RefreshToken = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    TokenExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GoogleEmail = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    CalendarId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoogleIntegrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoogleIntegrations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GoogleIntegrations_GoogleEmail",
                table: "GoogleIntegrations",
                column: "GoogleEmail");

            migrationBuilder.CreateIndex(
                name: "IX_GoogleIntegrations_IsActive",
                table: "GoogleIntegrations",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_GoogleIntegrations_UserId",
                table: "GoogleIntegrations",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GoogleIntegrations");
        }
    }
}
