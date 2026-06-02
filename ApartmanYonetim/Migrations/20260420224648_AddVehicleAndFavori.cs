using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApartmanYonetim.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleAndFavori : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FavoriSayfalar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Baslik = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Ikon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Sira = table.Column<int>(type: "int", nullable: false),
                    EklemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoriSayfalar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FavoriSayfalar_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Plaka = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Marka = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Renk = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AracTipi = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OtoparkNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ZiyaretciMi = table.Column<bool>(type: "bit", nullable: false),
                    ZiyaretGiris = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ZiyaretCikis = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ApartmentId = table.Column<int>(type: "int", nullable: true),
                    KaydedenId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehicles_Apartments_ApartmentId",
                        column: x => x.ApartmentId,
                        principalTable: "Apartments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Vehicles_Kullanicilar_KaydedenId",
                        column: x => x.KaydedenId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FavoriSayfalar_KullaniciId",
                table: "FavoriSayfalar",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_ApartmentId",
                table: "Vehicles",
                column: "ApartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_KaydedenId",
                table: "Vehicles",
                column: "KaydedenId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FavoriSayfalar");

            migrationBuilder.DropTable(
                name: "Vehicles");
        }
    }
}
