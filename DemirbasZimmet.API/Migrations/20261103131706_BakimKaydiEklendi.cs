using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemirbasZimmet.API.Migrations
{
    /// <inheritdoc />
    public partial class BakimKaydiEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BakimKayitlari",
                columns: table => new
                {
                    BakimKaydiId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemirbasId = table.Column<int>(type: "int", nullable: false),
                    BaslangicTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BitisTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArizaAciklamasi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    YapilanIslem = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    BakimiYapan = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Maliyet = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Durum = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BakimKayitlari", x => x.BakimKaydiId);
                    table.ForeignKey(
                        name: "FK_BakimKayitlari_Demirbaslar_DemirbasId",
                        column: x => x.DemirbasId,
                        principalTable: "Demirbaslar",
                        principalColumn: "DemirbasId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BakimKayitlari_DemirbasId",
                table: "BakimKayitlari",
                column: "DemirbasId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BakimKayitlari");
        }
    }
}
