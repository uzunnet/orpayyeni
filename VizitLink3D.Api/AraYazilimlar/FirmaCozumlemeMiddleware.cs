using VizitLink3D.Api.VeriTabani;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.AraYazilimlar;

public class FirmaCozumlemeMiddleware(RequestDelegate sonraki, IConfiguration konfigurasyon)
{
    public async Task InvokeAsync(HttpContext baglam)
    {
        using var kapsam = baglam.RequestServices.CreateScope();
        var vt = kapsam.ServiceProvider.GetRequiredService<VizitLink3DDbContext>();

        var host = baglam.Request.Host.Host.ToLowerInvariant();
        var varsayilanFirmaSlug = konfigurasyon["Saas:VarsayilanFirmaSlug"] ?? "orpay";
        var localTekFirmaZorla = bool.TryParse(konfigurasyon["Saas:LocalTekFirmaZorla"], out var localZorla) && localZorla;
        var headerIleFirmaGecisiAktif = bool.TryParse(konfigurasyon["Saas:HeaderIleFirmaGecisiAktif"], out var headerGecisi) && headerGecisi;
        var queryIleFirmaGecisiAktif = bool.TryParse(konfigurasyon["Saas:QueryIleFirmaGecisiAktif"], out var queryGecisi) && queryGecisi;
        var localhostMu = host is "localhost" or "127.0.0.1" or "::1";

        string? firmaSlug = null;

        if (localhostMu && localTekFirmaZorla)
        {
            firmaSlug = varsayilanFirmaSlug;
        }
        else
        {
            // 1. X-Firma başlığı (isteğe bağlı)
            if (headerIleFirmaGecisiAktif)
                firmaSlug = baglam.Request.Headers["X-Firma"].FirstOrDefault();

            // 2. Query parametresi (isteğe bağlı)
            if (string.IsNullOrEmpty(firmaSlug) && queryIleFirmaGecisiAktif)
                firmaSlug = baglam.Request.Query["firma"].FirstOrDefault();
        }

        if (!string.IsNullOrEmpty(firmaSlug))
        {
            var secilenFirma = await vt.Firmalar
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Slug == firmaSlug && f.AktifMi);

            if (secilenFirma != null)
            {
                baglam.Items["FirmaId"] = secilenFirma.Id;
                baglam.Items["FirmaDomain"] = secilenFirma.Domain;
                baglam.Items["FirmaSlug"] = secilenFirma.Slug;
                baglam.Items["FirmaAd"] = secilenFirma.Ad;
                await sonraki(baglam);
                return;
            }
        }

        // Üretim: domain eşleştirme
        if (!localhostMu)
        {
            var firma = await vt.Firmalar
                .AsNoTracking()
                .FirstOrDefaultAsync(f =>
                    (f.Domain == host || f.YedekDomain == host) && f.AktifMi);

            if (firma != null)
            {
                baglam.Items["FirmaId"] = firma.Id;
                baglam.Items["FirmaDomain"] = firma.Domain;
                baglam.Items["FirmaSlug"] = firma.Slug;
                baglam.Items["FirmaAd"] = firma.Ad;
                await sonraki(baglam);
                return;
            }
        }

        // Varsayılan firma
        var varsayilanFirma = await vt.Firmalar
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Slug == varsayilanFirmaSlug && f.AktifMi);

        if (varsayilanFirma != null)
        {
            baglam.Items["FirmaId"] = varsayilanFirma.Id;
            baglam.Items["FirmaDomain"] = varsayilanFirma.Domain;
            baglam.Items["FirmaSlug"] = varsayilanFirma.Slug;
            baglam.Items["FirmaAd"] = varsayilanFirma.Ad;
        }

        await sonraki(baglam);
    }
}
