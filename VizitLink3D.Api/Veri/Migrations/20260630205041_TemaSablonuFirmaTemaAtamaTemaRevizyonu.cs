using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.Api.Veri.Migrations
{
    /// <inheritdoc />
    public partial class TemaSablonuFirmaTemaAtamaTemaRevizyonu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FirmaTemaAtamalari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirmaId = table.Column<int>(type: "INTEGER", nullable: false),
                    TemaSablonuId = table.Column<int>(type: "INTEGER", nullable: false),
                    Tur = table.Column<string>(type: "TEXT", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OzelDegiskenlerJson = table.Column<string>(type: "TEXT", nullable: true),
                    AtamaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FirmaTemaAtamalari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TemaRevizyonlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TemaSablonuId = table.Column<int>(type: "INTEGER", nullable: false),
                    Versiyon = table.Column<int>(type: "INTEGER", nullable: false),
                    KaynakTipi = table.Column<string>(type: "TEXT", nullable: false),
                    HamDesignMd = table.Column<string>(type: "TEXT", nullable: true),
                    UretilenManifestJson = table.Column<string>(type: "TEXT", nullable: false),
                    Notlar = table.Column<string>(type: "TEXT", nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemaRevizyonlari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TemaSablonlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Kod = table.Column<string>(type: "TEXT", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: false),
                    Kaynak = table.Column<string>(type: "TEXT", nullable: false),
                    StitchProjeId = table.Column<string>(type: "TEXT", nullable: true),
                    GlassmorphismAktif = table.Column<bool>(type: "INTEGER", nullable: false),
                    Premium = table.Column<bool>(type: "INTEGER", nullable: false),
                    Fiyat = table.Column<decimal>(type: "TEXT", nullable: false),
                    ParaBirimi = table.Column<string>(type: "TEXT", nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "TEXT", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    VarsayilanMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    Etiketler = table.Column<string>(type: "TEXT", nullable: false),
                    Versiyon = table.Column<int>(type: "INTEGER", nullable: false),
                    SilindiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    RenklerJson = table.Column<string>(type: "TEXT", nullable: false),
                    TipografiJson = table.Column<string>(type: "TEXT", nullable: false),
                    GeometriJson = table.Column<string>(type: "TEXT", nullable: false),
                    GolgelerJson = table.Column<string>(type: "TEXT", nullable: false),
                    GlassmorphismJson = table.Column<string>(type: "TEXT", nullable: false),
                    AnimasyonJson = table.Column<string>(type: "TEXT", nullable: false),
                    LayoutJson = table.Column<string>(type: "TEXT", nullable: false),
                    IkonSeti = table.Column<string>(type: "TEXT", nullable: false),
                    AdAnahtar = table.Column<string>(type: "TEXT", nullable: false),
                    AciklamaAnahtar = table.Column<string>(type: "TEXT", nullable: false),
                    AdVarsayilanTr = table.Column<string>(type: "TEXT", nullable: false),
                    AdVarsayilanEn = table.Column<string>(type: "TEXT", nullable: false),
                    AciklamaVarsayilanTr = table.Column<string>(type: "TEXT", nullable: false),
                    AciklamaVarsayilanEn = table.Column<string>(type: "TEXT", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemaSablonlari", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FirmaTemaAtamalari");

            migrationBuilder.DropTable(
                name: "TemaRevizyonlari");

            migrationBuilder.DropTable(
                name: "TemaSablonlari");
        }
    }
}
