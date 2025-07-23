using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarritoC.Migrations
{
    /// <inheritdoc />
    public partial class UniqueClienteDdni : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Persona_DNI",
                table: "Persona",
                column: "DNI",
                unique: true,
                filter: "[DNI] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Persona_DNI",
                table: "Persona");
        }
    }
}
