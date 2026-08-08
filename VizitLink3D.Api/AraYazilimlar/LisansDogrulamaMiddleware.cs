using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Api.Servisler.Kimlik;
using System.Net;

namespace VizitLink3D.Api.AraYazilimlar;

public class LisansDogrulamaMiddleware(RequestDelegate sonraki, ILogger<LisansDogrulamaMiddleware> logger)
{
    // localhost ve Coolify'in otomatik test domaini (*.sslip.io) lisanstan muaf —
    // gercek production domaini (3dvizitlink.com.tr) seed'de lisansli. sslip.io gecici
    // test/staging erisimi oldugundan lisanssiz tam calismali.
    private static readonly string[] YerelDomainler = { "localhost", "127.0.0.1", "::1", "sslip.io", "loca.lt", "192.168.", "10.", "172.", "orpay.uzunreklam.com" };
    private static readonly string[] PublicGetYollari =
    {
        "/api/kapak-modelleri", "/api/menu", "/api/vizitlink3d",
        "/api/urunler", "/api/renkler", "/api/malzemeler", "/api/kaplamalar",
        // Urun/katalog gorselleri ve medya dosyalari herkese acik icerik —
        // lisanssiz da servis edilmeli (aksi halde production'da gorseller 402 doner).
        "/api/medya/dosya", "/api/medya-havuzu/dosya"
    };

    // Login, dil ve DB yukleme gibi yollar hem GET hem POST'a aciktir —
    // kullanici giris yapmadan lisans durumunu gorebilmeli.
    private static readonly string[] BagimsizPublicYollar =
    {
        "/api/kimlik/giris",
        "/api/dil",
        "/api/db-yukle"
    };

    public async Task InvokeAsync(HttpContext baglam)
    {
        var host = baglam.Request.Host.Host.ToLowerInvariant();

        if (YerelDomainler.Any(d => host.Contains(d)))
        {
            await sonraki(baglam);
            return;
        }

        // Bagimsiz yollar: Hem GET hem POST'a acik
        if (BagimsizPublicYollar.Any(y => baglam.Request.Path.StartsWithSegments(y)))
        {
            await sonraki(baglam);
            return;
        }

        // Diger public yollar: Sadece GET
        if (baglam.Request.Method == "GET" &&
            PublicGetYollari.Any(y => baglam.Request.Path.StartsWithSegments(y)))
        {
            await sonraki(baglam);
            return;
        }

        // Lisans kontrolü
        using var kapsam = baglam.RequestServices.CreateScope();
        // LisansDogrulama her zaman ana VT'ye (vizitlink3d.db) baglanmali,
        // firmaya ait DB'ye degil. Yogunluk kaynagini dogrudan kullan.
        var anaVt = kapsam.ServiceProvider.GetRequiredService<AnaVizitLink3DDbContext>();
        var lisansServisi = new VizitLink3D.Api.Servisler.Kimlik.LisansServisi(
            anaVt,
            kapsam.ServiceProvider.GetRequiredService<ILogger<LisansServisi>>(),
            kapsam.ServiceProvider.GetRequiredService<IConfiguration>());
        var durum = await lisansServisi.DomainKontrolAsync(host);

        if (!durum.GecerliMi)
        {
            logger.LogWarning("Lisans dogrulamasi basarisiz. {Host} {Sebep}", host, durum.Sebep);
            baglam.Response.StatusCode = 402;
            baglam.Response.ContentType = "application/json; charset=utf-8";
            var mesaj = System.Text.Json.JsonSerializer.Serialize(new
            {
                basariliMi = false,
                mesaj = durum.Sebep,
                kalanGun = durum.KalanGun,
                ekSuredeMi = durum.EkSuredeMi
            });
            await baglam.Response.WriteAsync(mesaj);
            return;
        }

        // Uyari header'lari (UI'da gostermek icin)
        if (durum.Uyari != null)
        {
            baglam.Response.OnStarting(() =>
            {
                baglam.Response.Headers["X-Lisans-Kalan-Gun"] = durum.KalanGun.ToString();
                baglam.Response.Headers["X-Lisans-Uyari"] = durum.Uyari;
                baglam.Response.Headers["X-Lisans-Bitis"] = durum.BitisTarihi?.ToString("yyyy-MM-dd");
                baglam.Response.Headers["X-Lisans-EkSure"] = durum.EkSuredeMi ? "true" : "false";
                return Task.CompletedTask;
            });
        }

        await sonraki(baglam);
    }
}
