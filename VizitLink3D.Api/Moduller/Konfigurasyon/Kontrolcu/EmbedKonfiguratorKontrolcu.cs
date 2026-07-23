using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Api.Moduller.Konfigurasyon.Dogrulayicilar;
using VizitLink3D.Api.Moduller.Konfigurasyon.Dtolar;
using VizitLink3D.Api.Moduller.Konfigurasyon.Komutlar;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;

namespace VizitLink3D.Api.Moduller.Konfigurasyon.Kontrolcu;

/// <summary>
/// Embed (iframe/widget) konfigüratör endpoint'leri.
/// ApiAnahtarDogrulamaMiddleware tarafından korunur.
/// "Embed" kapsamlı API anahtarı + origin doğrulaması zorunludur.
/// API anahtarının FirmaId'si tenant izolasyonunu sağlar.
/// Yalnız public-safe DTO döner; teknik mesh/admin alanları dönmez.
/// </summary>
[ApiController]
[Route("api/embed/konfigurator")]
[AllowAnonymous]  // Kimlik doğrulama middleware tarafından yapılır
[EnableRateLimiting("Embed")]
public class EmbedKonfiguratorKontrolcu(
    IMediator mediator,
    VizitLink3DDbContext vt) : ControllerBase
{
    /// <summary>
    /// Ürün slug'ına göre embed-safe konfigüratör verisini getirir.
    /// API anahtarının FirmaId'si ile tenant izolasyonu sağlanır.
    /// </summary>
    [HttpGet("{slug}")]
    public async Task<Cevap<PublicKonfiguratorDto>> KonfiguratorGetir(string slug)
    {
        var dogrulayici = new PublicKonfiguratorSorguDogrulayici();
        var dogrulamaSonucu = await dogrulayici.ValidateAsync(slug);
        if (!dogrulamaSonucu.IsValid)
        {
            var hataMesaji = string.Join("; ", dogrulamaSonucu.Errors.Select(e => e.ErrorMessage));
            return Cevap<PublicKonfiguratorDto>.Hata(hataMesaji);
        }

        // Mevcut PublicKonfiguratorSorgusu handler'ını kullan.
        // FirmaId, middleware tarafından API anahtarından okunup HttpContext.Items'a yazıldı.
        // KiraciServisi.MevcutFirmaId bunu okuyacak.
        return await mediator.Send(new PublicKonfiguratorSorgusu(slug));
    }

    /// <summary>
    /// Müşteri konfigürasyon seçimini kaydeder.
    /// API anahtarının FirmaId'si ile tenant+ürün aidiyeti doğrulanır.
    /// OturumAnahtari backend tarafından oluşturulur.
    /// </summary>
    [HttpPost("{slug}/secimler")]
    public async Task<Cevap<KonfigurasyonDetayDto>> SecimKaydet(
        string slug,
        [FromBody] PublicSecimKaydetDto dto)
    {
        // Slug validasyonu
        var slugDogrulayici = new PublicKonfiguratorSorguDogrulayici();
        var slugDogrulamaSonucu = await slugDogrulayici.ValidateAsync(slug);
        if (!slugDogrulamaSonucu.IsValid)
        {
            var hataMesaji = string.Join("; ", slugDogrulamaSonucu.Errors.Select(e => e.ErrorMessage));
            return Cevap<KonfigurasyonDetayDto>.Hata(hataMesaji);
        }

        // Body validasyonu
        var dogrulayici = new PublicSecimKaydetDogrulayici();
        var dogrulamaSonucu = await dogrulayici.ValidateAsync(dto);
        if (!dogrulamaSonucu.IsValid)
        {
            var hataMesaji = string.Join("; ", dogrulamaSonucu.Errors.Select(e => e.ErrorMessage));
            return Cevap<KonfigurasyonDetayDto>.Hata(hataMesaji);
        }

        // Slug ile UrunId eşleşmesini doğrula (tenant izolasyonu handler'da)
        var apiKeyFirmaId = HttpContext.Items["ApiKeyFirmaId"] as int?;
        if (apiKeyFirmaId is null or 0)
            return Cevap<KonfigurasyonDetayDto>.Hata("API anahtarı tenant bilgisi eksik.");

        var urun = await vt.Urunler
            .AsNoTracking()
            .FirstOrDefaultAsync(u =>
                u.Slug == slug &&
                u.Id == dto.UrunId &&
                u.FirmaId == apiKeyFirmaId.Value &&
                !u.SilindiMi &&
                u.AktifMi);

        if (urun is null)
            return Cevap<KonfigurasyonDetayDto>.Hata("Slug ile ürün eşleşmedi veya ürün bu firmaya ait değil.");

        // Mevcut PublicSecimKaydetKomutu handler'ını kullan
        return await mediator.Send(new PublicSecimKaydetKomutu(
            dto.UrunId, dto.MusteriNotu, dto.Secimler));
    }
}
