using System.Globalization;
using System.Net;
using System.Text;

namespace VizitLink3D.Ortak.Modeller.Urunler;

public sealed class OrpayKatalogUrunu
{
    private static readonly CultureInfo TrKultur = new("tr-TR");

    public required string Slug { get; init; }
    public required string Ad { get; init; }
    public required string Kod { get; init; }
    public required string KoleksiyonGrubu { get; init; }
    public required int SayfaNo { get; init; }
    public int[] EkGaleriSayfalari { get; init; } = [];
    public required decimal Fiyat { get; init; }
    public decimal? BoyDolabiFiyati { get; init; }
    public required string[] Renkler { get; init; }
    public required string[] Ozellikler { get; init; }
    public required OrpayKatalogOlcusu[] Olculer { get; init; }
    public bool OneCikanMi { get; init; } = true;
    public bool YeniMi { get; init; } = true;

    public IReadOnlyList<int> TumSayfalar => [SayfaNo, .. EkGaleriSayfalari];
    public string HeroGorselUrl => $"/medya/orpay-katalog/sayfa-{SayfaNo:000}-hero.{OrpayKatalogUzantiGetir($"sayfa-{SayfaNo:000}-hero")}";
    public string KatalogGorselUrl => $"/medya/orpay-katalog/sayfa-{SayfaNo:000}-spread.{OrpayKatalogUzantiGetir($"sayfa-{SayfaNo:000}-spread")}";
    public string TeknikGorselUrl => $"/medya/orpay-katalog/sayfa-{SayfaNo:000}-detay.{OrpayKatalogUzantiGetir($"sayfa-{SayfaNo:000}-detay")}";

    /// <summary>
    /// Orpay katalog gorselleri kismen buyuk PNG'lerden webp'e donusturuldu (kucuk
    /// olanlar PNG olarak kaldi). Bu yuzden uzanti sabit degil; API ve UI wwwroot'unda
    /// gercekte hangi dosya varsa o kullanilir (self-healing).
    /// </summary>
    private static string OrpayKatalogUzantiGetir(string dosyaAdiUzantisiz)
    {
        var kok = OrpayKatalogMedyaKokunuBul();
        if (kok != null)
        {
            if (File.Exists(Path.Combine(kok, dosyaAdiUzantisiz + ".webp")))
                return "webp";

            if (File.Exists(Path.Combine(kok, dosyaAdiUzantisiz + ".svg")))
                return "svg";
        }

        return "png";
    }

    private static string? OrpayKatalogMedyaKokunuBul()
    {
        var adaylar = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "wwwroot", "medya", "orpay-katalog"),
            Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "medya", "orpay-katalog"),
            Path.Combine(Directory.GetCurrentDirectory(), "VizitLink3D.Api", "wwwroot", "medya", "orpay-katalog"),
            Path.Combine(Directory.GetCurrentDirectory(), "VizitLink3D.UI", "wwwroot", "medya", "orpay-katalog"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "VizitLink3D.Api", "wwwroot", "medya", "orpay-katalog"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "VizitLink3D.UI", "wwwroot", "medya", "orpay-katalog")
        };

        return adaylar.FirstOrDefault(Directory.Exists);
    }

    public string KisaAciklamaOlustur()
    {
        var ilkOzellikler = string.Join(", ", Ozellikler.Take(3));
        return $"{KoleksiyonGrubu} koleksiyonundaki {Ad}; {ilkOzellikler.ToLowerInvariant()} ve {Renkler.Length} renk seçeneğiyle sunulur.";
    }

    public string AciklamaHtmlOlustur()
    {
        var olcuHtml = new StringBuilder();
        foreach (var olcu in Olculer)
        {
            olcuHtml.Append("<div class=\"gb-olcu-kart\">");
            olcuHtml.Append($"<h4>{Kodla(olcu.Baslik)}</h4>");
            olcuHtml.Append("<ul>");
            olcuHtml.Append($"<li>Yükseklik: {Kodla(olcu.Yukseklik)}</li>");
            olcuHtml.Append($"<li>Genişlik: {Kodla(olcu.Genislik)}</li>");
            olcuHtml.Append($"<li>Derinlik: {Kodla(olcu.Derinlik)}</li>");
            olcuHtml.Append("</ul></div>");
        }

        var ozellikHtml = string.Join(string.Empty, Ozellikler.Select(o => $"<li>{Kodla(o)}</li>"));
        var renkHtml = string.Join(string.Empty, Renkler.Select(r => $"<li>{Kodla(r)}</li>"));
        var tumFiyatlar = BoyDolabiFiyati.HasValue
            ? $"<p><strong>Liste Fiyatı:</strong> {FiyatYaz(Fiyat)}<br /><strong>Boy Dolabı:</strong> {FiyatYaz(BoyDolabiFiyati.Value)}</p>"
            : $"<p><strong>Liste Fiyatı:</strong> {FiyatYaz(Fiyat)}</p>";

        return $"""
            <div class="gb-katalog-icerik">
              <p class="gb-katalog-rozet">{Kodla(KoleksiyonGrubu)} Koleksiyonu</p>
              <h2>{Kodla(Ad)}</h2>
              <p>{Kodla(Ad)}, Orpay 2026 katalog sayfa kompozisyonu ile birebir beslenen; koleksiyon renkleri, teknik ölçüler ve fiyat etiketi korunmuş özel bir ürün sayfasıdır.</p>
              {tumFiyatlar}
              <h3>Özellikler</h3>
              <ul>{ozellikHtml}</ul>
              <h3>Renkler</h3>
              <ul>{renkHtml}</ul>
              <h3>Teknik Ölçüler</h3>
              <div class="gb-olcu-grid">{olcuHtml}</div>
            </div>
            """;
    }

    private static string Kodla(string deger) => WebUtility.HtmlEncode(deger);

    private static string FiyatYaz(decimal fiyat) => fiyat.ToString("N0", TrKultur) + " TL";
}

