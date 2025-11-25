using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TOHPO.Data.Migrations
{
    /// <inheritdoc />
    public partial class models_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categoria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Impuesto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Porcentaje = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Impuesto", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Materia_Prima",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    Unidad_Medida = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materia_Prima", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Presentacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cantidad = table.Column<double>(type: "float", nullable: false),
                    Unidad_Medida = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Presentacion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Proveedor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Correo_Electronico = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Producto",
                columns: table => new
                {
                    CodigoReferencia = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Codigo_Barra = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Es_Materia_Prima = table.Column<bool>(type: "bit", nullable: false),
                    Es_De_Terceros = table.Column<bool>(type: "bit", nullable: false),
                    Unidad_Medida = table.Column<int>(type: "int", nullable: false),
                    Tiempo_De_Vida = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    Id_Categoria = table.Column<int>(type: "int", nullable: false),
                    Id_Materia_Prima = table.Column<int>(type: "int", nullable: false),
                    Id_Presentacion = table.Column<int>(type: "int", nullable: false),
                    Id_Impuesto = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Producto", x => x.CodigoReferencia);
                    table.ForeignKey(
                        name: "FK_Producto_Categoria_Id_Categoria",
                        column: x => x.Id_Categoria,
                        principalTable: "Categoria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Producto_Impuesto_Id_Impuesto",
                        column: x => x.Id_Impuesto,
                        principalTable: "Impuesto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Producto_Materia_Prima_Id_Materia_Prima",
                        column: x => x.Id_Materia_Prima,
                        principalTable: "Materia_Prima",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Producto_Presentacion_Id_Presentacion",
                        column: x => x.Id_Presentacion,
                        principalTable: "Presentacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Agente_Ventas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Correo_Electronico = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    Id_Proveedor = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agente_Ventas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Agente_Ventas_Proveedor_Id_Proveedor",
                        column: x => x.Id_Proveedor,
                        principalTable: "Proveedor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Producto_Proveedor",
                columns: table => new
                {
                    Codigo_Producto = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Id_Proveedor = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Producto_Proveedor", x => new { x.Codigo_Producto, x.Id_Proveedor });
                    table.ForeignKey(
                        name: "FK_Producto_Proveedor_Producto_Codigo_Producto",
                        column: x => x.Codigo_Producto,
                        principalTable: "Producto",
                        principalColumn: "CodigoReferencia",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Producto_Proveedor_Proveedor_Id_Proveedor",
                        column: x => x.Id_Proveedor,
                        principalTable: "Proveedor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Receta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rendimiento = table.Column<double>(type: "float", nullable: false),
                    Instrucciones = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Detalle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cantidad_Empaque = table.Column<double>(type: "float", nullable: false),
                    Fecha_Creacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    Codigo_Producto = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Receta_Producto_Codigo_Producto",
                        column: x => x.Codigo_Producto,
                        principalTable: "Producto",
                        principalColumn: "CodigoReferencia",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agente_Ventas_Id_Proveedor",
                table: "Agente_Ventas",
                column: "Id_Proveedor");

            migrationBuilder.CreateIndex(
                name: "IX_Producto_Id_Categoria",
                table: "Producto",
                column: "Id_Categoria");

            migrationBuilder.CreateIndex(
                name: "IX_Producto_Id_Impuesto",
                table: "Producto",
                column: "Id_Impuesto");

            migrationBuilder.CreateIndex(
                name: "IX_Producto_Id_Materia_Prima",
                table: "Producto",
                column: "Id_Materia_Prima");

            migrationBuilder.CreateIndex(
                name: "IX_Producto_Id_Presentacion",
                table: "Producto",
                column: "Id_Presentacion");

            migrationBuilder.CreateIndex(
                name: "IX_Producto_Proveedor_Id_Proveedor",
                table: "Producto_Proveedor",
                column: "Id_Proveedor");

            migrationBuilder.CreateIndex(
                name: "IX_Receta_Codigo_Producto",
                table: "Receta",
                column: "Codigo_Producto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Agente_Ventas");

            migrationBuilder.DropTable(
                name: "Producto_Proveedor");

            migrationBuilder.DropTable(
                name: "Receta");

            migrationBuilder.DropTable(
                name: "Proveedor");

            migrationBuilder.DropTable(
                name: "Producto");

            migrationBuilder.DropTable(
                name: "Categoria");

            migrationBuilder.DropTable(
                name: "Impuesto");

            migrationBuilder.DropTable(
                name: "Materia_Prima");

            migrationBuilder.DropTable(
                name: "Presentacion");
        }
    }
}
