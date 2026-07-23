using System;

namespace VizitLink3D.Ortak.Modeller.Urunler;

public class UrunKonfigurasyonKurali
{
    public int Id { get; set; }
    public int UrunId { get; set; }
    public string KuralTipi { get; set; } = string.Empty;
    public int Parca1Id { get; set; }
    public int Parca2Id { get; set; }
    public int? Parca1RenkId { get; set; }
    public int? Parca2RenkId { get; set; }
    public int? Parca1MalzemeId { get; set; }
    public int? Parca2MalzemeId { get; set; }
    public bool AktifMi { get; set; } = true;
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
}
