using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.VeriTabani;

public static class TohumVerisi
{
    public static async Task TohumlaAsync(VizitLink3DDbContext vt)
    {
        // === 1. VARSAYILAN FIRMA ===
        if (!vt.Firmalar.Any())
        {
            vt.Firmalar.Add(new Firma
            {
                Ad = "ORPAY",
                Unvan = "ORPAY Orman Urunleri",
                Slug = "orpay",
                AciklamaKisa = "Orman urunleri ve yapi malzemeleri",
                Aciklama = "ORPAY, 1992 yilindan beri orman urunleri ve yapi malzemeleri sektorunde hizmet vermektedir.",
                Domain = "localhost",
                Eposta = "info@orpay.com.tr",
                Telefon1 = "+90 312 000 00 00",
                Adres = "Ankara / Turkiye",
                Sehir = "Ankara",
                KurulusYili = 1992,
                SiteTema = "gold",
                AdminTema = "gold",
                AktifMi = true,
                OlusturulmaTarihi = DateTime.UtcNow
            });
            await vt.SaveChangesAsync();
        }

        // Eski goldbanyo slug'ini orpay'e guncelle (varsa)
        var goldbanyoFirma = await vt.Firmalar.FirstOrDefaultAsync(f => f.Slug == "goldbanyo");
        if (goldbanyoFirma != null)
        {
            goldbanyoFirma.Slug = "orpay";
            goldbanyoFirma.Ad = "ORPAY";
            goldbanyoFirma.Unvan = "ORPAY Orman Urunleri";
            goldbanyoFirma.Domain = "localhost";
            goldbanyoFirma.Eposta = "iletisim@orpayormanurunleri.com.tr";
            await vt.SaveChangesAsync();
        }

        var firma = await vt.Firmalar.FirstAsync(f => f.Slug == "orpay");

        // === 2. ADMIN KULLANICI ===
        if (!vt.Kullanicilar.Any(k => k.KullaniciAdi == "admin"))
        {
            vt.Kullanicilar.Add(new Kullanici
            {
                FirmaId = firma.Id,
                AdSoyad = "ORPAY Admin",
                Eposta = "admin@orpay.com.tr",
                KullaniciAdi = "admin",
                SifreHash = BCrypt.Net.BCrypt.HashPassword("Orpay2026!"),
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
                new SistemAyari { Anahtar = "site.baslik", Deger = "ORPAY Orman Urunleri" },
                new SistemAyari { Anahtar = "site.aciklama", Deger = "ORPAY - Kaliteli orman urunleri ve yapi malzemeleri" },
                new SistemAyari { Anahtar = "site.telefon", Deger = "+90 312 000 00 00" },
                new SistemAyari { Anahtar = "site.eposta", Deger = "info@orpay.com.tr" }
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
    }

    public static async Task OrpayKatalogMedyaUzantilariniDuzeltAsync(VizitLink3DDbContext vt)
    {
        await Task.CompletedTask;
    }

    public static async Task OrpayIletisimBilgileriniDuzeltAsync(VizitLink3DDbContext vt)
    {
        await Task.CompletedTask;
    }

    public static async Task TemizleSlaytResimlerAsync(VizitLink3DDbContext vt, string webRootPath)
    {
        await Task.CompletedTask;
    }
}

