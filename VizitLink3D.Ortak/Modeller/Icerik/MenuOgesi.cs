using System;

namespace VizitLink3D.Ortak.Modeller;

public class MenuOgesi
{
    public int Id { get; set; }
    public int? FirmaId { get; set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public Firma? Firma { get; set; }
    public string Baslik { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int? UstMenuId { get; set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public MenuOgesi? UstMenu { get; set; }
    public List<MenuOgesi> AltMenuler { get; set; } = new();
    public int Sira { get; set; } = 0;
    public bool AktifMi { get; set; } = true;
    public bool YeniSekmede { get; set; } = false;
    public string? Ikon { get; set; }
    public string Konum { get; set; } = "AnaMenu";

    // Yetki ve Kontrol
    public string? GerekliRol { get; set; }
    public bool SuperAdminGerekliMi { get; set; } = false;
    public string? YetkiAnahtari { get; set; }
    public bool KilitliMi { get; set; } = false;
    public bool SistemMenusuMu { get; set; } = false;

    // Soft Delete
    public bool SilindiMi { get; set; } = false;
    public DateTime? SilinmeTarihi { get; set; }

    // Audit
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
}
