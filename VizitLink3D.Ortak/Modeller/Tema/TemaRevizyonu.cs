namespace VizitLink3D.Ortak.Modeller.Tema;

public class TemaRevizyonu
{
    public int Id { get; set; }
    public int TemaSablonuId { get; set; }
    public int Versiyon { get; set; }
    public string KaynakTipi { get; set; } = string.Empty;
    public string? HamDesignMd { get; set; }
    public string UretilenManifestJson { get; set; } = "{}";
    public string? Notlar { get; set; }
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
}
