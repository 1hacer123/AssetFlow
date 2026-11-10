using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemirbasZimmet.API.Migrations
{
    /// <inheritdoc />
    public partial class PersonelEpostaUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Eposta",
                table: "Personeller",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Personeller_Eposta",
                table: "Personeller",
                column: "Eposta",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Personeller_Eposta",
                table: "Personeller");

            migrationBuilder.AlterColumn<string>(
                name: "Eposta",
                table: "Personeller",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
