using System.IO;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VizitLink3D.Api.Modeller;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;

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
                Ad = "ORPAY",
                Unvan = "ORPAY Orman Ürünleri Ltd. Şti.",
                Slug = Environment.GetEnvironmentVariable("Saas__VarsayilanFirmaSlug") ?? "vizitlink3d",
                AciklamaKisa = "3D urun konfigurator ve dijital showroom platformu",
                Aciklama = "ORPAY, kapı yüzeyleri ve orman ürünlerinde yüksek kaliteyi musterilerine sunan üretim firmasıdır.",
                Domain = "localhost",
                Eposta = "bilgi@3dvizitlink.com.tr",
                Telefon1 = "+90 000 000 00 00",
                Adres = "Turkiye",
                Sehir = "Istanbul",
                KurulusYili = 2024,
                SiteTema = "orpay-luxe-industrial",
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
                AdSoyad = "ORPAY Yonetici",
                Eposta = "admin@orpay.com.tr",
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
                new SistemAyari { Anahtar = "site.baslik", Deger = "ORPAY" },
                new SistemAyari { Anahtar = "site.aciklama", Deger = "ORPAY - 3D Dijital Showroom Platformu" },
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

    /// <summary>
    /// Ana tohumdan sonra çalışır. Eksik kayıtları BİREBİR SaveChanges ile tamamlar.
    /// Mevcut verileri bozmaz — sadece eksik olanları ekler.
    /// </summary>
    public static async Task PatchSeedAsync(VizitLink3DDbContext vt)
    {
        Log.Information("PatchSeed: Eksik tohum verileri kontrol ediliyor...");

        var firma = await vt.Firmalar.FirstOrDefaultAsync();
        if (firma == null) return;

        // 1. OneCikanMi işaretle (önceden False olan ürünlere)
        await OneCikanlariIsaretleAsync(vt);

        // 2. UrunMedyalari tamamla (dosya sisteminden tara)
        await UrunMedyalariniTekTekEkleAsync(vt);

        // 3. UrunYerellestirmeleri tamamla
        await UrunYerellestirmeleriniTekTekEkleAsync(vt);

        // 4. SayfaIcerigi tamamla
        await SayfaIceriginiTekTekEkleAsync(vt, firma.Id);

        Log.Information("PatchSeed tamamlandı.");
    }

    private static async Task OneCikanlariIsaretleAsync(VizitLink3DDbContext vt)
    {
        var birincilOneCikanlar = new HashSet<string>
        {
            "hisar", "gala", "matrix", "yildiz", "sahra", "vizyon", "patras",
            "nova", "duru", "irmak", "milas", "istanbul", "ulubey", "sedef",
            "inci", "lento", "royal", "hitit", "safir", "uludag", "avangarde",
            "tebriz", "finike", "damla", "kale"
        };

        var guncellenecek = await vt.Urunler
            .Where(u => !u.SilindiMi && !u.OneCikanMi && birincilOneCikanlar.Contains(u.Slug))
            .ToListAsync();

        foreach (var urun in guncellenecek)
        {
            urun.OneCikanMi = true;
            await vt.SaveChangesAsync();
        }

        if (guncellenecek.Any())
            Log.Information("PatchSeed: {Adet} ürüne OneCikanMi=true işaretlendi.", guncellenecek.Count);
    }

    private static async Task UrunMedyalariniTekTekEkleAsync(VizitLink3DDbContext vt)
    {
        var medyaOlmayanUrunIdleri = await vt.Urunler
            .Where(u => !u.SilindiMi && !vt.UrunMedyalari.Any(m => m.UrunId == u.Id))
            .Select(u => new { u.Id, u.Kod, u.UrunKategoriId })
            .ToListAsync();

        if (!medyaOlmayanUrunIdleri.Any()) return;

        var medyaKoku = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "medya", "urunler");
        int eklenen = 0;

        foreach (var urun in medyaOlmayanUrunIdleri)
        {
            var kategori = await vt.UrunKategorileri.FirstOrDefaultAsync(k => k.Id == urun.UrunKategoriId);
            if (kategori == null) continue;

            var klasorYolu = Path.Combine(medyaKoku, kategori.Slug, urun.Kod);
            if (!Directory.Exists(klasorYolu)) continue;

            var dosyalar = Directory.GetFiles(klasorYolu, "*.*", SearchOption.TopDirectoryOnly)
                .Where(d => d.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                         || d.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                         || d.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f)
                .ToList();

            for (int i = 0; i < dosyalar.Count; i++)
            {
                var dosyaAdi = Path.GetFileName(dosyalar[i]);
                var medyaUrl = $"/medya/urunler/{kategori.Slug}/{urun.Kod}/{dosyaAdi}";

                var medya = new UrunMedya
                {
                    UrunId = urun.Id,
                    MedyaUrl = medyaUrl,
                    MedyaTuru = "Resim",
                    SiraNo = i + 1,
                    AnaGosterim = (i == 0),
                };
                vt.UrunMedyalari.Add(medya);
                await vt.SaveChangesAsync();
                eklenen++;
            }
        }

        Log.Information("PatchSeed: {Adet} medya kaydı eklendi.", eklenen);
    }

    private static async Task UrunYerellestirmeleriniTekTekEkleAsync(VizitLink3DDbContext vt)
    {
        int eklenen = 0;

        foreach (var (model, baslik, kisaAciklama) in YerellestirmeVerileri)
        {
            var slug = ModeldenSlug(model);
            var urun = await vt.Urunler.FirstOrDefaultAsync(u => u.Slug == slug);
            if (urun == null) continue;

            var mevcut = await vt.UrunYerellestirmeleri
                .FirstOrDefaultAsync(y => y.UrunId == urun.Id && y.Dil == "tr");
            if (mevcut != null) continue;

            vt.UrunYerellestirmeleri.Add(new UrunYerellestirme
            {
                UrunId = urun.Id,
                Dil = "tr",
                Ad = baslik,
                KisaAciklama = kisaAciklama,
                Aciklama = $"<div class=\"orpay-ozellikler\"><h4>Teknik Özellikler</h4><ul><li><strong>Standart En:</strong> 80 - 90 cm</li><li><strong>Standart Boy:</strong> 205 - 210 cm</li><li><strong>Kanat Kalınlığı:</strong> 42 mm</li><li><strong>Kasa Seçenekleri:</strong> 18mm MDF, Sandwich Kasa</li><li><strong>Pervaz:</strong> Geçmeli, ayarlı pervaz sistemi</li></ul><p>{kisaAciklama}</p></div>"
            });
            await vt.SaveChangesAsync();
            eklenen++;
        }

        Log.Information("PatchSeed: {Adet} yerellestirme kaydı eklendi.", eklenen);
    }

    private static async Task SayfaIceriginiTekTekEkleAsync(VizitLink3DDbContext vt, int firmaId)
    {
        var mevcut = await vt.SayfaIcerikleri
            .FirstOrDefaultAsync(s => s.Anahtar == "hakkimizda" && s.Dil == "tr" && s.FirmaId == firmaId);
        if (mevcut != null) return;

        vt.SayfaIcerikleri.Add(new SayfaIcerigi
        {
            FirmaId = firmaId,
            Anahtar = "hakkimizda",
            Bolum = "metin",
            Dil = "tr",
            Deger = "ORPAY, kapı yüzeyleri ve orman ürünlerinde yüksek kaliteyi musterilerine sunan üretim firmasıdır.\n\nModern teknolojilerle gelistirilen platformumuz, urunlerinizi interaktif bir sekilde sergilemenizi, musterilerinizin urunleri 3 boyutlu olarak incelemesini ve kendi tercihlerine gore ozellestirmesini saglar.\n\nSurekli gelisen altyapimiz ve yenilenen teknolojimiz ile musteri memnuniyetini on planda tutuyoruz.",
            GuncellemeTarihi = DateTime.UtcNow,
        });
        await vt.SaveChangesAsync();
        Log.Information("PatchSeed: SayfaIcerigi (hakkimizda) eklendi.");
    }
}
