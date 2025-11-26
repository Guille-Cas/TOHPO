using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TOHPO.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixProductoImpuestoRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Producto_Categoria_Id_Categoria",
                table: "Producto");

            migrationBuilder.DropForeignKey(
                name: "FK_Producto_Impuesto_Id_Impuesto",
                table: "Producto");

            migrationBuilder.DropForeignKey(
                name: "FK_Producto_Materia_Prima_Id_Materia_Prima",
                table: "Producto");

            migrationBuilder.DropForeignKey(
                name: "FK_Producto_Presentacion_Id_Presentacion",
                table: "Producto");

            migrationBuilder.RenameIndex(
                name: "IX_Producto_Id_Impuesto",
                table: "Producto",
                newName: "IX_Producto_Impuesto");

            migrationBuilder.AddForeignKey(
                name: "FK_Producto_Categoria_Id_Categoria",
                table: "Producto",
                column: "Id_Categoria",
                principalTable: "Categoria",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Producto_Impuesto_Id_Impuesto",
                table: "Producto",
                column: "Id_Impuesto",
                principalTable: "Impuesto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Producto_Materia_Prima_Id_Materia_Prima",
                table: "Producto",
                column: "Id_Materia_Prima",
                principalTable: "Materia_Prima",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Producto_Presentacion_Id_Presentacion",
                table: "Producto",
                column: "Id_Presentacion",
                principalTable: "Presentacion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Producto_Categoria_Id_Categoria",
                table: "Producto");

            migrationBuilder.DropForeignKey(
                name: "FK_Producto_Impuesto_Id_Impuesto",
                table: "Producto");

            migrationBuilder.DropForeignKey(
                name: "FK_Producto_Materia_Prima_Id_Materia_Prima",
                table: "Producto");

            migrationBuilder.DropForeignKey(
                name: "FK_Producto_Presentacion_Id_Presentacion",
                table: "Producto");

            migrationBuilder.RenameIndex(
                name: "IX_Producto_Impuesto",
                table: "Producto",
                newName: "IX_Producto_Id_Impuesto");

            migrationBuilder.AddForeignKey(
                name: "FK_Producto_Categoria_Id_Categoria",
                table: "Producto",
                column: "Id_Categoria",
                principalTable: "Categoria",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Producto_Impuesto_Id_Impuesto",
                table: "Producto",
                column: "Id_Impuesto",
                principalTable: "Impuesto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Producto_Materia_Prima_Id_Materia_Prima",
                table: "Producto",
                column: "Id_Materia_Prima",
                principalTable: "Materia_Prima",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Producto_Presentacion_Id_Presentacion",
                table: "Producto",
                column: "Id_Presentacion",
                principalTable: "Presentacion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
