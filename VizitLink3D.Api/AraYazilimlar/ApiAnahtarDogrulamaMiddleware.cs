using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Api.Moduller.Konfigurasyon.Servisler;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.Api.AraYazilimlar;

/// <summary>
/// Embed/Entegrasyon API endpoint'lerinde API anahtarı doğrulaması yapar.
/// Header: X-Konfigurator-Anahtari (öncelikli) veya X-Api-Key (geriye uyumlu).
/// API anahtarı URL/query'de ASLA kullanılmaz — sadece header ile taşınır.
/// 
/// COOKIE TABANLI EMBED AUTH (Paket-4C):
///   /api/embed/konfigurator/veri → Embed session cookie ile de dogrulanabilir.
///   Cookie varsa ve gecerliyse API key zorunlu DEGILDIR.
///   Diger /api/embed/* yollari cookie tanimaz — API key zorunludur.
/// 
/// Kapsam kuralları:
///   /api/embed/*       → "Embed" kapsamı zorunlu, origin zorunlu
///   /api/entegrasyon/* → "SunucuEntegrasyonu" kapsamı zorunlu, origin kontrolü YOK
/// 
/// Tenant izolasyonu: API anahtarının FirmaId'si host/default tenant'ı EZER.
/// Body/query'den tenant bilgisi kabul edilmez.
/// JWT auth varsa API key kontrolü atlanır (first-party akışı kırılmaz).
/// </summary>
public class ApiAnahtarDogrulamaMiddleware
{
    private readonly RequestDelegate _sonraki;
    private static readonly string[] KorunanYolOnekleri = ["/api/embed/", "/api/entegrasyon/"];

    /// <summary>Cookie tabanli auth'a izin verilen /api/embed/* alt yollari.</summary>
    private static readonly string[] CookieIzinliEmbedYollari = ["/api/embed/konfigurator/veri"];

    /// <summary>Header adları — öncelik sırasıyla</summary>
    private static readonly string[] AnahtarHeaderAdlari = ["X-Konfigurator-Anahtari", "X-Api-Key"];

    public ApiAnahtarDogrulamaMiddleware(RequestDelegate sonraki)
    {
        _sonraki = sonraki;
    }

    public async Task InvokeAsync(HttpContext baglam)
    {
        var yol = baglam.Request.Path.Value?.ToLowerInvariant() ?? "";

        // Sadece korunan yol önekleri için API key doğrulaması yap
        var korunanYol = KorunanYolOnekleri.FirstOrDefault(yol.StartsWith);
        if (korunanYol is null)
        {
            await _sonraki(baglam);
            return;
        }

        // JWT ile kimlik doğrulanmışsa API key kontrolüne gerek yok (admin/first-party akışı)
        if (baglam.User.Identity?.IsAuthenticated == true)
        {
            await _sonraki(baglam);
            return;
        }

        // ================================================================
        // COOKIE TABANLI EMBED AUTH (Paket-4C)
        // Sadece belirli /api/embed/* yollarinda embed session cookie kontrolu
        // ================================================================
        if (yol.StartsWith("/api/embed/") && CookieIzinliEmbedYollari.Any(y => yol.Equals(y, StringComparison.OrdinalIgnoreCase)))
        {
            var embedTokenServisi = baglam.RequestServices.GetRequiredService<IEmbedTokenServisi>();
            var cookiePayload = EmbedSessionYardimci.CookieDogrula(baglam, embedTokenServisi);

            if (cookiePayload is not null)
            {
                // Cookie gecerli → API key kontrolune gerek yok
                // Tenant bilgilerini cookie payload'indan HttpContext'e yaz
                baglam.Items["FirmaId"] = cookiePayload.FirmaId;
                baglam.Items["ApiKeyFirmaId"] = cookiePayload.FirmaId;
                baglam.Items["EmbedSessionPayload"] = cookiePayload;

                await _sonraki(baglam);
                return;
            }
            // Cookie yok/gecersiz → asagidaki API key kontrolune devam et
        }

        // API anahtarını header'dan al — query/URL'den ASLA
        var apiKey = AnahtarHeaderAdlari
            .Select(h => baglam.Request.Headers[h].FirstOrDefault())
            .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            baglam.Response.StatusCode = 401;
            baglam.Response.ContentType = "application/json; charset=utf-8";
            await baglam.Response.WriteAsJsonAsync(Cevap<object>.Hata(
                "API anahtarı gerekli. Header: X-Konfigurator-Anahtari ile gönderin."));
            return;
        }

        // API anahtarını hash'le
        var apiKeyHash = Sha256Hash(apiKey);

        // API anahtarı veya hash'i ASLA loglanmaz

        // Veritabanından hash ile anahtarı bul
        using var kapsam = baglam.RequestServices.CreateScope();
        var vt = kapsam.ServiceProvider.GetRequiredService<VizitLink3DDbContext>();

        var anahtarKaydi = await vt.FirmaApiAnahtarlari
            .AsNoTracking()
            .Include(a => a.Firma)
            .FirstOrDefaultAsync(a => a.ApiKeyHash == apiKeyHash);

        if (anahtarKaydi is null)
        {
            baglam.Response.StatusCode = 401;
            baglam.Response.ContentType = "application/json; charset=utf-8";
            await baglam.Response.WriteAsJsonAsync(Cevap<object>.Hata("Geçersiz API anahtarı."));
            return;
        }

