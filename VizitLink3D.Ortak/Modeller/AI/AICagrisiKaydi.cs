namespace VizitLink3D.Ortak.Modeller.AI;

public class AICagrisiKaydi
{
    public long Id { get; set; }
    public int SaglayiciId { get; set; }
    public AISaglayicisi? Saglayici { get; set; }

    public string? KullaniciId { get; set; }
    public string KullanimAmaci { get; set; } = "MetinYaz";

    public int IstekTokenSayisi { get; set; }
    public int CevapTokenSayisi { get; set; }
    public decimal ToplamMaliyetUsd { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public string? Prompt { get; set; }
    public AICagriDurumu Durum { get; set; } = AICagriDurumu.Basarili;
    public string? HataMesaji { get; set; }

    public long SureMs { get; set; }
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
}
