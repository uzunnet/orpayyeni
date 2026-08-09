using System.IO;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Api.Modeller;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;

namespace VizitLink3D.Api.VeriTabani;

public static partial class TohumVerisi
{
    #region Model Veri Listeleri

    /// <summary>Tum katalog modelleri (ModelAdi, KategoriSlug).</summary>
    private static readonly List<(string Model, string KategoriSlug)> KatalogModelleri = new()
    {
        // === Gobekli Melamin (29) ===
        ("AVANGARDE",     "gobekli-melamin"),
        ("AVANGARDE_Renk","gobekli-melamin"),
        ("DAMLA",         "gobekli-melamin"),
        ("DURU",          "gobekli-melamin"),
        ("FINIKE",        "gobekli-melamin"),
        ("GALA",          "gobekli-melamin"),
        ("HISAR",         "gobekli-melamin"),
        ("HITIT",         "gobekli-melamin"),
        ("INCI",          "gobekli-melamin"),
        ("IRMAK",         "gobekli-melamin"),
        ("ISTANBUL",      "gobekli-melamin"),
        ("KALE",          "gobekli-melamin"),
        ("LENTO",         "gobekli-melamin"),
        ("MATRIX",        "gobekli-melamin"),
        ("MILAS",         "gobekli-melamin"),
        ("NOVA",          "gobekli-melamin"),
        ("PATRAS",        "gobekli-melamin"),
        ("ROYAL",         "gobekli-melamin"),
        ("ROYAL_Renk",    "gobekli-melamin"),
        ("RUSTIK",        "gobekli-melamin"),
        ("SAFIR",         "gobekli-melamin"),
        ("SAHRA",         "gobekli-melamin"),
        ("SEDEF",         "gobekli-melamin"),
        ("TEBRIZ",        "gobekli-melamin"),
        ("TUGRA",         "gobekli-melamin"),
        ("ULUBEY",        "gobekli-melamin"),
        ("ULUDAG",        "gobekli-melamin"),
        ("VIZYON",        "gobekli-melamin"),
        ("YILDIZ",        "gobekli-melamin"),

        // === Duz Melamin (7) ===
        ("SONOMO",        "duz-melamin"),
        ("LIKYA",         "duz-melamin"),
        ("BEYAZ",         "duz-melamin"),
        ("YENI_MESE",     "duz-melamin"),
        ("BAMBU",         "duz-melamin"),
        ("BUZ_MESE",      "duz-melamin"),
        ("ZEUGMA",        "duz-melamin"),

        // === Membran OM (26) ===
        ("OM_01", "membran-om"), ("OM_02", "membran-om"), ("OM_03", "membran-om"),
        ("OM_04", "membran-om"), ("OM_05", "membran-om"), ("OM_06", "membran-om"),
        ("OM_07", "membran-om"), ("OM_08", "membran-om"), ("OM_09", "membran-om"),
        ("OM_10", "membran-om"), ("OM_11", "membran-om"), ("OM_12", "membran-om"),
        ("OM_13", "membran-om"), ("OM_14", "membran-om"), ("OM_15", "membran-om"),
        ("OM_16", "membran-om"), ("OM_17", "membran-om"), ("OM_18", "membran-om"),
        ("OM_19", "membran-om"), ("OM_20", "membran-om"), ("OM_21", "membran-om"),
        ("OM_22", "membran-om"), ("OM_23", "membran-om"), ("OM_24", "membran-om"),
        ("OM_25", "membran-om"), ("OM_26", "membran-om"),

        // === Lake OL (9) ===
        ("OL_501", "lake-ol"), ("OL_502", "lake-ol"), ("OL_503", "lake-ol"),
        ("OL_504", "lake-ol"), ("OL_505", "lake-ol"), ("OL_506", "lake-ol"),
        ("OL_507", "lake-ol"), ("OL_508", "lake-ol"), ("OL_509", "lake-ol"),

        // === Laminant (1) ===
        ("Laminant_Yuzey", "laminant"),

        // === Panel Kapi (13) ===
        ("Amerikan_Panel", "panel-kapi"),
        ("ANATOLIA",       "panel-kapi"),
        ("ASOS",           "panel-kapi"),
        ("ASPENDOS",       "panel-kapi"),
        ("EFES",           "panel-kapi"),
        ("HITIT_Panel",    "panel-kapi"),
        ("OLIMPOS",        "panel-kapi"),
        ("PATRAS_Panel",   "panel-kapi"),
        ("PATRAS_Panel_2", "panel-kapi"),
        ("PERGE",          "panel-kapi"),
        ("SARA",           "panel-kapi"),
        ("SUMELA",         "panel-kapi"),
        ("TURUVA",         "panel-kapi"),

        // === Bilesenler (2) ===
        ("Kasa",   "bilesenler"),
        ("Pervaz", "bilesenler"),

        // === Diger (1) ===
        ("PVC_Lake_Yuzey", "diger"),
    };

