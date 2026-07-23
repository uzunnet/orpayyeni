using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.Api.Veri.Migrations
{
    /// <inheritdoc />
    public partial class AISaglayicisiVeKayitEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AISaglayicilari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Tip = table.Column<int>(type: "INTEGER", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    ApiKeyEncrypted = table.Column<string>(type: "TEXT", nullable: false),
                    Model = table.Column<string>(type: "TEXT", nullable: false),
                    AylikLimitUsd = table.Column<decimal>(type: "TEXT", nullable: false),
                    KullanilanUsd = table.Column<decimal>(type: "TEXT", nullable: false),
                    SonSifirlamaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    EkBaslik = table.Column<string>(type: "TEXT", nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AISaglayicilari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AICagrisiKayitlari",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SaglayiciId = table.Column<int>(type: "INTEGER", nullable: false),
                    KullaniciId = table.Column<string>(type: "TEXT", nullable: true),
                    KullanimAmaci = table.Column<string>(type: "TEXT", nullable: false),
                    IstekTokenSayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    CevapTokenSayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    ToplamMaliyetUsd = table.Column<decimal>(type: "TEXT", nullable: false),
                    Prompt = table.Column<string>(type: "TEXT", nullable: true),
                    Durum = table.Column<int>(type: "INTEGER", nullable: false),
                    HataMesaji = table.Column<string>(type: "TEXT", nullable: true),
                    SureMs = table.Column<long>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AICagrisiKayitlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AICagrisiKayitlari_AISaglayicilari_SaglayiciId",
                        column: x => x.SaglayiciId,
                        principalTable: "AISaglayicilari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AICagrisiKayitlari_SaglayiciId",
                table: "AICagrisiKayitlari",
                column: "SaglayiciId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AICagrisiKayitlari");

            migrationBuilder.DropTable(
                name: "AISaglayicilari");
        }
    }
}
