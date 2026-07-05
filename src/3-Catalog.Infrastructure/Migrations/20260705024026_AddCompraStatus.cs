using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _3Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompraStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataProcessamentoPagamento",
                table: "Compras",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoRecusa",
                table: "Compras",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Compras",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pendente");

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "Compras",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Compras_UsuarioId",
                table: "Compras",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Compras_UsuarioId",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "DataProcessamentoPagamento",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "MotivoRecusa",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Compras");
        }
    }
}
