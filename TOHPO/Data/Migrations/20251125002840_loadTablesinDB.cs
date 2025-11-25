using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TOHPO.Data.Migrations
{
    /// <inheritdoc />
    public partial class loadTablesinDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.CreateTable(
                name: "Compra",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Numero_Factura = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Iva = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Gran_Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    Id_Proveedor = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Compra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Compra_Proveedor_Id_Proveedor",
                        column: x => x.Id_Proveedor,
                        principalTable: "Proveedor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Inventario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    Codigo_Producto = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inventario_Producto_Codigo_Producto",
                        column: x => x.Codigo_Producto,
                        principalTable: "Producto",
                        principalColumn: "CodigoReferencia",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pedido",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha_Creacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Fecha_Entrega = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Abono = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Saldo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    Id_Cliente = table.Column<int>(type: "int", nullable: false),
                    Id_Agente_Ventas = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedido", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pedido_Agente_Ventas_Id_Agente_Ventas",
                        column: x => x.Id_Agente_Ventas,
                        principalTable: "Agente_Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pedido_Cliente_Id_Cliente",
                        column: x => x.Id_Cliente,
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Produccion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Obra = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Fecha_Planeada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produccion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Venta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Concepto = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Costo_Total_Gravado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Iva = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Hora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Id_Cliente = table.Column<int>(type: "int", nullable: false),
                    Id_Agente_Ventas = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Venta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Venta_Agente_Ventas_Id_Agente_Ventas",
                        column: x => x.Id_Agente_Ventas,
                        principalTable: "Agente_Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Venta_Cliente_Id_Cliente",
                        column: x => x.Id_Cliente,
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Compra_Detalle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    Costo_Unitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Porcentaje_Descuento = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Monto_Descuento = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Id_Compra = table.Column<int>(type: "int", nullable: false),
                    Codigo_Producto = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Compra_Detalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Compra_Detalle_Compra_Id_Compra",
                        column: x => x.Id_Compra,
                        principalTable: "Compra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Compra_Detalle_Producto_Codigo_Producto",
                        column: x => x.Codigo_Producto,
                        principalTable: "Producto",
                        principalColumn: "CodigoReferencia",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Compra_Metodo_Pago",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Id_Compra = table.Column<int>(type: "int", nullable: false),
                    Id_Metodo_Pago = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Compra_Metodo_Pago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Compra_Metodo_Pago_Compra_Id_Compra",
                        column: x => x.Id_Compra,
                        principalTable: "Compra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Compra_Metodo_Pago_Metodo_Pago_Id_Metodo_Pago",
                        column: x => x.Id_Metodo_Pago,
                        principalTable: "Metodo_Pago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Detalle_Inventario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Cantidad_Materia_Prima = table.Column<int>(type: "int", nullable: false),
                    Id_Inventario = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Detalle_Inventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Detalle_Inventario_Inventario_Id_Inventario",
                        column: x => x.Id_Inventario,
                        principalTable: "Inventario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Movimiento_Inventario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Id_Inventario = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movimiento_Inventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Movimiento_Inventario_Inventario_Id_Inventario",
                        column: x => x.Id_Inventario,
                        principalTable: "Inventario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pedido_Detalle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    Precio_Unitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Id_Pedido = table.Column<int>(type: "int", nullable: false),
                    Codigo_Producto = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedido_Detalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pedido_Detalle_Pedido_Id_Pedido",
                        column: x => x.Id_Pedido,
                        principalTable: "Pedido",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pedido_Detalle_Producto_Codigo_Producto",
                        column: x => x.Codigo_Producto,
                        principalTable: "Producto",
                        principalColumn: "CodigoReferencia",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Produccion_Detalle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cantidad_Productos = table.Column<int>(type: "int", nullable: false),
                    Cantidad_Preparacion = table.Column<int>(type: "int", nullable: false),
                    Id_Produccion = table.Column<int>(type: "int", nullable: false),
                    Codigo_Producto = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produccion_Detalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Produccion_Detalle_Produccion_Id_Produccion",
                        column: x => x.Id_Produccion,
                        principalTable: "Produccion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Produccion_Detalle_Producto_Codigo_Producto",
                        column: x => x.Codigo_Producto,
                        principalTable: "Producto",
                        principalColumn: "CodigoReferencia",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Detalle_Venta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    Precio_Unitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Porcentaje_Descuento = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Monto_Descuento = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Monto_Impuesto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Id_Venta = table.Column<int>(type: "int", nullable: false),
                    Codigo_Producto = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Detalle_Venta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Detalle_Venta_Producto_Codigo_Producto",
                        column: x => x.Codigo_Producto,
                        principalTable: "Producto",
                        principalColumn: "CodigoReferencia",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Detalle_Venta_Venta_Id_Venta",
                        column: x => x.Id_Venta,
                        principalTable: "Venta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Venta_Metodo_Pago",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Id_Venta = table.Column<int>(type: "int", nullable: false),
                    Id_Metodo_Pago = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Venta_Metodo_Pago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Venta_Metodo_Pago_Metodo_Pago_Id_Metodo_Pago",
                        column: x => x.Id_Metodo_Pago,
                        principalTable: "Metodo_Pago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Venta_Metodo_Pago_Venta_Id_Venta",
                        column: x => x.Id_Venta,
                        principalTable: "Venta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Compra_Fecha",
                table: "Compra",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_Compra_Id_Proveedor",
                table: "Compra",
                column: "Id_Proveedor");

            migrationBuilder.CreateIndex(
                name: "IX_Compra_Detalle_Codigo_Producto",
                table: "Compra_Detalle",
                column: "Codigo_Producto");

            migrationBuilder.CreateIndex(
                name: "IX_Compra_Detalle_Id_Compra",
                table: "Compra_Detalle",
                column: "Id_Compra");

            migrationBuilder.CreateIndex(
                name: "IX_Compra_Metodo_Pago_Id_Compra",
                table: "Compra_Metodo_Pago",
                column: "Id_Compra");

            migrationBuilder.CreateIndex(
                name: "IX_Compra_Metodo_Pago_Id_Metodo_Pago",
                table: "Compra_Metodo_Pago",
                column: "Id_Metodo_Pago");

            migrationBuilder.CreateIndex(
                name: "IX_Detalle_Inventario_Id_Inventario",
                table: "Detalle_Inventario",
                column: "Id_Inventario");

            migrationBuilder.CreateIndex(
                name: "IX_Detalle_Venta_Codigo_Producto",
                table: "Detalle_Venta",
                column: "Codigo_Producto");

            migrationBuilder.CreateIndex(
                name: "IX_Detalle_Venta_Id_Venta",
                table: "Detalle_Venta",
                column: "Id_Venta");

            migrationBuilder.CreateIndex(
                name: "IX_Inventario_Codigo_Producto",
                table: "Inventario",
                column: "Codigo_Producto");

            migrationBuilder.CreateIndex(
                name: "IX_Movimiento_Inventario_Id_Inventario",
                table: "Movimiento_Inventario",
                column: "Id_Inventario");

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_Id_Agente_Ventas",
                table: "Pedido",
                column: "Id_Agente_Ventas");

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_Id_Cliente",
                table: "Pedido",
                column: "Id_Cliente");

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_Detalle_Codigo_Producto",
                table: "Pedido_Detalle",
                column: "Codigo_Producto");

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_Detalle_Id_Pedido",
                table: "Pedido_Detalle",
                column: "Id_Pedido");

            migrationBuilder.CreateIndex(
                name: "IX_Produccion_Fecha",
                table: "Produccion",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_Produccion_Detalle_Codigo_Producto",
                table: "Produccion_Detalle",
                column: "Codigo_Producto");

            migrationBuilder.CreateIndex(
                name: "IX_Produccion_Detalle_Id_Produccion",
                table: "Produccion_Detalle",
                column: "Id_Produccion");

            migrationBuilder.CreateIndex(
                name: "IX_Venta_Fecha",
                table: "Venta",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_Venta_Id_Agente_Ventas",
                table: "Venta",
                column: "Id_Agente_Ventas");

            migrationBuilder.CreateIndex(
                name: "IX_Venta_Id_Cliente",
                table: "Venta",
                column: "Id_Cliente");

            migrationBuilder.CreateIndex(
                name: "IX_Venta_Metodo_Pago_Id_Metodo_Pago",
                table: "Venta_Metodo_Pago",
                column: "Id_Metodo_Pago");

            migrationBuilder.CreateIndex(
                name: "IX_Venta_Metodo_Pago_Id_Venta",
                table: "Venta_Metodo_Pago",
                column: "Id_Venta");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Compra_Detalle");

            migrationBuilder.DropTable(
                name: "Compra_Metodo_Pago");

            migrationBuilder.DropTable(
                name: "Detalle_Inventario");

            migrationBuilder.DropTable(
                name: "Detalle_Venta");

            migrationBuilder.DropTable(
                name: "Movimiento_Inventario");

            migrationBuilder.DropTable(
                name: "Pedido_Detalle");

            migrationBuilder.DropTable(
                name: "Produccion_Detalle");

            migrationBuilder.DropTable(
                name: "Venta_Metodo_Pago");

            migrationBuilder.DropTable(
                name: "Compra");

            migrationBuilder.DropTable(
                name: "Inventario");

            migrationBuilder.DropTable(
                name: "Pedido");

            migrationBuilder.DropTable(
                name: "Produccion");

            migrationBuilder.DropTable(
                name: "Venta");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
