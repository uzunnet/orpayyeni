using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.Api.Veri.Migrations
{
    /// <inheritdoc />
    public partial class MedyaHavuzuEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MedyaKlasorleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirmaId = table.Column<int>(type: "INTEGER", nullable: true),
                    UstKlasorId = table.Column<int>(type: "INTEGER", nullable: true),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", nullable: true),
                    Ikon = table.Column<string>(type: "TEXT", nullable: true),
                    Renk = table.Column<string>(type: "TEXT", nullable: true),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedyaKlasorleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedyaKlasorleri_MedyaKlasorleri_UstKlasorId",
                        column: x => x.UstKlasorId,
                        principalTable: "MedyaKlasorleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Medyalar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirmaId = table.Column<int>(type: "INTEGER", nullable: true),
                    Tip = table.Column<int>(type: "INTEGER", nullable: false),
                    Kaynak = table.Column<int>(type: "INTEGER", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    OrijinalAd = table.Column<string>(type: "TEXT", nullable: true),
                    DosyaYolu = table.Column<string>(type: "TEXT", nullable: true),
                    MiniaturYolu = table.Column<string>(type: "TEXT", nullable: true),
                    KaynakUrl = table.Column<string>(type: "TEXT", nullable: true),
                    BoyutByte = table.Column<long>(type: "INTEGER", nullable: false),
                    Genislik = table.Column<int>(type: "INTEGER", nullable: true),
                    Yukseklik = table.Column<int>(type: "INTEGER", nullable: true),
                    SureSaniye = table.Column<int>(type: "INTEGER", nullable: true),
                    MimeTipi = table.Column<string>(type: "TEXT", nullable: true),
                    Hash = table.Column<string>(type: "TEXT", nullable: true),
                    AltMetin = table.Column<string>(type: "TEXT", nullable: true),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    EtiketlerJson = table.Column<string>(type: "TEXT", nullable: true),
                    KlasorId = table.Column<int>(type: "INTEGER", nullable: true),
                    KullanimSayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    YukleyenKullaniciId = table.Column<string>(type: "TEXT", nullable: true),
                    SilindiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SilinmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medyalar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Medyalar_MedyaKlasorleri_KlasorId",
                        column: x => x.KlasorId,
                        principalTable: "MedyaKlasorleri",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MedyaKullanimlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MedyaId = table.Column<int>(type: "INTEGER", nullable: false),
                    EntiteAdi = table.Column<string>(type: "TEXT", nullable: false),
                    EntiteId = table.Column<int>(type: "INTEGER", nullable: false),
                    AlanAdi = table.Column<string>(type: "TEXT", nullable: true),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedyaKullanimlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedyaKullanimlari_Medyalar_MedyaId",
                        column: x => x.MedyaId,
                        principalTable: "Medyalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedyaKlasorleri_UstKlasorId",
                table: "MedyaKlasorleri",
                column: "UstKlasorId");

            migrationBuilder.CreateIndex(
                name: "IX_MedyaKullanimlari_EntiteAdi_EntiteId",
                table: "MedyaKullanimlari",
                columns: new[] { "EntiteAdi", "EntiteId" });

            migrationBuilder.CreateIndex(
                name: "IX_MedyaKullanimlari_MedyaId",
                table: "MedyaKullanimlari",
                column: "MedyaId");

            migrationBuilder.CreateIndex(
                name: "IX_Medyalar_KlasorId",
                table: "Medyalar",
                column: "KlasorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedyaKullanimlari");

            migrationBuilder.DropTable(
                name: "Medyalar");

            migrationBuilder.DropTable(
                name: "MedyaKlasorleri");
        }
    }
}
