using System;
using System.Text.Json.Serialization;

namespace VizitLink3D.Ortak.Modeller;

public class SohbetMesaji
{
    public int Id { get; set; }
    public string GonderenKimlik { get; set; } = string.Empty;
    public string GonderenIsmi { get; set; } = "Ziyaretci";
    public bool YoneticiMi { get; set; } = false;
    public string Icerik { get; set; } = string.Empty;
    public DateTime GonderimZamani { get; set; } = DateTime.UtcNow;
    public int OturumId { get; set; }
    [JsonIgnore]
    public SohbetOturumu? Oturum { get; set; }
}
