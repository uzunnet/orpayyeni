using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.Api.Veri.Migrations
{
    /// <inheritdoc />
    public partial class UrunOmurgasiEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KaplamaSecenekleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MalzemeId = table.Column<int>(type: "INTEGER", nullable: true),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    HexKod = table.Column<string>(type: "TEXT", nullable: true),
                    ResimUrl = table.Column<string>(type: "TEXT", nullable: true),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SilindiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SilinmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KaplamaSecenekleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Malzemeler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    Tip = table.Column<string>(type: "TEXT", nullable: false),
                    ResimUrl = table.Column<string>(type: "TEXT", nullable: true),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SilindiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SilinmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Malzemeler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PdfSayfaGorselleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PdfKaynagiId = table.Column<int>(type: "INTEGER", nullable: false),
                    SayfaNo = table.Column<int>(type: "INTEGER", nullable: false),
                    MedyaId = table.Column<long>(type: "INTEGER", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    UrunId = table.Column<int>(type: "INTEGER", nullable: true),
                    UruneBaglandiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PdfSayfaGorselleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RalRenkleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Kod = table.Column<string>(type: "TEXT", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    HexKod = table.Column<string>(type: "TEXT", nullable: true),
                    Grup = table.Column<string>(type: "TEXT", nullable: true),
                    KatalogId = table.Column<int>(type: "INTEGER", nullable: false),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    YuzeyTipi = table.Column<string>(type: "TEXT", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RalRenkleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RenkKataloglari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RenkKataloglari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeklifIstekleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UrunId = table.Column<int>(type: "INTEGER", nullable: true),
                    MusteriKonfigurasyonuId = table.Column<int>(type: "INTEGER", nullable: true),
                    MusteriAdSoyad = table.Column<string>(type: "TEXT", nullable: true),
                    Eposta = table.Column<string>(type: "TEXT", nullable: true),
                    Telefon = table.Column<string>(type: "TEXT", nullable: true),
                    Not = table.Column<string>(type: "TEXT", nullable: true),
                    EkranGoruntusuMedyaId = table.Column<string>(type: "TEXT", nullable: true),
                    Durum = table.Column<string>(type: "TEXT", nullable: false),
                    SilindiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SilinmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeklifIstekleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeklifIstekleri_MusteriKonfigurasyonlari_MusteriKonfigurasyonuId",
                        column: x => x.MusteriKonfigurasyonuId,
                        principalTable: "MusteriKonfigurasyonlari",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UrunAilesileri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    VarsayilanDetaySablonu = table.Column<string>(type: "TEXT", nullable: true),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SilindiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SilinmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunAilesileri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UrunKategorileri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    Slug = table.Column<string>(type: "TEXT", nullable: false),
                    UstKategoriId = table.Column<int>(type: "INTEGER", nullable: true),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SilindiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SilinmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunKategorileri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UrunKonfigurasyonKurallari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UrunId = table.Column<int>(type: "INTEGER", nullable: false),
                    KuralTipi = table.Column<string>(type: "TEXT", nullable: false),
                    Parca1Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Parca2Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Parca1RenkId = table.Column<int>(type: "INTEGER", nullable: true),
                    Parca2RenkId = table.Column<int>(type: "INTEGER", nullable: true),
                    Parca1MalzemeId = table.Column<int>(type: "INTEGER", nullable: true),
                    Parca2MalzemeId = table.Column<int>(type: "INTEGER", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunKonfigurasyonKurallari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UrunKonfigurasyonSablonlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UrunId = table.Column<int>(type: "INTEGER", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    DetaySablonu = table.Column<string>(type: "TEXT", nullable: false),
                    HeroAktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    TeknikOzellikAktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    PdfKaynakAktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    BenzerUrunlerAktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    TeklifFormuAktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    UcBoyutIlkAcilacakMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    AnimasyonTipi = table.Column<string>(type: "TEXT", nullable: true),
                    MobilPanelDavranisi = table.Column<string>(type: "TEXT", nullable: true),
                    RenkPaneliKonumu = table.Column<string>(type: "TEXT", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunKonfigurasyonSablonlari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UrunMedyalari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UrunId = table.Column<int>(type: "INTEGER", nullable: false),
                    MedyaUrl = table.Column<string>(type: "TEXT", nullable: false),
                    MedyaTuru = table.Column<string>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    AnaGosterim = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunMedyalari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UrunParcaGruplari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UrunId = table.Column<int>(type: "INTEGER", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunParcaGruplari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UrunParcaMalzemeSecenekleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UrunUcBoyutParcasiId = table.Column<int>(type: "INTEGER", nullable: false),
                    MalzemeId = table.Column<int>(type: "INTEGER", nullable: true),
                    KaplamaSecenegiId = table.Column<int>(type: "INTEGER", nullable: true),
                    EkAciklama = table.Column<string>(type: "TEXT", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunParcaMalzemeSecenekleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UrunParcaRenkSecenekleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UrunUcBoyutParcasiId = table.Column<int>(type: "INTEGER", nullable: false),
                    RalRengiId = table.Column<int>(type: "INTEGER", nullable: true),
                    RenkKataloguId = table.Column<int>(type: "INTEGER", nullable: true),
                    EkAciklama = table.Column<string>(type: "TEXT", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunParcaRenkSecenekleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UrunPdfKaynaklari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    MedyaId = table.Column<long>(type: "INTEGER", nullable: false),
                    SayfaSayisi = table.Column<int>(type: "INTEGER", nullable: true),
                    CozumlemeDurumu = table.Column<string>(type: "TEXT", nullable: true),
                    HataMesaji = table.Column<string>(type: "TEXT", nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CozumlemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunPdfKaynaklari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UrunUcBoyutParcalari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UrunUcBoyutModeliId = table.Column<int>(type: "INTEGER", nullable: false),
                    ParcaGrubuId = table.Column<int>(type: "INTEGER", nullable: true),
                    GorunenAd = table.Column<string>(type: "TEXT", nullable: false),
                    MeshAdi = table.Column<string>(type: "TEXT", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SecilebilirMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    RenklenebilirMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    MalzemeDegisebilirMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    HareketliMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    GizlenebilirMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    HareketTipi = table.Column<string>(type: "TEXT", nullable: false),
                    MinDeger = table.Column<double>(type: "REAL", nullable: true),
                    MaxDeger = table.Column<double>(type: "REAL", nullable: true),
                    VarsayilanDeger = table.Column<double>(type: "REAL", nullable: true),
                    VarsayilanRenkId = table.Column<int>(type: "INTEGER", nullable: true),
                    VarsayilanMalzemeId = table.Column<int>(type: "INTEGER", nullable: true),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunUcBoyutParcalari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UrunYerellestirmeleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UrunId = table.Column<int>(type: "INTEGER", nullable: false),
                    Dil = table.Column<string>(type: "TEXT", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", nullable: true),
                    KisaAciklama = table.Column<string>(type: "TEXT", nullable: true),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    SeoBaslik = table.Column<string>(type: "TEXT", nullable: true),
                    SeoAciklama = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunYerellestirmeleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeklifIstegiParcalari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TeklifIstegiId = table.Column<int>(type: "INTEGER", nullable: false),
                    ParcaAdi = table.Column<string>(type: "TEXT", nullable: false),
                    RenkAdi = table.Column<string>(type: "TEXT", nullable: true),
                    RenkKodu = table.Column<string>(type: "TEXT", nullable: true),
                    MalzemeAdi = table.Column<string>(type: "TEXT", nullable: true),
                    Olcu = table.Column<string>(type: "TEXT", nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeklifIstegiParcalari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeklifIstegiParcalari_TeklifIstekleri_TeklifIstegiId",
                        column: x => x.TeklifIstegiId,
                        principalTable: "TeklifIstekleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Urunler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Slug = table.Column<string>(type: "TEXT", nullable: false),
                    Kod = table.Column<string>(type: "TEXT", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    KisaAciklama = table.Column<string>(type: "TEXT", nullable: true),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    UrunAilesiId = table.Column<int>(type: "INTEGER", nullable: false),
                    UrunKategoriId = table.Column<int>(type: "INTEGER", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OneCikanMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    YeniMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    Fiyat = table.Column<decimal>(type: "TEXT", nullable: true),
                    Birim = table.Column<string>(type: "TEXT", nullable: true),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AnaGorselMedyaId = table.Column<long>(type: "INTEGER", nullable: true),
                    VarsayilanUcBoyutModeliId = table.Column<int>(type: "INTEGER", nullable: true),
                    SeoBaslik = table.Column<string>(type: "TEXT", nullable: true),
                    SeoAciklama = table.Column<string>(type: "TEXT", nullable: true),
                    SilindiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SilinmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Urunler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Urunler_UrunAilesileri_UrunAilesiId",
                        column: x => x.UrunAilesiId,
                        principalTable: "UrunAilesileri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UrunParcaEslemeleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UrunUcBoyutModeliId = table.Column<int>(type: "INTEGER", nullable: false),
                    UrunUcBoyutParcasiId = table.Column<int>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunParcaEslemeleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UrunParcaEslemeleri_UrunUcBoyutModelleri_UrunUcBoyutModeliId",
                        column: x => x.UrunUcBoyutModeliId,
                        principalTable: "UrunUcBoyutModelleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UrunParcaEslemeleri_UrunUcBoyutParcalari_UrunUcBoyutParcasiId",
                        column: x => x.UrunUcBoyutParcasiId,
                        principalTable: "UrunUcBoyutParcalari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeklifIstegiParcalari_TeklifIstegiId",
                table: "TeklifIstegiParcalari",
                column: "TeklifIstegiId");

            migrationBuilder.CreateIndex(
                name: "IX_TeklifIstekleri_MusteriKonfigurasyonuId",
                table: "TeklifIstekleri",
                column: "MusteriKonfigurasyonuId");

            migrationBuilder.CreateIndex(
                name: "IX_Urunler_Slug",
                table: "Urunler",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Urunler_UrunAilesiId",
                table: "Urunler",
                column: "UrunAilesiId");

            migrationBuilder.CreateIndex(
                name: "IX_UrunParcaEslemeleri_UrunUcBoyutModeliId",
                table: "UrunParcaEslemeleri",
                column: "UrunUcBoyutModeliId");

            migrationBuilder.CreateIndex(
                name: "IX_UrunParcaEslemeleri_UrunUcBoyutParcasiId",
                table: "UrunParcaEslemeleri",
                column: "UrunUcBoyutParcasiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KaplamaSecenekleri");

            migrationBuilder.DropTable(
                name: "Malzemeler");

            migrationBuilder.DropTable(
                name: "PdfSayfaGorselleri");

            migrationBuilder.DropTable(
                name: "RalRenkleri");

            migrationBuilder.DropTable(
                name: "RenkKataloglari");

            migrationBuilder.DropTable(
                name: "TeklifIstegiParcalari");

            migrationBuilder.DropTable(
                name: "UrunKategorileri");

            migrationBuilder.DropTable(
                name: "UrunKonfigurasyonKurallari");

            migrationBuilder.DropTable(
                name: "UrunKonfigurasyonSablonlari");

            migrationBuilder.DropTable(
                name: "Urunler");

            migrationBuilder.DropTable(
                name: "UrunMedyalari");

            migrationBuilder.DropTable(
                name: "UrunParcaEslemeleri");

            migrationBuilder.DropTable(
                name: "UrunParcaGruplari");

            migrationBuilder.DropTable(
                name: "UrunParcaMalzemeSecenekleri");

            migrationBuilder.DropTable(
                name: "UrunParcaRenkSecenekleri");

            migrationBuilder.DropTable(
                name: "UrunPdfKaynaklari");

            migrationBuilder.DropTable(
                name: "UrunYerellestirmeleri");

            migrationBuilder.DropTable(
                name: "TeklifIstekleri");

            migrationBuilder.DropTable(
                name: "UrunAilesileri");

            migrationBuilder.DropTable(
                name: "UrunUcBoyutParcalari");
        }
    }
}
