using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.VeriTabani;

public static partial class TohumVerisi
{
    public static async Task TohumlaAsync(VizitLink3DDbContext vt)
    {
        // === 1. PLATFORM VARSAYILAN FIRMASI ===
        if (!vt.Firmalar.Any())
        {
            vt.Firmalar.Add(new Firma
            {
                Ad = "VizitLink3D Platform",
                Unvan = "VizitLink3D Platform Varsayilan Firma",
                Slug = "vizitlink3d",
                AciklamaKisa = "3D urun konfigurator ve dijital showroom platformu",
                Aciklama = "VizitLink3D, urunlerinizi 3 boyutlu olarak musterilerinize sunmanizi saglayan dijital showroom ve konfigurator platformudur.",
                Domain = "localhost",
                Eposta = "bilgi@3dvizitlink.com.tr",
                Telefon1 = "+90 000 000 00 00",
                Adres = "Turkiye",
                Sehir = "Istanbul",
                KurulusYili = 2024,
                SiteTema = "vizitlink3d",
                AdminTema = "vizitlink3d",
                AktifMi = true,
                OlusturulmaTarihi = DateTime.UtcNow
            });
            await vt.SaveChangesAsync();
        }

        var firma = await vt.Firmalar.FirstAsync();

        // === 2. ADMIN KULLANICI ===
        if (!vt.Kullanicilar.Any(k => k.KullaniciAdi == "admin"))
        {
            vt.Kullanicilar.Add(new Kullanici
            {
                FirmaId = firma.Id,
                AdSoyad = "VIZITLINK3D Yonetici",
                Eposta = "admin@3dvizitlink.com.tr",
                KullaniciAdi = "admin",
                SifreHash = BCrypt.Net.BCrypt.HashPassword("Admin2026!"),
                Rol = Rol.SuperAdmin,
                EmailDogrulandiMi = true,
                AktifMi = true,
                OlusturulmaTarihi = DateTime.UtcNow
            });
            await vt.SaveChangesAsync();
        }

        // === 3. DILLER ===
        if (!vt.Diller.Any())
        {
            vt.Diller.AddRange(
                new Dil { Kod = "tr", Ad = "Turkce", AktifMi = true, VarsayilanMi = true, SiraNo = 1 },
                new Dil { Kod = "en", Ad = "English", AktifMi = true, VarsayilanMi = false, SiraNo = 2 }
            );
            await vt.SaveChangesAsync();
        }

        // === 4. TEMEL SISTEM AYARLARI ===
        if (!vt.SistemAyarlari.Any())
        {
            vt.SistemAyarlari.AddRange(
                new SistemAyari { Anahtar = "site.baslik", Deger = "VizitLink3D" },
                new SistemAyari { Anahtar = "site.aciklama", Deger = "VizitLink3D - 3D Dijital Showroom Platformu" },
                new SistemAyari { Anahtar = "site.telefon", Deger = "+90 000 000 00 00" },
                new SistemAyari { Anahtar = "site.eposta", Deger = "bilgi@3dvizitlink.com.tr" }
            );
            await vt.SaveChangesAsync();
        }

        // === 5. ADMIN MENULERI ===
        if (!vt.MenuOgeleri.Any(m => m.Konum == "AdminMenu"))
        {
            var adminMenuleri = new List<MenuOgesi>
            {
                new MenuOgesi { FirmaId = firma.Id, Baslik = "Gosterge Panosu", Url = "/yonetim", Ikon = "Dashboard", Sira = 1, Konum = "AdminMenu", SistemMenusuMu = true, AktifMi = true },
                new MenuOgesi { FirmaId = firma.Id, Baslik = "Icerik Yonetimi", Url = "", Ikon = "Article", Sira = 10, Konum = "AdminMenu", SistemMenusuMu = true, AktifMi = true },
                new MenuOgesi { FirmaId = firma.Id, Baslik = "Sayfalar", Url = "/yonetim/sayfalar", Ikon = "Description", Sira = 11, Konum = "AdminMenu", SistemMenusuMu = true, AktifMi = true },
                new MenuOgesi { FirmaId = firma.Id, Baslik = "Haberler", Url = "/yonetim/haberler", Ikon = "Newspaper", Sira = 12, Konum = "AdminMenu", SistemMenusuMu = true, AktifMi = true },
                new MenuOgesi { FirmaId = firma.Id, Baslik = "Medya Havuzu", Url = "/yonetim/medya-havuzu", Ikon = "PermMedia", Sira = 20, Konum = "AdminMenu", SistemMenusuMu = true, AktifMi = true },
                new MenuOgesi { FirmaId = firma.Id, Baslik = "Menu Yonetimi", Url = "/yonetim/menu-yonetimi", Ikon = "Menu", Sira = 30, Konum = "AdminMenu", SistemMenusuMu = true, AktifMi = true },
                new MenuOgesi { FirmaId = firma.Id, Baslik = "Ayarlar", Url = "", Ikon = "Settings", Sira = 90, Konum = "AdminMenu", SistemMenusuMu = true, AktifMi = true },
                new MenuOgesi { FirmaId = firma.Id, Baslik = "Firma Ayarlari", Url = "/yonetim/firma-ayarlari", Ikon = "Business", Sira = 91, Konum = "AdminMenu", SistemMenusuMu = true, AktifMi = true },
                new MenuOgesi { FirmaId = firma.Id, Baslik = "Tema Ayarlari", Url = "/yonetim/tema", Ikon = "Palette", Sira = 92, Konum = "AdminMenu", SistemMenusuMu = true, AktifMi = true },
            };

            vt.MenuOgeleri.AddRange(adminMenuleri);
            await vt.SaveChangesAsync();

            var icerikYonetim = await vt.MenuOgeleri.FirstAsync(m => m.Baslik == "Icerik Yonetimi" && m.Konum == "AdminMenu");
            var ayarlar = await vt.MenuOgeleri.FirstAsync(m => m.Baslik == "Ayarlar" && m.Konum == "AdminMenu");

            var altMenuler = await vt.MenuOgeleri
                .Where(m => m.Konum == "AdminMenu" && m.Baslik != "Gosterge Panosu" && m.Baslik != "Icerik Yonetimi"
                    && m.Baslik != "Medya Havuzu" && m.Baslik != "Menu Yonetimi" && m.Baslik != "Ayarlar")
                .ToListAsync();

            foreach (var alt in altMenuler)
            {
                if (alt.Baslik == "Sayfalar" || alt.Baslik == "Haberler")
                    alt.UstMenuId = icerikYonetim.Id;
                else if (alt.Baslik == "Firma Ayarlari" || alt.Baslik == "Tema Ayarlari")
                    alt.UstMenuId = ayarlar.Id;
            }
            await vt.SaveChangesAsync();
        }

        // === 6. ANA MENU (public) ===
        if (!vt.MenuOgeleri.Any(m => m.Konum == "AnaMenu"))
        {
            vt.MenuOgeleri.AddRange(
                new MenuOgesi { FirmaId = firma.Id, Baslik = "Ana Sayfa", Url = "/", Ikon = "Home", Sira = 1, Konum = "AnaMenu", AktifMi = true },
                new MenuOgesi { FirmaId = firma.Id, Baslik = "Urunler", Url = "/urunler", Ikon = "Category", Sira = 2, Konum = "AnaMenu", AktifMi = true },
                new MenuOgesi { FirmaId = firma.Id, Baslik = "Kurumsal", Url = "/hakkimizda", Ikon = "Info", Sira = 3, Konum = "AnaMenu", AktifMi = true },
                new MenuOgesi { FirmaId = firma.Id, Baslik = "Iletisim", Url = "/iletisim", Ikon = "ContactMail", Sira = 4, Konum = "AnaMenu", AktifMi = true }
            );
            await vt.SaveChangesAsync();
        }

        // === 7. KATALOG VERISI ===
        await KatalogVerisiniTohumlaAsync(vt);
    }
}
