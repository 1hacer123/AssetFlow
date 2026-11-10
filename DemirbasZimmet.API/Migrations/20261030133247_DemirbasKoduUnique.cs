using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemirbasZimmet.API.Migrations
{
    /// <inheritdoc />
    public partial class DemirbasKoduUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DemirbasKodu",
                table: "Demirbaslar",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Demirbaslar_DemirbasKodu",
                table: "Demirbaslar",
                column: "DemirbasKodu",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Demirbaslar_DemirbasKodu",
                table: "Demirbaslar");

            migrationBuilder.AlterColumn<string>(
                name: "DemirbasKodu",
                table: "Demirbaslar",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