public sealed class OrpayKatalogOlcusu
{
    public required string Baslik { get; init; }
    public required string Yukseklik { get; init; }
    public required string Genislik { get; init; }
    public required string Derinlik { get; init; }
}

public static class OrpayKatalogUrunleri
{
    private static readonly Dictionary<string, int> KoleksiyonSiralari = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Exclusive"] = 1,
        ["Premium"] = 2,
        ["Trend"] = 3,
        ["Standart"] = 4,
        ["Diger"] = 5
    };

    private static readonly Dictionary<string, string> KoleksiyonAciklamalari = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Exclusive"] = "İmza koleksiyonları. Geniş modül yapısı, yüksek yüzey kalitesi ve katalog sayfasındaki premium sunum ile öne çıkar.",
        ["Premium"] = "Zengin malzeme ve renk kombinasyonlarıyla showroom etkisi veren seçkin ürün grubu.",
        ["Trend"] = "Çağdaş banyolar için dengeli fiyat, güçlü malzeme ve sıcak renk seçimleri sunan seri.",
        ["Standart"] = "Günlük kullanıma uygun, ölçü dengesi ve fiyat avantajı yüksek Orpay ürünleri.",
        ["Diger"] = "Özel içerik olarak eklenmiş ürünler."
    };

    public static int KoleksiyonSirasiBul(string koleksiyonGrubu) =>
        KoleksiyonSiralari.TryGetValue(koleksiyonGrubu, out var sira) ? sira : int.MaxValue;

    public static string KoleksiyonAciklamasiBul(string koleksiyonGrubu) =>
        KoleksiyonAciklamalari.TryGetValue(koleksiyonGrubu, out var aciklama) ? aciklama : string.Empty;

    public static IReadOnlyList<OrpayKatalogUrunu> Tum { get; } =
    [
        new()
        {
            Slug = "hermes-120",
            Ad = "Hermes 120",
            Kod = "HERMES-120",
            KoleksiyonGrubu = "Exclusive",
            SayfaNo = 6,
            Fiyat = 122500m,
            BoyDolabiFiyati = 60000m,
            Renkler = ["Siyah", "Bej", "Kahve", "Krem"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Stone Lavabo"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "120 cm", Derinlik = "50 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "75 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "160 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "hermes-80",
            Ad = "Hermes 80",
            Kod = "HERMES-80",
            KoleksiyonGrubu = "Exclusive",
            SayfaNo = 7,
            Fiyat = 102000m,
            BoyDolabiFiyati = 60000m,
            Renkler = ["Siyah", "Bej", "Kahve", "Krem"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Stone Lavabo"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "50 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "75 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "160 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "bottega-120",
            Ad = "Bottega 120",
            Kod = "BOTTEGA-120",
            KoleksiyonGrubu = "Exclusive",
            SayfaNo = 8,
            Fiyat = 117500m,
            BoyDolabiFiyati = 60000m,
            Renkler = ["Beyaz", "Siyah"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Stone Lavabo"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "120 cm", Derinlik = "50 cm" },
                new() { Baslik = "Ayna", Yukseklik = "90 cm", Genislik = "110 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "bottega-100",
            Ad = "Bottega 100",
            Kod = "BOTTEGA-100",
            KoleksiyonGrubu = "Exclusive",
            SayfaNo = 9,
            Fiyat = 110000m,
            BoyDolabiFiyati = 60000m,
            Renkler = ["Beyaz", "Siyah"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Stone Lavabo"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "100 cm", Derinlik = "50 cm" },
                new() { Baslik = "Ayna", Yukseklik = "90 cm", Genislik = "100 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "bottega-80",
            Ad = "Bottega 80",
            Kod = "BOTTEGA-80",
            KoleksiyonGrubu = "Exclusive",
            SayfaNo = 10,
            Fiyat = 102000m,
            Renkler = ["Beyaz", "Siyah"],
            BoyDolabiFiyati = null,
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Stone Lavabo"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "50 cm" },
                new() { Baslik = "Ayna", Yukseklik = "90 cm", Genislik = "80 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "giorgio-120",
            Ad = "Giorgio 120",
            Kod = "GIORGIO-120",
            KoleksiyonGrubu = "Exclusive",
            SayfaNo = 12,
            Fiyat = 117500m,
            BoyDolabiFiyati = 60000m,
            Renkler = ["Gri", "Siyah", "Ahşap"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Stone Lavabo"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "120 cm", Derinlik = "50 cm" },
                new() { Baslik = "Ayna", Yukseklik = "90 cm", Genislik = "100 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "giorgio-100",
            Ad = "Giorgio 100",
            Kod = "GIORGIO-100",
            KoleksiyonGrubu = "Exclusive",
            SayfaNo = 13,
            Fiyat = 110000m,
            BoyDolabiFiyati = 60000m,
            Renkler = ["Gri", "Siyah", "Ahşap"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Stone Lavabo"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "100 cm", Derinlik = "50 cm" },
                new() { Baslik = "Ayna", Yukseklik = "90 cm", Genislik = "100 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "giorgio-80",
            Ad = "Giorgio 80",
            Kod = "GIORGIO-80",
            KoleksiyonGrubu = "Exclusive",
            SayfaNo = 14,
            Fiyat = 102000m,
            Renkler = ["Gri", "Siyah", "Ahşap"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Stone Lavabo"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "50 cm" },
                new() { Baslik = "Ayna", Yukseklik = "90 cm", Genislik = "70 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "diago-120",
            Ad = "Diago 120",
            Kod = "DIAGO-120",
            KoleksiyonGrubu = "Exclusive",
            SayfaNo = 16,
            Fiyat = 112500m,
            BoyDolabiFiyati = 55000m,
            Renkler = ["Siyah", "Kahve Rengi"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Cam Lavabo"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "120 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "75 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "diago-100",
            Ad = "Diago 100",
            Kod = "DIAGO-100",
            KoleksiyonGrubu = "Exclusive",
            SayfaNo = 17,
            Fiyat = 99500m,
            Renkler = ["Siyah", "Kahve Rengi"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Cam Lavabo"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "100 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "75 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "capelli-150",
            Ad = "Capelli 150",
            Kod = "CAPELLI-150",
            KoleksiyonGrubu = "Premium",
            SayfaNo = 19,
            Fiyat = 97000m,
            Renkler = ["Mocha", "Gri"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "150 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "75 cm", Derinlik = "5 cm" }
            ]
        },
        new()
        {
            Slug = "capelli-100",
            Ad = "Capelli 100",
            Kod = "CAPELLI-100",
            KoleksiyonGrubu = "Premium",
            SayfaNo = 20,
            Fiyat = 67500m,
            Renkler = ["Mocha", "Gri"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "100 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "75 cm", Derinlik = "5 cm" }
            ]
        },
        new()
        {
            Slug = "hera-120",
            Ad = "Hera 120",
            Kod = "HERA-120",
            KoleksiyonGrubu = "Exclusive",
            SayfaNo = 21,
            EkGaleriSayfalari = [22],
            Fiyat = 110000m,
            Renkler = ["Hareli Meşe", "Antik Ceviz"],
            Ozellikler = ["Soft Kapak", "Doğal Ağaç", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "120 cm", Derinlik = "46,5 cm" },
                new() { Baslik = "Ayna", Yukseklik = "69 cm", Genislik = "120 cm", Derinlik = "5 cm" }
            ]
        },
        new()
        {
            Slug = "hera-80",
            Ad = "Hera 80",
            Kod = "HERA-80",
            KoleksiyonGrubu = "Exclusive",
            SayfaNo = 23,
            Fiyat = 104000m,
            Renkler = ["Hareli Meşe", "Antik Ceviz"],
            Ozellikler = ["Soft Kapak", "Doğal Ağaç", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "46,5 cm" },
                new() { Baslik = "Ayna", Yukseklik = "69 cm", Genislik = "80 cm", Derinlik = "5 cm" }
            ]
        },
        new()
        {
            Slug = "cavalli-110",
            Ad = "Cavalli 110",
            Kod = "CAVALLI-110",
            KoleksiyonGrubu = "Premium",
            SayfaNo = 24,
            EkGaleriSayfalari = [25],
            Fiyat = 72000m,
            BoyDolabiFiyati = 40000m,
            Renkler = ["Antrasit", "Beyaz"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "110 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "90 cm", Genislik = "70 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "45 cm", Derinlik = "32 cm" }
            ]
        },
        new()
        {
            Slug = "cavalli-90",
            Ad = "Cavalli 90",
            Kod = "CAVALLI-90",
            KoleksiyonGrubu = "Trend",
            SayfaNo = 26,
            Fiyat = 66000m,
            Renkler = ["Antrasit", "Beyaz"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "90 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "90 cm", Genislik = "70 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "45 cm", Derinlik = "32 cm" }
            ]
        },
        new()
        {
            Slug = "tiffany-120",
            Ad = "Tiffany 120",
            Kod = "TIFFANY-120",
            KoleksiyonGrubu = "Premium",
            SayfaNo = 28,
            Fiyat = 67500m,
            BoyDolabiFiyati = 30000m,
            Renkler = ["Antrasit", "Beyaz", "Mocha"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "120 cm", Derinlik = "45 cm" },
                new() { Baslik = "Ayna", Yukseklik = "90 cm", Genislik = "70 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "120 cm", Genislik = "36 cm", Derinlik = "36 cm" }
            ]
        },
        new()
        {
            Slug = "tiffany-100",
            Ad = "Tiffany 100",
            Kod = "TIFFANY-100",
            KoleksiyonGrubu = "Trend",
            SayfaNo = 29,
            EkGaleriSayfalari = [30],
            Fiyat = 62000m,
            Renkler = ["Antrasit", "Beyaz", "Mocha"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "100 cm", Derinlik = "45 cm" },
                new() { Baslik = "Ayna", Yukseklik = "90 cm", Genislik = "70 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "120 cm", Genislik = "36 cm", Derinlik = "36 cm" }
            ]
        },
        new()
        {
            Slug = "siena-80",
            Ad = "Siena 80",
            Kod = "SIENA-80",
            KoleksiyonGrubu = "Premium",
            SayfaNo = 31,
            EkGaleriSayfalari = [32],
            Fiyat = 97500m,
            BoyDolabiFiyati = 30000m,
            Renkler = ["Bej", "Gri"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "50 cm" },
                new() { Baslik = "Ayna", Yukseklik = "90 cm", Genislik = "60 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "valentino-100",
            Ad = "Valentino 100",
            Kod = "VALENTINO-100",
            KoleksiyonGrubu = "Premium",
            SayfaNo = 33,
            EkGaleriSayfalari = [34],
            Fiyat = 67500m,
            BoyDolabiFiyati = 27500m,
            Renkler = ["Spesiyal Gri", "Krem"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "100 cm", Derinlik = "50 cm" },
                new() { Baslik = "Ayna", Yukseklik = "70 cm", Genislik = "90 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "zanussi-100",
            Ad = "Zanussi 100",
            Kod = "ZANUSSI-100",
            KoleksiyonGrubu = "Premium",
            SayfaNo = 35,
            Fiyat = 75000m,
            BoyDolabiFiyati = 27500m,
            Renkler = ["Spesiyal Gri", "Krem"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "100 cm", Derinlik = "50 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "75 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "zanussi-80",
            Ad = "Zanussi 80",
            Kod = "ZANUSSI-80",
            KoleksiyonGrubu = "Premium",
            SayfaNo = 36,
            Fiyat = 67500m,
            BoyDolabiFiyati = 27500m,
            Renkler = ["Spesiyal Gri", "Krem"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "50 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "75 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "perla-80",
            Ad = "Perla 80",
            Kod = "PERLA-80",
            KoleksiyonGrubu = "Trend",
            SayfaNo = 37,
            EkGaleriSayfalari = [38, 39],
            Fiyat = 57500m,
            BoyDolabiFiyati = 27500m,
            Renkler = ["Siyah", "Yeşil", "Turuncu"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Kolay Montaj", "Kolay Temizlenir", "Cam Lavabo"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "90 cm", Genislik = "70 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "valencia-90",
            Ad = "Valencia 90",
            Kod = "VALENCIA-90",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 40,
            Fiyat = 49000m,
            Renkler = ["Amerikan Ceviz"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "90 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "75 cm", Derinlik = "5 cm" }
            ]
        },
        new()
        {
            Slug = "otto-100",
            Ad = "Otto 100",
            Kod = "OTTO-100",
            KoleksiyonGrubu = "Trend",
            SayfaNo = 41,
            Fiyat = 59900m,
            BoyDolabiFiyati = 37500m,
            Renkler = ["Gri", "Kum Beji"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "100 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "100 cm", Derinlik = "16 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "otto-80",
            Ad = "Otto 80",
            Kod = "OTTO-80",
            KoleksiyonGrubu = "Trend",
            SayfaNo = 42,
            Fiyat = 57500m,
            BoyDolabiFiyati = 37500m,
            Renkler = ["Gri", "Kum Beji"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "80 cm", Derinlik = "16 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "otto-65",
            Ad = "Otto 65",
            Kod = "OTTO-65",
            KoleksiyonGrubu = "Trend",
            SayfaNo = 43,
            Fiyat = 55000m,
            BoyDolabiFiyati = 37500m,
            Renkler = ["Gri", "Kum Beji"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "65 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "65 cm", Derinlik = "16 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "lugano-100",
            Ad = "Lugano 100",
            Kod = "LUGANO-100",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 44,
            Fiyat = 49000m,
            BoyDolabiFiyati = 28800m,
            Renkler = ["Gri", "Beyaz"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "100 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "75 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "lugano-80",
            Ad = "Lugano 80",
            Kod = "LUGANO-80",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 45,
            Fiyat = 45000m,
            BoyDolabiFiyati = 28800m,
            Renkler = ["Gri", "Beyaz"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "75 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "lugano-plus-100",
            Ad = "Lugano Plus 100",
            Kod = "LUGANO-PLUS-100",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 46,
            Fiyat = 49000m,
            BoyDolabiFiyati = 28800m,
            Renkler = ["Gri", "Beyaz"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "100 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "100 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "lugano-plus-80",
            Ad = "Lugano Plus 80",
            Kod = "LUGANO-PLUS-80",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 47,
            Fiyat = 45000m,
            BoyDolabiFiyati = 28800m,
            Renkler = ["Gri", "Beyaz"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "80 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "dolce-100",
            Ad = "Dolce 100",
            Kod = "DOLCE-100",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 48,
            Fiyat = 40000m,
            BoyDolabiFiyati = 27500m,
            Renkler = ["Beyaz", "Gri"],
            Ozellikler = ["Soft Kapak", "Dokunmatik Ledli Ayna", "Mdf Ahşap", "Kolay Montaj", "Renk Seçenekleri", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "100 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "90 cm", Genislik = "70 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "dolce-80",
            Ad = "Dolce 80",
            Kod = "DOLCE-80",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 49,
            Fiyat = 37500m,
            BoyDolabiFiyati = 27500m,
            Renkler = ["Beyaz", "Gri"],
            Ozellikler = ["Soft Kapak", "Dokunmatik Ledli Ayna", "Mdf Ahşap", "Kolay Montaj", "Renk Seçenekleri", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "90 cm", Genislik = "70 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "dolce-plus-100-80",
            Ad = "Dolce Plus 100-80",
            Kod = "DOLCE-PLUS-100-80",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 50,
            Fiyat = 40000m,
            BoyDolabiFiyati = 27500m,
            Renkler = ["Beyaz", "Gri"],
            Ozellikler = ["Soft Kapak", "Dokunmatik Ledli Ayna", "Mdf Ahşap", "Kolay Montaj", "Renk Seçenekleri", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "100 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "100 cm", Derinlik = "16 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "rocco-100",
            Ad = "Rocco 100",
            Kod = "ROCCO-100",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 51,
            Fiyat = 47750m,
            Renkler = ["Antrasit", "Bej", "Ay Taşı"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "100 cm", Derinlik = "45 cm" },
                new() { Baslik = "Ayna", Yukseklik = "90 cm", Genislik = "70 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "rocco-80",
            Ad = "Rocco 80",
            Kod = "ROCCO-80",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 52,
            Fiyat = 45000m,
            BoyDolabiFiyati = 27500m,
            Renkler = ["Antrasit", "Bej", "Ay Taşı"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "45 cm" },
                new() { Baslik = "Ayna", Yukseklik = "90 cm", Genislik = "70 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "rocco-65",
            Ad = "Rocco 65",
            Kod = "ROCCO-65",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 53,
            Fiyat = 41000m,
            Renkler = ["Antrasit", "Bej", "Ay Taşı"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "65 cm", Derinlik = "45 cm" },
                new() { Baslik = "Ayna", Yukseklik = "90 cm", Genislik = "60 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "rocco-plus-100",
            Ad = "Rocco Plus 100",
            Kod = "ROCCO-PLUS-100",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 54,
            Fiyat = 47750m,
            Renkler = ["Antrasit", "Bej", "Ay Taşı"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "100 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "100 cm", Derinlik = "16 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "rocco-plus-80",
            Ad = "Rocco Plus 80",
            Kod = "ROCCO-PLUS-80",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 55,
            Fiyat = 45000m,
            BoyDolabiFiyati = 27500m,
            Renkler = ["Antrasit", "Bej", "Ay Taşı"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "80 cm", Derinlik = "16 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "rocco-plus-65",
            Ad = "Rocco Plus 65",
            Kod = "ROCCO-PLUS-65",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 56,
            Fiyat = 41000m,
            Renkler = ["Antrasit", "Bej", "Ay Taşı"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "65 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "65 cm", Derinlik = "16 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "vedenda-80",
            Ad = "Vedenda 80",
            Kod = "VEDENDA-80",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 57,
            EkGaleriSayfalari = [58],
            Fiyat = 45000m,
            BoyDolabiFiyati = 31000m,
            Renkler = ["Yeşil", "Beyaz"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "90 cm", Genislik = "80 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "vedenda-plus-80",
            Ad = "Vedenda Plus 80",
            Kod = "VEDENDA-PLUS-80",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 59,
            EkGaleriSayfalari = [60],
            Fiyat = 45000m,
            BoyDolabiFiyati = 31000m,
            Renkler = ["Vizon", "Beyaz"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "90 cm", Genislik = "80 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "arte-80",
            Ad = "Arte 80",
            Kod = "ARTE-80",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 61,
            Fiyat = 32500m,
            BoyDolabiFiyati = 22500m,
            Renkler = ["Antrasit", "Bej"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "90 cm", Genislik = "70 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "arte-100",
            Ad = "Arte 100",
            Kod = "ARTE-100",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 62,
            Fiyat = 35000m,
            BoyDolabiFiyati = 22500m,
            Renkler = ["Antrasit", "Bej"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri", "Doğal Ağaç"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "100 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "90 cm", Genislik = "70 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "elsa-120",
            Ad = "Elsa 120",
            Kod = "ELSA-120",
            KoleksiyonGrubu = "Trend",
            SayfaNo = 63,
            Fiyat = 55000m,
            Renkler = ["Beyaz", "Antrasit"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "120 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "85 cm", Genislik = "55 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "elsa-80",
            Ad = "Elsa 80",
            Kod = "ELSA-80",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 64,
            EkGaleriSayfalari = [65],
            Fiyat = 35000m,
            BoyDolabiFiyati = 27500m,
            Renkler = ["Beyaz", "Antrasit"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "85 cm", Genislik = "55 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "elsa-plus-80",
            Ad = "Elsa Plus 80",
            Kod = "ELSA-PLUS-80",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 66,
            EkGaleriSayfalari = [67],
            Fiyat = 35000m,
            BoyDolabiFiyati = 27500m,
            Renkler = ["Beyaz", "Antrasit"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "80 cm", Derinlik = "16 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "elsa-60",
            Ad = "Elsa 60",
            Kod = "ELSA-60",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 68,
            Fiyat = 31000m,
            Renkler = ["Beyaz", "Antrasit"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "60 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "60 cm", Derinlik = "16 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "oscar-100",
            Ad = "Oscar 100",
            Kod = "OSCAR-100",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 69,
            Fiyat = 48000m,
            Renkler = ["Beyaz"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "100 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "70 cm", Genislik = "100 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "120 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "oscar-80",
            Ad = "Oscar 80",
            Kod = "OSCAR-80",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 70,
            Fiyat = 43800m,
            BoyDolabiFiyati = 23750m,
            Renkler = ["Beyaz"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "70 cm", Genislik = "80 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "120 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "picasso-80",
            Ad = "Picasso 80",
            Kod = "PICASSO-80",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 71,
            EkGaleriSayfalari = [72],
            Fiyat = 45000m,
            BoyDolabiFiyati = 25800m,
            Renkler = ["Beyaz", "Antrasit"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "53 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "75 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "mabel-100",
            Ad = "Mabel 100",
            Kod = "MABEL-100",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 73,
            Fiyat = 40000m,
            BoyDolabiFiyati = 25800m,
            Renkler = ["Gri", "Kum Beji"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "100 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "100 cm", Derinlik = "16 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "120 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "mabel-80",
            Ad = "Mabel 80",
            Kod = "MABEL-80",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 74,
            Fiyat = 37500m,
            BoyDolabiFiyati = 25800m,
            Renkler = ["Gri", "Kum Beji"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "80 cm", Derinlik = "16 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "120 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "valery-80",
            Ad = "Valery 80",
            Kod = "VALERY-80",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 75,
            Fiyat = 43000m,
            BoyDolabiFiyati = 25500m,
            Renkler = ["Bej", "Krem"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "75 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "hira-115",
            Ad = "Hira 115",
            Kod = "HIRA-115",
            KoleksiyonGrubu = "Trend",
            SayfaNo = 78,
            Fiyat = 57500m,
            Renkler = ["Beyaz", "Antrasit"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "115 cm", Derinlik = "45 cm" },
                new() { Baslik = "Ayna", Yukseklik = "70 cm", Genislik = "50 cm", Derinlik = "16 cm" }
            ]
        },
        new()
        {
            Slug = "perimelis-100",
            Ad = "Perimelis 100",
            Kod = "PERIMELIS-100",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 79,
            Fiyat = 33000m,
            Renkler = ["Ceviz", "Erik", "Florida"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "100 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "100 cm", Derinlik = "16 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "perimelis-80",
            Ad = "Perimelis 80",
            Kod = "PERIMELIS-80",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 80,
            Fiyat = 27500m,
            Renkler = ["Ceviz", "Erik", "Florida"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "80 cm", Derinlik = "16 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "perimelis-65",
            Ad = "Perimelis 65",
            Kod = "PERIMELIS-65",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 80,
            Fiyat = 22500m,
            BoyDolabiFiyati = 18000m,
            Renkler = ["Ceviz", "Erik", "Florida"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "65 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "65 cm", Derinlik = "16 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "aria-100",
            Ad = "Aria 100",
            Kod = "ARIA-100",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 81,
            Fiyat = 32500m,
            BoyDolabiFiyati = 22500m,
            Renkler = ["Beyaz", "Antrasit"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "120 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "100 cm", Derinlik = "16 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "aria-80",
            Ad = "Aria 80",
            Kod = "ARIA-80",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 82,
            Fiyat = 30000m,
            Renkler = ["Beyaz", "Antrasit"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "80 cm", Derinlik = "16 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "aria-65",
            Ad = "Aria 65",
            Kod = "ARIA-65",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 83,
            Fiyat = 25800m,
            Renkler = ["Beyaz", "Antrasit"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "65 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "65 cm", Derinlik = "16 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "tria-80",
            Ad = "Tria 80",
            Kod = "TRIA-80",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 84,
            EkGaleriSayfalari = [85],
            Fiyat = 27500m,
            BoyDolabiFiyati = 22500m,
            Renkler = ["Gri", "Beyaz"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "80 cm", Derinlik = "16 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "piedra-55",
            Ad = "Piedra 55",
            Kod = "PIEDRA-55",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 86,
            EkGaleriSayfalari = [87, 88],
            Fiyat = 37500m,
            Renkler = ["Antrasit", "Kahve", "Gri"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "55 cm", Derinlik = "34 cm" },
                new() { Baslik = "Ayna", Yukseklik = "70 cm", Genislik = "50 cm", Derinlik = "5 cm" }
            ]
        },
        new()
        {
            Slug = "rio-80",
            Ad = "Rio 80",
            Kod = "RIO-80",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 89,
            EkGaleriSayfalari = [90, 91],
            Fiyat = 30000m,
            BoyDolabiFiyati = 22500m,
            Renkler = ["Luna Gri", "Vizon", "Beyaz"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "80 cm", Derinlik = "16 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "risus-80",
            Ad = "Risus 80",
            Kod = "RISUS-80",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 92,
            EkGaleriSayfalari = [93, 94],
            Fiyat = 30000m,
            BoyDolabiFiyati = 22500m,
            Renkler = ["Luna Gri", "Vizon", "Beyaz"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "80 cm", Derinlik = "16 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "verto-50",
            Ad = "Verto 50",
            Kod = "VERTO-50",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 95,
            Fiyat = 23800m,
            Renkler = ["Erik", "Florida", "Antrasit"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "50 cm", Derinlik = "40 cm" },
                new() { Baslik = "Ayna", Yukseklik = "85 cm", Genislik = "55 cm", Derinlik = "5 cm" }
            ]
        },
        new()
        {
            Slug = "verto-60",
            Ad = "Verto 60",
            Kod = "VERTO-60",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 95,
            Fiyat = 24500m,
            Renkler = ["Erik", "Florida", "Antrasit"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "60 cm", Derinlik = "40 cm" },
                new() { Baslik = "Ayna", Yukseklik = "85 cm", Genislik = "55 cm", Derinlik = "5 cm" }
            ]
        },
        new()
        {
            Slug = "eco-80",
            Ad = "Eco 80",
            Kod = "ECO-80",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 96,
            Fiyat = 15000m,
            Renkler = ["Gri", "Beyaz"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "80 cm", Derinlik = "16 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "eco-65",
            Ad = "Eco 65",
            Kod = "ECO-65",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 96,
            Fiyat = 14000m,
            Renkler = ["Gri", "Beyaz"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "65 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "65 cm", Derinlik = "16 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "eco-plus-80",
            Ad = "Eco Plus 80",
            Kod = "ECO-PLUS-80",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 97,
            Fiyat = 20000m,
            Renkler = ["Vizon", "Antrasit", "Gri"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "80 cm", Derinlik = "16 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "eco-plus-65",
            Ad = "Eco Plus 65",
            Kod = "ECO-PLUS-65",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 97,
            Fiyat = 19000m,
            Renkler = ["Vizon", "Antrasit", "Gri"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "65 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "65 cm", Derinlik = "16 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "eco-plus-100",
            Ad = "Eco Plus 100",
            Kod = "ECO-PLUS-100",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 98,
            Fiyat = 22000m,
            BoyDolabiFiyati = 17500m,
            Renkler = ["Vizon", "Antrasit", "Gri"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "100 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "100 cm", Derinlik = "16 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "paco-65",
            Ad = "Paco 65",
            Kod = "PACO-65",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 99,
            Fiyat = 22500m,
            Renkler = ["Erik", "Antrasit", "Beyaz", "Ceviz"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "65 cm", Derinlik = "33 cm" },
                new() { Baslik = "Ayna", Yukseklik = "60 cm", Genislik = "60 cm", Derinlik = "5 cm" }
            ]
        },
        new()
        {
            Slug = "paco-plus-65",
            Ad = "Paco Plus 65",
            Kod = "PACO-PLUS-65",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 100,
            Fiyat = 20000m,
            Renkler = ["Erik", "Antrasit", "Beyaz", "Ceviz"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "65 cm", Derinlik = "33 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "65 cm", Derinlik = "16 cm" }
            ]
        },
        new()
        {
            Slug = "nicci-60",
            Ad = "Nicci 60",
            Kod = "NICCI-60",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 101,
            Fiyat = 14000m,
            Renkler = ["Beyaz", "Ceviz", "Antrasit", "Ahşap"],
            Ozellikler = ["MDF Ahşap", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "60 cm", Derinlik = "25,5 cm" },
                new() { Baslik = "Ayna", Yukseklik = "90 cm", Genislik = "40 cm", Derinlik = "5 cm" }
            ]
        },
        new()
        {
            Slug = "adonis-50",
            Ad = "Adonis 50",
            Kod = "ADONIS-50",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 102,
            Fiyat = 14000m,
            Renkler = ["Antrasit", "Beyaz", "Ceviz", "Vizon"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "50 cm", Derinlik = "38 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "50 cm", Derinlik = "16 cm" }
            ]
        },
        new()
        {
            Slug = "oliy-45",
            Ad = "Oliy 45",
            Kod = "OLIY-45",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 103,
            Fiyat = 9200m,
            Renkler = ["Antrasit", "Beyaz", "Vizon"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "45 cm", Derinlik = "28 cm" },
                new() { Baslik = "Ayna", Yukseklik = "53 cm", Genislik = "39 cm", Derinlik = "5 cm" }
            ]
        }
    ];

    public static OrpayKatalogUrunu? SlugIleGetir(string? slug) =>
        string.IsNullOrWhiteSpace(slug)
            ? null
            : Tum.FirstOrDefault(x => x.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));

    public static int KoleksiyonSirasiGetir(string? koleksiyonGrubu)
    {
        if (string.IsNullOrWhiteSpace(koleksiyonGrubu))
        {
            return KoleksiyonSiralari["Diger"];
        }

        return KoleksiyonSiralari.TryGetValue(koleksiyonGrubu, out var sira)
            ? sira
            : KoleksiyonSiralari["Diger"];
    }

    public static string KoleksiyonAciklamasiGetir(string? koleksiyonGrubu)
    {
        if (string.IsNullOrWhiteSpace(koleksiyonGrubu))
        {
            return KoleksiyonAciklamalari["Diger"];
        }

        return KoleksiyonAciklamalari.TryGetValue(koleksiyonGrubu, out var aciklama)
            ? aciklama
            : KoleksiyonAciklamalari["Diger"];
    }
}