        // Geçerlilik kontrolleri (aktif mi + süresi dolmuş mu + silinmiş mi)
        if (!anahtarKaydi.GecerliMi())
        {
            baglam.Response.StatusCode = 403;
            baglam.Response.ContentType = "application/json; charset=utf-8";

            var sebep = !anahtarKaydi.AktifMi
                ? "API anahtarı devre dışı bırakılmış."
                : anahtarKaydi.SuresiDolduMu()
                    ? "API anahtarının süresi dolmuş."
                    : "API anahtarı geçersiz.";

            await baglam.Response.WriteAsJsonAsync(Cevap<object>.Hata(sebep));
            return;
        }

        // Yol bazlı kapsam kontrolü
        var embedYoluMu = yol.StartsWith("/api/embed/");
        var entegrasyonYoluMu = yol.StartsWith("/api/entegrasyon/");

        if (embedYoluMu)
        {
            // Embed kapsamı zorunlu
            if (!anahtarKaydi.KapsamVarMi("Embed"))
            {
                baglam.Response.StatusCode = 403;
                baglam.Response.ContentType = "application/json; charset=utf-8";
                await baglam.Response.WriteAsJsonAsync(Cevap<object>.Hata(
                    "API anahtarı bu işlem için yetkili değil. 'Embed' kapsamı gerekli."));
                return;
            }

            // Origin kontrolü (Embed için zorunlu)
            var origin = baglam.Request.Headers.Origin.FirstOrDefault()
                ?? baglam.Request.Headers.Referer.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(origin))
            {
                baglam.Response.StatusCode = 403;
                baglam.Response.ContentType = "application/json; charset=utf-8";
                await baglam.Response.WriteAsJsonAsync(Cevap<object>.Hata(
                    "Embed istekleri için Origin/Referer header'ı zorunludur."));
                return;
            }

            if (!anahtarKaydi.OriginIzınliMi(origin))
            {
                baglam.Response.StatusCode = 403;
                baglam.Response.ContentType = "application/json; charset=utf-8";
                // Origin değerini hataya yazma — bilgi sızması riski
                await baglam.Response.WriteAsJsonAsync(Cevap<object>.Hata(
                    "Bu origin için API anahtarı yetkili değil."));
                return;
            }
        }
        else if (entegrasyonYoluMu)
        {
            // SunucuEntegrasyonu kapsamı zorunlu
            if (!anahtarKaydi.KapsamVarMi("SunucuEntegrasyonu"))
            {
                baglam.Response.StatusCode = 403;
                baglam.Response.ContentType = "application/json; charset=utf-8";
                await baglam.Response.WriteAsJsonAsync(Cevap<object>.Hata(
                    "API anahtarı bu işlem için yetkili değil. 'SunucuEntegrasyonu' kapsamı gerekli."));
                return;
            }

            // Sunucu entegrasyonunda origin kontrolü YAPILMAZ
            // (server-to-server isteklerde Origin header'ı olmayabilir)
        }
        else
        {
            // Tanınmayan korunan yol — güvenlik gereği reddet
            baglam.Response.StatusCode = 403;
            baglam.Response.ContentType = "application/json; charset=utf-8";
            await baglam.Response.WriteAsJsonAsync(Cevap<object>.Hata("Geçersiz API yolu."));
            return;
        }

        // === TENANT İZOLASYONU ===
        // API anahtarının FirmaId'si host/default tenant'ı EZER.
        // Body/query'den tenant bilgisi kabul edilmez.
        if (anahtarKaydi.FirmaId > 0)
        {
            baglam.Items["FirmaId"] = anahtarKaydi.FirmaId;

            // Firma slug/ad bilgilerini de güncelle (varsa)
            if (!string.IsNullOrWhiteSpace(anahtarKaydi.Firma?.Slug))
                baglam.Items["FirmaSlug"] = anahtarKaydi.Firma.Slug;
            if (!string.IsNullOrWhiteSpace(anahtarKaydi.Firma?.Domain))
                baglam.Items["FirmaDomain"] = anahtarKaydi.Firma.Domain;
        }

        // API key bilgilerini HttpContext'e ekle (handler'lar için)
        baglam.Items["ApiKeyId"] = anahtarKaydi.Id;
        baglam.Items["ApiKeyFirmaId"] = anahtarKaydi.FirmaId;
        baglam.Items["ApiKeyKapsam"] = anahtarKaydi.Kapsam;

        // Son kullanım tarihini güncelle (arka planda, isteği bloklamadan)
        _ = Task.Run(async () =>
        {
            try
            {
                using var guncellemeKapsami = baglam.RequestServices.CreateScope();
                var guncellemeVt = guncellemeKapsami.ServiceProvider.GetRequiredService<VizitLink3DDbContext>();
                await guncellemeVt.FirmaApiAnahtarlari
                    .Where(a => a.Id == anahtarKaydi.Id)
                    .ExecuteUpdateAsync(s => s.SetProperty(a => a.SonKullanimTarihi, DateTime.UtcNow));
            }
            catch
            {
                // Son kullanım güncelleme hatası ana isteği etkilemez
            }
        });

        await _sonraki(baglam);
    }

    /// <summary>
    /// SHA256 hash hesaplar. API anahtarı loglanmaz.
    /// </summary>
    public static string Sha256Hash(string metin)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(metin));
        return Convert.ToHexStringLower(hashBytes);
    }
}
