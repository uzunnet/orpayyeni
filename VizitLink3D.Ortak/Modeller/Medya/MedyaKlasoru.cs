namespace VizitLink3D.Ortak.Modeller.Medya;

public class MedyaKlasoru
{
    public int Id { get; set; }
    public int? FirmaId { get; set; }
    public int? UstKlasorId { get; set; }
    public MedyaKlasoru? UstKlasor { get; set; }
    public List<MedyaKlasoru> AltKlasorler { get; set; } = new();
    public List<Medya> Medyalar { get; set; } = new();

    public string Ad { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Ikon { get; set; }
    public string? Renk { get; set; }
    public int SiraNo { get; set; }
    public bool AktifMi { get; set; } = true;

    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
}