    /// <summary>UrunYerellestirme verileri (sadece TR, Gobekli Melamin).</summary>
    private static readonly List<(string Model, string Baslik, string KisaAciklama)> YerellestirmeVerileri = new()
    {
        ("HISAR",    "Yaşam Alanınızdaki Güçlü İmza: Hisar",
                     "İsmini dayanıklılık ve asaletten alan Hisar, dikey ve yatay çizgilerin kusursuz dengesini sunuyor."),
        ("GALA",     "Yaşam Alanınızdaki Asil Dokunuş: Gala",
                     "Göz alıcı detayları ve zarif çizgileriyle mekanlarınıza prestij katar."),
        ("MATRIX",   "Geometrinin Estetik Dokunuşu",
                     "Sıradanlıktan uzak, asimetrik hatların mükemmel dengesi. Yalın beyazın zarafetini modern bir tasarım diliyle buluşturan bu model, yaşam alanlarınıza sadece bir kapı değil, bir sanat eseri kazandırıyor."),
        ("YILDIZ",   "Modern Çizgilerin Işıltısı",
                     "Sıradanlıktan uzak, asimetrik hatların mükemmel dengesi. Yalın beyazın zarafetini modern bir tasarım diliyle buluşturuyor."),
        ("SAHRA",    "Modern Çizgiler, Zarif Detaylar",
                     "Yalın beyazın ferahlığını, dikey antrasit/ahşap şeridin sofistike dokunuşuyla birleştiriyoruz."),
        ("VIZYON",   "Zarafetin En Yalın Hali",
                     "Modern minimalist çizgilerin klasik panel tasarımıyla buluştuğu bu model, mekanlarınıza ferahlık ve dinginlik katar."),
        ("PATRAS",   "Modern Çizgiler, Zarif Detaylar",
                     "Yalın beyazın ferahlığını, dikey antrasit/ahşap şeridin sofistike dokunuşuyla birleştiriyoruz."),
        ("NOVA",     "Yüksek Standart, Kusursuz Detay",
                     "Dayanıklı iç yapısı ve kaliteli lake yüzeyi ile uzun ömürlü kullanım sunar."),
        ("DURU",     "Yüksek Standart, Kusursuz Detay",
                     "Dayanıklı iç yapısı ve kaliteli lake yüzeyi ile uzun ömürlü kullanım sunar."),
        ("IRMAK",    "Zarafetin En Yalın Hali",
                     "Modern minimalist çizgilerin klasik panel tasarımıyla buluştuğu bu model, mekanlarınıza ferahlık katar."),
        ("MILAS",    "Geometrinin Estetik Dokunuşu",
                     "Sıradanlıktan uzak, asimetrik hatların mükemmel dengesi."),
        ("ISTANBUL", "Zarafetin En Yalın Hali",
                     "Modern minimalist çizgilerin klasik panel tasarımıyla buluştuğu bu model, mekanlarınıza ferahlık katar."),
        ("ULUBEY",   "Geometrinin Estetik Dokunuşu",
                     "Sıradanlıktan uzak, asimetrik hatların mükemmel dengesi. Beyaz ve Kumtaşı renk seçenekleriyle."),
        ("SEDEF",    "Yüksek Standart, Kusursuz Detay",
                     "Dayanıklı iç yapısı ve kaliteli lake yüzeyi ile uzun ömürlü kullanım sunar."),
        ("INCI",     "Geometrinin Estetik Dokunuşu",
                     "Sıradanlıktan uzak, asimetrik hatların mükemmel dengesi."),
        ("LENTO",    "Geometrinin Estetik Dokunuşu",
                     "Sıradanlıktan uzak, asimetrik hatların mükemmel dengesi."),
        ("ROYAL",    "Modern Çizgiler, Zarif Detaylar",
                     "Yalın beyazın ferahlığını, dikey antrasit/ahşap şeridin sofistike dokunuşuyla birleştiriyoruz."),
        ("HITIT",    "Geçmişin İzleri, Geleceğin Çizgileri",
                     "Geleneksel motiflerin modern minimalizmle buluştuğu Hitit modeli, mekanlarınıza hem karakteristik bir derinlik hem de ferah bir şıklık katar."),
        ("SAFIR",    "Yüksek Standart, Kusursuz Detay",
                     "Dayanıklı iç yapısı ve kaliteli lake yüzeyi ile uzun ömürlü kullanım sunar."),
        ("ULUDAG",   "Geleneksel Çizgilerin Modern Yorumu",
                     "Klasik panel tasarımını, beyazın zamansız zarafetiyle buluşturan Uludağ modeli, mekanlarınıza ferahlık ve derinlik katar."),
        ("AVANGARDE","Geçmişin İzleri, Geleceğin Çizgileri",
                     "Geleneksel motiflerin modern minimalizmle buluştuğu bu model, mekanlarınıza karakteristik bir derinlik katar."),
        ("TEBRIZ",   "Zarafetin Yeni Adı: Tebriz",
                     "Geleneksel çıta işçiliğini modern çizgilerle buluşturan Tebriz, yüksek kaliteli yüzey kaplaması ve estetik detaylarıyla evinizin girişinde prestijli bir karşılama yaratır."),
        ("FINIKE",   "Doğanın Klasik Çizgilerle Buluşması",
                     "Yaşam alanlarınıza sıcaklık katan Finike modeli, meşe dokusunun doğal güzelliğini modern geometrik hatlarla birleştiriyor."),
        ("DAMLA",    "Yaşam Alanlarınıza Modern Bir Dokunuş",
                     "Sadelik ve estetiğin mükemmel dengesi. Geometrik dairesel formlar ve net çizgileriyle mekanlarınıza derinlik katar."),
        ("KALE",     "Modern Çizgiler, Klasik Zarafet",
                     "Yaşam alanlarınıza sadeliğin gücünü taşıyan KALE serisi, yatay panel detayları ve pürüzsüz beyaz yüzeyiyle modern iç mimarinin vazgeçilmezi."),
        ("RUSTIK",   "Geometrinin Estetik Dokunuşu",
                     "Sıradanlıktan uzak, asimetrik hatların mükemmel dengesi."),
        ("TUGRA",    "Zarafetin Yeni Adı: Tuğra",
                     "Geleneksel çıta işçiliğini modern çizgilerle buluşturan Tuğra, kapıdan daha fazlasını sunar."),
    };

