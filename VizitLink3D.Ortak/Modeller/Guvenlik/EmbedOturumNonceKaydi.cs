using System.Text.Json.Serialization;

namespace VizitLink3D.Ortak.Modeller.Guvenlik;

/// <summary>
/// Embed oturum nonce kaydi. Nonce duz metni veritabaninda asla saklanmaz,
/// yalnizca SHA256 hash degeri tutulur.
/// Multi-instance SaaS ortaminda unique constraint ile atomik one-time tuketim saglanir.
/// </summary>
public class EmbedOturumNonceKaydi
{
    public int Id { get; set; }

    /// <summary>
    /// SHA256 hash (hex string). API yanitlarinda gonderilmez.
    /// Unique index ile ayni nonce'in tekrar kullanimi engellenir.
    /// </summary>
    [JsonIgnore]
    public string NonceHash { get; set; } = string.Empty;

    /// <summary>
    /// Nonce'in gecerlilik bitis tarihi (UTC).
    /// Bu tarihten sonra nonce gecersizdir; soft-delete ile temizlenir.
    /// </summary>
    public DateTime SonKullanmaTarihi { get; set; }

    /// <summary>
    /// Kaydin olusturulma tarihi (UTC).
    /// </summary>
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Soft-delete bayragi. Fiziksel silme yapilmaz.
    /// </summary>
    public bool SilindiMi { get; set; }

    /// <summary>
    /// Soft-delete isleminin gerceklestigi tarih (UTC).
    /// </summary>
    public DateTime? SilinmeTarihi { get; set; }
}
