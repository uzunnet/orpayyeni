using System.Text.Json.Serialization;

namespace VizitLink3D.Ortak.Modeller.AI;

public class AISaglayicisi
{
    public int Id { get; set; }
    public AISaglayiciTipi Tip { get; set; } = AISaglayiciTipi.OpenAI;
    public string Ad { get; set; } = string.Empty;

    [JsonIgnore]
    public string ApiKeyEncrypted { get; set; } = string.Empty;

    public string Model { get; set; } = "gpt-4o-mini";
    public decimal AylikLimitUsd { get; set; } = 100;
    public decimal KullanilanUsd { get; set; }
    public DateTime SonSifirlamaTarihi { get; set; } = DateTime.UtcNow;

    public bool AktifMi { get; set; } = true;
    public int SiraNo { get; set; }

    public string? EkBaslik { get; set; }

    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
}
