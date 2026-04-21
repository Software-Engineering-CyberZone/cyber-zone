using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberZone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HardwarePcNumberUniquePerClub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Hardwares_ClubId",
                table: "Hardwares");

            migrationBuilder.DropIndex(
                name: "IX_Hardwares_PcNumber",
                table: "Hardwares");

            migrationBuilder.CreateIndex(
                name: "IX_Hardwares_ClubId_PcNumber",
                table: "Hardwares",
                columns: new[] { "ClubId", "PcNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Hardwares_ClubId_PcNumber",
                table: "Hardwares");

            migrationBuilder.CreateIndex(
                name: "IX_Hardwares_ClubId",
                table: "Hardwares",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_Hardwares_PcNumber",
                table: "Hardwares",
                column: "PcNumber",
                unique: true);
        }
    }
}
