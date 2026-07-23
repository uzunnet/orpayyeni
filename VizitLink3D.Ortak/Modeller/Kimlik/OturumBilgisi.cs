using System;
using System.Text.Json.Serialization;

namespace VizitLink3D.Ortak.Modeller;

public class OturumBilgisi
{
    [JsonIgnore]
    public string Token { get; set; } = string.Empty;
    [JsonIgnore]
    public string RefreshToken { get; set; } = string.Empty;
    public bool IkiAsamaliDogrulamaGerekliMi { get; set; } = false;
    public string AdSoyad { get; set; } = string.Empty;
    public string Eposta { get; set; } = string.Empty;
    public DateTime GecerlilikTarihi { get; set; }
    public int KullaniciId { get; set; }
    public string Rol { get; set; } = string.Empty;
    public int? SubeId { get; set; }
    public string YonlendirmeAdresi { get; set; } = "/yonetim/panel";
}
