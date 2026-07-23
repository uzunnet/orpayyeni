using VizitLink3D.Ortak.Modeller;
using System.Net;
using System.Text.Json;

namespace VizitLink3D.Api.AraYazilimlar;

/// <summary>
/// Merkezi hata yonetimi middleware'i.
/// Tum kontrolculerdeki try-catch bloklarini gereksiz kilar (anayasa §7).
/// Her hata Cevap<T>.Hata() formatinda JSON olarak doner.
/// Production'da detay vermez, development'da stack trace dondurur.
/// Her istege otomatik CorrelationId ekler.
/// </summary>
public class HataYonetimiMiddleware(RequestDelegate sonraki, ILogger<HataYonetimiMiddleware> logger, IWebHostEnvironment ortam)
{
    public async Task InvokeAsync(HttpContext baglam)
    {
        // Her istege CorrelationId ekle
        var correlationId = baglam.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString("N")[..12];
        baglam.Items["CorrelationId"] = correlationId;
        baglam.Response.Headers["X-Correlation-ID"] = correlationId;

        try
        {
            await sonraki(baglam);
        }
        catch (Exception hata)
        {
            await HataYakalaAsync(baglam, hata, correlationId);
        }
    }

    private async Task HataYakalaAsync(HttpContext baglam, Exception hata, string correlationId)
    {
        // Log — gizli bilgi icermemeli (anayasa §15)
        logger.LogError(hata, "Islenmemis hata yakalandi. {CorrelationId} {Yol} {Yontem}",
            correlationId, baglam.Request.Path, baglam.Request.Method);

        baglam.Response.ContentType = "application/json; charset=utf-8";

        var hataMesaji = ortam.IsDevelopment()
            ? $"Hata: {hata.ToString()} | CorrelationId: {correlationId}"
            : "Beklenmeyen bir hata olustu. Lutfen daha sonra tekrar deneyiniz.";

        var cevap = Cevap<object>.Hata(hataMesaji);

        switch (hata)
        {
            case UnauthorizedAccessException:
                baglam.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                break;
            case KeyNotFoundException:
                baglam.Response.StatusCode = (int)HttpStatusCode.NotFound;
                cevap = Cevap<object>.Hata("Istenen kaynak bulunamadi.");
                break;
            case ArgumentException or ArgumentNullException:
                baglam.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                cevap = Cevap<object>.Hata(hata.Message);
                break;
            case InvalidOperationException:
                baglam.Response.StatusCode = (int)HttpStatusCode.Conflict;
                cevap = Cevap<object>.Hata(hata.Message);
                break;
            default:
                baglam.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }

        await baglam.Response.WriteAsync(JsonSerializer.Serialize(cevap));
    }
}
