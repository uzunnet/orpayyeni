namespace VizitLink3D.Ortak.Modeller.Urunler;

/// <summary>
/// 3D parça hareket tipleri enum'ı.
/// DB'de string olarak saklanır, API seviyesinde doğrulanır.
/// </summary>
public enum HareketTuru
{
    Sabit = 0,
    Menteseli = 1,
    Surgulu = 2,
    Cekmece = 3,
    YukariAcilir = 4,
    Pivot = 5,
    Recliner = 6
}
