using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TOHPO.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRecetaMateriasPrimas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Receta_Materia_Prima",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_Receta = table.Column<int>(type: "int", nullable: false),
                    Id_Materia_Prima = table.Column<int>(type: "int", nullable: false),
                    Cantidad_Requerida = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Unidad_Medida = table.Column<int>(type: "int", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receta_Materia_Prima", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Receta_Materia_Prima_Materia_Prima_Id_Materia_Prima",
                        column: x => x.Id_Materia_Prima,
                        principalTable: "Materia_Prima",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Receta_Materia_Prima_Receta_Id_Receta",
                        column: x => x.Id_Receta,
                        principalTable: "Receta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Receta_Materia_Prima_Materia_Prima",
                table: "Receta_Materia_Prima",
                column: "Id_Materia_Prima");

            migrationBuilder.CreateIndex(
                name: "IX_Receta_Materia_Prima_Receta",
                table: "Receta_Materia_Prima",
                column: "Id_Receta");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Receta_Materia_Prima");
        }
    }
}
