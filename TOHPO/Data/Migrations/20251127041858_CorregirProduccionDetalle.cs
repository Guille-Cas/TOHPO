using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TOHPO.Data.Migrations
{
    /// <inheritdoc />
    public partial class CorregirProduccionDetalle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cantidad_Preparacion",
                table: "Produccion_Detalle");

            migrationBuilder.RenameColumn(
                name: "Cantidad_Productos",
                table: "Produccion_Detalle",
                newName: "Id_Receta");

            migrationBuilder.RenameIndex(
                name: "IX_Produccion_Detalle_Codigo_Producto",
                table: "Produccion_Detalle",
                newName: "IX_Produccion_Detalle_Producto");

            migrationBuilder.AddColumn<double>(
                name: "Cantidad_Producida",
                table: "Produccion_Detalle",
                type: "float(18)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Cantidad_Programada",
                table: "Produccion_Detalle",
                type: "float(18)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "Estado",
                table: "Produccion_Detalle",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "Fecha_Fin",
                table: "Produccion_Detalle",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Fecha_Inicio",
                table: "Produccion_Detalle",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Observaciones",
                table: "Produccion_Detalle",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Produccion_Detalle_Receta",
                table: "Produccion_Detalle",
                column: "Id_Receta");

            migrationBuilder.AddForeignKey(
                name: "FK_Produccion_Detalle_Receta_Id_Receta",
                table: "Produccion_Detalle",
                column: "Id_Receta",
                principalTable: "Receta",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Produccion_Detalle_Receta_Id_Receta",
                table: "Produccion_Detalle");

            migrationBuilder.DropIndex(
                name: "IX_Produccion_Detalle_Receta",
                table: "Produccion_Detalle");

            migrationBuilder.DropColumn(
                name: "Cantidad_Producida",
                table: "Produccion_Detalle");

            migrationBuilder.DropColumn(
                name: "Cantidad_Programada",
                table: "Produccion_Detalle");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Produccion_Detalle");

            migrationBuilder.DropColumn(
                name: "Fecha_Fin",
                table: "Produccion_Detalle");

            migrationBuilder.DropColumn(
                name: "Fecha_Inicio",
                table: "Produccion_Detalle");

            migrationBuilder.DropColumn(
                name: "Observaciones",
                table: "Produccion_Detalle");

            migrationBuilder.RenameColumn(
                name: "Id_Receta",
                table: "Produccion_Detalle",
                newName: "Cantidad_Productos");

            migrationBuilder.RenameIndex(
                name: "IX_Produccion_Detalle_Producto",
                table: "Produccion_Detalle",
                newName: "IX_Produccion_Detalle_Codigo_Producto");

            migrationBuilder.AddColumn<int>(
                name: "Cantidad_Preparacion",
                table: "Produccion_Detalle",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
