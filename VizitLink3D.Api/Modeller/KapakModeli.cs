using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.Api.Modeller;

/// <summary>
/// Kapi ve mobilya kapak modellerini temsil eder.
/// Hem kapak (mutfak/banyo) hem kapi (ic/dis mekan) modellerini kapsar.
/// Kategori alani KapiKategorisi tablosuna FK ile baglanir.
/// </summary>
public class KapakModeli
{
    public int Id { get; set; }
    public string ModelAdi { get; set; } = string.Empty;
    public string ModelKodu { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Kategori { get; set; } = string.Empty;
    public string ModelTuru { get; set; } = "Kapak"; // Kapak veya Kapi

    // Kategori FK (opsiyonel — ileri asamada string Kategori yerine gecer)
    public int? KategoriId { get; set; }

    [JsonIgnore]
    public KapiKategorisi? KapiKategorisi { get; set; }

    public string? AnaGorselUrl { get; set; }
    public string? Aciklama { get; set; }
    public string? OnYazi { get; set; }
    public bool OneCikanMi { get; set; }
    public bool YeniMi { get; set; }
    public decimal? Fiyat { get; set; }
    public string? ModelDosyaYolu { get; set; }

    // Olcu bilgileri
    public int? MinYukseklik { get; set; }
    public int? MaxYukseklik { get; set; }
    public int? MinGenislik { get; set; }
    public int? MaxGenislik { get; set; }

    // Teknik ozellikler (JSON)
    public string? TeknikOzelliklerJson { get; set; }
    public string? SertifikalarJson { get; set; }
    public string? KullanimAlanlariJson { get; set; }

    // Renk ve nitelik (JSON)
    public string RenkSecenekleriJson { get; set; } = "[]";
    public string NiteliklerJson { get; set; } = "[]";

    [JsonIgnore]
    public string UygulamaGorselleriJson { get; set; } = "[]";

    // SEO
    public string? SeoBaslik { get; set; }
    public string? SeoAciklama { get; set; }
    public string? SeoAnahtarKelimeler { get; set; }

    // Siralama
    public int SiraNo { get; set; }

    // Galeri (1:N)
    [JsonIgnore]
    public List<KapiModeliResim>? GaleriResimleri { get; set; }

    // Audit
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }

    // [NotMapped] JSON yardimci alanlar
    [NotMapped]
    public List<RenkDto> RenkSecenekleri
    {
        get => string.IsNullOrEmpty(RenkSecenekleriJson) ? new() : JsonSerializer.Deserialize<List<RenkDto>>(RenkSecenekleriJson) ?? new();
        set => RenkSecenekleriJson = JsonSerializer.Serialize(value);
    }

    [NotMapped]
    public List<string> Nitelikler
    {
        get => string.IsNullOrEmpty(NiteliklerJson) ? new() : JsonSerializer.Deserialize<List<string>>(NiteliklerJson) ?? new();
        set => NiteliklerJson = JsonSerializer.Serialize(value);
    }

    [NotMapped]
    public List<string> UygulamaGorselleri
    {
        get => string.IsNullOrEmpty(UygulamaGorselleriJson) ? new() : JsonSerializer.Deserialize<List<string>>(UygulamaGorselleriJson) ?? new();
        set => UygulamaGorselleriJson = JsonSerializer.Serialize(value);
    }
}

public class RenkDto
{
    public string Ad { get; set; } = string.Empty;
    public string HexKod { get; set; } = "#FFFFFF";
    public string? Grup { get; set; }
    public string? UreticiKodu { get; set; }
}
