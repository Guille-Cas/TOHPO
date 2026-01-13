using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TOHPO.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEstadoToClienteAndMotivoRecordatorio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClienteId",
                table: "Venta",
                type: "int",
                nullable: true);

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

            migrationBuilder.AddColumn<bool>(
                name: "Estado",
                table: "Motivo_Recordatorio",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Estado",
                table: "Cliente",
                type: "bit",
                nullable: false,
                defaultValue: false);

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

            migrationBuilder.CreateIndex(
                name: "IX_Venta_ClienteId",
                table: "Venta",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_Metodo_Pago_Id_Metodo_Pago",
                table: "Pedido_Metodo_Pago",
                column: "Id_Metodo_Pago");

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_Metodo_Pago_Id_Pedido",
                table: "Pedido_Metodo_Pago",
                column: "Id_Pedido");

            migrationBuilder.AddForeignKey(
                name: "FK_Venta_Cliente_ClienteId",
                table: "Venta",
                column: "ClienteId",
                principalTable: "Cliente",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Venta_Cliente_ClienteId",
                table: "Venta");

            migrationBuilder.DropTable(
                name: "Pedido_Metodo_Pago");

            migrationBuilder.DropIndex(
                name: "IX_Venta_ClienteId",
                table: "Venta");

            migrationBuilder.DropColumn(
                name: "ClienteId",
                table: "Venta");

            migrationBuilder.DropColumn(
                name: "Se_Daña",
                table: "Producto");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Motivo_Recordatorio");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Cliente");

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
        }
    }
}
