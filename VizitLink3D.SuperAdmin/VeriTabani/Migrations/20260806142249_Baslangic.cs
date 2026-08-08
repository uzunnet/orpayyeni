using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VizitLink3D.SuperAdmin.VeriTabani.Migrations
{
    /// <inheritdoc />
    public partial class Baslangic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    AdminTema = table.Column<string>(type: "TEXT", nullable: true),
                    SiteTema = table.Column<string>(type: "TEXT", nullable: true),
                    MenuYatayAralik = table.Column<int>(type: "INTEGER", nullable: false),
                    MenuDikeyPadding = table.Column<int>(type: "INTEGER", nullable: false),
                    LogoMaxYukseklik = table.Column<int>(type: "INTEGER", nullable: false),
                    YetkiliAdSoyad = table.Column<string>(type: "TEXT", nullable: true),
                    VergiNo = table.Column<string>(type: "TEXT", nullable: true),
                    VergiDairesi = table.Column<string>(type: "TEXT", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    DemoMu = table.Column<bool>(type: "INTEGER", nullable: false),
                    AktifSablonId = table.Column<int>(type: "INTEGER", nullable: true),
                    AktifModulKodlariJson = table.Column<string>(type: "TEXT", nullable: true),
                    Sektor = table.Column<string>(type: "TEXT", nullable: true),
                    MedyaKlasoru = table.Column<string>(type: "TEXT", nullable: true),
                    PaketTipi = table.Column<string>(type: "TEXT", nullable: true),
                    MaxKullaniciSayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Firmalar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Moduller",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Kod = table.Column<string>(type: "TEXT", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    Kategori = table.Column<string>(type: "TEXT", nullable: true),
                    VarsayilanMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SistemModuluMu = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Moduller", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FirmaModulAtamalari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirmaId = table.Column<int>(type: "INTEGER", nullable: false),
                    ModulId = table.Column<int>(type: "INTEGER", nullable: false),
                    AtanmaTarihi = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FirmaModulAtamalari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FirmaModulAtamalari_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FirmaModulAtamalari_Moduller_ModulId",
                        column: x => x.ModulId,
                        principalTable: "Moduller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Moduller",
                columns: new[] { "Id", "Aciklama", "Ad", "Kategori", "Kod", "SistemModuluMu", "VarsayilanMi" },
                values: new object[,]
                {
                    { 1, null, "Blog", "Icerik", "blog", false, true },
                    { 2, null, "Galeri", "Icerik", "galeri", false, true },
                    { 3, null, "Iletisim Formu", "Iletisim", "iletisim", false, true },
                    { 4, null, "Canli Sohbet", "Iletisim", "sohbet", false, true },
                    { 5, null, "Medya Havuzu", "Medya", "medya_havuzu", false, true },
                    { 6, null, "AI Asistan", "AI", "ai_asistan", false, true },
                    { 7, null, "3D Goruntu", "Gorsel", "3d_goruntu", false, true },
                    { 8, null, "Urun Yonetimi", "E-Ticaret", "urunler", false, true },
                    { 9, null, "Haber Yonetimi", "Icerik", "haberler", false, true },
                    { 10, null, "Sayfa Yonetimi", "Icerik", "sayfalar", false, true },
                    { 11, null, "Menu Yonetimi", "Yonetim", "menu_yonetimi", false, true },
                    { 12, null, "Tema Yonetimi", "Tasarim", "tema_yonetimi", false, true },
                    { 13, null, "Proje Yonetimi", "Is", "proje_yonetimi", false, false },
                    { 14, null, "Slayt Yonetimi", "Icerik", "slayt_yonetimi", false, true },
                    { 15, null, "Referanslar", "Pazarlama", "referanslar", false, false },
                    { 16, null, "Sertifikalar", "Kurumsal", "sertifikalar", false, false },
                    { 17, null, "SSS", "Icerik", "sss", false, false },
                    { 18, null, "Katalog Yonetimi", "Pazarlama", "katalog", false, false },
                    { 19, null, "Bayi Yonetimi", "Is", "bayi_yonetimi", false, false },
                    { 20, null, "Ekip Yonetimi", "Kurumsal", "ekip_yonetimi", false, false },
                    { 21, null, "PWA Offline", "Teknik", "pwa_offline", false, false },
                    { 22, null, "Audit Log", "Guvenlik", "audit_log", true, true },
                    { 23, null, "Lisans Yonetimi", "Sistem", "lisans_yonetimi", true, true },
                    { 24, null, "Bildirimler", "Iletisim", "bildirimler", false, false }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Firmalar_Domain",
                table: "Firmalar",
                column: "Domain",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Firmalar_Slug",
                table: "Firmalar",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FirmaModulAtamalari_FirmaId",
                table: "FirmaModulAtamalari",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_FirmaModulAtamalari_ModulId",
                table: "FirmaModulAtamalari",
                column: "ModulId");

            migrationBuilder.CreateIndex(
                name: "IX_Moduller_Kod",
                table: "Moduller",
                column: "Kod",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FirmaModulAtamalari");

            migrationBuilder.DropTable(
                name: "Firmalar");

            migrationBuilder.DropTable(
                name: "Moduller");
        }
    }
}
