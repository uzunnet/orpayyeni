using System;

namespace VizitLink3D.Ortak.Modeller.Urunler;

public class UrunParcaMalzemeSecenegi
{
    public int Id { get; set; }
    public int UrunUcBoyutParcasiId { get; set; }
    public int? MalzemeId { get; set; }
    public int? KaplamaSecenegiId { get; set; }
    public string? EkAciklama { get; set; }
    public bool AktifMi { get; set; } = true;
}