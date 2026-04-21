using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberZone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddManagedClubToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ManagedClubId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ManagedClubId",
                table: "AspNetUsers",
                column: "ManagedClubId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Clubs_ManagedClubId",
                table: "AspNetUsers",
                column: "ManagedClubId",
                principalTable: "Clubs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Clubs_ManagedClubId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ManagedClubId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ManagedClubId",
                table: "AspNetUsers");
        }
    }
}
