using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarritoC.Migrations
{
    /// <inheritdoc />
    public partial class UniqueNombreSucursalCategoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Sucursal_Nombre",
                table: "Sucursal",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_Nombre",
                table: "Categorias",
                column: "Nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sucursal_Nombre",
                table: "Sucursal");

            migrationBuilder.DropIndex(
                name: "IX_Categorias_Nombre",
                table: "Categorias");
        }
    }
}
