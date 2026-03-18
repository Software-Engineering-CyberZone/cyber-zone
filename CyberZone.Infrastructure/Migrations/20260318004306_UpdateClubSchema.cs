using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberZone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateClubSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Rating",
                table: "Clubs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Clubs");
        }
    }
}
