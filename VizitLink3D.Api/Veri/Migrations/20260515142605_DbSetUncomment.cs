using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.Api.Veri.Migrations
{
    /// <inheritdoc />
    public partial class DbSetUncomment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MusteriKonfigurasyonlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UrunId = table.Column<int>(type: "INTEGER", nullable: false),
                    KullaniciId = table.Column<int>(type: "INTEGER", nullable: true),
                    OturumAnahtari = table.Column<string>(type: "TEXT", nullable: true),
                    Not = table.Column<string>(type: "TEXT", nullable: true),
                    SilindiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SilinmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusteriKonfigurasyonlari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MusteriKonfigurasyonParcalari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MusteriKonfigurasyonuId = table.Column<int>(type: "INTEGER", nullable: false),
                    UrunUcBoyutParcasiId = table.Column<int>(type: "INTEGER", nullable: false),
                    SeciliRenkId = table.Column<int>(type: "INTEGER", nullable: true),
                    SeciliMalzemeId = table.Column<int>(type: "INTEGER", nullable: true),
                    SeciliKaplamaId = table.Column<int>(type: "INTEGER", nullable: true),
                    Deger = table.Column<double>(type: "REAL", nullable: true),
                    GorunurMu = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusteriKonfigurasyonParcalari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UrunUcBoyutModelleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UrunId = table.Column<int>(type: "INTEGER", nullable: false),
                    ModelAdi = table.Column<string>(type: "TEXT", nullable: false),
                    ModelDosyaYolu = table.Column<string>(type: "TEXT", nullable: false),
                    AnalizJson = table.Column<string>(type: "TEXT", nullable: true),
                    ModelTipi = table.Column<string>(type: "TEXT", nullable: true),
                    ModelYolu = table.Column<string>(type: "TEXT", nullable: true),
                    MedyaId = table.Column<long>(type: "INTEGER", nullable: false),
                    OnizlemeMedyaId = table.Column<long>(type: "INTEGER", nullable: true),
                    DosyaBoyutuByte = table.Column<long>(type: "INTEGER", nullable: false),
                    ModelAnalizJson = table.Column<string>(type: "TEXT", nullable: true),
                    KameraAyarJson = table.Column<string>(type: "TEXT", nullable: true),
                    IsikAyarJson = table.Column<string>(type: "TEXT", nullable: true),
                    CevreAyarJson = table.Column<string>(type: "TEXT", nullable: true),
                    VarsayilanMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    Versiyon = table.Column<int>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SilindiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SilinmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunUcBoyutModelleri", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MusteriKonfigurasyonlari");

            migrationBuilder.DropTable(
                name: "MusteriKonfigurasyonParcalari");

            migrationBuilder.DropTable(
                name: "UrunUcBoyutModelleri");
        }
    }
}
