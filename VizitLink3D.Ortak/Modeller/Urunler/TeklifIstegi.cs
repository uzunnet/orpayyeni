using System;
using System.Text.Json.Serialization;

namespace VizitLink3D.Ortak.Modeller.Urunler;

public class TeklifIstegi
{
    public int Id { get; set; }
    public int? UrunId { get; set; }
    public int? MusteriKonfigurasyonuId { get; set; }
    public string? MusteriAdSoyad { get; set; }
    public string? Eposta { get; set; }
    public string? Telefon { get; set; }
    public string? Not { get; set; }
    public string? EkranGoruntusuMedyaId { get; set; }
    public string Durum { get; set; } = "Bekliyor";
    public bool SilindiMi { get; set; }
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }

    [JsonIgnore]
    public MusteriKonfigurasyonu? MusteriKonfigurasyonu { get; set; }
}
