using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Models;

public class RenkDto
{
    public string Ad { get; set; } = string.Empty;
    public string HexKod { get; set; } = "#FFFFFF";
    public string? Grup { get; set; }
    public string? UreticiKodu { get; set; }

    /// <summary>
    /// VizitLink3D standart renk paletini döndürür.
    /// DRY kuralı gereği tek kaynak — KapakDetay ve TohumVerisi bu metodu kullanır.
    /// Veritabanında renk tanımı yoksa fallback olarak devreye girer.
    /// </summary>
    public static List<RenkDto> VarsayilanPalet() =>
    [
        new() { Ad = "Kırık Beyaz",    HexKod = "#F5F2ED", Grup = "Beyazlar",      UreticiKodu = "W-001" },
        new() { Ad = "Parlak Beyaz",   HexKod = "#FFFFFF", Grup = "Beyazlar",      UreticiKodu = "W-002" },
        new() { Ad = "Krem",           HexKod = "#E8DCC8", Grup = "Beyazlar",      UreticiKodu = "W-003" },
        new() { Ad = "Açık Gri",       HexKod = "#D4D4D4", Grup = "Gri Tonları",   UreticiKodu = "G-001" },
        new() { Ad = "Orta Gri",       HexKod = "#9A9A9A", Grup = "Gri Tonları",   UreticiKodu = "G-002" },
        new() { Ad = "Antrasit",       HexKod = "#3D3D3D", Grup = "Gri Tonları",   UreticiKodu = "G-003" },
        new() { Ad = "Mat Siyah",      HexKod = "#1A1A1A", Grup = "Koyu Renkler",  UreticiKodu = "D-001" },
        new() { Ad = "Koyu Lacivert",  HexKod = "#1C2B4A", Grup = "Koyu Renkler",  UreticiKodu = "D-002" },
        new() { Ad = "Petrol Yeşili",  HexKod = "#2D5A4A", Grup = "Doğal Renkler", UreticiKodu = "N-001" },
        new() { Ad = "Sage Yeşili",    HexKod = "#7D9B76", Grup = "Doğal Renkler", UreticiKodu = "N-002" },
        new() { Ad = "Toprak Kırmızı", HexKod = "#8B4543", Grup = "Sıcak Renkler", UreticiKodu = "H-001" },
        new() { Ad = "Bal Sarısı",     HexKod = "#C8952A", Grup = "Sıcak Renkler", UreticiKodu = "H-002" },
    ];
}

