using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.AraYazilimlar;

public class FirmaCozumlemeMiddleware(RequestDelegate sonraki, IConfiguration konfigurasyon)
{
    public async Task InvokeAsync(HttpContext baglam)
    {
        using var kapsam = baglam.RequestServices.CreateScope();
        var vt = kapsam.ServiceProvider.GetRequiredService<VizitLink3DDbContext>();

        var host = baglam.Request.Host.Host.ToLowerInvariant();
        var varsayilanFirmaSlug = konfigurasyon["Saas:VarsayilanFirmaSlug"] ?? "platform";
        var localhostMu = host is "localhost" or "127.0.0.1" or "::1" || host.EndsWith(".orca.localhost");

        Firma? secilenFirma = null;

        // Localhost / Orca: hostname'den slug cikar veya varsayilan kullan
        if (localhostMu)
        {
            var slug = varsayilanFirmaSlug;
            // Orca hostname: orpay-2.orca.localhost -> orpay
            if (host.EndsWith(".orca.localhost"))
            {
                var onEk = host.Split('.')[0];       // orpay-2
                slug = onEk.Split('-')[0];          // orpay
            }
            // X-Firma header (Blazor WASM tarayici fetch)
            var xFirma = baglam.Request.Headers["X-Firma"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xFirma))
            {
                if (xFirma.Contains(".orca.localhost")) { var onEk2 = xFirma.Split(".")[0]; slug = onEk2.Split("-")[0]; }
                else { var firma = await vt.Firmalar.AsNoTracking().FirstOrDefaultAsync(f => f.Domain == xFirma && f.AktifMi); if (firma != null) slug = firma.Slug; }
            }
            secilenFirma = await vt.Firmalar
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Slug == slug && f.AktifMi);
        }
        else
        {
            // Uretim: sadece Host header'dan domain eslestirme
            secilenFirma = await vt.Firmalar
                .AsNoTracking()
                .FirstOrDefaultAsync(f =>
                    (f.Domain == host || f.YedekDomain == host) && f.AktifMi);
        }

        // Domain eslesmezse varsayilan firmaya dus
        if (secilenFirma == null)
        {
            secilenFirma = await vt.Firmalar
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Slug == varsayilanFirmaSlug && f.AktifMi)
                ?? await vt.Firmalar.AsNoTracking().FirstOrDefaultAsync(f => f.AktifMi);
        }

        if (secilenFirma != null)
        {
            baglam.Items["FirmaId"] = secilenFirma.Id;
            baglam.Items["FirmaDomain"] = secilenFirma.Domain;
            baglam.Items["FirmaSlug"] = secilenFirma.Slug;
            baglam.Items["FirmaAd"] = secilenFirma.Ad;

            // FAZ 3: Her firma icin dinamik DB, medya, i18n yollari
            baglam.Items["VeriTabaniYolu"] = Path.Combine("firmalar", secilenFirma.Slug, $"{secilenFirma.Slug}.db");
            baglam.Items["MedyaKlasoru"] = Path.Combine("firmalar", secilenFirma.Slug, "medya");
            baglam.Items["I18nKlasoru"] = Path.Combine("firmalar", secilenFirma.Slug, "i18n");

            // FAZ 3: Modul yetki kontrolu icin aktif modul kodlari (JSON string)
            baglam.Items["AktifModulKodlari"] = secilenFirma.AktifModulKodlariJson ?? string.Empty;
        }

        await sonraki(baglam);
    }
}