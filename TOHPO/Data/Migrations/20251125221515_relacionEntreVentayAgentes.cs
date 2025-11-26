using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TOHPO.Data.Migrations
{
    /// <inheritdoc />
    public partial class relacionEntreVentayAgentes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Agente_Ventas_Proveedor_Id_Proveedor",
                table: "Agente_Ventas");

            migrationBuilder.DropForeignKey(
                name: "FK_Venta_Agente_Ventas_Id_Agente_Ventas",
                table: "Venta");

            migrationBuilder.DropForeignKey(
                name: "FK_Venta_Cliente_Id_Cliente",
                table: "Venta");

            migrationBuilder.RenameIndex(
                name: "IX_Venta_Id_Cliente",
                table: "Venta",
                newName: "IX_Venta_Cliente");

            migrationBuilder.RenameIndex(
                name: "IX_Venta_Id_Agente_Ventas",
                table: "Venta",
                newName: "IX_Venta_Agente_Ventas");

            migrationBuilder.RenameIndex(
                name: "IX_Agente_Ventas_Id_Proveedor",
                table: "Agente_Ventas",
                newName: "IX_Agente_Ventas_Proveedor");

            migrationBuilder.AddForeignKey(
                name: "FK_Agente_Ventas_Proveedor_Id_Proveedor",
                table: "Agente_Ventas",
                column: "Id_Proveedor",
                principalTable: "Proveedor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Venta_Agente_Ventas_Id_Agente_Ventas",
                table: "Venta",
                column: "Id_Agente_Ventas",
                principalTable: "Agente_Ventas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Venta_Cliente_Id_Cliente",
                table: "Venta",
                column: "Id_Cliente",
                principalTable: "Cliente",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Agente_Ventas_Proveedor_Id_Proveedor",
                table: "Agente_Ventas");

            migrationBuilder.DropForeignKey(
                name: "FK_Venta_Agente_Ventas_Id_Agente_Ventas",
                table: "Venta");

            migrationBuilder.DropForeignKey(
                name: "FK_Venta_Cliente_Id_Cliente",
                table: "Venta");

            migrationBuilder.RenameIndex(
                name: "IX_Venta_Cliente",
                table: "Venta",
                newName: "IX_Venta_Id_Cliente");

            migrationBuilder.RenameIndex(
                name: "IX_Venta_Agente_Ventas",
                table: "Venta",
                newName: "IX_Venta_Id_Agente_Ventas");

            migrationBuilder.RenameIndex(
                name: "IX_Agente_Ventas_Proveedor",
                table: "Agente_Ventas",
                newName: "IX_Agente_Ventas_Id_Proveedor");

            migrationBuilder.AddForeignKey(
                name: "FK_Agente_Ventas_Proveedor_Id_Proveedor",
                table: "Agente_Ventas",
                column: "Id_Proveedor",
                principalTable: "Proveedor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Venta_Agente_Ventas_Id_Agente_Ventas",
                table: "Venta",
                column: "Id_Agente_Ventas",
                principalTable: "Agente_Ventas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Venta_Cliente_Id_Cliente",
                table: "Venta",
                column: "Id_Cliente",
                principalTable: "Cliente",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
