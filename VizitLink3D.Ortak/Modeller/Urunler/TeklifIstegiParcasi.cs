using System;
using System.Text.Json.Serialization;

namespace VizitLink3D.Ortak.Modeller.Urunler;

public class TeklifIstegiParcasi
{
    public int Id { get; set; }
    public int TeklifIstegiId { get; set; }
    public string ParcaAdi { get; set; } = string.Empty;
    public string? RenkAdi { get; set; }
    public string? RenkKodu { get; set; }
    public string? MalzemeAdi { get; set; }
    public string? Olcu { get; set; }
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public TeklifIstegi? TeklifIstegi { get; set; }
}
