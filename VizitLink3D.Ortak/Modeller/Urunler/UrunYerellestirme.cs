using System;

namespace VizitLink3D.Ortak.Modeller.Urunler;

public class UrunYerellestirme
{
    public int Id { get; set; }
    public int UrunId { get; set; }
    public string Dil { get; set; } = "tr";
    public string? Ad { get; set; }
    public string? KisaAciklama { get; set; }
    public string? Aciklama { get; set; }
    public string? SeoBaslik { get; set; }
    public string? SeoAciklama { get; set; }
}