    /// <summary>Urun kategorileri (Ad, Slug, SiraNo).</summary>
    private static readonly List<(string Ad, string Slug, int SiraNo)> KategoriVerileri = new()
    {
        ("Göbekli Melamin", "gobekli-melamin", 1),
        ("Düz Melamin",     "duz-melamin",     2),
        ("Membran OM",      "membran-om",      3),
        ("Lake OL",         "lake-ol",         4),
        ("Laminant",        "laminant",        5),
        ("Panel Kapı",      "panel-kapi",      6),
        ("Bileşenler",      "bilesenler",      7),
        ("Diğer",           "diger",           8),
    };

    #endregion

    #region Yardimci Metotlar

    /// <summary>Model adindan Turkce gosterim adi uretir.</summary>
    private static string ModeldenGorunenAd(string model)
    {
        // Ozel durumlar: ASCII model adi → dogru Turkce gosterim
        var ozelDurumlar = new Dictionary<string, string>
        {
            ["ISTANBUL"]      = "İstanbul",
            ["ULUDAG"]        = "Uludağ",
            ["TUGRA"]         = "Tuğra",
            ["YENI_MESE"]     = "Yeni Meşe",
            ["BUZ_MESE"]      = "Buz Meşe",
            ["TEBRIZ"]        = "Tebriz",
            ["FINIKE"]        = "Finike",
            ["SAHRA"]         = "Sahra",
            ["SEDEF"]         = "Sedef",
            ["HITIT"]         = "Hitit",
            ["HITIT_Panel"]   = "Hitit Panel",
            ["Amerikan_Panel"]= "Amerikan Panel",
            ["Laminant_Yuzey"]= "Laminant Yüzey",
            ["PVC_Lake_Yuzey"]= "PVC Lake Yüzey",
        };

        if (ozelDurumlar.TryGetValue(model, out var ozelAd))
            return ozelAd;

        // Varsayilan: _ → bosluk, her kelime ilk harf buyuk (invariant)
        var parcalar = model.Split('_');
        var sonuc = new List<string>(parcalar.Length);
        foreach (var p in parcalar)
        {
            if (p.Length == 0) continue;
            sonuc.Add(char.ToUpperInvariant(p[0]) + p[1..].ToLowerInvariant());
        }
        return string.Join(" ", sonuc);
    }

    /// <summary>Model adindan ASCII slug uretir.</summary>
    private static string ModeldenSlug(string model)
    {
        return model.ToLowerInvariant().Replace('_', '-');
    }

