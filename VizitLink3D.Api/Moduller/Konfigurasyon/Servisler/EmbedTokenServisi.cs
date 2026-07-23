using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;

namespace VizitLink3D.Api.Moduller.Konfigurasyon.Servisler;

/// <summary>
/// DataProtection tabanli time-limited embed token servisi.
/// 
/// IKI TOKEN TURU:
/// 1) Bootstrap token (5 dk) — URL'de sadece ilk GET'te bulunur. Cookie ile degistirilir.
/// 2) Session token (30 dk) — HttpOnly/Secure/SameSite=None cookie'de tasinir.
/// 
/// Token: FirmaId + UrunSlug + HedefOrigin + Nonce + ZamanDamgasi + OturumId
/// Token KEY ICERMEZ — yalniz erisim anahtari.
/// Token storage/console/log'a YAZILMAZ.
/// </summary>
public interface IEmbedTokenServisi
{
    /// <summary>
    /// Bootstrap time-limited embed token olusturur (URL'de bir defa kullanilir, 5 dk gecerli).
    /// </summary>
    string TokenOlustur(int firmaId, string urunSlug, string hedefOrigin);

    /// <summary>
    /// Bootstrap token dogrular, basarisizsa null doner.
    /// </summary>
    EmbedTokenPayload? TokenDogrula(string token);

    /// <summary>
    /// Session cookie icin uzun omurlu token olusturur (30 dk gecerli).
    /// Bootstrap token'dan farkli koruma amaci kullanir.
    /// </summary>
    string SessionTokenOlustur(EmbedTokenPayload payload);

    /// <summary>
    /// Session token dogrular, basarisizsa null doner.
    /// </summary>
    EmbedTokenPayload? SessionTokenDogrula(string sessionToken);

    /// <summary>
    /// Nonce replay kontrolu. Nonce daha once gorulmemisse kaydedip true doner.
    /// Gorulmusse false doner (replay saldirisi).
    /// </summary>
    Task<bool> NonceDogrulaVeKaydetAsync(string nonce);

    /// <summary>
    /// Guvenli rastgele nonce uretir (16 byte hex).
    /// </summary>
    static string NonceUret()
    {
        var bytes = RandomNumberGenerator.GetBytes(16);
        return Convert.ToHexStringLower(bytes);
    }

    /// <summary>
    /// Session token koruma amaci (bootstrap'tan farkli olmali).
    /// </summary>
    static string SessionTokenAmaci => "VizitLink3D.Embed.Oturum";

    /// <summary>
    /// Session token gecerlilik suresi.
    /// </summary>
    static TimeSpan SessionGecerlilik => TimeSpan.FromMinutes(30);
}

/// <summary>
/// Token icinde tasinan bilgiler — KEY ICERMEZ.
/// </summary>
public class EmbedTokenPayload
{
    public int FirmaId { get; init; }
    public string UrunSlug { get; init; } = string.Empty;
    public string HedefOrigin { get; init; } = string.Empty;
    public string Nonce { get; init; } = string.Empty;
    public string OturumId { get; init; } = string.Empty;
    public DateTime Olusturma { get; init; }
}

/// <summary>
/// Embed token API yaniti.
/// </summary>
public record EmbedOturumYanitDto(
    string IframeUrl,
    int GecerlilikSaniye
);

/// <summary>
/// Embed oturum olusturma istegi.
/// </summary>
public record EmbedOturumIstekDto(
    string HedefOrigin
);

public class EmbedTokenServisi : IEmbedTokenServisi
{
    private const string TOKEN_AMACI = "VizitLink3D.Embed.Token";
    private static readonly TimeSpan BOOTSTRAP_GECERLILIK = TimeSpan.FromMinutes(5);
    private static readonly JsonSerializerOptions JSON_OPT = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private readonly ITimeLimitedDataProtector _koruyucu;
    private readonly ITimeLimitedDataProtector _oturumKoruyucu;
    private readonly IEmbedNonceDeposu _nonceDeposu;

    public EmbedTokenServisi(IDataProtectionProvider koruma, IEmbedNonceDeposu nonceDeposu)
    {
        _koruyucu = koruma
            .CreateProtector(TOKEN_AMACI)
            .ToTimeLimitedDataProtector();
        _oturumKoruyucu = koruma
            .CreateProtector(IEmbedTokenServisi.SessionTokenAmaci)
            .ToTimeLimitedDataProtector();
        _nonceDeposu = nonceDeposu;
    }

    public string TokenOlustur(int firmaId, string urunSlug, string hedefOrigin)
    {
        var payload = new EmbedTokenPayload
        {
            FirmaId = firmaId,
            UrunSlug = urunSlug,
            HedefOrigin = hedefOrigin,
            Nonce = IEmbedTokenServisi.NonceUret(),
            OturumId = Guid.NewGuid().ToString("N"),
            Olusturma = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(payload, JSON_OPT);
        return _koruyucu.Protect(json, BOOTSTRAP_GECERLILIK);
    }

    public EmbedTokenPayload? TokenDogrula(string token)
    {
        try
        {
            var json = _koruyucu.Unprotect(token);
            var payload = JsonSerializer.Deserialize<EmbedTokenPayload>(json, JSON_OPT);
            return payload;
        }
        catch
        {
            return null;
        }
    }

    public string SessionTokenOlustur(EmbedTokenPayload payload)
    {
        var json = JsonSerializer.Serialize(payload, JSON_OPT);
        return _oturumKoruyucu.Protect(json, IEmbedTokenServisi.SessionGecerlilik);
    }

    public EmbedTokenPayload? SessionTokenDogrula(string sessionToken)
    {
        try
        {
            var json = _oturumKoruyucu.Unprotect(sessionToken);
            var payload = JsonSerializer.Deserialize<EmbedTokenPayload>(json, JSON_OPT);
            return payload;
        }
        catch
        {
            return null;
        }
    }

    public Task<bool> NonceDogrulaVeKaydetAsync(string nonce)
    {
        // Nonce replay korumasi: IEmbedNonceDeposu soyutlamasi uzerinden,
        // TTL expiration ile atomik one-time consume.
        // Nonce daha once gorulmemisse → true, gorulmusse → false (replay).
        // 6 dk TTL: bootstrap 5 dk + 1 dk buffer.
        return _nonceDeposu.DeneVeKaydetAsync(nonce, TimeSpan.FromMinutes(6));
    }
}
