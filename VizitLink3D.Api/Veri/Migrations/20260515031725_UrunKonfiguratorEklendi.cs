using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VizitLink3D.Api.Veri.Migrations
{
    /// <inheritdoc />
    public partial class UrunKonfiguratorEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Malzemeler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    Tip = table.Column<string>(type: "TEXT", nullable: false),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
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
                name: "RenkKataloglari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RenkKataloglari", x => x.Id);
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
                    Slug = table.Column<string>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
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
                    table.ForeignKey(
                        name: "FK_UrunKategorileri_UrunKategorileri_UstKategoriId",
                        column: x => x.UstKategoriId,
                        principalTable: "UrunKategorileri",
                        principalColumn: "Id");
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
                name: "KaplamaSecenekleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MalzemeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    HexKod = table.Column<string>(type: "TEXT", nullable: true),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KaplamaSecenekleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KaplamaSecenekleri_Malzemeler_MalzemeId",
                        column: x => x.MalzemeId,
                        principalTable: "Malzemeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RalRenkleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KatalogId = table.Column<int>(type: "INTEGER", nullable: false),
                    Kod = table.Column<string>(type: "TEXT", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    HexKod = table.Column<string>(type: "TEXT", nullable: false),
                    Grup = table.Column<string>(type: "TEXT", nullable: true),
                    YuzeyTipi = table.Column<string>(type: "TEXT", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RalRenkleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RalRenkleri_RenkKataloglari_KatalogId",
                        column: x => x.KatalogId,
                        principalTable: "RenkKataloglari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UrunParcaGruplari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UrunAilesiId = table.Column<int>(type: "INTEGER", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunParcaGruplari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UrunParcaGruplari_UrunAilesileri_UrunAilesiId",
                        column: x => x.UrunAilesiId,
                        principalTable: "UrunAilesileri",
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
                    AnaGorselMedyaId = table.Column<long>(type: "INTEGER", nullable: true),
                    VarsayilanUcBoyutModeliId = table.Column<int>(type: "INTEGER", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OneCikanMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    YeniMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    SeoBaslik = table.Column<string>(type: "TEXT", nullable: true),
                    SeoAciklama = table.Column<string>(type: "TEXT", nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
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
                    table.ForeignKey(
                        name: "FK_Urunler_UrunKategorileri_UrunKategoriId",
                        column: x => x.UrunKategoriId,
                        principalTable: "UrunKategorileri",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MusteriKonfigurasyonlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UrunId = table.Column<int>(type: "INTEGER", nullable: false),
                    OturumAnahtari = table.Column<string>(type: "TEXT", nullable: true),
                    KullaniciId = table.Column<int>(type: "INTEGER", nullable: true),
                    Not = table.Column<string>(type: "TEXT", nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SilindiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SilinmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusteriKonfigurasyonlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MusteriKonfigurasyonlari_Urunler_UrunId",
                        column: x => x.UrunId,
                        principalTable: "Urunler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UrunKonfigurasyonSablonlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UrunId = table.Column<int>(type: "INTEGER", nullable: false),
                    DetaySablonu = table.Column<string>(type: "TEXT", nullable: false),
                    HeroAktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    UcBoyutIlkAcilacakMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    TeknikOzellikAktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    PdfKaynakAktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    BenzerUrunlerAktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    TeklifFormuAktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    AnimasyonTipi = table.Column<string>(type: "TEXT", nullable: true),
                    RenkPaneliKonumu = table.Column<string>(type: "TEXT", nullable: true),
                    MobilPanelDavranisi = table.Column<string>(type: "TEXT", nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunKonfigurasyonSablonlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UrunKonfigurasyonSablonlari_Urunler_UrunId",
                        column: x => x.UrunId,
                        principalTable: "Urunler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UrunMedyalari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UrunId = table.Column<int>(type: "INTEGER", nullable: false),
                    MedyaId = table.Column<long>(type: "INTEGER", nullable: false),
                    MedyaTipi = table.Column<string>(type: "TEXT", nullable: false),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunMedyalari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UrunMedyalari_Urunler_UrunId",
                        column: x => x.UrunId,
                        principalTable: "Urunler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UrunUcBoyutModelleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UrunId = table.Column<int>(type: "INTEGER", nullable: false),
                    MedyaId = table.Column<long>(type: "INTEGER", nullable: false),
                    ModelYolu = table.Column<string>(type: "TEXT", nullable: false),
                    OnizlemeMedyaId = table.Column<long>(type: "INTEGER", nullable: true),
                    ModelTipi = table.Column<string>(type: "TEXT", nullable: false),
                    DosyaBoyutuByte = table.Column<long>(type: "INTEGER", nullable: false),
                    Versiyon = table.Column<int>(type: "INTEGER", nullable: false),
                    VarsayilanMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    KameraAyarJson = table.Column<string>(type: "TEXT", nullable: true),
                    IsikAyarJson = table.Column<string>(type: "TEXT", nullable: true),
                    CevreAyarJson = table.Column<string>(type: "TEXT", nullable: true),
                    ModelAnalizJson = table.Column<string>(type: "TEXT", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SilindiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SilinmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunUcBoyutModelleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UrunUcBoyutModelleri_Urunler_UrunId",
                        column: x => x.UrunId,
                        principalTable: "Urunler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    SeoAciklama = table.Column<string>(type: "TEXT", nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunYerellestirmeleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UrunYerellestirmeleri_Urunler_UrunId",
                        column: x => x.UrunId,
                        principalTable: "Urunler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeklifIstekleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UrunId = table.Column<int>(type: "INTEGER", nullable: true),
                    MusteriKonfigurasyonuId = table.Column<int>(type: "INTEGER", nullable: true),
                    EkranGoruntusuMedyaId = table.Column<string>(type: "TEXT", nullable: true),
                    MusteriAdSoyad = table.Column<string>(type: "TEXT", nullable: true),
                    Telefon = table.Column<string>(type: "TEXT", nullable: true),
                    Eposta = table.Column<string>(type: "TEXT", nullable: true),
                    Not = table.Column<string>(type: "TEXT", nullable: true),
                    Durum = table.Column<string>(type: "TEXT", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SilindiMi = table.Column<bool>(type: "INTEGER", nullable: false),
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
                    table.ForeignKey(
                        name: "FK_TeklifIstekleri_Urunler_UrunId",
                        column: x => x.UrunId,
                        principalTable: "Urunler",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UrunUcBoyutParcalari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UrunUcBoyutModeliId = table.Column<int>(type: "INTEGER", nullable: false),
                    MeshAdi = table.Column<string>(type: "TEXT", nullable: false),
                    GorunenAd = table.Column<string>(type: "TEXT", nullable: false),
                    ParcaGrubuId = table.Column<int>(type: "INTEGER", nullable: true),
                    SecilebilirMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    RenklenebilirMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    MalzemeDegisebilirMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    GizlenebilirMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    HareketliMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    HareketTipi = table.Column<string>(type: "TEXT", nullable: false),
                    MinDeger = table.Column<double>(type: "REAL", nullable: true),
                    MaxDeger = table.Column<double>(type: "REAL", nullable: true),
                    VarsayilanDeger = table.Column<double>(type: "REAL", nullable: true),
                    VarsayilanRenkId = table.Column<int>(type: "INTEGER", nullable: true),
                    VarsayilanMalzemeId = table.Column<int>(type: "INTEGER", nullable: true),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunUcBoyutParcalari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UrunUcBoyutParcalari_UrunParcaGruplari_ParcaGrubuId",
                        column: x => x.ParcaGrubuId,
                        principalTable: "UrunParcaGruplari",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UrunUcBoyutParcalari_UrunUcBoyutModelleri_UrunUcBoyutModeliId",
                        column: x => x.UrunUcBoyutModeliId,
                        principalTable: "UrunUcBoyutModelleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeklifIstegiParcalari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TeklifIstegiId = table.Column<int>(type: "INTEGER", nullable: false),
                    ParcaAdi = table.Column<string>(type: "TEXT", nullable: false),
                    RenkKodu = table.Column<string>(type: "TEXT", nullable: true),
                    RenkAdi = table.Column<string>(type: "TEXT", nullable: true),
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
                    table.ForeignKey(
                        name: "FK_MusteriKonfigurasyonParcalari_MusteriKonfigurasyonlari_MusteriKonfigurasyonuId",
                        column: x => x.MusteriKonfigurasyonuId,
                        principalTable: "MusteriKonfigurasyonlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MusteriKonfigurasyonParcalari_UrunUcBoyutParcalari_UrunUcBoyutParcasiId",
                        column: x => x.UrunUcBoyutParcasiId,
                        principalTable: "UrunUcBoyutParcalari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UrunKonfigurasyonKurallari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UrunId = table.Column<int>(type: "INTEGER", nullable: false),
                    Parca1Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Parca1RenkId = table.Column<int>(type: "INTEGER", nullable: true),
                    Parca1MalzemeId = table.Column<int>(type: "INTEGER", nullable: true),
                    Parca2Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Parca2RenkId = table.Column<int>(type: "INTEGER", nullable: true),
                    Parca2MalzemeId = table.Column<int>(type: "INTEGER", nullable: true),
                    KuralTipi = table.Column<string>(type: "TEXT", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunKonfigurasyonKurallari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UrunKonfigurasyonKurallari_UrunUcBoyutParcalari_Parca1Id",
                        column: x => x.Parca1Id,
                        principalTable: "UrunUcBoyutParcalari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UrunKonfigurasyonKurallari_UrunUcBoyutParcalari_Parca2Id",
                        column: x => x.Parca2Id,
                        principalTable: "UrunUcBoyutParcalari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UrunKonfigurasyonKurallari_Urunler_UrunId",
                        column: x => x.UrunId,
                        principalTable: "Urunler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UrunParcaEslemeleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UrunUcBoyutParcasiId = table.Column<int>(type: "INTEGER", nullable: false),
                    ParcaGrubuId = table.Column<int>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunParcaEslemeleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UrunParcaEslemeleri_UrunParcaGruplari_ParcaGrubuId",
                        column: x => x.ParcaGrubuId,
                        principalTable: "UrunParcaGruplari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UrunParcaEslemeleri_UrunUcBoyutParcalari_UrunUcBoyutParcasiId",
                        column: x => x.UrunUcBoyutParcasiId,
                        principalTable: "UrunUcBoyutParcalari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UrunParcaMalzemeSecenekleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UrunUcBoyutParcasiId = table.Column<int>(type: "INTEGER", nullable: false),
                    MalzemeId = table.Column<int>(type: "INTEGER", nullable: false),
                    KaplamaSecenegiId = table.Column<int>(type: "INTEGER", nullable: true),
                    VarsayilanMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunParcaMalzemeSecenekleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UrunParcaMalzemeSecenekleri_KaplamaSecenekleri_KaplamaSecenegiId",
                        column: x => x.KaplamaSecenegiId,
                        principalTable: "KaplamaSecenekleri",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UrunParcaMalzemeSecenekleri_Malzemeler_MalzemeId",
                        column: x => x.MalzemeId,
                        principalTable: "Malzemeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UrunParcaMalzemeSecenekleri_UrunUcBoyutParcalari_UrunUcBoyutParcasiId",
                        column: x => x.UrunUcBoyutParcasiId,
                        principalTable: "UrunUcBoyutParcalari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UrunParcaRenkSecenekleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UrunUcBoyutParcasiId = table.Column<int>(type: "INTEGER", nullable: false),
                    RalRengiId = table.Column<int>(type: "INTEGER", nullable: false),
                    VarsayilanMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunParcaRenkSecenekleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UrunParcaRenkSecenekleri_RalRenkleri_RalRengiId",
                        column: x => x.RalRengiId,
                        principalTable: "RalRenkleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UrunParcaRenkSecenekleri_UrunUcBoyutParcalari_UrunUcBoyutParcasiId",
                        column: x => x.UrunUcBoyutParcasiId,
                        principalTable: "UrunUcBoyutParcalari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "UrunAilesileri",
                columns: new[] { "Id", "Aciklama", "Ad", "AktifMi", "GuncellenmeTarihi", "OlusturulmaTarihi", "SilindiMi", "SilinmeTarihi", "SiraNo", "Slug", "VarsayilanDetaySablonu" },
                values: new object[,]
                {
                    { 1, "Modern duþakabin sistemleri", "Duþakabin", true, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, null, 1, "dusakabin", "DusakabinKonfigurator" },
                    { 2, "Banyo dolabý modelleri", "Banyo Dolabý", true, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, null, 2, "banyo-dolabi", "BanyoKonfigurator" },
                    { 3, "Vestiyer sistemleri", "Vestiyer", true, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, null, 3, "vestiyer", "Endustriyel3D" },
                    { 4, "Ýç ve dýþ kapý modelleri", "Kapý", true, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, null, 4, "kapi", "KapiKonfigurator" },
                    { 5, "Mobilya kapak sistemleri", "Dolap Kapaðý", true, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, null, 5, "dolap-kapagi", "KapakKonfigurator" }
                });

            migrationBuilder.InsertData(
                table: "Urunler",
                columns: new[] { "Id", "Aciklama", "Ad", "AktifMi", "AnaGorselMedyaId", "GuncellenmeTarihi", "KisaAciklama", "Kod", "OlusturulmaTarihi", "OneCikanMi", "SeoAciklama", "SeoBaslik", "SilindiMi", "SilinmeTarihi", "SiraNo", "Slug", "UrunAilesiId", "UrunKategoriId", "VarsayilanUcBoyutModeliId", "YeniMi" },
                values: new object[,]
                {
                    { 1, null, "Luna Duþakabin", true, null, null, "Çerçevesiz temperli cam, krom profil", "DSK-001", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, null, null, false, null, 1, "dusakabin-luna", 1, null, null, true },
                    { 2, null, "Aria Banyo Dolabý", true, null, null, "LED aydýnlatmalý, yumuþak kapanýr kapak", "BD-001", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, null, null, false, null, 1, "banyo-dolabi-aria", 2, null, null, true },
                    { 3, null, "Imperial Kapý", true, null, null, "Masif ahþap görünümlü, bronz kulp", "KPI-001", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, null, null, false, null, 1, "kapi-imperial", 4, null, null, false }
                });

            migrationBuilder.CreateIndex(
                name: "IX_KaplamaSecenekleri_MalzemeId",
                table: "KaplamaSecenekleri",
                column: "MalzemeId");

            migrationBuilder.CreateIndex(
                name: "IX_MusteriKonfigurasyonlari_UrunId",
                table: "MusteriKonfigurasyonlari",
                column: "UrunId");

            migrationBuilder.CreateIndex(
                name: "IX_MusteriKonfigurasyonParcalari_MusteriKonfigurasyonuId",
                table: "MusteriKonfigurasyonParcalari",
                column: "MusteriKonfigurasyonuId");

            migrationBuilder.CreateIndex(
                name: "IX_MusteriKonfigurasyonParcalari_UrunUcBoyutParcasiId",
                table: "MusteriKonfigurasyonParcalari",
                column: "UrunUcBoyutParcasiId");

            migrationBuilder.CreateIndex(
                name: "IX_RalRenkleri_KatalogId",
                table: "RalRenkleri",
                column: "KatalogId");

            migrationBuilder.CreateIndex(
                name: "IX_TeklifIstegiParcalari_TeklifIstegiId",
                table: "TeklifIstegiParcalari",
                column: "TeklifIstegiId");

            migrationBuilder.CreateIndex(
                name: "IX_TeklifIstekleri_MusteriKonfigurasyonuId",
                table: "TeklifIstekleri",
                column: "MusteriKonfigurasyonuId");

            migrationBuilder.CreateIndex(
                name: "IX_TeklifIstekleri_UrunId",
                table: "TeklifIstekleri",
                column: "UrunId");

            migrationBuilder.CreateIndex(
                name: "IX_UrunKategorileri_UstKategoriId",
                table: "UrunKategorileri",
                column: "UstKategoriId");

            migrationBuilder.CreateIndex(
                name: "IX_UrunKonfigurasyonKurallari_Parca1Id",
                table: "UrunKonfigurasyonKurallari",
                column: "Parca1Id");

            migrationBuilder.CreateIndex(
                name: "IX_UrunKonfigurasyonKurallari_Parca2Id",
                table: "UrunKonfigurasyonKurallari",
                column: "Parca2Id");

            migrationBuilder.CreateIndex(
                name: "IX_UrunKonfigurasyonKurallari_UrunId",
                table: "UrunKonfigurasyonKurallari",
                column: "UrunId");

            migrationBuilder.CreateIndex(
                name: "IX_UrunKonfigurasyonSablonlari_UrunId",
                table: "UrunKonfigurasyonSablonlari",
                column: "UrunId");

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
                name: "IX_Urunler_UrunKategoriId",
                table: "Urunler",
                column: "UrunKategoriId");

            migrationBuilder.CreateIndex(
                name: "IX_UrunMedyalari_UrunId",
                table: "UrunMedyalari",
                column: "UrunId");

            migrationBuilder.CreateIndex(
                name: "IX_UrunParcaEslemeleri_ParcaGrubuId",
                table: "UrunParcaEslemeleri",
                column: "ParcaGrubuId");

            migrationBuilder.CreateIndex(
                name: "IX_UrunParcaEslemeleri_UrunUcBoyutParcasiId",
                table: "UrunParcaEslemeleri",
                column: "UrunUcBoyutParcasiId");

            migrationBuilder.CreateIndex(
                name: "IX_UrunParcaGruplari_UrunAilesiId",
                table: "UrunParcaGruplari",
                column: "UrunAilesiId");

            migrationBuilder.CreateIndex(
                name: "IX_UrunParcaMalzemeSecenekleri_KaplamaSecenegiId",
                table: "UrunParcaMalzemeSecenekleri",
                column: "KaplamaSecenegiId");

            migrationBuilder.CreateIndex(
                name: "IX_UrunParcaMalzemeSecenekleri_MalzemeId",
                table: "UrunParcaMalzemeSecenekleri",
                column: "MalzemeId");

            migrationBuilder.CreateIndex(
                name: "IX_UrunParcaMalzemeSecenekleri_UrunUcBoyutParcasiId",
                table: "UrunParcaMalzemeSecenekleri",
                column: "UrunUcBoyutParcasiId");

            migrationBuilder.CreateIndex(
                name: "IX_UrunParcaRenkSecenekleri_RalRengiId",
                table: "UrunParcaRenkSecenekleri",
                column: "RalRengiId");

            migrationBuilder.CreateIndex(
                name: "IX_UrunParcaRenkSecenekleri_UrunUcBoyutParcasiId",
                table: "UrunParcaRenkSecenekleri",
                column: "UrunUcBoyutParcasiId");

            migrationBuilder.CreateIndex(
                name: "IX_UrunUcBoyutModelleri_UrunId",
                table: "UrunUcBoyutModelleri",
                column: "UrunId");

            migrationBuilder.CreateIndex(
                name: "IX_UrunUcBoyutParcalari_ParcaGrubuId",
                table: "UrunUcBoyutParcalari",
                column: "ParcaGrubuId");

            migrationBuilder.CreateIndex(
                name: "IX_UrunUcBoyutParcalari_UrunUcBoyutModeliId",
                table: "UrunUcBoyutParcalari",
                column: "UrunUcBoyutModeliId");

            migrationBuilder.CreateIndex(
                name: "IX_UrunYerellestirmeleri_UrunId",
                table: "UrunYerellestirmeleri",
                column: "UrunId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MusteriKonfigurasyonParcalari");

            migrationBuilder.DropTable(
                name: "PdfSayfaGorselleri");

            migrationBuilder.DropTable(
                name: "TeklifIstegiParcalari");

            migrationBuilder.DropTable(
                name: "UrunKonfigurasyonKurallari");

            migrationBuilder.DropTable(
                name: "UrunKonfigurasyonSablonlari");

            migrationBuilder.DropTable(
                name: "UrunMedyalari");

            migrationBuilder.DropTable(
                name: "UrunParcaEslemeleri");

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
                name: "KaplamaSecenekleri");

            migrationBuilder.DropTable(
                name: "RalRenkleri");

            migrationBuilder.DropTable(
                name: "UrunUcBoyutParcalari");

            migrationBuilder.DropTable(
                name: "MusteriKonfigurasyonlari");

            migrationBuilder.DropTable(
                name: "Malzemeler");

            migrationBuilder.DropTable(
                name: "RenkKataloglari");

            migrationBuilder.DropTable(
                name: "UrunParcaGruplari");

            migrationBuilder.DropTable(
                name: "UrunUcBoyutModelleri");

            migrationBuilder.DropTable(
                name: "Urunler");

            migrationBuilder.DropTable(
                name: "UrunAilesileri");

            migrationBuilder.DropTable(
                name: "UrunKategorileri");
        }
    }
}
