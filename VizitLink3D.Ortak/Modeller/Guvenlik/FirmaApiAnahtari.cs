using System.Text.Json.Serialization;

namespace VizitLink3D.Ortak.Modeller.Guvenlik;

/// <summary>
/// Firma (tenant) bazlı API erişim anahtarı.
/// Embed/iframe veya public API için scope-tabanlı yetkilendirme sağlar.
/// API anahtarı ilk oluşturmada sadece bir kez düz metin olarak gösterilir,
/// veritabanında SHA256 hash olarak saklanır.
/// </summary>
public class FirmaApiAnahtari
{
    public int Id { get; set; }

    /// <summary>Hangi firmaya ait</summary>
    public int FirmaId { get; set; }

    /// <summary>Görünen ad (yönetici için)</summary>
    public string AnahtarAd { get; set; } = string.Empty;

    /// <summary>
    /// API anahtarının SHA256 hash'i.
    /// Düz metin anahtar ASLA saklanmaz — sadece hash ile doğrulama yapılır.
    /// </summary>
    [JsonIgnore]
    public string ApiKeyHash { get; set; } = string.Empty;

    /// <summary>
    /// Anahtarın ilk 8 karakteri (prefix) — yönetici UI'da hangi anahtar olduğunu
    /// hatırlamak için. Hash'ten geri dönülemez olduğu için prefix düz metin saklanır.
    /// </summary>
    public string AnahtarOnEki { get; set; } = string.Empty;

    /// <summary>
    /// Kapsam/izinler — virgülle ayrılmış izin listesi.
    /// Geçerli değerler: PublicOkuma, KonfigurasyonKaydetme, Embed
    /// </summary>
    public string Kapsam { get; set; } = "PublicOkuma";

    /// <summary>
    /// İzin verilen origin'ler — JSON string dizisi.
    /// Örn: ["https://orpayormanurunleri.com.tr", "https://www.orpayormanurunleri.com.tr"]
    /// Boş/null ise tüm origin'lere izin verilmez (CORS sıkı).
    /// Embed scope'u için zorunludur.
    /// </summary>
    public string? IzinVerilenDomainler { get; set; }

    public bool AktifMi { get; set; } = true;

    /// <summary>Anahtarın geçerlilik bitiş tarihi. Null = süresiz.</summary>
    public DateTime? SonKullanmaTarihi { get; set; }

    /// <summary>En son ne zaman kullanıldığı</summary>
    public DateTime? SonKullanimTarihi { get; set; }

    // === AUDIT ===
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
    public int? OlusturanKullaniciId { get; set; }
    public int? GuncelleyenKullaniciId { get; set; }

    // === SOFT DELETE ===
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }

    // === NAVIGATION ===
    [JsonIgnore]
    public Firma? Firma { get; set; }

    // === YARDIMCI METOTLAR ===

    /// <summary>Kapsam dizesini ayrıştırır</summary>
    public List<string> KapsamListesi()
    {
        return string.IsNullOrWhiteSpace(Kapsam)
            ? []
            : Kapsam.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList();
    }

    /// <summary>Belirtilen kapsama sahip mi?</summary>
    public bool KapsamVarMi(string kapsam)
    {
        return KapsamListesi().Contains(kapsam, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Belirtilen origin izinli mi?</summary>
    public bool OriginIzınliMi(string? origin)
    {
        if (string.IsNullOrWhiteSpace(IzinVerilenDomainler))
            return false;

        if (string.IsNullOrWhiteSpace(origin))
            return false;

        try
        {
            var domainler = System.Text.Json.JsonSerializer.Deserialize<List<string>>(IzinVerilenDomainler);
            if (domainler is null || domainler.Count == 0)
                return false;

            return domainler.Any(d =>
                string.Equals(d.Trim(), origin.Trim(), StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Anahtar süresi dolmuş mu?</summary>
    public bool SuresiDolduMu()
    {
        return SonKullanmaTarihi.HasValue && SonKullanmaTarihi.Value < DateTime.UtcNow;
    }

    /// <summary>Anahtar geçerli mi? (aktif + süresi dolmamış + silinmemiş)</summary>
    public bool GecerliMi()
    {
        return AktifMi && !SilindiMi && !SuresiDolduMu();
    }
}