public class KapakModeliDto
{
    public int Id { get; set; }
    public string ModelAdi { get; set; } = string.Empty;
    public string ModelKodu { get; set; } = string.Empty;
    public string Kategori { get; set; } = string.Empty;
    public string ModelTuru { get; set; } = "Kapak";
    public string KategoriSlug { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string RenkAdi { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string OnYazi { get; set; } = string.Empty;
    public string Aciklama { get; set; } = string.Empty;
    public string AnaGorselUrl { get; set; } = string.Empty;
    public string GorselYolu { get; set; } = string.Empty;
    public string ResimUrl { get; set; } = string.Empty;
    public bool YeniMi { get; set; }
    public bool OneCikanMi { get; set; }
    public List<string> UygulamaGorselleri { get; set; } = [];
    public decimal? Fiyat { get; set; }
    public string? ModelDosyaYolu { get; set; }
    public List<RalRengi> RenkSecenekleri { get; set; } = [];
    public int? MinYukseklik { get; set; }
    public int? MaxYukseklik { get; set; }
    public int? MinGenislik { get; set; }
    public int? MaxGenislik { get; set; }
    public List<string> Nitelikler { get; set; } = [];
    public int SiraNo { get; set; }

    public static async Task<KapakModeliDto> UrunuKapakDtoyaDonustur(
        Urun u, 
        ApiIstemcisi api, 
        Dictionary<int, UrunKategori> kategoriler)
    {
        var dto = new KapakModeliDto
        {
            Id = u.Id,
            ModelAdi = u.Ad,
            ModelKodu = u.Kod,
            Kategori = u.UrunKategoriId.HasValue && kategoriler.TryGetValue(u.UrunKategoriId.Value, out var kat) ? kat.Ad : "Standart",
            KategoriSlug = u.UrunKategoriId.HasValue && kategoriler.TryGetValue(u.UrunKategoriId.Value, out var kat2) ? kat2.Slug : "standart",
            ModelTuru = u.UrunAilesiId == 2 ? "Kapi" : "Kapak",
            Slug = u.Slug,
            OnYazi = u.KisaAciklama ?? string.Empty,
            Aciklama = u.Aciklama ?? string.Empty,
            AnaGorselUrl = UrunGorunumYardimcisi.AnaGorselUrl(u, api.ApiBaseUrl),
            ResimUrl = UrunGorunumYardimcisi.AnaGorselUrl(u, api.ApiBaseUrl),
            GorselYolu = UrunGorunumYardimcisi.AnaGorselUrl(u, api.ApiBaseUrl),
            Url = u.UrunAilesiId == 2 ? $"/kapi/{u.Id}/{u.Kod}" : $"/kapak/{u.Id}/{u.Kod}",
            YeniMi = u.YeniMi,
            OneCikanMi = u.OneCikanMi,
            Fiyat = u.Fiyat ?? 0,
            Nitelikler = !string.IsNullOrWhiteSpace(u.KisaAciklama) ? [u.KisaAciklama] : []
        };

        // 3D model yükleme
        try
        {
            var modeller = await api.GetAsync<List<UrunUcBoyutModeli>>($"api/urunler/{u.Id}/uc-boyut-modelleri");
            if (modeller != null && modeller.Any())
            {
                var model = modeller.FirstOrDefault(m => m.VarsayilanMi) ?? modeller.First();
                var yol = string.IsNullOrWhiteSpace(model.ModelYolu) ? model.ModelDosyaYolu : model.ModelYolu;
                dto.ModelDosyaYolu = yol;
            }
        }
        catch { }

        // Renk seçenekleri
        try
        {
            var renkler = await api.GetAsync<List<RalRengi>>($"api/urunler/{u.Id}/renkler");
            if (renkler != null) dto.RenkSecenekleri = renkler;
        }
        catch { }

        // Uygulama görselleri (Galeri)
        try
        {
            var galeri = await api.GetAsync<List<UrunMedya>>($"api/urunler/{u.Id}/medyalar");
            if (galeri != null)
            {
                dto.UygulamaGorselleri = galeri
                    .Where(m => m.MedyaTuru == "Gorsel")
                    .Select(m => m.MedyaUrl.StartsWith('/') ? $"{api.ApiBaseUrl}{m.MedyaUrl}" : m.MedyaUrl)
                    .ToList();
            }
        }
        catch { }

        return dto;
    }
}

/// <summary>
/// Katalog renk adlarını (Siyah, Bej, Kahve...) swatch için hex koda çevirir.
/// OrpayKatalogUrunleri statik kataloğundaki tüm renk adlarını kapsar.
/// </summary>
public static class orpayKatalogRenkPaleti
{
    private static readonly Dictionary<string, string> AdHex = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Ahşap"] = "#8B5A2B",
        ["Amerikan Ceviz"] = "#5C4033",
        ["Antik Ceviz"] = "#4A3728",
        ["Antrasit"] = "#383838",
        ["Bej"] = "#D9C7A3",
        ["Beyaz"] = "#FFFFFF",
        ["Gri"] = "#9A9A9A",
        ["Hareli Meşe"] = "#A67C52",
        ["Kahve"] = "#5C4033",
        ["Kahve Rengi"] = "#6F4E37",
        ["Krem"] = "#F0E6D2",
        ["Kum Beji"] = "#E3D0B5",
        ["Mocha"] = "#6F4E37",
        ["Siyah"] = "#1A1A1A",
        ["Spesiyal Gri"] = "#6B6B6B",
        ["Turuncu"] = "#D2691E",
        ["Yeşil"] = "#4A7C59",
    };

    public static string HexBul(string renkAdi) =>
        AdHex.TryGetValue(renkAdi, out var hex) ? hex : "#B0A99A";
}

public static class UrunGorunumYardimcisi
{
    public static string AnaGorselUrl(Urun urun, string apiBaseUrl)
    {
        if (urun.AnaGorselMedyaId is long medyaId and > 0)
            return $"{apiBaseUrl.TrimEnd('/')}/api/medya/dosya/{medyaId}";

        var katalogVerisi = KatalogVerisiBul(urun);
        if (katalogVerisi is not null)
            return katalogVerisi.HeroGorselUrl;

        return "/medya/vizitlink3d_default.png";
    }

