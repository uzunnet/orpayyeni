namespace VizitLink3D.Ortak.Modeller;

/// <summary>
/// SuperAdmin veritabaninda firma-modul iliskisini tutar.
/// Hangi firmanin hangi modullere erisimi oldugunu belirler.
/// </summary>
public class FirmaModulAtama
{
    public int Id { get; set; }
    public int FirmaId { get; set; }
    public int ModulId { get; set; }
    public DateTimeOffset AtanmaTarihi { get; set; } = DateTimeOffset.UtcNow;

    public Firma? Firma { get; set; }
    public Modul? Modul { get; set; }
}
