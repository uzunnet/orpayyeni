namespace VizitLink3D.Ortak.Modeller.Medya;

public class MedyaKullanim
{
    public int Id { get; set; }
    public int MedyaId { get; set; }
    public Medya? Medya { get; set; }

    public string EntiteAdi { get; set; } = string.Empty;
    public int EntiteId { get; set; }
    public string? AlanAdi { get; set; }
    public int SiraNo { get; set; }

    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
}
