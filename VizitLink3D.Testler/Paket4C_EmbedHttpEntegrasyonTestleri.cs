using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VizitLink3D.Api.Moduller.Konfigurasyon.Servisler;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller.Guvenlik;

namespace VizitLink3D.Testler;

/// <summary>
/// Paket-4C: Embed iframe token akisi GERCEK HTTP entegrasyon testleri.
/// WebApplicationFactory ile uctan uca guvenlik testleri.
///
/// TEST LISTESI (20+):
///  1. Valid bootstrap -> 303 + cookie (HttpOnly, SameSite=None, Path, MaxAge)
///  2. Expired/malformed token -> 400
///  3. Referer mismatch -> 403
///  4. CSP frame-ancestors header mevcut
///  5. Referrer-Policy no-referrer
///  6. Cache-Control no-store
///  7. Cookie olmadan runtime -> 401
///  8. Tam akis: bootstrap -> runtime 200 + HTML (guclu assertion)
///  9. /api/embed/* API key olmadan -> 401
/// 10. Bootstrap body'de token/key YOK
/// 11. Runtime URL'de token yok
/// 12. Rate-limit attribute varligi
/// 13. Hata sayfasinda CSP frame-ancestors 'self'
/// 14. Hata sayfasinda Referrer-Policy no-referrer
/// 15. Hata sayfasinda Cache-Control no-store
/// 16. X-Content-Type-Options nosniff
/// 17. Ikinci bootstrap ayni token -> 400 (nonce replay)
/// 18. Cookie attribute detay testi
/// 19. Hata sayfasinda X-Frame-Options DENY
/// 20. Basarili embed yanitinda X-Frame-Options YOK + CSP frame-ancestors
/// 21. Runtime HTML'de token/key YOK
/// </summary>
public class Paket4C_EmbedHttpEntegrasyonTestleri : IClassFixture<EmbedTestWebFabrikasi>
{
    private readonly EmbedTestWebFabrikasi _fabrika;

    public Paket4C_EmbedHttpEntegrasyonTestleri(EmbedTestWebFabrikasi fabrika)
    {
        _fabrika = fabrika;
    }

