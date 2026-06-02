using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApartmanYonetim.Migrations
{
    /// <inheritdoc />
    public partial class AddZiyaretciBakimBildirimProfil : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfilFoto",
                table: "Kullanicilar",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BakimGorevler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Baslik = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Kategori = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Periyot = table.Column<int>(type: "int", nullable: false),
                    Durum = table.Column<int>(type: "int", nullable: false),
                    PlanlananTarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TamamlanmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Maliyet = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Firma = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notlar = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OlusturanId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DemirbasId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BakimGorevler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BakimGorevler_Demirbaslar_DemirbasId",
                        column: x => x.DemirbasId,
                        principalTable: "Demirbaslar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BakimGorevler_Kullanicilar_OlusturanId",
                        column: x => x.OlusturanId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Ziyaretciler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdSoyad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AracPlaka = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Durum = table.Column<int>(type: "int", nullable: false),
                    BeklenenGirisSaati = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GercekGirisSaati = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CikisSaati = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApartmentId = table.Column<int>(type: "int", nullable: true),
                    DavetEdenId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OnaylayanId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ziyaretciler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ziyaretciler_Apartments_ApartmentId",
                        column: x => x.ApartmentId,
                        principalTable: "Apartments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Ziyaretciler_Kullanicilar_DavetEdenId",
                        column: x => x.DavetEdenId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ziyaretciler_Kullanicilar_OnaylayanId",
                        column: x => x.OnaylayanId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BakimGorevler_DemirbasId",
                table: "BakimGorevler",
                column: "DemirbasId");

            migrationBuilder.CreateIndex(
                name: "IX_BakimGorevler_OlusturanId",
                table: "BakimGorevler",
                column: "OlusturanId");

            migrationBuilder.CreateIndex(
                name: "IX_Ziyaretciler_ApartmentId",
                table: "Ziyaretciler",
                column: "ApartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Ziyaretciler_DavetEdenId",
                table: "Ziyaretciler",
                column: "DavetEdenId");

            migrationBuilder.CreateIndex(
                name: "IX_Ziyaretciler_OnaylayanId",
                table: "Ziyaretciler",
                column: "OnaylayanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BakimGorevler");

            migrationBuilder.DropTable(
                name: "Ziyaretciler");

            migrationBuilder.DropColumn(
                name: "ProfilFoto",
                table: "Kullanicilar");
        }
    }
}
