using System;

namespace VizitLink3D.Ortak.Modeller.Urunler;

public class UrunKategori
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public string Slug { get; set; } = string.Empty;
    public int? UstKategoriId { get; set; }
    public int SiraNo { get; set; }
    public bool AktifMi { get; set; } = true;
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }
}