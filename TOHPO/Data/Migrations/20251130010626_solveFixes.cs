using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TOHPO.Data.Migrations
{
    /// <inheritdoc />
    public partial class solveFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedido_Agente_Ventas_Id_Agente_Ventas",
                table: "Pedido");

            migrationBuilder.DropForeignKey(
                name: "FK_Venta_Agente_Ventas_Id_Agente_Ventas",
                table: "Venta");

            migrationBuilder.DropIndex(
                name: "IX_Venta_Agente_Ventas",
                table: "Venta");

            migrationBuilder.DropIndex(
                name: "IX_Pedido_Id_Agente_Ventas",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "Id_Agente_Ventas",
                table: "Venta");

            migrationBuilder.DropColumn(
                name: "Id_Agente_Ventas",
                table: "Pedido");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Id_Agente_Ventas",
                table: "Venta",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Id_Agente_Ventas",
                table: "Pedido",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Venta_Agente_Ventas",
                table: "Venta",
                column: "Id_Agente_Ventas");

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_Id_Agente_Ventas",
                table: "Pedido",
                column: "Id_Agente_Ventas");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedido_Agente_Ventas_Id_Agente_Ventas",
                table: "Pedido",
                column: "Id_Agente_Ventas",
                principalTable: "Agente_Ventas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Venta_Agente_Ventas_Id_Agente_Ventas",
                table: "Venta",
                column: "Id_Agente_Ventas",
                principalTable: "Agente_Ventas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
