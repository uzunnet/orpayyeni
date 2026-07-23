using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.Api.Veri.Migrations
{
    /// <inheritdoc />
    public partial class IlkKurulum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLoglar",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ZamanDamgasi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CorrelationId = table.Column<string>(type: "TEXT", nullable: true),
                    KullaniciId = table.Column<string>(type: "TEXT", nullable: true),
                    FirmaId = table.Column<string>(type: "TEXT", nullable: true),
                    Eylem = table.Column<string>(type: "TEXT", nullable: false),
                    EskiDeger = table.Column<string>(type: "TEXT", nullable: true),
                    YeniDeger = table.Column<string>(type: "TEXT", nullable: true),
                    IPAdresi = table.Column<string>(type: "TEXT", nullable: true),
                    Tarayici = table.Column<string>(type: "TEXT", nullable: true),
                    ImzaHash = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLoglar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BultenAboneleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Eposta = table.Column<string>(type: "TEXT", nullable: false),
                    AdSoyad = table.Column<string>(type: "TEXT", nullable: true),
                    AbonelikTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IptalTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    DogrulamaToken = table.Column<string>(type: "TEXT", nullable: true),
                    DogrulandiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    KaynakSayfa = table.Column<string>(type: "TEXT", nullable: true),
                    IPAdresi = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BultenAboneleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CanliSohbetMesajlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OturumId = table.Column<string>(type: "TEXT", nullable: false),
                    GonderenAd = table.Column<string>(type: "TEXT", nullable: false),
                    MesajMetni = table.Column<string>(type: "TEXT", nullable: false),
                    YoneticiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    Tarih = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OkunduMu = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CanliSohbetMesajlari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ceviriler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Anahtar = table.Column<string>(type: "TEXT", nullable: false),
                    Dil = table.Column<string>(type: "TEXT", nullable: false),
                    Deger = table.Column<string>(type: "TEXT", nullable: false),
                    Bolum = table.Column<string>(type: "TEXT", nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ceviriler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Diller",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Kod = table.Column<string>(type: "TEXT", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    Bayrak = table.Column<string>(type: "TEXT", nullable: true),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    VarsayilanMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Diller", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EkipUyeleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AdSoyad = table.Column<string>(type: "TEXT", nullable: false),
                    Unvan = table.Column<string>(type: "TEXT", nullable: true),
                    Bio = table.Column<string>(type: "TEXT", nullable: true),
                    Resim = table.Column<string>(type: "TEXT", nullable: true),
                    Linkedin = table.Column<string>(type: "TEXT", nullable: true),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EkipUyeleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EpostaSablonlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    Konu = table.Column<string>(type: "TEXT", nullable: false),
                    IcerikHtml = table.Column<string>(type: "TEXT", nullable: false),
                    Tip = table.Column<string>(type: "TEXT", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EpostaSablonlari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Firmalar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    Unvan = table.Column<string>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", nullable: false),
                    AciklamaKisa = table.Column<string>(type: "TEXT", nullable: true),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    Domain = table.Column<string>(type: "TEXT", nullable: true),
                    YedekDomain = table.Column<string>(type: "TEXT", nullable: true),
                    Logo = table.Column<string>(type: "TEXT", nullable: true),
                    Favicon = table.Column<string>(type: "TEXT", nullable: true),
                    Eposta = table.Column<string>(type: "TEXT", nullable: true),
                    Telefon1 = table.Column<string>(type: "TEXT", nullable: true),
                    Telefon2 = table.Column<string>(type: "TEXT", nullable: true),
                    Whatsapp = table.Column<string>(type: "TEXT", nullable: true),
                    Adres = table.Column<string>(type: "TEXT", nullable: true),
                    Sehir = table.Column<string>(type: "TEXT", nullable: true),
                    Ilce = table.Column<string>(type: "TEXT", nullable: true),
                    PostaKodu = table.Column<string>(type: "TEXT", nullable: true),
                    Ulke = table.Column<string>(type: "TEXT", nullable: true),
                    Enlem = table.Column<double>(type: "REAL", nullable: true),
                    Boylam = table.Column<double>(type: "REAL", nullable: true),
                    CalismaSaatleri = table.Column<string>(type: "TEXT", nullable: true),
                    KurulusYili = table.Column<int>(type: "INTEGER", nullable: true),
                    Twitter = table.Column<string>(type: "TEXT", nullable: true),
                    Facebook = table.Column<string>(type: "TEXT", nullable: true),
                    Instagram = table.Column<string>(type: "TEXT", nullable: true),
                    YoutubeKanal = table.Column<string>(type: "TEXT", nullable: true),
                    Pinterest = table.Column<string>(type: "TEXT", nullable: true),
                    LinkedIn = table.Column<string>(type: "TEXT", nullable: true),
                    TiktokKanal = table.Column<string>(type: "TEXT", nullable: true),
                    TasarimRengi1 = table.Column<string>(type: "TEXT", nullable: true),
                    TasarimRengi2 = table.Column<string>(type: "TEXT", nullable: true),
                    TasarimRengi3 = table.Column<string>(type: "TEXT", nullable: true),
                    MenuYatayAralik = table.Column<int>(type: "INTEGER", nullable: false),
                    MenuDikeyPadding = table.Column<int>(type: "INTEGER", nullable: false),
                    LogoMaxYukseklik = table.Column<int>(type: "INTEGER", nullable: false),
                    YetkiliAdSoyad = table.Column<string>(type: "TEXT", nullable: true),
                    VergiNo = table.Column<string>(type: "TEXT", nullable: true),
                    VergiDairesi = table.Column<string>(type: "TEXT", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    DemoMu = table.Column<bool>(type: "INTEGER", nullable: false),
                    AktifSablonId = table.Column<int>(type: "INTEGER", nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Firmalar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GaleriGorselleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    Baslik = table.Column<string>(type: "TEXT", nullable: true),
                    AltMetin = table.Column<string>(type: "TEXT", nullable: true),
                    Sira = table.Column<int>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GaleriGorselleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HizmetAdimlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Baslik = table.Column<string>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    Ikon = table.Column<string>(type: "TEXT", nullable: true),
                    AdimNo = table.Column<int>(type: "INTEGER", nullable: false),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HizmetAdimlari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KapiKategorileri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Slug = table.Column<string>(type: "TEXT", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    KapakResim = table.Column<string>(type: "TEXT", nullable: true),
                    Ikon = table.Column<string>(type: "TEXT", nullable: true),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SeoBaslik = table.Column<string>(type: "TEXT", nullable: true),
                    SeoAciklama = table.Column<string>(type: "TEXT", nullable: true),
                    SeoAnahtarKelimeler = table.Column<string>(type: "TEXT", nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KapiKategorileri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Kataloglar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Baslik = table.Column<string>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    KapakResim = table.Column<string>(type: "TEXT", nullable: true),
                    PdfDosyaYolu = table.Column<string>(type: "TEXT", nullable: false),
                    DosyaBoyutuMb = table.Column<double>(type: "REAL", nullable: true),
                    SayfaSayisi = table.Column<int>(type: "INTEGER", nullable: true),
                    Yil = table.Column<int>(type: "INTEGER", nullable: true),
                    IndirilmeSayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kataloglar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MobilyaKategorileri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Slug = table.Column<string>(type: "TEXT", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    KapakResim = table.Column<string>(type: "TEXT", nullable: true),
                    Ikon = table.Column<string>(type: "TEXT", nullable: true),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SeoBaslik = table.Column<string>(type: "TEXT", nullable: true),
                    SeoAciklama = table.Column<string>(type: "TEXT", nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobilyaKategorileri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MusteriYorumlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MusteriAdi = table.Column<string>(type: "TEXT", nullable: false),
                    MusteriUnvan = table.Column<string>(type: "TEXT", nullable: true),
                    MusteriSehir = table.Column<string>(type: "TEXT", nullable: true),
                    Avatar = table.Column<string>(type: "TEXT", nullable: true),
                    Yorum = table.Column<string>(type: "TEXT", nullable: false),
                    Puan = table.Column<int>(type: "INTEGER", nullable: false),
                    ProjeId = table.Column<int>(type: "INTEGER", nullable: true),
                    Onaylandi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OneCikan = table.Column<bool>(type: "INTEGER", nullable: false),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    YorumTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusteriYorumlari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjeKategorileri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjeKategorileri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Referanslar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    Logo = table.Column<string>(type: "TEXT", nullable: true),
                    Tip = table.Column<string>(type: "TEXT", nullable: false),
                    WebSite = table.Column<string>(type: "TEXT", nullable: true),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Referanslar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SayfaIcerikleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Anahtar = table.Column<string>(type: "TEXT", nullable: false),
                    Bolum = table.Column<string>(type: "TEXT", nullable: false),
                    Deger = table.Column<string>(type: "TEXT", nullable: false),
                    Dil = table.Column<string>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SayfaIcerikleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sertifikalar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    Resim = table.Column<string>(type: "TEXT", nullable: true),
                    PdfDosya = table.Column<string>(type: "TEXT", nullable: true),
                    VerilmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GecerlilikTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    VerenKurum = table.Column<string>(type: "TEXT", nullable: true),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sertifikalar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SikSorulanSorular",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Soru = table.Column<string>(type: "TEXT", nullable: false),
                    Cevap = table.Column<string>(type: "TEXT", nullable: false),
                    KategoriAdi = table.Column<string>(type: "TEXT", nullable: true),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    GoruntulemeSayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    FaydaliMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SikSorulanSorular", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SistemAyarlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Anahtar = table.Column<string>(type: "TEXT", nullable: false),
                    Deger = table.Column<string>(type: "TEXT", nullable: false),
                    Tip = table.Column<string>(type: "TEXT", nullable: true),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SistemAyarlari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Slaytlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Baslik = table.Column<string>(type: "TEXT", nullable: true),
                    AltBaslik = table.Column<string>(type: "TEXT", nullable: true),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    ArkaplanResim = table.Column<string>(type: "TEXT", nullable: true),
                    ArkaplanResimMobil = table.Column<string>(type: "TEXT", nullable: true),
                    ButonMetni1 = table.Column<string>(type: "TEXT", nullable: true),
                    ButonLink1 = table.Column<string>(type: "TEXT", nullable: true),
                    ButonMetni2 = table.Column<string>(type: "TEXT", nullable: true),
                    ButonLink2 = table.Column<string>(type: "TEXT", nullable: true),
                    AnimasyonTipi = table.Column<string>(type: "TEXT", nullable: true),
                    GecisHizi = table.Column<int>(type: "INTEGER", nullable: false),
                    GosterimSuresi = table.Column<int>(type: "INTEGER", nullable: false),
                    MetinHizalama = table.Column<string>(type: "TEXT", nullable: true),
                    MetinRengi = table.Column<string>(type: "TEXT", nullable: true),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    BaslangicTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    BitisTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Slaytlar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subeler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    Adres = table.Column<string>(type: "TEXT", nullable: true),
                    Sehir = table.Column<string>(type: "TEXT", nullable: true),
                    Ilce = table.Column<string>(type: "TEXT", nullable: true),
                    Telefon = table.Column<string>(type: "TEXT", nullable: true),
                    Eposta = table.Column<string>(type: "TEXT", nullable: true),
                    Enlem = table.Column<double>(type: "REAL", nullable: true),
                    Boylam = table.Column<double>(type: "REAL", nullable: true),
                    CalismaSaatleri = table.Column<string>(type: "TEXT", nullable: true),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    SubeYetkilisi = table.Column<string>(type: "TEXT", nullable: true),
                    SubeYetkilisiTelefon = table.Column<string>(type: "TEXT", nullable: true),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subeler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TanitimVideolari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Baslik = table.Column<string>(type: "TEXT", nullable: true),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    VideoUrl = table.Column<string>(type: "TEXT", nullable: true),
                    KapakResim = table.Column<string>(type: "TEXT", nullable: true),
                    SureSaniye = table.Column<int>(type: "INTEGER", nullable: true),
                    GoruntulemeSayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TanitimVideolari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ZiyaretKayitlari",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Tarih = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IP = table.Column<string>(type: "TEXT", nullable: true),
                    Sayfa = table.Column<string>(type: "TEXT", nullable: true),
                    Referer = table.Column<string>(type: "TEXT", nullable: true),
                    Tarayici = table.Column<string>(type: "TEXT", nullable: true),
                    Cihaz = table.Column<string>(type: "TEXT", nullable: true),
                    Sehir = table.Column<string>(type: "TEXT", nullable: true),
                    Ulke = table.Column<string>(type: "TEXT", nullable: true),
                    OturumSuresi = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZiyaretKayitlari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BlogYazilari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirmaId = table.Column<int>(type: "INTEGER", nullable: true),
                    Baslik = table.Column<string>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", nullable: false),
                    Ozet = table.Column<string>(type: "TEXT", nullable: false),
                    Icerik = table.Column<string>(type: "TEXT", nullable: false),
                    AnaResimUrl = table.Column<string>(type: "TEXT", nullable: false),
                    SeoBaslik = table.Column<string>(type: "TEXT", nullable: true),
                    SeoAciklama = table.Column<string>(type: "TEXT", nullable: true),
                    Etiketler = table.Column<string>(type: "TEXT", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OkunmaSayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    YayinTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogYazilari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlogYazilari_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IletisimMesajlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirmaId = table.Column<int>(type: "INTEGER", nullable: true),
                    AdSoyad = table.Column<string>(type: "TEXT", nullable: false),
                    Eposta = table.Column<string>(type: "TEXT", nullable: false),
                    Telefon = table.Column<string>(type: "TEXT", nullable: true),
                    Konu = table.Column<string>(type: "TEXT", nullable: true),
                    Mesaj = table.Column<string>(type: "TEXT", nullable: false),
                    OkunduMu = table.Column<bool>(type: "INTEGER", nullable: false),
                    OkunmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CevaplandiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    CevapTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CevapMetni = table.Column<string>(type: "TEXT", nullable: true),
                    OncelikSeviyesi = table.Column<string>(type: "TEXT", nullable: true),
                    EtiketlerJson = table.Column<string>(type: "TEXT", nullable: true),
                    IPAdresi = table.Column<string>(type: "TEXT", nullable: true),
                    Tarayici = table.Column<string>(type: "TEXT", nullable: true),
                    Cihaz = table.Column<string>(type: "TEXT", nullable: true),
                    Tarih = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IletisimMesajlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IletisimMesajlari_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Kategoriler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FirmaId = table.Column<int>(type: "INTEGER", nullable: true),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", nullable: false),
                    ResimUrl = table.Column<string>(type: "TEXT", nullable: false),
                    Sira = table.Column<int>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: false),
                    UstKategoriId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kategoriler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kategoriler_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Kategoriler_Kategoriler_UstKategoriId",
                        column: x => x.UstKategoriId,
                        principalTable: "Kategoriler",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Kullanicilar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirmaId = table.Column<int>(type: "INTEGER", nullable: true),
                    AdSoyad = table.Column<string>(type: "TEXT", nullable: false),
                    Eposta = table.Column<string>(type: "TEXT", nullable: false),
                    Telefon = table.Column<string>(type: "TEXT", nullable: true),
                    KullaniciAdi = table.Column<string>(type: "TEXT", nullable: false),
                    SifreHash = table.Column<string>(type: "TEXT", nullable: false),
                    PinHash = table.Column<string>(type: "TEXT", nullable: true),
                    DesenHash = table.Column<string>(type: "TEXT", nullable: true),
                    WebAuthnPublicKey = table.Column<string>(type: "TEXT", nullable: true),
                    SifreSifirlamaToken = table.Column<string>(type: "TEXT", nullable: true),
                    TokenGecerlilikTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EmailDogrulamaToken = table.Column<string>(type: "TEXT", nullable: true),
                    TotpAnahtari = table.Column<string>(type: "TEXT", nullable: true),
                    RefreshToken = table.Column<string>(type: "TEXT", nullable: true),
                    RefreshTokenBitisTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Rol = table.Column<int>(type: "INTEGER", nullable: false),
                    EmailDogrulandiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    IkiAdimDogrulamaAktif = table.Column<bool>(type: "INTEGER", nullable: false),
                    TelefonDogrulandiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    KilitlendiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    BasarisizGirisDenemesi = table.Column<int>(type: "INTEGER", nullable: false),
                    KilitAcmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SonGirisTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SonGirisIP = table.Column<string>(type: "TEXT", nullable: true),
                    ProfilResmiUrl = table.Column<string>(type: "TEXT", nullable: true),
                    TercihEdilenDil = table.Column<string>(type: "TEXT", nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OlusturanKullaniciId = table.Column<int>(type: "INTEGER", nullable: true),
                    SilindiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SilinmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kullanicilar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kullanicilar_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Lisanslar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirmaId = table.Column<int>(type: "INTEGER", nullable: false),
                    BirincilDomain = table.Column<string>(type: "TEXT", nullable: false),
                    YedekDomain = table.Column<string>(type: "TEXT", nullable: true),
                    BaslangicTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BitisTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LisansTipi = table.Column<string>(type: "TEXT", nullable: false),
                    LisansAnahtari = table.Column<string>(type: "TEXT", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SonDogrulamaTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lisanslar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lisanslar_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MenuOgeleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirmaId = table.Column<int>(type: "INTEGER", nullable: true),
                    Baslik = table.Column<string>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    UstMenuId = table.Column<int>(type: "INTEGER", nullable: true),
                    Sira = table.Column<int>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    YeniSekmede = table.Column<bool>(type: "INTEGER", nullable: false),
                    Ikon = table.Column<string>(type: "TEXT", nullable: true),
                    Konum = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuOgeleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuOgeleri_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MenuOgeleri_MenuOgeleri_UstMenuId",
                        column: x => x.UstMenuId,
                        principalTable: "MenuOgeleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KapakModelleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ModelAdi = table.Column<string>(type: "TEXT", nullable: false),
                    ModelKodu = table.Column<string>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", nullable: false),
                    Kategori = table.Column<string>(type: "TEXT", nullable: false),
                    ModelTuru = table.Column<string>(type: "TEXT", nullable: false),
                    KategoriId = table.Column<int>(type: "INTEGER", nullable: true),
                    KapiKategorisiId = table.Column<int>(type: "INTEGER", nullable: true),
                    AnaGorselUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    OnYazi = table.Column<string>(type: "TEXT", nullable: true),
                    OneCikanMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    YeniMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    Fiyat = table.Column<decimal>(type: "TEXT", nullable: true),
                    ModelDosyaYolu = table.Column<string>(type: "TEXT", nullable: true),
                    MinYukseklik = table.Column<int>(type: "INTEGER", nullable: true),
                    MaxYukseklik = table.Column<int>(type: "INTEGER", nullable: true),
                    MinGenislik = table.Column<int>(type: "INTEGER", nullable: true),
                    MaxGenislik = table.Column<int>(type: "INTEGER", nullable: true),
                    TeknikOzelliklerJson = table.Column<string>(type: "TEXT", nullable: true),
                    SertifikalarJson = table.Column<string>(type: "TEXT", nullable: true),
                    KullanimAlanlariJson = table.Column<string>(type: "TEXT", nullable: true),
                    RenkSecenekleriJson = table.Column<string>(type: "TEXT", nullable: false),
                    NiteliklerJson = table.Column<string>(type: "TEXT", nullable: false),
                    UygulamaGorselleriJson = table.Column<string>(type: "TEXT", nullable: false),
                    SeoBaslik = table.Column<string>(type: "TEXT", nullable: true),
                    SeoAciklama = table.Column<string>(type: "TEXT", nullable: true),
                    SeoAnahtarKelimeler = table.Column<string>(type: "TEXT", nullable: true),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SilindiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SilinmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KapakModelleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KapakModelleri_KapiKategorileri_KapiKategorisiId",
                        column: x => x.KapiKategorisiId,
                        principalTable: "KapiKategorileri",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "KapiKategorisiYerellestirmeleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KapiKategorisiId = table.Column<int>(type: "INTEGER", nullable: false),
                    Dil = table.Column<string>(type: "TEXT", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    SeoBaslik = table.Column<string>(type: "TEXT", nullable: true),
                    SeoAciklama = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KapiKategorisiYerellestirmeleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KapiKategorisiYerellestirmeleri_KapiKategorileri_KapiKategorisiId",
                        column: x => x.KapiKategorisiId,
                        principalTable: "KapiKategorileri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MobilyaKategorisiYerellestirmeleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MobilyaKategorisiId = table.Column<int>(type: "INTEGER", nullable: false),
                    Dil = table.Column<string>(type: "TEXT", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobilyaKategorisiYerellestirmeleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MobilyaKategorisiYerellestirmeleri_MobilyaKategorileri_MobilyaKategorisiId",
                        column: x => x.MobilyaKategorisiId,
                        principalTable: "MobilyaKategorileri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MobilyaUrunleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Slug = table.Column<string>(type: "TEXT", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    MobilyaKategorisiId = table.Column<int>(type: "INTEGER", nullable: false),
                    AnaGorselUrl = table.Column<string>(type: "TEXT", nullable: true),
                    GaleriResimleriJson = table.Column<string>(type: "TEXT", nullable: true),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    OneCikanMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SeoBaslik = table.Column<string>(type: "TEXT", nullable: true),
                    SeoAciklama = table.Column<string>(type: "TEXT", nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobilyaUrunleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MobilyaUrunleri_MobilyaKategorileri_MobilyaKategorisiId",
                        column: x => x.MobilyaKategorisiId,
                        principalTable: "MobilyaKategorileri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Projeler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Slug = table.Column<string>(type: "TEXT", nullable: false),
                    Baslik = table.Column<string>(type: "TEXT", nullable: false),
                    KisaAciklama = table.Column<string>(type: "TEXT", nullable: true),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    KategoriId = table.Column<int>(type: "INTEGER", nullable: false),
                    MusteriAdi = table.Column<string>(type: "TEXT", nullable: true),
                    MusteriSehir = table.Column<string>(type: "TEXT", nullable: true),
                    ProjeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    KapakResim = table.Column<string>(type: "TEXT", nullable: true),
                    OneCikanMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SeoBaslik = table.Column<string>(type: "TEXT", nullable: true),
                    SeoAciklama = table.Column<string>(type: "TEXT", nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projeler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projeler_ProjeKategorileri_KategoriId",
                        column: x => x.KategoriId,
                        principalTable: "ProjeKategorileri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlogResim",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BlogYazisiId = table.Column<int>(type: "INTEGER", nullable: false),
                    ResimUrl = table.Column<string>(type: "TEXT", nullable: false),
                    Sira = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogResim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlogResim_BlogYazilari_BlogYazisiId",
                        column: x => x.BlogYazisiId,
                        principalTable: "BlogYazilari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KapiModeliResimleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KapakModeliId = table.Column<int>(type: "INTEGER", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    AltMetin = table.Column<string>(type: "TEXT", nullable: true),
                    Sira = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KapiModeliResimleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KapiModeliResimleri_KapakModelleri_KapakModeliId",
                        column: x => x.KapakModeliId,
                        principalTable: "KapakModelleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KapiModeliYerellestirmeleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KapakModeliId = table.Column<int>(type: "INTEGER", nullable: false),
                    Dil = table.Column<string>(type: "TEXT", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    OnYazi = table.Column<string>(type: "TEXT", nullable: true),
                    SeoBaslik = table.Column<string>(type: "TEXT", nullable: true),
                    SeoAciklama = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KapiModeliYerellestirmeleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KapiModeliYerellestirmeleri_KapakModelleri_KapakModeliId",
                        column: x => x.KapakModeliId,
                        principalTable: "KapakModelleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MobilyaUrunuYerellestirmeleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MobilyaUrunuId = table.Column<int>(type: "INTEGER", nullable: false),
                    Dil = table.Column<string>(type: "TEXT", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobilyaUrunuYerellestirmeleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MobilyaUrunuYerellestirmeleri_MobilyaUrunleri_MobilyaUrunuId",
                        column: x => x.MobilyaUrunuId,
                        principalTable: "MobilyaUrunleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjeResimleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    AltMetin = table.Column<string>(type: "TEXT", nullable: true),
                    Sira = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjeResimleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjeResimleri_Projeler_ProjeId",
                        column: x => x.ProjeId,
                        principalTable: "Projeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Kullanicilar",
                columns: new[] { "Id", "AdSoyad", "AktifMi", "BasarisizGirisDenemesi", "DesenHash", "EmailDogrulamaToken", "EmailDogrulandiMi", "Eposta", "FirmaId", "GuncellenmeTarihi", "IkiAdimDogrulamaAktif", "KilitAcmaTarihi", "KilitlendiMi", "KullaniciAdi", "OlusturanKullaniciId", "OlusturulmaTarihi", "PinHash", "ProfilResmiUrl", "RefreshToken", "RefreshTokenBitisTarihi", "Rol", "SifreHash", "SifreSifirlamaToken", "SilindiMi", "SilinmeTarihi", "SonGirisIP", "SonGirisTarihi", "Telefon", "TelefonDogrulandiMi", "TercihEdilenDil", "TokenGecerlilikTarihi", "TotpAnahtari", "WebAuthnPublicKey" },
                values: new object[] { 1, "VIZITLINK3D Yonetici", true, 0, null, null, true, "admin@3dvizitlink.com.tr", null, null, false, null, false, "admin", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, 3, "$2a$11$nt1W5l252hapG97qf8lIlOORhjfjq5RiX/pmTk.4tIZwuJrsuwslm", null, false, null, null, null, null, false, "tr", null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_BlogResim_BlogYazisiId",
                table: "BlogResim",
                column: "BlogYazisiId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogYazilari_FirmaId",
                table: "BlogYazilari",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_Ceviriler_Anahtar_Dil",
                table: "Ceviriler",
                columns: new[] { "Anahtar", "Dil" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Diller_Kod",
                table: "Diller",
                column: "Kod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IletisimMesajlari_FirmaId",
                table: "IletisimMesajlari",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_KapakModelleri_KapiKategorisiId",
                table: "KapakModelleri",
                column: "KapiKategorisiId");

            migrationBuilder.CreateIndex(
                name: "IX_KapakModelleri_Slug",
                table: "KapakModelleri",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KapiKategorisiYerellestirmeleri_KapiKategorisiId_Dil",
                table: "KapiKategorisiYerellestirmeleri",
                columns: new[] { "KapiKategorisiId", "Dil" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KapiModeliResimleri_KapakModeliId",
                table: "KapiModeliResimleri",
                column: "KapakModeliId");

            migrationBuilder.CreateIndex(
                name: "IX_KapiModeliYerellestirmeleri_KapakModeliId_Dil",
                table: "KapiModeliYerellestirmeleri",
                columns: new[] { "KapakModeliId", "Dil" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kategoriler_FirmaId",
                table: "Kategoriler",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_Kategoriler_UstKategoriId",
                table: "Kategoriler",
                column: "UstKategoriId");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_Eposta",
                table: "Kullanicilar",
                column: "Eposta",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_FirmaId",
                table: "Kullanicilar",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_KullaniciAdi",
                table: "Kullanicilar",
                column: "KullaniciAdi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lisanslar_FirmaId",
                table: "Lisanslar",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuOgeleri_FirmaId",
                table: "MenuOgeleri",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuOgeleri_UstMenuId",
                table: "MenuOgeleri",
                column: "UstMenuId");

            migrationBuilder.CreateIndex(
                name: "IX_MobilyaKategorisiYerellestirmeleri_MobilyaKategorisiId_Dil",
                table: "MobilyaKategorisiYerellestirmeleri",
                columns: new[] { "MobilyaKategorisiId", "Dil" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MobilyaUrunleri_MobilyaKategorisiId",
                table: "MobilyaUrunleri",
                column: "MobilyaKategorisiId");

            migrationBuilder.CreateIndex(
                name: "IX_MobilyaUrunuYerellestirmeleri_MobilyaUrunuId_Dil",
                table: "MobilyaUrunuYerellestirmeleri",
                columns: new[] { "MobilyaUrunuId", "Dil" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projeler_KategoriId",
                table: "Projeler",
                column: "KategoriId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjeResimleri_ProjeId",
                table: "ProjeResimleri",
                column: "ProjeId");

            migrationBuilder.CreateIndex(
                name: "IX_SayfaIcerikleri_Bolum_Anahtar_Dil",
                table: "SayfaIcerikleri",
                columns: new[] { "Bolum", "Anahtar", "Dil" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLoglar");

            migrationBuilder.DropTable(
                name: "BlogResim");

            migrationBuilder.DropTable(
                name: "BultenAboneleri");

            migrationBuilder.DropTable(
                name: "CanliSohbetMesajlari");

            migrationBuilder.DropTable(
                name: "Ceviriler");

            migrationBuilder.DropTable(
                name: "Diller");

            migrationBuilder.DropTable(
                name: "EkipUyeleri");

            migrationBuilder.DropTable(
                name: "EpostaSablonlari");

            migrationBuilder.DropTable(
                name: "GaleriGorselleri");

            migrationBuilder.DropTable(
                name: "HizmetAdimlari");

            migrationBuilder.DropTable(
                name: "IletisimMesajlari");

            migrationBuilder.DropTable(
                name: "KapiKategorisiYerellestirmeleri");

            migrationBuilder.DropTable(
                name: "KapiModeliResimleri");

            migrationBuilder.DropTable(
                name: "KapiModeliYerellestirmeleri");

            migrationBuilder.DropTable(
                name: "Kataloglar");

            migrationBuilder.DropTable(
                name: "Kategoriler");

            migrationBuilder.DropTable(
                name: "Kullanicilar");

            migrationBuilder.DropTable(
                name: "Lisanslar");

            migrationBuilder.DropTable(
                name: "MenuOgeleri");

            migrationBuilder.DropTable(
                name: "MobilyaKategorisiYerellestirmeleri");

            migrationBuilder.DropTable(
                name: "MobilyaUrunuYerellestirmeleri");

            migrationBuilder.DropTable(
                name: "MusteriYorumlari");

            migrationBuilder.DropTable(
                name: "ProjeResimleri");

            migrationBuilder.DropTable(
                name: "Referanslar");

            migrationBuilder.DropTable(
                name: "SayfaIcerikleri");

            migrationBuilder.DropTable(
                name: "Sertifikalar");

            migrationBuilder.DropTable(
                name: "SikSorulanSorular");

            migrationBuilder.DropTable(
                name: "SistemAyarlari");

            migrationBuilder.DropTable(
                name: "Slaytlar");

            migrationBuilder.DropTable(
                name: "Subeler");

            migrationBuilder.DropTable(
                name: "TanitimVideolari");

            migrationBuilder.DropTable(
                name: "ZiyaretKayitlari");

            migrationBuilder.DropTable(
                name: "BlogYazilari");

            migrationBuilder.DropTable(
                name: "KapakModelleri");

            migrationBuilder.DropTable(
                name: "MobilyaUrunleri");

            migrationBuilder.DropTable(
                name: "Projeler");

            migrationBuilder.DropTable(
                name: "Firmalar");

            migrationBuilder.DropTable(
                name: "KapiKategorileri");

            migrationBuilder.DropTable(
                name: "MobilyaKategorileri");

            migrationBuilder.DropTable(
                name: "ProjeKategorileri");
        }
    }
}
