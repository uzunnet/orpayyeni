using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.Api.Veri.Migrations
{
    /// <inheritdoc />
    public partial class FirmaApiAnahtariVeKonfigurasyonIyilestirme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Aci",
                table: "MusteriKonfigurasyonParcalari",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GuncellenmeTarihi",
                table: "MusteriKonfigurasyonParcalari",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "HareketDegeri",
                table: "MusteriKonfigurasyonParcalari",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeciliDoku",
                table: "MusteriKonfigurasyonParcalari",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SilindiMi",
                table: "MusteriKonfigurasyonParcalari",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SilinmeTarihi",
                table: "MusteriKonfigurasyonParcalari",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Durum",
                table: "MusteriKonfigurasyonlari",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "FirmaId",
                table: "MusteriKonfigurasyonlari",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GuncelleyenKullaniciId",
                table: "MusteriKonfigurasyonlari",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OlusturanKullaniciId",
                table: "MusteriKonfigurasyonlari",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ToplamFiyat",
                table: "MusteriKonfigurasyonlari",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FirmaApiAnahtarlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirmaId = table.Column<int>(type: "INTEGER", nullable: false),
                    AnahtarAd = table.Column<string>(type: "TEXT", nullable: false),
                    ApiKeyHash = table.Column<string>(type: "TEXT", nullable: false),
                    AnahtarOnEki = table.Column<string>(type: "TEXT", nullable: false),
                    Kapsam = table.Column<string>(type: "TEXT", nullable: false),
                    IzinVerilenDomainler = table.Column<string>(type: "TEXT", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SonKullanmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SonKullanimTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OlusturanKullaniciId = table.Column<int>(type: "INTEGER", nullable: true),
                    GuncelleyenKullaniciId = table.Column<int>(type: "INTEGER", nullable: true),
                    SilindiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SilinmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FirmaApiAnahtarlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FirmaApiAnahtarlari_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MusteriKonfigurasyonParcalari_MusteriKonfigurasyonuId",
                table: "MusteriKonfigurasyonParcalari",
                column: "MusteriKonfigurasyonuId");

            migrationBuilder.CreateIndex(
                name: "IX_MusteriKonfigurasyonParcalari_SeciliKaplamaId",
                table: "MusteriKonfigurasyonParcalari",
                column: "SeciliKaplamaId");

            migrationBuilder.CreateIndex(
                name: "IX_MusteriKonfigurasyonParcalari_SeciliMalzemeId",
                table: "MusteriKonfigurasyonParcalari",
                column: "SeciliMalzemeId");

            migrationBuilder.CreateIndex(
                name: "IX_MusteriKonfigurasyonParcalari_SeciliRenkId",
                table: "MusteriKonfigurasyonParcalari",
                column: "SeciliRenkId");

            migrationBuilder.CreateIndex(
                name: "IX_MusteriKonfigurasyonParcalari_UrunUcBoyutParcasiId",
                table: "MusteriKonfigurasyonParcalari",
                column: "UrunUcBoyutParcasiId");

            migrationBuilder.CreateIndex(
                name: "IX_MusteriKonfigurasyonlari_FirmaId",
                table: "MusteriKonfigurasyonlari",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_MusteriKonfigurasyonlari_UrunId",
                table: "MusteriKonfigurasyonlari",
                column: "UrunId");

            migrationBuilder.CreateIndex(
                name: "IX_FirmaApiAnahtarlari_ApiKeyHash",
                table: "FirmaApiAnahtarlari",
                column: "ApiKeyHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FirmaApiAnahtarlari_FirmaId_AnahtarAd",
                table: "FirmaApiAnahtarlari",
                columns: new[] { "FirmaId", "AnahtarAd" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MusteriKonfigurasyonlari_Firmalar_FirmaId",
                table: "MusteriKonfigurasyonlari",
                column: "FirmaId",
                principalTable: "Firmalar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MusteriKonfigurasyonlari_Urunler_UrunId",
                table: "MusteriKonfigurasyonlari",
                column: "UrunId",
                principalTable: "Urunler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MusteriKonfigurasyonParcalari_KaplamaSecenekleri_SeciliKaplamaId",
                table: "MusteriKonfigurasyonParcalari",
                column: "SeciliKaplamaId",
                principalTable: "KaplamaSecenekleri",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MusteriKonfigurasyonParcalari_Malzemeler_SeciliMalzemeId",
                table: "MusteriKonfigurasyonParcalari",
                column: "SeciliMalzemeId",
                principalTable: "Malzemeler",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MusteriKonfigurasyonParcalari_MusteriKonfigurasyonlari_MusteriKonfigurasyonuId",
                table: "MusteriKonfigurasyonParcalari",
                column: "MusteriKonfigurasyonuId",
                principalTable: "MusteriKonfigurasyonlari",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MusteriKonfigurasyonParcalari_RalRenkleri_SeciliRenkId",
                table: "MusteriKonfigurasyonParcalari",
                column: "SeciliRenkId",
                principalTable: "RalRenkleri",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MusteriKonfigurasyonParcalari_UrunUcBoyutParcalari_UrunUcBoyutParcasiId",
                table: "MusteriKonfigurasyonParcalari",
                column: "UrunUcBoyutParcasiId",
                principalTable: "UrunUcBoyutParcalari",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MusteriKonfigurasyonlari_Firmalar_FirmaId",
                table: "MusteriKonfigurasyonlari");

            migrationBuilder.DropForeignKey(
                name: "FK_MusteriKonfigurasyonlari_Urunler_UrunId",
                table: "MusteriKonfigurasyonlari");

            migrationBuilder.DropForeignKey(
                name: "FK_MusteriKonfigurasyonParcalari_KaplamaSecenekleri_SeciliKaplamaId",
                table: "MusteriKonfigurasyonParcalari");

            migrationBuilder.DropForeignKey(
                name: "FK_MusteriKonfigurasyonParcalari_Malzemeler_SeciliMalzemeId",
                table: "MusteriKonfigurasyonParcalari");

            migrationBuilder.DropForeignKey(
                name: "FK_MusteriKonfigurasyonParcalari_MusteriKonfigurasyonlari_MusteriKonfigurasyonuId",
                table: "MusteriKonfigurasyonParcalari");

            migrationBuilder.DropForeignKey(
                name: "FK_MusteriKonfigurasyonParcalari_RalRenkleri_SeciliRenkId",
                table: "MusteriKonfigurasyonParcalari");

            migrationBuilder.DropForeignKey(
                name: "FK_MusteriKonfigurasyonParcalari_UrunUcBoyutParcalari_UrunUcBoyutParcasiId",
                table: "MusteriKonfigurasyonParcalari");

            migrationBuilder.DropTable(
                name: "FirmaApiAnahtarlari");

            migrationBuilder.DropIndex(
                name: "IX_MusteriKonfigurasyonParcalari_MusteriKonfigurasyonuId",
                table: "MusteriKonfigurasyonParcalari");

            migrationBuilder.DropIndex(
                name: "IX_MusteriKonfigurasyonParcalari_SeciliKaplamaId",
                table: "MusteriKonfigurasyonParcalari");

            migrationBuilder.DropIndex(
                name: "IX_MusteriKonfigurasyonParcalari_SeciliMalzemeId",
                table: "MusteriKonfigurasyonParcalari");

            migrationBuilder.DropIndex(
                name: "IX_MusteriKonfigurasyonParcalari_SeciliRenkId",
                table: "MusteriKonfigurasyonParcalari");

            migrationBuilder.DropIndex(
                name: "IX_MusteriKonfigurasyonParcalari_UrunUcBoyutParcasiId",
                table: "MusteriKonfigurasyonParcalari");

            migrationBuilder.DropIndex(
                name: "IX_MusteriKonfigurasyonlari_FirmaId",
                table: "MusteriKonfigurasyonlari");

            migrationBuilder.DropIndex(
                name: "IX_MusteriKonfigurasyonlari_UrunId",
                table: "MusteriKonfigurasyonlari");

            migrationBuilder.DropColumn(
                name: "Aci",
                table: "MusteriKonfigurasyonParcalari");

            migrationBuilder.DropColumn(
                name: "GuncellenmeTarihi",
                table: "MusteriKonfigurasyonParcalari");

            migrationBuilder.DropColumn(
                name: "HareketDegeri",
                table: "MusteriKonfigurasyonParcalari");

            migrationBuilder.DropColumn(
                name: "SeciliDoku",
                table: "MusteriKonfigurasyonParcalari");

            migrationBuilder.DropColumn(
                name: "SilindiMi",
                table: "MusteriKonfigurasyonParcalari");

            migrationBuilder.DropColumn(
                name: "SilinmeTarihi",
                table: "MusteriKonfigurasyonParcalari");

            migrationBuilder.DropColumn(
                name: "Durum",
                table: "MusteriKonfigurasyonlari");

            migrationBuilder.DropColumn(
                name: "FirmaId",
                table: "MusteriKonfigurasyonlari");

            migrationBuilder.DropColumn(
                name: "GuncelleyenKullaniciId",
                table: "MusteriKonfigurasyonlari");

            migrationBuilder.DropColumn(
                name: "OlusturanKullaniciId",
                table: "MusteriKonfigurasyonlari");

            migrationBuilder.DropColumn(
                name: "ToplamFiyat",
                table: "MusteriKonfigurasyonlari");
        }
    }
}
