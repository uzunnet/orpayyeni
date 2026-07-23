namespace VizitLink3D.Api.AraYazilimlar;

/// <summary>
/// Guvenlik header'lari middleware'i (anayasa §3.2, §17).
/// HSTS, X-Frame-Options, X-Content-Type-Options, CSP ve
/// Referrer-Policy header'larini otomatik ekler.
/// </summary>
public class GuvenlikHeaderlariMiddleware(RequestDelegate sonraki, IConfiguration yapilandirma, IWebHostEnvironment ortam)
{
    public async Task InvokeAsync(HttpContext baglam)
    {
        if (!baglam.Response.Headers.ContainsKey("X-Content-Type-Options"))
            baglam.Response.Headers["X-Content-Type-Options"] = "nosniff";

        if (!baglam.Response.Headers.ContainsKey("X-Frame-Options"))
            baglam.Response.Headers["X-Frame-Options"] = "DENY";

        if (!baglam.Response.Headers.ContainsKey("Referrer-Policy"))
            baglam.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        if (!baglam.Response.Headers.ContainsKey("X-XSS-Protection"))
            baglam.Response.Headers["X-XSS-Protection"] = "1; mode=block";

        // Permissions-Policy: gereksiz API'leri kisitla
        if (!baglam.Response.Headers.ContainsKey("Permissions-Policy"))
            baglam.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

        // HSTS: appsettings Guvenlik:HstsAktif ile acilir/kapanir (varsayilan: production'da acik)
        var hstsAktif = yapilandirma.GetValue<bool?>("Guvenlik:HstsAktif") ?? !ortam.IsDevelopment();
        if (hstsAktif && !baglam.Response.Headers.ContainsKey("Strict-Transport-Security"))
            baglam.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";

        // Content-Security-Policy: Blazor WASM + MudBlazor + Google Fonts uyumlu
        if (!baglam.Response.Headers.ContainsKey("Content-Security-Policy"))
            baglam.Response.Headers["Content-Security-Policy"] =
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval' blob: https://cdnjs.cloudflare.com; " +
                "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net https://cdnjs.cloudflare.com; " +
                "font-src 'self' https://fonts.gstatic.com; " +
                "img-src 'self' data: blob: https://images.unsplash.com https://*.unsplash.com https://orpayormanurunleri.com.tr https://*.orpayormanurunleri.com.tr; " +
                "connect-src 'self' blob: data: http: https: ws: wss: http://localhost:* ws://localhost:* wss://localhost:*; " +
                "frame-src 'self' https://www.youtube.com https://www.youtube-nocookie.com https://youtube.com https://youtube-nocookie.com https://www.google.com; " +
                "frame-ancestors 'self'; " +
                "worker-src 'self' blob:; " +
                "media-src 'self' blob:;";

        await sonraki(baglam);
    }
}
