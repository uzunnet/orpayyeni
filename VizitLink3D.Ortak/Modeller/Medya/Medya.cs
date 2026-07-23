using System.Text.Json.Serialization;

namespace VizitLink3D.Ortak.Modeller.Medya;

public class Medya
{
    public int Id { get; set; }
    public int? FirmaId { get; set; }

    public MedyaTipi Tip { get; set; } = MedyaTipi.Resim;
    public MedyaKaynagi Kaynak { get; set; } = MedyaKaynagi.Yerel;

    public string Ad { get; set; } = string.Empty;
    public string? OrijinalAd { get; set; }
    public string? DosyaYolu { get; set; }
    public string? MiniaturYolu { get; set; }

    public string? KaynakUrl { get; set; }

    public long BoyutByte { get; set; }
    public int? Genislik { get; set; }
    public int? Yukseklik { get; set; }
    public int? SureSaniye { get; set; }
    public string? MimeTipi { get; set; }

    public string? Hash { get; set; }

    public string? AltMetin { get; set; }
    public string? Aciklama { get; set; }
    public string? EtiketlerJson { get; set; }

    public int? KlasorId { get; set; }
    [JsonIgnore]
    public MedyaKlasoru? Klasor { get; set; }

    public int KullanimSayisi { get; set; }
    public string? YukleyenKullaniciId { get; set; }

    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }

    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
}