    /// <summary>Model adindan UPPER kod uretir.</summary>
    private static string ModeldenKod(string model)
    {
        return model.ToUpperInvariant();
    }

    #endregion

    #region Asama Metotlari

    /// <summary>Katalog verisini tohumlar (kategoriler, urunler, yerellestirme, ayarlar, sayfa icerigi).</summary>
    public static async Task KatalogVerisiniTohumlaAsync(VizitLink3DDbContext vt)
    {
        var firma = await vt.Firmalar.FirstOrDefaultAsync();
        if (firma == null) return;

        var simdi = DateTime.UtcNow;

        // 1. UrunAilesi
        var urunAilesiId = await UrunAilesiniTohumlaAsync(vt, simdi);

        // 2. Kategoriler
        var kategoriHaritasi = await KategorileriTohumlaAsync(vt, simdi);

        // 3. Urunler
        var urunHaritasi = await UrunleriTohumlaAsync(vt, firma.Id, urunAilesiId, kategoriHaritasi, simdi);

        // 4. UrunYerellestirme (TR)
        await UrunYerellestirmeleriniTohumlaAsync(vt, urunHaritasi);

        // 5. SistemAyarlari guncelle/ekle
        await SistemAyarlariniGuncelleAsync(vt);

        // 6. SayfaIcerigi (Hakkimizda)
        await SayfaIceriginiTohumlaAsync(vt, firma.Id);

        // 7. UrunMedya (gorseller)
        await UrunMedyalariniTohumlaAsync(vt);
    }

    private static async Task<int> UrunAilesiniTohumlaAsync(VizitLink3DDbContext vt, DateTime simdi)
    {
        var mevcut = await vt.UrunAilesileri.FirstOrDefaultAsync(a => a.Slug == "orpay-kapi-sistemleri");
        if (mevcut != null)
            return mevcut.Id;

        var aile = new UrunAilesi
        {
            Ad = "ORPAY Kapı Sistemleri",
            Slug = "orpay-kapi-sistemleri",
            Aciklama = "ORPAY kapı sistemleri ürün ailesi",
            SiraNo = 1,
            AktifMi = true,
            OlusturulmaTarihi = simdi,
        };
        vt.UrunAilesileri.Add(aile);
        await vt.SaveChangesAsync();
        return aile.Id;
    }

    private static async Task<Dictionary<string, int>> KategorileriTohumlaAsync(VizitLink3DDbContext vt, DateTime simdi)
    {
        var harita = new Dictionary<string, int>();

        foreach (var (ad, slug, siraNo) in KategoriVerileri)
        {
            var mevcut = await vt.UrunKategorileri.FirstOrDefaultAsync(k => k.Slug == slug);
            if (mevcut != null)
            {
                harita[slug] = mevcut.Id;
                continue;
            }

            var kategori = new UrunKategori
            {
                Ad = ad,
                Slug = slug,
                SiraNo = siraNo,
                AktifMi = true,
                OlusturulmaTarihi = simdi,
            };
            vt.UrunKategorileri.Add(kategori);
            await vt.SaveChangesAsync();
            harita[slug] = kategori.Id;
        }

        return harita;
    }

    private static async Task<Dictionary<string, int>> UrunleriTohumlaAsync(
        VizitLink3DDbContext vt, int firmaId, int urunAilesiId,
        Dictionary<string, int> kategoriHaritasi, DateTime simdi)
    {
        var harita = new Dictionary<string, int>();

        foreach (var (model, kategoriSlug) in KatalogModelleri)
        {
            var slug = ModeldenSlug(model);

            var mevcut = await vt.Urunler.FirstOrDefaultAsync(u => u.Slug == slug && u.FirmaId == firmaId);
            if (mevcut != null)
            {
                harita[model] = mevcut.Id;
                continue;
            }

            var urun = new Urun
            {
                Ad = ModeldenGorunenAd(model),
                Slug = slug,
                Kod = ModeldenKod(model),
                UrunAilesiId = urunAilesiId,
                UrunKategoriId = kategoriHaritasi.GetValueOrDefault(kategoriSlug),
                FirmaId = firmaId,
                AktifMi = true,
                OneCikanMi = false,
                YeniMi = false,
                SiraNo = 0,
                OlusturulmaTarihi = simdi,
                KisaAciklama = $"{ModeldenGorunenAd(model)} modeli ORPAY kalitesiyle.",
            };
            vt.Urunler.Add(urun);
            await vt.SaveChangesAsync();
            harita[model] = urun.Id;
        }

        return harita;
    }