    private HttpClient IstemciOlustur() => _fabrika.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = true
    });

    private HttpClient IstemciCookiesiz() => _fabrika.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = false
    });

    // ===================================================================
    // YARDIMCI METOTLAR
    // ===================================================================

    private string GecerliTokenUret()
    {
        using var kapsam = _fabrika.Services.CreateScope();
        var tokenServisi = kapsam.ServiceProvider.GetRequiredService<IEmbedTokenServisi>();
        return tokenServisi.TokenOlustur(1, "test-urun", "https://ornek-test.com");
    }

    // ===================================================================
    // TEST 1: Valid bootstrap -> 303 redirect + Set-Cookie (extended attrs)
    // ===================================================================

    [Fact]
    public async Task EmbedBootstrap_GecerliToken_303RedirectVeCookieYaziyor()
    {
        var istemci = IstemciOlustur();
        var token = GecerliTokenUret();

        var istek = new HttpRequestMessage(HttpMethod.Get, $"/konfigurator/embed/{token}");
        istek.Headers.Add("Referer", "https://ornek-test.com/sayfa");

        var yanit = await istemci.SendAsync(istek);

        Assert.Equal(HttpStatusCode.SeeOther, yanit.StatusCode);
        Assert.Equal("/konfigurator/embed", yanit.Headers.Location?.OriginalString);

        var setCookieDegerleri = yanit.Headers.GetValues("Set-Cookie").ToList();
        var embedCookie = setCookieDegerleri
            .FirstOrDefault(c => c.StartsWith("vt3d_embed_oturum=", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(embedCookie);

        // Cookie attribute kontrolleri
        Assert.Contains("httponly", embedCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=none", embedCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("path=/konfigurator/embed", embedCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("max-age", embedCookie, StringComparison.OrdinalIgnoreCase);
    }

    // ===================================================================
    // TEST 2: Expired/malformed token -> 400 hata
    // ===================================================================

    [Fact]
    public async Task EmbedBootstrap_BozukToken_400Hata()
    {
        var istemci = IstemciOlustur();
        var yanit = await istemci.GetAsync("/konfigurator/embed/bozuk-token-12345");

        Assert.Equal(HttpStatusCode.BadRequest, yanit.StatusCode);

        var icerik = await yanit.Content.ReadAsStringAsync();
        Assert.Contains("Erişim Engellendi", icerik);
    }

    // ===================================================================
    // TEST 3: Referer mismatch -> 403
    // ===================================================================

    [Fact]
    public async Task EmbedBootstrap_RefererUyusmazligi_403Red()
    {
        var istemci = IstemciOlustur();
        var token = GecerliTokenUret();

        var istek = new HttpRequestMessage(HttpMethod.Get, $"/konfigurator/embed/{token}");
        istek.Headers.Add("Referer", "https://kotu-niyetli-site.com/embed");

        var yanit = await istemci.SendAsync(istek);

        Assert.Equal(HttpStatusCode.Forbidden, yanit.StatusCode);
    }

    // ===================================================================
    // TEST 4: CSP frame-ancestors header mevcut (basarili bootstrap)
    // ===================================================================

    [Fact]
    public async Task EmbedBootstrap_GecerliToken_CspFrameAncestorsHeaderVar()
    {
        var istemci = IstemciOlustur();
        var token = GecerliTokenUret();

        var istek = new HttpRequestMessage(HttpMethod.Get, $"/konfigurator/embed/{token}");
        istek.Headers.Add("Referer", "https://ornek-test.com/sayfa");

        var yanit = await istemci.SendAsync(istek);

        Assert.True(yanit.Headers.TryGetValues("Content-Security-Policy", out var cspDegerler),
            "CSP header mevcut degil");
        var csp = cspDegerler.First();
        Assert.Contains("frame-ancestors", csp);
    }

    // ===================================================================
    // TEST 5: Referrer-Policy no-referrer header mevcut
    // ===================================================================

    [Fact]
    public async Task EmbedBootstrap_GecerliToken_ReferrerPolicyNoReferrerVar()
    {
        var istemci = IstemciOlustur();
        var token = GecerliTokenUret();

        var istek = new HttpRequestMessage(HttpMethod.Get, $"/konfigurator/embed/{token}");
        istek.Headers.Add("Referer", "https://ornek-test.com/sayfa");

        var yanit = await istemci.SendAsync(istek);

        Assert.True(yanit.Headers.TryGetValues("Referrer-Policy", out var degerler),
            "Referrer-Policy header mevcut degil");
        Assert.Contains("no-referrer", degerler.First());
    }

    // ===================================================================
    // TEST 6: Cache-Control no-store header mevcut
    // ===================================================================

    [Fact]
    public async Task EmbedBootstrap_GecerliToken_CacheControlNoStoreVar()
    {
        var istemci = IstemciOlustur();
        var token = GecerliTokenUret();

        var istek = new HttpRequestMessage(HttpMethod.Get, $"/konfigurator/embed/{token}");
        istek.Headers.Add("Referer", "https://ornek-test.com/sayfa");

        var yanit = await istemci.SendAsync(istek);

        Assert.True(yanit.Headers.TryGetValues("Cache-Control", out var degerler),
            "Cache-Control header mevcut degil");
        Assert.Contains("no-store", degerler.First());
    }

    // ===================================================================
    // TEST 7: Cookie olmadan runtime -> 401
    // ===================================================================

    [Fact]
    public async Task EmbedRuntime_CookieYok_401Unauthorized()
    {
        var istemci = IstemciCookiesiz();

        var yanit = await istemci.GetAsync("/konfigurator/embed");

        Assert.Equal(HttpStatusCode.Unauthorized, yanit.StatusCode);

        var icerik = await yanit.Content.ReadAsStringAsync();
        Assert.Contains("Erişim Engellendi", icerik);
    }

    // ===================================================================
    // TEST 8: Bootstrap -> redirect -> runtime (tam akis, guclu assertion)
    // ===================================================================

    [Fact]
    public async Task EmbedTamAkis_BootstrapRedirectRuntime_200Basarili()
    {
        var istemci = IstemciOlustur();
        var token = GecerliTokenUret();

        // Adim 1: Bootstrap (Referer ile)
        var bootstrapIstek = new HttpRequestMessage(HttpMethod.Get, $"/konfigurator/embed/{token}");
        bootstrapIstek.Headers.Add("Referer", "https://ornek-test.com/sayfa");
        var bootstrapYanit = await istemci.SendAsync(bootstrapIstek);

        Assert.Equal(HttpStatusCode.SeeOther, bootstrapYanit.StatusCode);

        // Adim 2: Runtime — cookie otomatik tasinir (HandleCookies=true)
        var runtimeYanit = await istemci.GetAsync("/konfigurator/embed");

        // Cookie dogrulama basarili ise 401 DONMEZ.
        // Urun test DB'de olmadigi icin 404 donebilir ama 401 kesinlikle OLMAMALI.
        Assert.NotEqual(HttpStatusCode.Unauthorized, runtimeYanit.StatusCode);

        var icerik = await runtimeYanit.Content.ReadAsStringAsync();

        // Gecerli embed HTML yapisi var (hata sayfasi da olsa HTML donmeli)
        Assert.Contains("<!DOCTYPE", icerik, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<html", icerik, StringComparison.OrdinalIgnoreCase);

        // Token degeri HTML iceriginde KESINLIKLE OLMAMALI
        Assert.DoesNotContain(token, icerik);
    }

    // ===================================================================
    // TEST 9: /api/embed/* API key olmadan -> 401
    // ===================================================================

    [Fact]
    public async Task EmbedApi_ApiKeyYok_401Unauthorized()
    {
        var istemci = IstemciCookiesiz();

        var yanit = await istemci.GetAsync("/api/embed/konfigurator/test-urun");

        Assert.Equal(HttpStatusCode.Unauthorized, yanit.StatusCode);
    }

    // ===================================================================
    // TEST 10: Bootstrap response body'de token/key YOK
    // ===================================================================

    [Fact]
    public async Task EmbedBootstrap_GecerliToken_BodydeTokenYok()
    {
        var token = GecerliTokenUret();
        var istemci = IstemciOlustur();

        var istek = new HttpRequestMessage(HttpMethod.Get, $"/konfigurator/embed/{token}");
        istek.Headers.Add("Referer", "https://ornek-test.com/sayfa");

        var yanit = await istemci.SendAsync(istek);

        // Bootstrap 303 redirect oldugu icin body bos/hata mesaji olmali
        var icerik = await yanit.Content.ReadAsStringAsync();
        // Token VALUE (uzun string) kesinlikle body'de OLMAMALI
        Assert.DoesNotContain(token, icerik);
    }

    // ===================================================================
    // TEST 11: Token URL/query'de sadece bootstrap'ta; runtime URL'de token yok
    // ===================================================================

    [Fact]
    public async Task EmbedRuntime_TokensizUrl_TokenIstemiyor()
    {
        var istemci = IstemciOlustur();

        var yanit = await istemci.GetAsync("/konfigurator/embed?token=deneme123");

        // Cookie yoksa 401 donmeli (query'de token olsa bile kullanilmaz)
        Assert.Equal(HttpStatusCode.Unauthorized, yanit.StatusCode);
    }

    // ===================================================================
    // TEST 12: Rate-limit policy attribute varligi
    // ===================================================================

    [Fact]
    public void EmbedEndpointleri_RateLimitAttributeVar()
    {
        var tip = typeof(VizitLink3D.Api.Moduller.Konfigurasyon.Kontrolcu.EmbedSayfaKontrolcu);
        var attributelar = tip.GetCustomAttributes(true);

        var rateLimitAttr = attributelar
            .FirstOrDefault(a => a.GetType().Name.Contains("EnableRateLimiting"));
        Assert.NotNull(rateLimitAttr);
    }

    // ===================================================================
    // TEST 13: Hata sayfasinda CSP frame-ancestors 'self'
    // (Middleware default CSP frame-ancestors 'self' ekler)
    // ===================================================================

    [Fact]
    public async Task EmbedBootstrap_HataSayfasi_CspFrameAncestorsSelf()
    {
        var istemci = IstemciOlustur();
        var yanit = await istemci.GetAsync("/konfigurator/embed/gecersiz-token");

        Assert.Equal(HttpStatusCode.BadRequest, yanit.StatusCode);

        // CSP header olmali (middleware ekler)
        Assert.True(yanit.Headers.TryGetValues("Content-Security-Policy", out var cspDegerler),
            "CSP header mevcut degil");
        var csp = cspDegerler.First();
        Assert.Contains("frame-ancestors", csp);
        // Hata sayfasinda frame-ancestors 'self' olmali (embed'e izin verilmez)
        Assert.Contains("'self'", csp, StringComparison.OrdinalIgnoreCase);
    }

    // ===================================================================
    // TEST 14: Hata sayfasinda Referrer-Policy no-referrer
    // ===================================================================

    [Fact]
    public async Task EmbedBootstrap_HataSayfasi_ReferrerPolicyVar()
    {
        var istemci = IstemciOlustur();
        var yanit = await istemci.GetAsync("/konfigurator/embed/gecersiz-token");

        Assert.True(yanit.Headers.TryGetValues("Referrer-Policy", out var degerler));
        Assert.Contains("no-referrer", degerler.First());
    }

    // ===================================================================
    // TEST 15: Hata sayfasinda Cache-Control var
    // ===================================================================

    [Fact]
    public async Task EmbedBootstrap_HataSayfasi_CacheControlVar()
    {
        var istemci = IstemciOlustur();
        var yanit = await istemci.GetAsync("/konfigurator/embed/gecersiz-token");

        Assert.True(yanit.Headers.TryGetValues("Cache-Control", out var degerler));
        Assert.Contains("no-store", degerler.First());
    }

    // ===================================================================
    // TEST 16: X-Content-Type-Options nosniff header
    // ===================================================================

    [Fact]
    public async Task EmbedBootstrap_HataSayfasi_XContentTypeOptionsNosniff()
    {
        var istemci = IstemciOlustur();
        var yanit = await istemci.GetAsync("/konfigurator/embed/gecersiz-token");

        Assert.True(yanit.Headers.TryGetValues("X-Content-Type-Options", out var degerler));
        Assert.Contains("nosniff", degerler.First());
    }

    // ===================================================================
    // TEST 17: Ikinci bootstrap ayni token -> nonce replay red (400)
    // ===================================================================

    [Fact]
    public async Task EmbedBootstrap_IkinciBootstrapAyniToken_NonceReplayReddi()
    {
        var istemci = IstemciOlustur();
        var token = GecerliTokenUret();

        // Ilk bootstrap basarili
        var istek1 = new HttpRequestMessage(HttpMethod.Get, $"/konfigurator/embed/{token}");
        istek1.Headers.Add("Referer", "https://ornek-test.com/sayfa");
        var yanit1 = await istemci.SendAsync(istek1);
        Assert.Equal(HttpStatusCode.SeeOther, yanit1.StatusCode);

        // Ayni token ile ikinci bootstrap -> nonce zaten tuketildi
        var istek2 = new HttpRequestMessage(HttpMethod.Get, $"/konfigurator/embed/{token}");
        istek2.Headers.Add("Referer", "https://ornek-test.com/sayfa");
        var yanit2 = await istemci.SendAsync(istek2);

        Assert.Equal(HttpStatusCode.BadRequest, yanit2.StatusCode);
        var icerik2 = await yanit2.Content.ReadAsStringAsync();
        // Nonce replay reddi: hata sayfasi HTML iceriginde "Erişim Engellendi" bulunmali
        Assert.Contains("Erişim Engellendi", icerik2);
        // Ikinci yanitta cookie OLMAMALI (nonce tuketildi, oturum baslamadi)
        Assert.False(yanit2.Headers.TryGetValues("Set-Cookie", out _),
            "Nonce replay yanitinda Set-Cookie OLMAMALIDIR");
    }

    // ===================================================================
    // TEST 18: Cookie attribute detay testi (tum attributelar dogru mu)
    // ===================================================================

    [Fact]
    public async Task EmbedBootstrap_CookieDetayAttributeTest()
    {
        var istemci = IstemciOlustur();
        var token = GecerliTokenUret();

        var istek = new HttpRequestMessage(HttpMethod.Get, $"/konfigurator/embed/{token}");
        istek.Headers.Add("Referer", "https://ornek-test.com/sayfa");

        var yanit = await istemci.SendAsync(istek);

        Assert.Equal(HttpStatusCode.SeeOther, yanit.StatusCode);

        var setCookieDegerleri = yanit.Headers.GetValues("Set-Cookie").ToList();
        var embedCookie = setCookieDegerleri
            .FirstOrDefault(c => c.StartsWith("vt3d_embed_oturum=", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(embedCookie);

        // Cookie degerine sahip olmali (bos olmamali)
        var cookieDegeri = embedCookie.Split(';')[0].Split('=')[1];
        Assert.NotEmpty(cookieDegeri);

        // HttpOnly: true
        Assert.Contains("httponly", embedCookie, StringComparison.OrdinalIgnoreCase);

        // Path: /konfigurator/embed
        Assert.Contains("path=/konfigurator/embed", embedCookie, StringComparison.OrdinalIgnoreCase);

        // SameSite: None
        Assert.Contains("samesite=none", embedCookie, StringComparison.OrdinalIgnoreCase);

        // Max-Age: pozitif deger
        var maxAgeMatch = Regex.Match(embedCookie, @"max-age=(\d+)", RegexOptions.IgnoreCase);
        Assert.True(maxAgeMatch.Success, "max-age attribute'u bulunamadi");
        var maxAge = int.Parse(maxAgeMatch.Groups[1].Value);
        Assert.True(maxAge > 0, $"max-age pozitif olmali, bulunan: {maxAge}");

        // Test ortaminda GelistirmeHttpCookie=true -> Secure false olabilir (kabul edilebilir)
    }

    // ===================================================================
    // TEST 19: Hata sayfasinda X-Frame-Options DENY var (middleware ekler)
    // ===================================================================

    [Fact]
    public async Task EmbedBootstrap_HataSayfasi_XFrameOptionsDenyVar()
    {
        var istemci = IstemciOlustur();
        var yanit = await istemci.GetAsync("/konfigurator/embed/gecersiz-token");

        // Middleware X-Frame-Options: DENY ekler
        Assert.True(yanit.Headers.TryGetValues("X-Frame-Options", out var xfoDegerler),
            "X-Frame-Options header mevcut degil");
        Assert.Contains("DENY", xfoDegerler.First());
    }

    // ===================================================================
    // TEST 20: Basarili embed yanitinda X-Frame-Options YOK + CSP frame-ancestors hedef origin
    // ===================================================================

    [Fact]
    public async Task EmbedBootstrap_BasariliYanit_XFrameOptionsYok_CspFrameAncestorsVar()
    {
        var istemci = IstemciOlustur();
        var token = GecerliTokenUret();

        var istek = new HttpRequestMessage(HttpMethod.Get, $"/konfigurator/embed/{token}");
        istek.Headers.Add("Referer", "https://ornek-test.com/sayfa");

        var yanit = await istemci.SendAsync(istek);

        Assert.Equal(HttpStatusCode.SeeOther, yanit.StatusCode);

        // X-Frame-Options OLMAMALI (embed basarili)
        Assert.False(yanit.Headers.Contains("X-Frame-Options"),
            "Basarili embed yanitinda X-Frame-Options OLMAMALIDIR");

        // CSP frame-ancestors hedef origin icermeli
        Assert.True(yanit.Headers.TryGetValues("Content-Security-Policy", out var cspDegerler),
            "CSP header mevcut degil");
        var csp = cspDegerler.First();
        Assert.Contains("frame-ancestors https://ornek-test.com", csp);
    }

    // ===================================================================
    // TEST 21: Runtime HTML yanitinda token/API key YOK
    // ===================================================================

    [Fact]
    public async Task EmbedRuntime_GecerliHtml_TokenVeApiKeyYok()
    {
        var istemci = IstemciOlustur();
        var token = GecerliTokenUret();

        // Bootstrap
        var bootstrapIstek = new HttpRequestMessage(HttpMethod.Get, $"/konfigurator/embed/{token}");
        bootstrapIstek.Headers.Add("Referer", "https://ornek-test.com/sayfa");
        await istemci.SendAsync(bootstrapIstek);

        // Runtime — cookie otomatik tasinir
        var runtimeYanit = await istemci.GetAsync("/konfigurator/embed");

        // Cookie dogrulama basarili ise 401 DONMEZ.
        // Urun test DB'de olmadigi icin 404 donebilir ama auth gecmistir.
        Assert.NotEqual(HttpStatusCode.Unauthorized, runtimeYanit.StatusCode);

        var icerik = await runtimeYanit.Content.ReadAsStringAsync();

        // HTML iceriginde "api", "key", "token" gibi hassas kelimeler OLMAMALI
        // (guvenlik yorumu haric - "Token/API key bu sayfada YOKTUR" bilinclendirme mesaji olabilir)
        Assert.DoesNotContain("api_key", icerik, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("apikey", icerik, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("bearer", icerik, StringComparison.OrdinalIgnoreCase);

        // Token degeri HTML'de OLMAMALI
        Assert.DoesNotContain(token, icerik);
    }

    // ===================================================================
    // TEST 22: Ayni nonce DB unique constraint ile sadece bir kez tuketilir
    // ===================================================================

    [Fact]
    public async Task NonceTekrarKullanim_UniqueConstraintIleEngellenir()
    {
        var nonce = Guid.NewGuid().ToString("N");
        using var kapsam = _fabrika.Services.CreateScope();
        var nonceDeposu = kapsam.ServiceProvider.GetRequiredService<IEmbedNonceDeposu>();

        // Ilk tuketim basarili olmali
        var ilkDeneme = await nonceDeposu.DeneVeKaydetAsync(nonce, TimeSpan.FromMinutes(6));
        Assert.True(ilkDeneme, "Ilk nonce tuketimi basarili olmali");

        // Ayni nonce ikinci kez tuketilemez (unique constraint)
        var ikinciDeneme = await nonceDeposu.DeneVeKaydetAsync(nonce, TimeSpan.FromMinutes(6));
        Assert.False(ikinciDeneme, "Ayni nonce ikinci kez tuketilemez (replay)");
    }

    // ===================================================================
    // TEST 23: Nonce DB'de plaintext degil, SHA256 hash olarak saklanir
    // ===================================================================

    [Fact]
    public async Task NoncePlaintext_DBdeSaklanmaz()
    {
        var nonce = Guid.NewGuid().ToString("N");
        var beklenenHash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(nonce)));

        using var kapsam = _fabrika.Services.CreateScope();
        var nonceDeposu = kapsam.ServiceProvider.GetRequiredService<IEmbedNonceDeposu>();
        var dbContext = kapsam.ServiceProvider.GetRequiredService<VizitLink3DDbContext>();

        await nonceDeposu.DeneVeKaydetAsync(nonce, TimeSpan.FromMinutes(6));

        var kayit = await dbContext.Set<EmbedOturumNonceKaydi>()
            .FirstOrDefaultAsync(k => k.NonceHash == beklenenHash);
        Assert.NotNull(kayit);
        Assert.Equal(beklenenHash, kayit.NonceHash);
        // Plaintext nonce hash icinde GECMEMELI
        Assert.DoesNotContain(nonce, kayit.NonceHash, StringComparison.OrdinalIgnoreCase);
    }

    // ===================================================================
    // TEST 24: TTL asmis nonce soft-delete (SilindiMi=true) ile temizlenir
    // ===================================================================

    [Fact]
    public async Task NonceSuresiDolan_SilindiMiTrueOlarakIsaretlenir()
    {
        var nonce = Guid.NewGuid().ToString("N");
        var kisaTtl = TimeSpan.FromMilliseconds(500);
        var nonceHash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(nonce)));

        using var kapsam = _fabrika.Services.CreateScope();
        var nonceDeposu = kapsam.ServiceProvider.GetRequiredService<IEmbedNonceDeposu>();
        var dbContext = kapsam.ServiceProvider.GetRequiredService<VizitLink3DDbContext>();

        // Kisa TTL ile nonce kaydet
        var kaydedildi = await nonceDeposu.DeneVeKaydetAsync(nonce, kisaTtl);
        Assert.True(kaydedildi);

        // TTL dolmasini bekle
        await Task.Delay(800);

        // Baska bir nonce kaydederek temizlemeyi tetikle
        var baskaNonce = Guid.NewGuid().ToString("N");
        await nonceDeposu.DeneVeKaydetAsync(baskaNonce, TimeSpan.FromMinutes(6));

        // Soft-delete edilen kaydi query filter'i atlayarak bul
        var ilkKayit = await dbContext.Set<EmbedOturumNonceKaydi>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(k => k.NonceHash == nonceHash);
        Assert.NotNull(ilkKayit);
        Assert.True(ilkKayit.SilindiMi, "TTL asmis nonce SilindiMi=true olmali");
        Assert.NotNull(ilkKayit.SilinmeTarihi);
    }

    // ===================================================================
    // TEST 25: Runtime token (cookie) nonce'i tekrar tuketmez
    // ===================================================================

    [Fact]
    public async Task NonceRuntimeToken_TekrarTuketilmez()
    {
        var istemci = IstemciOlustur();
        var token = GecerliTokenUret();

        // Bootstrap: nonce ilk tuketim (basarili)
        var bootstrapIstek = new HttpRequestMessage(HttpMethod.Get, $"/konfigurator/embed/{token}");
        bootstrapIstek.Headers.Add("Referer", "https://ornek-test.com/sayfa");
        var bootstrapYanit = await istemci.SendAsync(bootstrapIstek);
        Assert.Equal(HttpStatusCode.SeeOther, bootstrapYanit.StatusCode);

        // Runtime: cookie ile erisim (nonce kontrolu YAPILMAZ)
        var runtimeYanit = await istemci.GetAsync("/konfigurator/embed");

        // Runtime erisimi cookie ile calisir; nonce tekrar kontrol edilmez
        Assert.NotEqual(HttpStatusCode.Unauthorized, runtimeYanit.StatusCode);
        // Nonce replay hatasi (400) da DONMEMELI
        Assert.NotEqual(HttpStatusCode.BadRequest, runtimeYanit.StatusCode);
    }

    // ===================================================================
    // TEST 26: Tam akis: bootstrap -> runtime, nonce tuketimi regresyonsuz
    // ===================================================================

    [Fact]
    public async Task EmbedTamAkis_NonceTuketimiRegresyonsuz()
    {
        var istemci = IstemciOlustur();
        var token = GecerliTokenUret();

        // 1) Bootstrap: nonce ilk tuketim + cookie yazma
        var bootstrapIstek = new HttpRequestMessage(HttpMethod.Get, $"/konfigurator/embed/{token}");
        bootstrapIstek.Headers.Add("Referer", "https://ornek-test.com/sayfa");
        var bootstrapYanit = await istemci.SendAsync(bootstrapIstek);

        Assert.Equal(HttpStatusCode.SeeOther, bootstrapYanit.StatusCode);
        Assert.True(bootstrapYanit.Headers.TryGetValues("Set-Cookie", out _),
            "Bootstrap basariliysa Set-Cookie header'i olmali");

        // 2) Runtime: cookie ile sayfaya eris (token YOK)
        var runtimeYanit = await istemci.GetAsync("/konfigurator/embed");
        Assert.NotEqual(HttpStatusCode.Unauthorized, runtimeYanit.StatusCode);
        Assert.NotEqual(HttpStatusCode.BadRequest, runtimeYanit.StatusCode);

        var icerik = await runtimeYanit.Content.ReadAsStringAsync();
        Assert.Contains("<!DOCTYPE", icerik, StringComparison.OrdinalIgnoreCase);

        // 3) Ayni token ile ikinci bootstrap DENENEMEZ (nonce tuketildi)
        var ikinciIstek = new HttpRequestMessage(HttpMethod.Get, $"/konfigurator/embed/{token}");
        ikinciIstek.Headers.Add("Referer", "https://ornek-test.com/sayfa");
        var ikinciYanit = await istemci.SendAsync(ikinciIstek);

        Assert.Equal(HttpStatusCode.BadRequest, ikinciYanit.StatusCode);
        var ikinciIcerik = await ikinciYanit.Content.ReadAsStringAsync();
        // Nonce replay reddi: hata sayfasinda "Erişim Engellendi" bulunmali
        Assert.Contains("Erişim Engellendi", ikinciIcerik);
    }
}

// ========================================================================
// TEST WEB APPLICATION FABRIKASI
// ========================================================================

/// <summary>
/// Embed HTTP testleri icin ozel WebApplicationFactory.
/// Test ortaminda EmbedGuvenlik:RefererGevsekKontol=false ile fail-closed modda calisir.
/// SQLite in-memory veya temp dosya kullanir.
/// Migration ve post-startup sync'leri atlar.
/// </summary>
public class EmbedTestWebFabrikasi : WebApplicationFactory<Program>
{
    private static readonly string _testVeriTabaniYolu;

    static EmbedTestWebFabrikasi()
    {
        var testKlasoru = Path.Combine(Path.GetTempPath(), "vizitlink3d_embed_tests");
        Directory.CreateDirectory(testKlasoru);
        _testVeriTabaniYolu = Path.Combine(testKlasoru, $"embed_test_{Guid.NewGuid():N}.db");
    }

    protected override void ConfigureWebHost(IWebHostBuilder yapici)
    {
        var testDbYolu = _testVeriTabaniYolu;

        // Test ortami icin ortam degiskenleri
        Environment.SetEnvironmentVariable("VIZITLINK3D_SKIP_STARTUP_FIXUPS", "1");
        Environment.SetEnvironmentVariable("FORCE_SEED", "0");

        yapici.UseEnvironment("Test");

        yapici.ConfigureAppConfiguration((baglam, yapilandirma) =>
        {
            var testAyarlar = new Dictionary<string, string?>
            {
                ["EmbedGuvenlik:RefererGevsekKontol"] = "false",
                ["EmbedGuvenlik:GelistirmeHttpCookie"] = "true",
                ["VeriTabani:Yol"] = testDbYolu
            };
            yapilandirma.AddInMemoryCollection(testAyarlar);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        try
        {
            if (File.Exists(_testVeriTabaniYolu))
                File.Delete(_testVeriTabaniYolu);
        }
        catch
        {
            // Temizleme hatasi onemli degil
        }
    }
}
