using System;

namespace VizitLink3D.Ortak.Modeller.Urunler;

public class UrunParcaRenkSecenegi
{
    public int Id { get; set; }
    public int UrunUcBoyutParcasiId { get; set; }
    public int? RalRengiId { get; set; }
    public int? RenkKataloguId { get; set; }
    public string? EkAciklama { get; set; }
    public bool AktifMi { get; set; } = true;
}