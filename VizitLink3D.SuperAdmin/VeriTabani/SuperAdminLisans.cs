namespace VizitLink3D.SuperAdmin.VeriTabani;

/// <summary>
/// SuperAdmin tarafindan yonetilen firma lisans bilgisini temsil eder.
/// Her lisans bir firmaya aittir ve sure bazlidir.
/// </summary>
public class SuperAdminLisans
{
    public int Id { get; set; }
    public int FirmaId { get; set; }
    public string? Domain { get; set; }
    public string Tip { get; set; } = string.Empty;
    public DateTimeOffset BaslangicTarihi { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset BitisTarihi { get; set; }
    public bool AktifMi { get; set; } = true;
    public string? Aciklama { get; set; }
    public DateTimeOffset OlusturulmaTarihi { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? GuncellenmeTarihi { get; set; }
}
