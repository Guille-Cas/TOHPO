using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TOHPO.Data.Migrations
{
    /// <inheritdoc />
    public partial class bugfixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "IX_Pedido_Metodo_Pago_Id_Metodo_Pago",
                table: "Pedido_Metodo_Pago",
                column: "Id_Metodo_Pago");

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_Metodo_Pago_Id_Pedido",
                table: "Pedido_Metodo_Pago",
                column: "Id_Pedido");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pedido_Metodo_Pago");
        }
    }
}
