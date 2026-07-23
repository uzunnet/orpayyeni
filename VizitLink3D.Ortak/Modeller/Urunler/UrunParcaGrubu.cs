using System;

namespace VizitLink3D.Ortak.Modeller.Urunler;

/// <summary>
/// Parça gruplama — ürün bazında mantıksal gruplar (örn: "Kapaklar", "Kulplar", "Camlar").
/// Tenant güvenli: FirmaId opsiyonel, ürün üzerinden tenant doğrulaması yapılır.
/// </summary>
public class UrunParcaGrubu
{
    public int Id { get; set; }
    public int UrunId { get; set; }
    public int? FirmaId { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public int SiraNo { get; set; }
    public bool AktifMi { get; set; } = true;

    // Audit + soft delete
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }
}
