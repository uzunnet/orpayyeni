using System;

namespace VizitLink3D.Ortak.Modeller;

public class SayfaDuzenAyari
{
    public int Id { get; set; }
    public string SayfaKodu { get; set; } = string.Empty;
    public string SayfaAdi { get; set; } = string.Empty;
    public int SutunAdet { get; set; } = 4;
    public int SatirAdet { get; set; } = 3;
    public int SayfaBasinaAdet { get; set; } = 12;
    public bool SayfalamaAktif { get; set; } = true;
    public bool AktifMi { get; set; } = true;
    public bool SilindiMi { get; set; } = false;
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
}
