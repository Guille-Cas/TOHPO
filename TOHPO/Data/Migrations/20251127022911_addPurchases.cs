using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TOHPO.Data.Migrations
{
    /// <inheritdoc />
    public partial class addPurchases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Compra");

            migrationBuilder.DropColumn(
                name: "Numero_Factura",
                table: "Compra");

            migrationBuilder.RenameColumn(
                name: "Gran_Total",
                table: "Compra",
                newName: "Costo_Total_Grabado");

            migrationBuilder.AddColumn<decimal>(
                name: "Monto_Impuesto",
                table: "Compra_Detalle",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "Compra_Detalle",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Concepto",
                table: "Compra",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Hora",
                table: "Compra",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Monto_Impuesto",
                table: "Compra_Detalle");

            migrationBuilder.DropColumn(
                name: "Subtotal",
                table: "Compra_Detalle");

            migrationBuilder.DropColumn(
                name: "Concepto",
                table: "Compra");

            migrationBuilder.DropColumn(
                name: "Hora",
                table: "Compra");

            migrationBuilder.RenameColumn(
                name: "Costo_Total_Grabado",
                table: "Compra",
                newName: "Gran_Total");

            migrationBuilder.AddColumn<bool>(
                name: "Estado",
                table: "Compra",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Numero_Factura",
                table: "Compra",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
