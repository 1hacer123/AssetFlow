using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemirbasZimmet.API.Migrations
{
    /// <inheritdoc />
    public partial class AktifBakimUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BakimKayitlari_DemirbasId",
                table: "BakimKayitlari");

            migrationBuilder.CreateIndex(
                name: "IX_BakimKayitlari_DemirbasId",
                table: "BakimKayitlari",
                column: "DemirbasId",
                unique: true,
                filter: "[Durum] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BakimKayitlari_DemirbasId",
                table: "BakimKayitlari");

            migrationBuilder.CreateIndex(
                name: "IX_BakimKayitlari_DemirbasId",
                table: "BakimKayitlari",
                column: "DemirbasId");
        }
    }
}
