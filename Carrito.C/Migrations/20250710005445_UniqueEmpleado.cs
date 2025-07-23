using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarritoC.Migrations
{
    /// <inheritdoc />
    public partial class UniqueEmpleado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Persona_Legajo",
                table: "Persona",
                column: "Legajo",
                unique: true,
                filter: "[Legajo] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Persona_Legajo",
                table: "Persona");
        }
    }
}
