using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace autos.Api.Migrations
{
    /// <inheritdoc />
    public partial class MarcaAutoNameUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MarcasAutos_Name",
                table: "MarcasAutos");

            migrationBuilder.CreateIndex(
                name: "IX_MarcasAutos_Name",
                table: "MarcasAutos",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MarcasAutos_Name",
                table: "MarcasAutos");

            migrationBuilder.CreateIndex(
                name: "IX_MarcasAutos_Name",
                table: "MarcasAutos",
                column: "Name");
        }
    }
}
