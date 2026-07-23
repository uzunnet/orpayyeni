using System.Security.Cryptography;
using System.Text.Json;

namespace VizitLink3D.Api.Moduller.Guvenlik.Dtolar;

/// <summary>
/// Yeni API anahtarı oluşturma isteği.
/// </summary>
public record FirmaApiAnahtariOlusturDto(
    string AnahtarAd,
    string Kapsam,
    string? IzinVerilenDomainler,
    DateTime? SonKullanmaTarihi,
    int? FirmaId = null  // SuperAdmin çapraz firma için opsiyonel
);

/// <summary>
/// API anahtarı güncelleme isteği (anahtar değeri değişmez).
/// </summary>
public record FirmaApiAnahtariGuncelleDto(
    string? AnahtarAd,
    string? Kapsam,
    string? IzinVerilenDomainler,
    DateTime? SonKullanmaTarihi,
    bool? AktifMi
);

/// <summary>
/// API anahtarı oluşturma yanıtı.
/// DuzMetinAnahtar sadece bu yanıtta bir kez gösterilir, sonra gösterilmez.
/// </summary>
public record FirmaApiAnahtariOlusturYanitDto(
    int Id,
    string AnahtarAd,
    string AnahtarOnEki,
    string DuzMetinAnahtar,  // SADECE BİR KEZ gösterilir
    string Kapsam,
    string? IzinVerilenDomainler,
    DateTime? SonKullanmaTarihi,
    DateTime OlusturulmaTarihi
);

/// <summary>
/// API anahtarı liste/görüntüleme DTO'su (düz metin anahtar ASLA gösterilmez).
/// </summary>
public record FirmaApiAnahtariListeDto(
    int Id,
    string AnahtarAd,
    string AnahtarOnEki,
    string Kapsam,
    string? IzinVerilenDomainler,
    bool AktifMi,
    DateTime? SonKullanmaTarihi,
    DateTime? SonKullanimTarihi,
    DateTime OlusturulmaTarihi
);

/// <summary>
/// API anahtarı oluşturma yardımcı servisi.
/// </summary>
public static class ApiAnahtarUretici
{
    private const string ONEK = "vt3d_";

    /// <summary>
    /// Geçerli kapsam değerleri.
    /// </summary>
    public static readonly HashSet<string> GecerliKapsamlar = new(StringComparer.OrdinalIgnoreCase)
    {
        "PublicOkuma",
        "KonfigurasyonKaydetme",
        "Embed",
        "SunucuEntegrasyonu"
    };

    /// <summary>
    /// Embed kapsamı için zorunlu kapsamlar (en az biri olmalı).
    /// </summary>
    public static readonly HashSet<string> EmbedZorunluKapsamlar = new(StringComparer.OrdinalIgnoreCase)
    {
        "Embed"
    };

    /// <summary>
    /// Sunucu entegrasyonu için zorunlu kapsamlar.
    /// </summary>
    public static readonly HashSet<string> EntegrasyonZorunluKapsamlar = new(StringComparer.OrdinalIgnoreCase)
    {
        "SunucuEntegrasyonu"
    };

    /// <summary>
    /// Güvenli rastgele API anahtarı üretir.
    /// Format: vt3d_ + 48 hex karakter (toplam 53 karakter)
    /// </summary>
    public static string AnahtarUret()
    {
        var rastgeleBytes = RandomNumberGenerator.GetBytes(24);
        var hex = Convert.ToHexStringLower(rastgeleBytes);
        return ONEK + hex;
    }

    /// <summary>
    /// SHA256 hash hesaplar.
    /// </summary>
    public static string HashHesapla(string duzMetin)
    {
        var hashBytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(duzMetin));
        return Convert.ToHexStringLower(hashBytes);
    }

    /// <summary>
    /// Kapsam dizesini doğrular. Geçersiz kapsam adlarını reddeder.
    /// </summary>
    public static (bool gecerliMi, string? hata) KapsamDogrula(string? kapsam)
    {
        if (string.IsNullOrWhiteSpace(kapsam))
            return (false, "Kapsam alanı zorunludur.");

        var kapsamListesi = kapsam
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        if (kapsamListesi.Count == 0)
            return (false, "En az bir kapsam belirtilmelidir.");

        var gecersizler = kapsamListesi
            .Where(k => !GecerliKapsamlar.Contains(k))
            .ToList();

        if (gecersizler.Count > 0)
            return (false, $"Geçersiz kapsam: {string.Join(", ", gecersizler)}. Geçerli değerler: {string.Join(", ", GecerliKapsamlar)}");

        return (true, null);
    }

    /// <summary>
    /// İzin verilen domainler JSON dizesini doğrular.
    /// </summary>
    public static (bool gecerliMi, string? hata, List<string>? domainler) IzinVerilenDomainlerDogrula(string? domainlerJson, string? kapsam = null)
    {
        if (string.IsNullOrWhiteSpace(domainlerJson))
        {
            // Embed kapsamı varsa domain zorunlu
            if (!string.IsNullOrWhiteSpace(kapsam) && kapsam.Contains("Embed", StringComparison.OrdinalIgnoreCase))
                return (false, "Embed kapsamı için izin verilen domain(ler) zorunludur.", null);

            return (true, null, null);
        }

        List<string> domainler;
        try
        {
            domainler = JsonSerializer.Deserialize<List<string>>(domainlerJson) ?? [];
        }
        catch (JsonException)
        {
            return (false, "İzin verilen domainler geçerli bir JSON dizi formatında olmalıdır. Örn: [\"https://example.com\"]", null);
        }

        if (domainler.Count == 0)
            return (false, "Domain listesi boş olamaz.", null);

        // Her domain'in geçerli bir URL formatında olduğunu kontrol et
        foreach (var domain in domainler)
        {
            var temizDomain = domain.Trim();
            if (string.IsNullOrWhiteSpace(temizDomain))
                return (false, "Domain listesinde boş değer olamaz.", null);

            if (!temizDomain.StartsWith("https://") && !temizDomain.StartsWith("http://"))
                return (false, $"Domain 'https://' veya 'http://' ile başlamalıdır: {temizDomain}", null);

            // Basit URL yapı kontrolü
            if (!Uri.TryCreate(temizDomain, UriKind.Absolute, out var uri))
                return (false, $"Geçersiz domain formatı: {temizDomain}", null);

            // Path/query içermemeli (sadece origin)
            if (!string.IsNullOrEmpty(uri.PathAndQuery) && uri.PathAndQuery != "/")
                return (false, $"Domain path veya query string içermemelidir: {temizDomain}", null);
        }

        // Embed kapsamı varsa en az bir domain zorunlu
        if (!string.IsNullOrWhiteSpace(kapsam) && kapsam.Contains("Embed", StringComparison.OrdinalIgnoreCase) && domainler.Count == 0)
            return (false, "Embed kapsamı için en az bir izin verilen domain belirtilmelidir.", null);

        return (true, null, domainler);
    }
}