    private static async Task UrunYerellestirmeleriniTohumlaAsync(
        VizitLink3DDbContext vt, Dictionary<string, int> urunHaritasi)
    {
        foreach (var (model, baslik, kisaAciklama) in YerellestirmeVerileri)
        {
            if (!urunHaritasi.TryGetValue(model, out var urunId))
                continue;

            var mevcut = await vt.UrunYerellestirmeleri
                .FirstOrDefaultAsync(y => y.UrunId == urunId && y.Dil == "tr");
            if (mevcut != null) continue;

            vt.UrunYerellestirmeleri.Add(new UrunYerellestirme
            {
                UrunId = urunId,
                Dil = "tr",
                Ad = baslik,
                KisaAciklama = kisaAciklama,
                Aciklama = $"<div class=\"orpay-ozellikler\"><h4>Teknik Özellikler</h4><ul><li><strong>Standart En:</strong> 80 - 90 cm</li><li><strong>Standart Boy:</strong> 205 - 210 cm</li><li><strong>Kanat Kalınlığı:</strong> 42 mm</li><li><strong>Kasa Seçenekleri:</strong> 18mm MDF, Sandwich Kasa</li><li><strong>Pervaz:</strong> Geçmeli, ayarlı pervaz sistemi</li></ul><p>{kisaAciklama}</p></div>"
            });
        }
        await vt.SaveChangesAsync();
    }

    private static async Task SistemAyarlariniGuncelleAsync(VizitLink3DDbContext vt)
    {
        var ayarlar = new Dictionary<string, string>
        {
            ["site.baslik"]     = "ORPAY",
            ["site.aciklama"]   = "3D dijital showroom ve urun konfigurator platformu.",
            ["site.telefon"]    = "+90 000 000 00 00",
            ["site.telefon_2"]  = "+90 000 000 00 01",
            ["site.eposta"]     = "bilgi@3dvizitlink.com.tr",
            ["site.adres"]      = "Turkiye",
            ["site.fabrika"]    = "",
            ["site.sehir"]      = "Istanbul",
            ["site.ilce"]       = "",
        };

        foreach (var (anahtar, deger) in ayarlar)
        {
            var mevcut = await vt.SistemAyarlari.FirstOrDefaultAsync(a => a.Anahtar == anahtar);
            if (mevcut != null)
            {
                mevcut.Deger = deger;
                mevcut.GuncellenmeTarihi = DateTime.UtcNow;
            }
            else
            {
                vt.SistemAyarlari.Add(new SistemAyari
                {
                    Anahtar = anahtar,
                    Deger = deger,
                    
                });
            }
        }
        await vt.SaveChangesAsync();
    }

    private static async Task SayfaIceriginiTohumlaAsync(VizitLink3DDbContext vt, int firmaId)
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
    }

    private static async Task UrunMedyalariniTohumlaAsync(VizitLink3DDbContext vt)
    {
        if (await vt.UrunMedyalari.AnyAsync()) return; // zaten varsa atla

        var tumUrunler = await vt.Urunler.ToListAsync();
        var medyaKoku = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "medya", "urunler");
        var urunMedyalar = new List<UrunMedya>();

        foreach (var urun in tumUrunler)
        {
            // Kategori slug'ini bul
            var kategori = await vt.UrunKategorileri.FirstOrDefaultAsync(k => k.Id == urun.UrunKategoriId);
            if (kategori == null) continue;

            var kategoriSlug = kategori.Slug;
            var modelKlasor = urun.Kod; // AVANGARDE, OM_01 gibi

            var klasorYolu = Path.Combine(medyaKoku, kategoriSlug, modelKlasor);
            if (!Directory.Exists(klasorYolu))
            {
                continue;
            }

            var dosyalar = Directory.GetFiles(klasorYolu, "*.jpg")
                .OrderBy(f => f)
                .ToList();

            for (int i = 0; i < dosyalar.Count; i++)
            {
                var dosyaAdi = Path.GetFileName(dosyalar[i]);
                var medyaUrl = $"/medya/urunler/{kategoriSlug}/{modelKlasor}/{dosyaAdi}";

                var medya = new UrunMedya
                {
                    UrunId = urun.Id,
                    MedyaUrl = medyaUrl,
                    MedyaTuru = "Resim",
                    SiraNo = i + 1,
                    AnaGosterim = (i == 0),
                    
                };
                urunMedyalar.Add(medya);
            }
        }

        if (urunMedyalar.Any())
        {
            vt.UrunMedyalari.AddRange(urunMedyalar);
            await vt.SaveChangesAsync();
        }
    }

    #endregion
}
