using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemirbasZimmet.API.Migrations
{
    /// <inheritdoc />
    public partial class BildirimIlgiliKayitEkle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IlgiliKayitId",
                table: "Bildirimler",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IlgiliKayitTuru",
                table: "Bildirimler",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IlgiliKayitId",
                table: "Bildirimler");

            migrationBuilder.DropColumn(
                name: "IlgiliKayitTuru",
                table: "Bildirimler");
        }
    }
}
