using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApartmanYonetim.Migrations
{
    /// <inheritdoc />
    public partial class AddBlockAndAidatTanimi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AidatTanimiId",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BlockId",
                table: "Apartments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AidatTanimlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Baslik = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Tutar = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Ay = table.Column<int>(type: "int", nullable: false),
                    Yil = table.Column<int>(type: "int", nullable: false),
                    SonOdemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TumDairelereUygula = table.Column<bool>(type: "bit", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AidatTanimlari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Blocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blocks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_AidatTanimiId",
                table: "Payments",
                column: "AidatTanimiId");

            migrationBuilder.CreateIndex(
                name: "IX_Apartments_BlockId",
                table: "Apartments",
                column: "BlockId");

            migrationBuilder.AddForeignKey(
                name: "FK_Apartments_Blocks_BlockId",
                table: "Apartments",
                column: "BlockId",
                principalTable: "Blocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_AidatTanimlari_AidatTanimiId",
                table: "Payments",
                column: "AidatTanimiId",
                principalTable: "AidatTanimlari",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Apartments_Blocks_BlockId",
                table: "Apartments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_AidatTanimlari_AidatTanimiId",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "AidatTanimlari");

            migrationBuilder.DropTable(
                name: "Blocks");

            migrationBuilder.DropIndex(
                name: "IX_Payments_AidatTanimiId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Apartments_BlockId",
                table: "Apartments");

            migrationBuilder.DropColumn(
                name: "AidatTanimiId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "BlockId",
                table: "Apartments");
        }
    }
}
