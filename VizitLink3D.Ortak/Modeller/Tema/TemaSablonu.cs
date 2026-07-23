namespace VizitLink3D.Ortak.Modeller.Tema;

public class TemaSablonu
{
    public int Id { get; set; }
    public string Kod { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Aciklama { get; set; } = string.Empty;
    public string Kaynak { get; set; } = "elle";
    public string? StitchProjeId { get; set; }
    public bool GlassmorphismAktif { get; set; }
    public bool Premium { get; set; }
    public decimal Fiyat { get; set; }
    public string ParaBirimi { get; set; } = "TRY";
    public string? ThumbnailUrl { get; set; }
    public TemaKapsam Kapsam { get; set; } = TemaKapsam.Her_ikisi;
    public bool AktifMi { get; set; } = true;
    public bool VarsayilanMi { get; set; }
    public string Etiketler { get; set; } = string.Empty;
    public int Versiyon { get; set; } = 1;
    public bool SilindiMi { get; set; }
    public string RenklerJson { get; set; } = "{}";
    public string TipografiJson { get; set; } = "{}";
    public string GeometriJson { get; set; } = "{}";
    public string GolgelerJson { get; set; } = "{}";
    public string GlassmorphismJson { get; set; } = "{}";
    public string AnimasyonJson { get; set; } = "{}";
    public string LayoutJson { get; set; } = "{}";
    public string IkonSeti { get; set; } = "Material Icons";
    public string AdAnahtar { get; set; } = string.Empty;
    public string AciklamaAnahtar { get; set; } = string.Empty;
    public string AdVarsayilanTr { get; set; } = string.Empty;
    public string AdVarsayilanEn { get; set; } = string.Empty;
    public string AciklamaVarsayilanTr { get; set; } = string.Empty;
    public string AciklamaVarsayilanEn { get; set; } = string.Empty;
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
}
