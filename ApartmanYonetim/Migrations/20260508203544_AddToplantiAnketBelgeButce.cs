using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApartmanYonetim.Migrations
{
    /// <inheritdoc />
    public partial class AddToplantiAnketBelgeButce : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Anketler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Baslik = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    BaslangicTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BitisTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false),
                    OlusturanId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anketler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Anketler_Kullanicilar_OlusturanId",
                        column: x => x.OlusturanId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Belgeler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Kategori = table.Column<int>(type: "int", nullable: false),
                    DosyaYolu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DosyaAdi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DosyaTipi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DosyaBoyutu = table.Column<long>(type: "bigint", nullable: false),
                    YuklemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    YukleyenId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HerkesGorebilir = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Belgeler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Belgeler_Kullanicilar_YukleyenId",
                        column: x => x.YukleyenId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ButcePlanlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Yil = table.Column<int>(type: "int", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OlusturanId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ButcePlanlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ButcePlanlar_Kullanicilar_OlusturanId",
                        column: x => x.OlusturanId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Toplantilar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Baslik = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Konum = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Durum = table.Column<int>(type: "int", nullable: false),
                    OlusturanId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notlar = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Toplantilar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Toplantilar_Kullanicilar_OlusturanId",
                        column: x => x.OlusturanId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "YoneticiBlocklar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    YoneticiId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BlockId = table.Column<int>(type: "int", nullable: false),
                    AtamaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YoneticiBlocklar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YoneticiBlocklar_Blocks_BlockId",
                        column: x => x.BlockId,
                        principalTable: "Blocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YoneticiBlocklar_Kullanicilar_YoneticiId",
                        column: x => x.YoneticiId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnketSiklari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnketId = table.Column<int>(type: "int", nullable: false),
                    Metin = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SiraNo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnketSiklari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnketSiklari_Anketler_AnketId",
                        column: x => x.AnketId,
                        principalTable: "Anketler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ButceKalemler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ButcePlanId = table.Column<int>(type: "int", nullable: false),
                    Kategori = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Tip = table.Column<int>(type: "int", nullable: false),
                    PlanlananTutar = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    GerceklesenTutar = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Ay = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ButceKalemler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ButceKalemler_ButcePlanlar_ButcePlanId",
                        column: x => x.ButcePlanId,
                        principalTable: "ButcePlanlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnketOylari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnketId = table.Column<int>(type: "int", nullable: false),
                    AnketSikId = table.Column<int>(type: "int", nullable: false),
                    KullaniciId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OyTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnketOylari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnketOylari_AnketSiklari_AnketSikId",
                        column: x => x.AnketSikId,
                        principalTable: "AnketSiklari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AnketOylari_Anketler_AnketId",
                        column: x => x.AnketId,
                        principalTable: "Anketler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AnketOylari_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Anketler_OlusturanId",
                table: "Anketler",
                column: "OlusturanId");

            migrationBuilder.CreateIndex(
                name: "IX_AnketOylari_AnketId_KullaniciId",
                table: "AnketOylari",
                columns: new[] { "AnketId", "KullaniciId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnketOylari_AnketSikId",
                table: "AnketOylari",
                column: "AnketSikId");

            migrationBuilder.CreateIndex(
                name: "IX_AnketOylari_KullaniciId",
                table: "AnketOylari",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_AnketSiklari_AnketId",
                table: "AnketSiklari",
                column: "AnketId");

            migrationBuilder.CreateIndex(
                name: "IX_Belgeler_YukleyenId",
                table: "Belgeler",
                column: "YukleyenId");

            migrationBuilder.CreateIndex(
                name: "IX_ButceKalemler_ButcePlanId",
                table: "ButceKalemler",
                column: "ButcePlanId");

            migrationBuilder.CreateIndex(
                name: "IX_ButcePlanlar_OlusturanId",
                table: "ButcePlanlar",
                column: "OlusturanId");

            migrationBuilder.CreateIndex(
                name: "IX_Toplantilar_OlusturanId",
                table: "Toplantilar",
                column: "OlusturanId");

            migrationBuilder.CreateIndex(
                name: "IX_YoneticiBlocklar_BlockId",
                table: "YoneticiBlocklar",
                column: "BlockId");

            migrationBuilder.CreateIndex(
                name: "IX_YoneticiBlocklar_YoneticiId_BlockId",
                table: "YoneticiBlocklar",
                columns: new[] { "YoneticiId", "BlockId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnketOylari");

            migrationBuilder.DropTable(
                name: "Belgeler");

            migrationBuilder.DropTable(
                name: "ButceKalemler");

            migrationBuilder.DropTable(
                name: "Toplantilar");

            migrationBuilder.DropTable(
                name: "YoneticiBlocklar");

            migrationBuilder.DropTable(
                name: "AnketSiklari");

            migrationBuilder.DropTable(
                name: "ButcePlanlar");

            migrationBuilder.DropTable(
                name: "Anketler");
        }
    }
}
