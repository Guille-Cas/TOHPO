using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TOHPO.Data.Migrations
{
    /// <inheritdoc />
    public partial class SolutionData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Agente_Ventas_Proveedor_Id_Proveedor",
                table: "Agente_Ventas");

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
                name: "FK_Pedido_Agente_Ventas_Id_Agente_Ventas",
                table: "Pedido");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedido_Detalle_Producto_Codigo_Producto",
                table: "Pedido_Detalle");

            migrationBuilder.DropForeignKey(
                name: "FK_Produccion_Detalle_Producto_Codigo_Producto",
                table: "Produccion_Detalle");

            migrationBuilder.DropForeignKey(
                name: "FK_Venta_Agente_Ventas_Id_Agente_Ventas",
                table: "Venta");

            migrationBuilder.DropForeignKey(
                name: "FK_Venta_Cliente_Id_Cliente",
                table: "Venta");

            migrationBuilder.DropIndex(
                name: "IX_Venta_Id_Agente_Ventas",
                table: "Venta");

            migrationBuilder.DropIndex(
                name: "IX_Pedido_Id_Agente_Ventas",
                table: "Pedido");

            migrationBuilder.DropIndex(
                name: "IX_Inventario_Codigo_Producto",
                table: "Inventario");

            migrationBuilder.DropColumn(
                name: "Id_Agente_Ventas",
                table: "Venta");

            migrationBuilder.DropColumn(
                name: "Cantidad_Preparacion",
                table: "Produccion_Detalle");

            migrationBuilder.DropColumn(
                name: "Id_Agente_Ventas",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Compra");

            migrationBuilder.DropColumn(
                name: "Gran_Total",
                table: "Compra");

            migrationBuilder.DropColumn(
                name: "Numero_Factura",
                table: "Compra");

            migrationBuilder.RenameIndex(
                name: "IX_Venta_Id_Cliente",
                table: "Venta",
                newName: "IX_Venta_Cliente");

            migrationBuilder.RenameColumn(
                name: "Cantidad_Productos",
                table: "Produccion_Detalle",
                newName: "Id_Receta");

            migrationBuilder.RenameIndex(
                name: "IX_Produccion_Detalle_Codigo_Producto",
                table: "Produccion_Detalle",
                newName: "IX_Produccion_Detalle_Producto");

            migrationBuilder.RenameIndex(
                name: "IX_Compra_Id_Proveedor",
                table: "Compra",
                newName: "IX_Compra_Proveedor");

            migrationBuilder.RenameIndex(
                name: "IX_Agente_Ventas_Id_Proveedor",
                table: "Agente_Ventas",
                newName: "IX_Agente_Ventas_Proveedor");

            migrationBuilder.AlterColumn<int>(
                name: "Id_Cliente",
                table: "Venta",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "ClienteId",
                table: "Venta",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Proveedor",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Direccion",
                table: "Proveedor",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Correo_Electronico",
                table: "Proveedor",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "Id_Materia_Prima",
                table: "Producto",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Id_Categoria",
                table: "Producto",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "Se_Daña",
                table: "Producto",
                type: "bit",
                nullable: false,
                defaultValue: false);

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

            migrationBuilder.AddColumn<bool>(
                name: "Estado",
                table: "Motivo_Recordatorio",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Existencia",
                table: "Inventario",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Precio_Compra",
                table: "Inventario",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Precio_Venta",
                table: "Inventario",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Reservado",
                table: "Inventario",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "Impuesto",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

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

            migrationBuilder.AddColumn<decimal>(
                name: "Costo_Total_Grabado",
                table: "Compra",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "Hora",
                table: "Compra",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "Correo_Electronico",
                table: "Cliente",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "Estado",
                table: "Cliente",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Correo_Electronico",
                table: "Agente_Ventas",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "Pedido_Metodo_Pago",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Id_Pedido = table.Column<int>(type: "int", nullable: false),
                    Id_Metodo_Pago = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedido_Metodo_Pago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pedido_Metodo_Pago_Metodo_Pago_Id_Metodo_Pago",
                        column: x => x.Id_Metodo_Pago,
                        principalTable: "Metodo_Pago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pedido_Metodo_Pago_Pedido_Id_Pedido",
                        column: x => x.Id_Pedido,
                        principalTable: "Pedido",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_Venta_ClienteId",
                table: "Venta",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Produccion_Detalle_Receta",
                table: "Produccion_Detalle",
                column: "Id_Receta");

            migrationBuilder.CreateIndex(
                name: "IX_Inventario_Producto",
                table: "Inventario",
                column: "Codigo_Producto",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_Metodo_Pago_Id_Metodo_Pago",
                table: "Pedido_Metodo_Pago",
                column: "Id_Metodo_Pago");

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_Metodo_Pago_Id_Pedido",
                table: "Pedido_Metodo_Pago",
                column: "Id_Pedido");

            migrationBuilder.CreateIndex(
                name: "IX_Receta_Materia_Prima_Materia_Prima",
                table: "Receta_Materia_Prima",
                column: "Id_Materia_Prima");

            migrationBuilder.CreateIndex(
                name: "IX_Receta_Materia_Prima_Receta",
                table: "Receta_Materia_Prima",
                column: "Id_Receta");

            migrationBuilder.AddForeignKey(
                name: "FK_Agente_Ventas_Proveedor_Id_Proveedor",
                table: "Agente_Ventas",
                column: "Id_Proveedor",
                principalTable: "Proveedor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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

            migrationBuilder.AddForeignKey(
                name: "FK_Produccion_Detalle_Receta_Id_Receta",
                table: "Produccion_Detalle",
                column: "Id_Receta",
                principalTable: "Receta",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Venta_Cliente_ClienteId",
                table: "Venta",
                column: "ClienteId",
                principalTable: "Cliente",
                principalColumn: "Id");

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

            migrationBuilder.DropForeignKey(
                name: "FK_Produccion_Detalle_Receta_Id_Receta",
                table: "Produccion_Detalle");

            migrationBuilder.DropForeignKey(
                name: "FK_Venta_Cliente_ClienteId",
                table: "Venta");

            migrationBuilder.DropForeignKey(
                name: "FK_Venta_Cliente_Id_Cliente",
                table: "Venta");

            migrationBuilder.DropTable(
                name: "Pedido_Metodo_Pago");

            migrationBuilder.DropTable(
                name: "Receta_Materia_Prima");

            migrationBuilder.DropIndex(
                name: "IX_Venta_ClienteId",
                table: "Venta");

            migrationBuilder.DropIndex(
                name: "IX_Produccion_Detalle_Receta",
                table: "Produccion_Detalle");

            migrationBuilder.DropIndex(
                name: "IX_Inventario_Producto",
                table: "Inventario");

            migrationBuilder.DropColumn(
                name: "ClienteId",
                table: "Venta");

            migrationBuilder.DropColumn(
                name: "Se_Daña",
                table: "Producto");

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

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Motivo_Recordatorio");

            migrationBuilder.DropColumn(
                name: "Existencia",
                table: "Inventario");

            migrationBuilder.DropColumn(
                name: "Precio_Compra",
                table: "Inventario");

            migrationBuilder.DropColumn(
                name: "Precio_Venta",
                table: "Inventario");

            migrationBuilder.DropColumn(
                name: "Reservado",
                table: "Inventario");

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
                name: "Costo_Total_Grabado",
                table: "Compra");

            migrationBuilder.DropColumn(
                name: "Hora",
                table: "Compra");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Cliente");

            migrationBuilder.RenameIndex(
                name: "IX_Venta_Cliente",
                table: "Venta",
                newName: "IX_Venta_Id_Cliente");

            migrationBuilder.RenameColumn(
                name: "Id_Receta",
                table: "Produccion_Detalle",
                newName: "Cantidad_Productos");

            migrationBuilder.RenameIndex(
                name: "IX_Produccion_Detalle_Producto",
                table: "Produccion_Detalle",
                newName: "IX_Produccion_Detalle_Codigo_Producto");

            migrationBuilder.RenameIndex(
                name: "IX_Compra_Proveedor",
                table: "Compra",
                newName: "IX_Compra_Id_Proveedor");

            migrationBuilder.RenameIndex(
                name: "IX_Agente_Ventas_Proveedor",
                table: "Agente_Ventas",
                newName: "IX_Agente_Ventas_Id_Proveedor");

            migrationBuilder.AlterColumn<int>(
                name: "Id_Cliente",
                table: "Venta",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Id_Agente_Ventas",
                table: "Venta",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Proveedor",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Direccion",
                table: "Proveedor",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Correo_Electronico",
                table: "Proveedor",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id_Materia_Prima",
                table: "Producto",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id_Categoria",
                table: "Producto",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Cantidad_Preparacion",
                table: "Produccion_Detalle",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Id_Agente_Ventas",
                table: "Pedido",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "Impuesto",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<bool>(
                name: "Estado",
                table: "Compra",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Gran_Total",
                table: "Compra",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Numero_Factura",
                table: "Compra",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Correo_Electronico",
                table: "Cliente",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Correo_Electronico",
                table: "Agente_Ventas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Venta_Id_Agente_Ventas",
                table: "Venta",
                column: "Id_Agente_Ventas");

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_Id_Agente_Ventas",
                table: "Pedido",
                column: "Id_Agente_Ventas");

            migrationBuilder.CreateIndex(
                name: "IX_Inventario_Codigo_Producto",
                table: "Inventario",
                column: "Codigo_Producto");

            migrationBuilder.AddForeignKey(
                name: "FK_Agente_Ventas_Proveedor_Id_Proveedor",
                table: "Agente_Ventas",
                column: "Id_Proveedor",
                principalTable: "Proveedor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_Pedido_Agente_Ventas_Id_Agente_Ventas",
                table: "Pedido",
                column: "Id_Agente_Ventas",
                principalTable: "Agente_Ventas",
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