    /// <summary>
    /// Ürünün Slug'ına göre statik ORPAY katalog kaydını bulur (varsa).
    /// Renkler/Ölçüler/KoleksiyonGrubu gibi DB'ye taşınmamış ama katalogda
    /// zaten hazır olan görüntüleme verisi için kullanılır.
    /// </summary>
    public static OrpayKatalogUrunu? KatalogVerisiBul(Urun urun) =>
        OrpayKatalogUrunleri.Tum.FirstOrDefault(k => k.Slug.Equals(urun.Slug, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Koleksiyon grubunu (Exclusive/Premium/Trend/Standart) statik katalogdan bulur.
    /// Katalogda kayıtlı değilse "Genel" döner.
    /// </summary>
    public static string KoleksiyonGrubuBul(Urun urun) =>
        KatalogVerisiBul(urun)?.KoleksiyonGrubu ?? "Genel";

    public static string KoleksiyonAdiBul(Urun urun, IReadOnlyDictionary<int, UrunAilesi> aileler)
    {
        if (aileler.TryGetValue(urun.UrunAilesiId, out var aile) && !string.IsNullOrWhiteSpace(aile.Ad))
            return aile.Ad;

        return urun.UrunKategoriId.HasValue ? "Koleksiyon" : "Ürün";
    }

    public static string KategoriAdiBul(Urun urun, IReadOnlyDictionary<int, UrunKategori> kategoriler)
    {
        if (urun.UrunKategoriId.HasValue &&
            kategoriler.TryGetValue(urun.UrunKategoriId.Value, out var kategori) &&
            !string.IsNullOrWhiteSpace(kategori.Ad))
        {
            return kategori.Ad;
        }

        return "Genel";
    }

    public static string OzetMetniBul(Urun urun)
    {
        if (!string.IsNullOrWhiteSpace(urun.KisaAciklama))
            return urun.KisaAciklama!;

        if (!string.IsNullOrWhiteSpace(urun.Aciklama))
            return urun.Aciklama!;

        return urun.Ad;
    }
}

public class HeroSliderOgesi
{
    public string GorselUrl { get; set; } = string.Empty;
    public string Etiket { get; set; } = string.Empty;
    public string Baslik1 { get; set; } = string.Empty;
    public string Baslik2 { get; set; } = string.Empty;
    public string Aciklama { get; set; } = string.Empty;
}


public class GirisYaniti
{
    public string Token { get; set; } = string.Empty;
    public string KullaniciAdi { get; set; } = string.Empty;
    public string? AdSoyad { get; set; }
}

public class DashboardOzetDto
{
    public int ToplamModel { get; set; }
    public int ToplamKategori { get; set; }
    public int OneCikanSayisi { get; set; }
    public int YeniModelSayisi { get; set; }
    public int ToplamSlayt { get; set; }
    public int ToplamSSS { get; set; }
    public int ToplamCeviri { get; set; }
    public int DesteklenenDil { get; set; }
    public int BekleyenMesajSayisi { get; set; }
    public int ToplamMesaj { get; set; }
    public List<KategoriDagilimDto>? KategoriDagilimi { get; set; }
    public List<KapakModeliDto>? SonEklenenler { get; set; }
    public List<SonMesajDto>? SonMesajlar { get; set; }
    public List<double>? SatisVerileri { get; set; }
    public List<string>? SatisAylar { get; set; }
}

public class SonMesajDto
{
    public string AdSoyad { get; set; } = string.Empty;
    public string Eposta { get; set; } = string.Empty;
    public string Konu { get; set; } = string.Empty;
    public DateTime Tarih { get; set; }
    public bool OkunduMu { get; set; }
}

public class KategoriDagilimDto
{
    public string Kategori { get; set; } = string.Empty;
    public int Adet { get; set; }
}

/// <summary>
/// IletisimMesajiDto — İletişim formundan gelen mesajların admin panelinde
/// görüntülenmesi için kullanılan veri transfer nesnesidir.
/// </summary>
public class IletisimMesajiDto
{
    public int Id { get; set; }
    public string AdSoyad { get; set; } = string.Empty;
    public string Eposta { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
    public string Konu { get; set; } = string.Empty;
    public string Mesaj { get; set; } = string.Empty;
    public DateTime Tarih { get; set; }
    public bool OkunduMu { get; set; }
}

/// <summary>
/// MesajListesiDto — Admin panelinde mesaj listesi API çağrısının
/// sayfalanmış cevabını taşıyan sarmalayıcı model.
/// </summary>
public class MesajListesiDto
{
    public int Toplam { get; set; }
    public List<IletisimMesajiDto> Mesajlar { get; set; } = [];
}

public class GaleriGorseliDto
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Baslik { get; set; }
    public string? AltMetin { get; set; }
    public int Sira { get; set; }
    public bool AktifMi { get; set; } = true;
    public DateTime OlusturulmaTarihi { get; set; }
}

public class SayfaDuzenAyariDto
{
    public int Id { get; set; }
    public string SayfaKodu { get; set; } = string.Empty;
    public string SayfaAdi { get; set; } = string.Empty;
    public int SutunAdet { get; set; } = 4;
    public int SatirAdet { get; set; } = 3;
    public int SayfaBasinaAdet { get; set; } = 12;
    public bool SayfalamaAktif { get; set; } = true;
    public bool AktifMi { get; set; } = true;
}

