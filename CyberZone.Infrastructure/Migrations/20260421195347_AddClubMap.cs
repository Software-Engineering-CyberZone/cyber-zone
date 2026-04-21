using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberZone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClubMap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClubMaps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Width = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    BackgroundColor = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    ClubId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubMaps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClubMaps_Clubs_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Clubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClubMapZones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    X = table.Column<int>(type: "int", nullable: false),
                    Y = table.Column<int>(type: "int", nullable: false),
                    Width = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    LabelColor = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    BorderColor = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    ClubMapId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubMapZones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClubMapZones_ClubMaps_ClubMapId",
                        column: x => x.ClubMapId,
                        principalTable: "ClubMaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClubMapElements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ElementType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    X = table.Column<int>(type: "int", nullable: false),
                    Y = table.Column<int>(type: "int", nullable: false),
                    Width = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    Rotation = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HardwareId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClubMapId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ZoneId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubMapElements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClubMapElements_ClubMapZones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "ClubMapZones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ClubMapElements_ClubMaps_ClubMapId",
                        column: x => x.ClubMapId,
                        principalTable: "ClubMaps",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ClubMapElements_Hardwares_HardwareId",
                        column: x => x.HardwareId,
                        principalTable: "Hardwares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClubMapElements_ClubMapId",
                table: "ClubMapElements",
                column: "ClubMapId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubMapElements_HardwareId",
                table: "ClubMapElements",
                column: "HardwareId",
                unique: true,
                filter: "[HardwareId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ClubMapElements_ZoneId",
                table: "ClubMapElements",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubMaps_ClubId",
                table: "ClubMaps",
                column: "ClubId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClubMapZones_ClubMapId",
                table: "ClubMapZones",
                column: "ClubMapId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClubMapElements");

            migrationBuilder.DropTable(
                name: "ClubMapZones");

            migrationBuilder.DropTable(
                name: "ClubMaps");
        }
    }
}
