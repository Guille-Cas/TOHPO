using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TOHPO.Data.Migrations
{
    /// <inheritdoc />
    public partial class reminderRecurrent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Detalles",
                table: "Recordatorio",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "EsRecurrente",
                table: "Recordatorio",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaFinRecurrencia",
                table: "Recordatorio",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntervaloRecurrencia",
                table: "Recordatorio",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaximoRepeticiones",
                table: "Recordatorio",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecordatorioPadreId",
                table: "Recordatorio",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoRecurrencia",
                table: "Recordatorio",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recordatorio_RecordatorioPadreId",
                table: "Recordatorio",
                column: "RecordatorioPadreId");

            migrationBuilder.AddForeignKey(
                name: "FK_Recordatorio_Recordatorio_RecordatorioPadreId",
                table: "Recordatorio",
                column: "RecordatorioPadreId",
                principalTable: "Recordatorio",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recordatorio_Recordatorio_RecordatorioPadreId",
                table: "Recordatorio");

            migrationBuilder.DropIndex(
                name: "IX_Recordatorio_RecordatorioPadreId",
                table: "Recordatorio");

            migrationBuilder.DropColumn(
                name: "EsRecurrente",
                table: "Recordatorio");

            migrationBuilder.DropColumn(
                name: "FechaFinRecurrencia",
                table: "Recordatorio");

            migrationBuilder.DropColumn(
                name: "IntervaloRecurrencia",
                table: "Recordatorio");

            migrationBuilder.DropColumn(
                name: "MaximoRepeticiones",
                table: "Recordatorio");

            migrationBuilder.DropColumn(
                name: "RecordatorioPadreId",
                table: "Recordatorio");

            migrationBuilder.DropColumn(
                name: "TipoRecurrencia",
                table: "Recordatorio");

            migrationBuilder.AlterColumn<string>(
                name: "Detalles",
                table: "Recordatorio",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
