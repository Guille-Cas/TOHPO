using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TOHPO.Data.Migrations
{
    /// <inheritdoc />
    public partial class SellRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Compra_Proveedor_Id_Proveedor",
                table: "Compra");

            migrationBuilder.DropForeignKey(
                name: "FK_Compra_Detalle_Producto_Codigo_Producto",
                table: "Compra_Detalle");

            migrationBuilder.DropForeignKey(
                name: "FK_Detalle_Venta_Producto_Codigo_Producto",
                table: "Detalle_Venta");

            migrationBuilder.DropForeignKey(
                name: "FK_Movimiento_Inventario_Inventario_Id_Inventario",
                table: "Movimiento_Inventario");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedido_Detalle_Producto_Codigo_Producto",
                table: "Pedido_Detalle");

            migrationBuilder.DropForeignKey(
                name: "FK_Produccion_Detalle_Producto_Codigo_Producto",
                table: "Produccion_Detalle");

            migrationBuilder.DropIndex(
                name: "IX_Inventario_Codigo_Producto",
                table: "Inventario");

            migrationBuilder.RenameIndex(
                name: "IX_Compra_Id_Proveedor",
                table: "Compra",
                newName: "IX_Compra_Proveedor");

            migrationBuilder.CreateIndex(
                name: "IX_Inventario_Producto",
                table: "Inventario",
                column: "Codigo_Producto",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Compra_Proveedor_Id_Proveedor",
                table: "Compra",
                column: "Id_Proveedor",
                principalTable: "Proveedor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Compra_Detalle_Producto_Codigo_Producto",
                table: "Compra_Detalle",
                column: "Codigo_Producto",
                principalTable: "Producto",
                principalColumn: "CodigoReferencia",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Detalle_Venta_Producto_Codigo_Producto",
                table: "Detalle_Venta",
                column: "Codigo_Producto",
                principalTable: "Producto",
                principalColumn: "CodigoReferencia",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Movimiento_Inventario_Inventario_Id_Inventario",
                table: "Movimiento_Inventario",
                column: "Id_Inventario",
                principalTable: "Inventario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedido_Detalle_Producto_Codigo_Producto",
                table: "Pedido_Detalle",
                column: "Codigo_Producto",
                principalTable: "Producto",
                principalColumn: "CodigoReferencia",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Produccion_Detalle_Producto_Codigo_Producto",
                table: "Produccion_Detalle",
                column: "Codigo_Producto",
                principalTable: "Producto",
                principalColumn: "CodigoReferencia",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Compra_Proveedor_Id_Proveedor",
                table: "Compra");

            migrationBuilder.DropForeignKey(
                name: "FK_Compra_Detalle_Producto_Codigo_Producto",
                table: "Compra_Detalle");

            migrationBuilder.DropForeignKey(
                name: "FK_Detalle_Venta_Producto_Codigo_Producto",
                table: "Detalle_Venta");

            migrationBuilder.DropForeignKey(
                name: "FK_Movimiento_Inventario_Inventario_Id_Inventario",
                table: "Movimiento_Inventario");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedido_Detalle_Producto_Codigo_Producto",
                table: "Pedido_Detalle");

            migrationBuilder.DropForeignKey(
                name: "FK_Produccion_Detalle_Producto_Codigo_Producto",
                table: "Produccion_Detalle");

            migrationBuilder.DropIndex(
                name: "IX_Inventario_Producto",
                table: "Inventario");

            migrationBuilder.RenameIndex(
                name: "IX_Compra_Proveedor",
                table: "Compra",
                newName: "IX_Compra_Id_Proveedor");

            migrationBuilder.CreateIndex(
                name: "IX_Inventario_Codigo_Producto",
                table: "Inventario",
                column: "Codigo_Producto");

            migrationBuilder.AddForeignKey(
                name: "FK_Compra_Proveedor_Id_Proveedor",
                table: "Compra",
                column: "Id_Proveedor",
                principalTable: "Proveedor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Compra_Detalle_Producto_Codigo_Producto",
                table: "Compra_Detalle",
                column: "Codigo_Producto",
                principalTable: "Producto",
                principalColumn: "CodigoReferencia",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Detalle_Venta_Producto_Codigo_Producto",
                table: "Detalle_Venta",
                column: "Codigo_Producto",
                principalTable: "Producto",
                principalColumn: "CodigoReferencia",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Movimiento_Inventario_Inventario_Id_Inventario",
                table: "Movimiento_Inventario",
                column: "Id_Inventario",
                principalTable: "Inventario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedido_Detalle_Producto_Codigo_Producto",
                table: "Pedido_Detalle",
                column: "Codigo_Producto",
                principalTable: "Producto",
                principalColumn: "CodigoReferencia",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Produccion_Detalle_Producto_Codigo_Producto",
                table: "Produccion_Detalle",
                column: "Codigo_Producto",
                principalTable: "Producto",
                principalColumn: "CodigoReferencia",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
