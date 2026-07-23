using Microsoft.AspNetCore.Http;
using VizitLink3D.Api.Moduller.Konfigurasyon.Servisler;

namespace VizitLink3D.Api.Moduller.Konfigurasyon.Servisler;

/// <summary>
/// Embed oturum cookie yardimci sinifi.
/// 
/// GUVENLIK:
/// - Cookie HttpOnly=true: JavaScript erisemez (XSS korumasi)
/// - Cookie Secure=true: sadece HTTPS uzerinden gonderilir
/// - Cookie SameSite=None: cross-site iframe embed icin gerekli
/// - Cookie Path=/konfigurator/embed: dar kapsam
/// - Cookie MaxAge 30 dk: session token TTL'i ile uyumlu
/// - Cookie IsEssential=true: GDPR onayina bagli olmadan calisir
/// 
/// CHIPS (Cookies Having Independent Partitioned State):
/// - Partitioned cookie henuz tum tarayicilarda desteklenmedigi icin
///   SameSite=None + Secure kullaniliyor.
/// - Cross-site cookie engellenirse (Safari ITP, Brave Shields, Firefox ETP Strict)
///   runtime anlasilir hata verir, token'i tekrar expose ETMEZ.
/// </summary>
public static class EmbedSessionYardimci
{
    public const string COOKIE_ADI = "vt3d_embed_oturum";
    public const string BAGLAM_PAYLOAD_ANAHTARI = "EmbedSessionPayload";

    /// <summary>
    /// Embed session cookie'si olusturur ve response'a ekler.
    /// </summary>
    public static void CookieYaz(HttpContext baglam, EmbedTokenPayload payload, IEmbedTokenServisi tokenServisi)
    {
        var sessionToken = tokenServisi.SessionTokenOlustur(payload);

        var ayarlar = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/konfigurator/embed",
            MaxAge = IEmbedTokenServisi.SessionGecerlilik,
            IsEssential = true
        };

        baglam.Response.Cookies.Append(COOKIE_ADI, sessionToken, ayarlar);
    }

    /// <summary>
    /// Embed session cookie'sinden payload'i dogrular ve dondurur.
    /// Cookie yoksa veya gecersizse null doner.
    /// </summary>
    public static EmbedTokenPayload? CookieDogrula(HttpContext baglam, IEmbedTokenServisi tokenServisi)
    {
        var sessionToken = baglam.Request.Cookies[COOKIE_ADI];
        if (string.IsNullOrWhiteSpace(sessionToken))
            return null;

        return tokenServisi.SessionTokenDogrula(sessionToken);
    }

    /// <summary>
    /// Embed session cookie'sini siler (logout/cikis).
    /// </summary>
    public static void CookieSil(HttpContext baglam)
    {
        var ayarlar = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/konfigurator/embed",
            MaxAge = TimeSpan.FromDays(-1),
            IsEssential = true
        };

        baglam.Response.Cookies.Append(COOKIE_ADI, "", ayarlar);
    }


}
