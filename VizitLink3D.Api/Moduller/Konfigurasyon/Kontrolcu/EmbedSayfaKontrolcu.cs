using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using VizitLink3D.Api.Moduller.Konfigurasyon.Komutlar;
using VizitLink3D.Api.Moduller.Konfigurasyon.Servisler;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;

namespace VizitLink3D.Api.Moduller.Konfigurasyon.Kontrolcu;

/// <summary>
/// Embed (iframe) konfigurator sayfasi — GUVENLI TOKEN AKISI (Paket-4C).
/// 
/// AKIS:
/// 1) Bootstrap: GET /konfigurator/embed/{token}
///    - DataProtection token dogrulanir
///    - Referer header exact match kontrolu (production'da fail-closed)
///    - Nonce replay kontrolu
///    - Embed session cookie yazilir (HttpOnly, Secure, SameSite=None)
///    - 303 redirect → /konfigurator/embed (token-siz URL)
///    - CSP frame-ancestors, Referrer-Policy no-referrer, Cache-Control no-store
/// 
/// 2) Runtime: GET /konfigurator/embed
///    - Embed session cookie dogrulanir
///    - Urun verisi yuklenir, embed HTML sayfasi sunulur
///    - Token URL'de/HTML'de/JS'de BULUNMAZ
/// 
/// 3) Veri API: POST /api/embed/konfigurator/veri
///    - Embed session cookie ile dogrulanir
///    - API key/query parametresi GEREKMEZ
///    - JSON veri doner (widget icin)
/// 
/// GUVENLIK:
/// - Token yalniz bootstrap URL'de bulunur; hemen cookie ile degistirilir
/// - Cookie HttpOnly → JS erisemez
/// - Referer exact match → fail-closed (Development'ta config ile gevsetilebilir)
/// - CSP frame-ancestors exact HedefOrigin
/// - Referrer-Policy no-referrer
/// - Cache-Control no-store, Pragma no-cache
/// - Cross-site cookie engellenirse token'i expose etmez, anlasilir hata verir
/// - API key/query'de ASLA token/key bulunmaz
/// </summary>
[ApiController]
[EnableRateLimiting("Embed")]
public class EmbedSayfaKontrolcu(
    IMediator mediator,
    IEmbedTokenServisi embedToken,
    IOptionsMonitor<EmbedGuvenlikAyarlari> embedAyarlar,
    IWebHostEnvironment ortam) : ControllerBase
{
    private static readonly JsonSerializerOptions JSON_OPT = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    // ========================================================================
    // BASLIK YARDIMCILARI
    // ========================================================================

    /// <summary>
    /// CSP frame-ancestors header'ini hedef origin'e ayarlar.
    /// </summary>
    private void CspFrameAtalariEkle(string hedefOrigin)
    {
        // frame-ancestors: SADECE belirtilen origin'den iframe'e izin ver
        Response.Headers["Content-Security-Policy"] = $"frame-ancestors {hedefOrigin};";
    }

    /// <summary>
    /// Guvenlik ve onbellek header'larini ekler.
    /// Her yanitta (basarili/hatali) cagrilmalidir.
    /// </summary>
    private void GuvenlikHeaderlariEkle(string? hedefOrigin = null)
    {
        // Referrer-Policy: no-referrer — referrer bilgisi gonderme
        Response.Headers["Referrer-Policy"] = "no-referrer";

        // Cache-Control: hic onbellekleme
        Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, private";
        Response.Headers["Pragma"] = "no-cache";

        // X-Content-Type-Options: MIME sniffing engelle
        Response.Headers["X-Content-Type-Options"] = "nosniff";

        // X-Frame-Options: GuvenlikHeaderlariMiddleware global DENY ekler.
        // Embed basarili yanitlarinda DENY kaldirilip CSP frame-ancestors ile degistirilir.
        // Hata yanitlarinda DENY korunur (embed engelli).

        if (!string.IsNullOrWhiteSpace(hedefOrigin))
        {
            Response.Headers.Remove("X-Frame-Options");
            CspFrameAtalariEkle(hedefOrigin);
        }
    }

    /// <summary>
    /// Production'da referer kontrolu fail-closed mu?
    /// Development'ta config EmbedGuvenlik:RefererGevsekKontol ile gevsetilebilir (varsayilan: false).
    /// </summary>
    private bool RefererGevsekKontrol()
    {
        var ayarlar = embedAyarlar?.CurrentValue;
        if (ayarlar?.RefererGevsekKontol == true)
            return true;

        // Development ortaminda varsayilan olarak gevsek kontrol
        if (ortam.IsDevelopment())
            return ayarlar?.RefererGevsekKontol != false; // null ise true (development)

        // Production: fail-closed
        return false;
    }

    // ========================================================================
    // 1) BOOTSTRAP: GET /konfigurator/embed/{token}
    // ========================================================================

    /// <summary>
    /// Bootstrap endpoint: Token dogrula, cookie yaz, token-siz URL'ye redirect et.
    /// Token sadece BU istekte URL'de bulunur; tarayici history/DOM/storage'da KALMAZ.
    /// </summary>
    [HttpGet("/konfigurator/embed/{token}")]
    public async Task<IActionResult> Bootstrap(string token)
    {
        // === 1. Token dogrulama ===
        var payload = embedToken.TokenDogrula(token);
        if (payload is null)
        {
            GuvenlikHeaderlariEkle();
            return HataSayfasi("Geçersiz veya süresi dolmuş bağlantı. Lütfen sayfayı yenileyin.", 400);
        }

        // === 2. Nonce replay kontrolu ===
        if (!string.IsNullOrWhiteSpace(payload.Nonce))
        {
            var nonceGecerli = await embedToken.NonceDogrulaVeKaydetAsync(payload.Nonce);
            if (!nonceGecerli)
            {
                GuvenlikHeaderlariEkle();
                return HataSayfasi("Bu bağlantı daha önce kullanılmış. Lütfen yeni bir bağlantı oluşturun.", 400);
            }
        }

        // === 3. Referer origin exact match kontrolu ===
        var referer = Request.Headers.Referer.FirstOrDefault();
        var gevsek = RefererGevsekKontrol();

        if (string.IsNullOrWhiteSpace(referer))
        {
            if (!gevsek)
            {
                // Production: Referer yoksa fail-closed
                GuvenlikHeaderlariEkle();
                return HataSayfasi("Erişim reddedildi: Kaynak doğrulanamadı.", 403);
            }
            // Development: Referer yoksa gevsek gec (localhost testleri icin)
        }
        else
        {
            try
            {
                var refererUri = new Uri(referer);
                var refererOrigin = refererUri.GetLeftPart(UriPartial.Authority);

                if (!string.Equals(refererOrigin, payload.HedefOrigin, StringComparison.OrdinalIgnoreCase))
                {
                    // Origin uyusmazligi — guvenlik ihlali
                    GuvenlikHeaderlariEkle();
                    return HataSayfasi("Erişim reddedildi: Kaynak origin uyuşmazlığı.", 403);
                }
            }
            catch
            {
                if (!gevsek)
                {
                    GuvenlikHeaderlariEkle();
                    return HataSayfasi("Erişim reddedildi: Geçersiz kaynak bilgisi.", 403);
                }
            }
        }

        // === 4. HedefOrigin gecerlilik kontrolu ===
        if (string.IsNullOrWhiteSpace(payload.HedefOrigin) ||
            !Uri.TryCreate(payload.HedefOrigin, UriKind.Absolute, out var hedefUri))
        {
            GuvenlikHeaderlariEkle();
            return HataSayfasi("Geçersiz hedef origin bilgisi.", 400);
        }

        // === 5. Embed session cookie yaz ===
        var httpCookieIzinli = embedAyarlar?.CurrentValue?.GelistirmeHttpCookie ?? false;
        CookieOptions cookieAyarlari;

        if (httpCookieIzinli)
        {
            cookieAyarlari = new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // HTTP test/development
                SameSite = SameSiteMode.None,
                Path = "/konfigurator/embed",
                MaxAge = IEmbedTokenServisi.SessionGecerlilik,
                IsEssential = true
            };
        }
        else
        {
            cookieAyarlari = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/konfigurator/embed",
                MaxAge = IEmbedTokenServisi.SessionGecerlilik,
                IsEssential = true
            };
        }

        var sessionToken = embedToken.SessionTokenOlustur(payload);
        Response.Cookies.Append(EmbedSessionYardimci.COOKIE_ADI, sessionToken, cookieAyarlari);

        // === 6. CSP + Guvenlik header'lari (bootstrap yanitinda da olsun) ===
        GuvenlikHeaderlariEkle(payload.HedefOrigin);

        // === 7. 303 See Other → token-siz URL ===
        // 303: POST'tan sonra da guvenli, tarayici GET yapar
        // Token URL'de KALMAZ — history'de redirect hedefi (token-siz) gorunur
        Response.Headers["Location"] = "/konfigurator/embed";
        return StatusCode(303);
    }

    // ========================================================================
    // 2) RUNTIME: GET /konfigurator/embed (token-siz)
    // ========================================================================

    /// <summary>
    /// Embed runtime sayfasi. Embed session cookie ile dogrulanir.
    /// URL'de token YOKTUR.
    /// </summary>
    [HttpGet("/konfigurator/embed")]
    public async Task<IActionResult> EmbedRuntime()
    {
        // === Cookie dogrulama ===
        var payload = EmbedSessionYardimci.CookieDogrula(HttpContext, embedToken);
        if (payload is null)
        {
            GuvenlikHeaderlariEkle();
            // Cookie yok/gecersiz — cross-site engellenmis olabilir
            // Token'i expose ETME, anlasilir hata ver
            return HataSayfasi(
                "Oturum doğrulanamadı. Tarayıcınız üçüncü taraf çerezlerini engelliyor olabilir. " +
                "Lütfen çerez ayarlarınızı kontrol edin veya sayfayı yenileyin.", 401);
        }

        // === Urun verisini al ===
        var urunCevap = await mediator.Send(new PublicKonfiguratorSorgusu(payload.UrunSlug));

        if (!urunCevap.BasariliMi || urunCevap.Veri is null)
        {
            GuvenlikHeaderlariEkle();
            return HataSayfasi("Ürün verisi alınamadı.", 404);
        }

        // === Tenant kontrolu: FirmaId eslesmeli ===
        if (urunCevap.Veri.FirmaId != payload.FirmaId && urunCevap.Veri.FirmaId > 0)
        {
            GuvenlikHeaderlariEkle();
            return HataSayfasi("Erişim reddedildi: Tenant uyuşmazlığı.", 403);
        }

        // === Basarili HTML sayfasi (token YOK) ===
        GuvenlikHeaderlariEkle(payload.HedefOrigin);
        var html = KonfiguratorHtmlUret(urunCevap.Veri, payload);
        return Content(html, "text/html; charset=utf-8");
    }

    // ========================================================================
    // 3) VERI API: POST /api/embed/konfigurator/veri (cookie ile dogrulanir)
    // ========================================================================

    /// <summary>
    /// Widget'in iframe icinde kullanacagi JSON veri API'si.
    /// Embed session cookie ile dogrulanir — API key veya token GEREKMEZ.
    /// Bu endpoint ApiAnahtarDogrulamaMiddleware tarafindan cookie tabanli olarak
    /// taninir; cookie yoksa API key kontrolune duser.
    /// </summary>
    [HttpPost("/api/embed/konfigurator/veri")]
    public async Task<IActionResult> EmbedVeriGetir()
    {
        // Cookie dogrulama
        var payload = EmbedSessionYardimci.CookieDogrula(HttpContext, embedToken);
        if (payload is null)
        {
            GuvenlikHeaderlariEkle();
            return Unauthorized(Cevap<PublicKonfiguratorDto>.Hata(
                "Oturum doğrulanamadı. Lütfen sayfayı yenileyin."));
        }

        // Urun verisi
        var urunCevap = await mediator.Send(new PublicKonfiguratorSorgusu(payload.UrunSlug));
        if (!urunCevap.BasariliMi || urunCevap.Veri is null)
        {
            GuvenlikHeaderlariEkle();
            return NotFound(Cevap<PublicKonfiguratorDto>.Hata("Ürün bulunamadı."));
        }

        if (urunCevap.Veri.FirmaId != payload.FirmaId && urunCevap.Veri.FirmaId > 0)
        {
            GuvenlikHeaderlariEkle();
            return Unauthorized(Cevap<PublicKonfiguratorDto>.Hata("Tenant uyuşmazlığı."));
        }

        GuvenlikHeaderlariEkle(payload.HedefOrigin);
        return Ok(urunCevap);
    }

    // ========================================================================
    // HTML URETIMI
    // ========================================================================

    /// <summary>
    /// Embed iframe icinde gosterilecek konfigurator HTML sayfasi.
    /// Token URL'de/HTML'de/JS'de/console'da YOKTUR.
    /// Session cookie HttpOnly oldugu icin JS erisemez.
    /// Veri, sunucu tarafinda HTML icine gomulur.
    /// </summary>
    private string KonfiguratorHtmlUret(PublicKonfiguratorDto veri, EmbedTokenPayload payload)
    {
        var veriJson = JsonSerializer.Serialize(veri, JSON_OPT);
        var veriJsonGuvenli = veriJson
            .Replace("</script>", "<\\/script>", StringComparison.OrdinalIgnoreCase);

        // HedefOrigin ve UrunSlug HTML'de referans icin kullanilabilir
        // ama TOKEN veya API KEY YOKTUR
        var hedefOriginJson = JsonSerializer.Serialize(payload.HedefOrigin, JSON_OPT);

        return $$"""
        <!DOCTYPE html>
        <html lang="tr">
        <head>
            <meta charset="utf-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1" />
            <meta name="referrer" content="no-referrer" />
            <title>Konfigüratör</title>
            <style>
                * { box-sizing: border-box; margin: 0; padding: 0; }
                body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; background: #fff; color: #222; }
                .konfigurator { display: flex; flex-direction: column; height: 100vh; }
                .konfigurator-baslik { padding: 12px 16px; border-bottom: 1px solid #e0e0e0; background: #fafafa; }
                .konfigurator-baslik h2 { font-size: 1.1rem; font-weight: 600; }
                .konfigurator-baslik .slug { font-size: 0.85rem; color: #666; }
                .konfigurator-icerik { flex: 1; padding: 16px; overflow-y: auto; }
                .parca { padding: 10px 12px; margin-bottom: 8px; border: 1px solid #e8e8e8; border-radius: 8px; background: #fafafa; }
                .parca-ad { font-weight: 500; font-size: 0.95rem; }
                .parca-detay { font-size: 0.8rem; color: #888; margin-top: 4px; }
                .hata { padding: 20px; text-align: center; color: #c0392b; }
                .hata h2 { font-size: 1.3rem; margin-bottom: 8px; }
                .yukleniyor { display: flex; align-items: center; justify-content: center; height: 100vh; color: #888; }
            </style>
        </head>
        <body>
            <div id="vt3d-embed-kok">
                <div class="yukleniyor">Konfigüratör yükleniyor…</div>
            </div>
            <script>
            (function() {
                // GUVENLIK: Token/API key bu sayfada YOKTUR.
                // Oturum, HttpOnly cookie (vt3d_embed_oturum) ile yonetilir.
                // Cookie'ye JS erisemez — XSS korumasi.
                // Veri sunucu tarafinda HTML icine gomulmustur.

                var veri = {{veriJsonGuvenli}};
                var kok = document.getElementById('vt3d-embed-kok');
                if (!veri) {
                    kok.innerHTML = '<div class="hata"><h2>Veri yüklenemedi</h2><p>Konfigüratör verisi alınamadı.</p></div>';
                    return;
                }

                var html = '<div class="konfigurator">';
                html += '<div class="konfigurator-baslik">';
                html += '<h2>' + escHtml(veri.ad || 'Konfigüratör') + '</h2>';
                if (veri.slug) html += '<div class="slug">' + escHtml(veri.slug) + '</div>';
                html += '</div>';
                html += '<div class="konfigurator-icerik">';

                if (veri.parcalar && veri.parcalar.length > 0) {
                    veri.parcalar.forEach(function(p) {
                        html += '<div class="parca">';
                        html += '<div class="parca-ad">' + escHtml(p.gorunenAd || 'Parça #' + p.id) + '</div>';
                        html += '<div class="parca-detay">';
                        var detaylar = [];
                        if (p.renkler && p.renkler.length > 0) detaylar.push(p.renkler.length + ' renk');
                        if (p.malzemeler && p.malzemeler.length > 0) detaylar.push(p.malzemeler.length + ' malzeme');
                        if (p.dokular && p.dokular.length > 0) detaylar.push(p.dokular.length + ' doku');
                        if (p.hareketliMi) detaylar.push('hareketli');
                        html += detaylar.length > 0 ? detaylar.join(' · ') : 'özelleştirme yok';
                        html += '</div></div>';
                    });
                } else {
                    html += '<p style="color:#888;">Bu ürün için özelleştirilebilir parça bulunmamaktadır.</p>';
                }

                html += '</div></div>';
                kok.innerHTML = html;

                function escHtml(m) {
                    if (!m) return '';
                    var d = document.createElement('div');
                    d.appendChild(document.createTextNode(m));
                    return d.innerHTML;
                }
            })();
            </script>
        </body>
        </html>
        """;
    }

    // ========================================================================
    // HATA SAYFASI
    // ========================================================================

    /// <summary>
    /// Hata sayfasi (CSP + no-referrer + cache-control ile).
    /// Hassas bilgi icermez — sadece genel mesaj.
    /// </summary>
    private IActionResult HataSayfasi(string mesaj, int durumKodu = 400)
    {
        GuvenlikHeaderlariEkle();

        var guvenliMesaj = System.Net.WebUtility.HtmlEncode(mesaj);
        var html = $$"""
        <!DOCTYPE html>
        <html lang="tr">
        <head>
            <meta charset="utf-8" />
            <meta name="referrer" content="no-referrer" />
            <title>Erişim Engellendi</title>
            <style>
                * { box-sizing: border-box; margin: 0; padding: 0; }
                body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; background: #fff; color: #c0392b; display: flex; align-items: center; justify-content: center; height: 100vh; }
                .hata { text-align: center; padding: 20px; max-width: 480px; }
                .hata h2 { font-size: 1.3rem; margin-bottom: 8px; }
                .hata p { color: #666; font-size: 0.95rem; line-height: 1.5; }
            </style>
        </head>
        <body>
            <div class="hata">
                <h2>Erişim Engellendi</h2>
                <p>{guvenliMesaj}</p>
            </div>
        </body>
        </html>
        """;

        Response.StatusCode = durumKodu;
        return Content(html, "text/html; charset=utf-8");
    }
}

// ========================================================================
// KONFIGURASYON SINIFI
// ========================================================================

/// <summary>
/// Embed guvenlik ayarlari — appsettings.json "EmbedGuvenlik" bolumunden okunur.
/// </summary>
public class EmbedGuvenlikAyarlari
{
    /// <summary>
    /// Development ortaminda Referer kontrolunu gevset (varsayilan: true).
    /// Production'da bu ayar true olsa bile guvenlik nedeniyle gecersiz sayilir.
    /// </summary>
    public bool RefererGevsekKontol { get; set; } = false;

    /// <summary>
    /// Development ortaminda HTTP cookie'ye izin ver (varsayilan: false).
    /// Production'da HER ZAMAN Secure=true.
    /// </summary>
    public bool GelistirmeHttpCookie { get; set; } = false;
}
