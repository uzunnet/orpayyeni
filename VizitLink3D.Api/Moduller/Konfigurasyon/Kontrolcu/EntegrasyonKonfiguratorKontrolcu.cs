using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Api.Moduller.Konfigurasyon.Dogrulayicilar;
using VizitLink3D.Api.Moduller.Konfigurasyon.Dtolar;
using VizitLink3D.Api.Moduller.Konfigurasyon.Komutlar;
using VizitLink3D.Api.Moduller.Konfigurasyon.Servisler;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;

namespace VizitLink3D.Api.Moduller.Konfigurasyon.Kontrolcu;

/// <summary>
/// Sunucular-arası (server-to-server) entegrasyon endpoint'leri.
/// ApiAnahtarDogrulamaMiddleware tarafından korunur.
/// "SunucuEntegrasyonu" kapsamlı API anahtarı zorunludur.
/// Origin kontrolü yapılmaz (server-to-server isteklerde Origin olmayabilir).
/// API anahtarının FirmaId'si tenant izolasyonunu sağlar.
/// </summary>
[ApiController]
[Route("api/entegrasyon/konfigurator")]
[EnableRateLimiting("Entegrasyon")]
public class EntegrasyonKonfiguratorKontrolcu(
    IMediator mediator,
    VizitLink3DDbContext vt,
    IEmbedTokenServisi embedToken) : ControllerBase
{
    /// <summary>
    /// Ürün slug'ına göre konfigüratör verisini getirir (sunucu entegrasyonu).
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
        return await mediator.Send(new PublicKonfiguratorSorgusu(slug));
    }

    /// <summary>
    /// Müşteri konfigürasyon seçimini kaydeder (sunucu entegrasyonu).
    /// API anahtarının FirmaId'si ile tenant+ürün aidiyeti doğrulanır.
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

        // Slug ile UrunId eşleşmesini doğrula
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

    /// <summary>
    /// Embed (iframe) oturumu icin time-limited token olusturur.
    /// - X-Konfigurator-Anahtari header'inda SunucuEntegrasyonu scope API anahtari zorunlu
    /// - HedefOrigin: iframe'in gomulecegi musteri sitesinin exact origin'i
    /// - Tenant izolasyonu: FirmaId API anahtarindan alinir
    /// - DataProtection time-limited token: 5 dk gecerli, FirmaId+urun+origin+nonce icerir
    /// - Token KEY ICERMEZ, storage/console'a YAZILMAZ
    /// </summary>
    [HttpPost("{slug}/embed-oturum")]
    public async Task<Cevap<EmbedOturumYanitDto>> EmbedOturumOlustur(
        string slug,
        [FromBody] EmbedOturumIstekDto dto)
    {
        // Slug validasyonu
        var slugDogrulayici = new PublicKonfiguratorSorguDogrulayici();
        var slugDogrulamaSonucu = await slugDogrulayici.ValidateAsync(slug);
        if (!slugDogrulamaSonucu.IsValid)
        {
            var hataMesaji = string.Join("; ", slugDogrulamaSonucu.Errors.Select(e => e.ErrorMessage));
            return Cevap<EmbedOturumYanitDto>.Hata(hataMesaji);
        }

        // HedefOrigin validasyonu
        var originDogrulayici = new EmbedOturumIstekDogrulayici();
        var originDogrulama = await originDogrulayici.ValidateAsync(dto);
        if (!originDogrulama.IsValid)
        {
            var hataMesaji = string.Join("; ", originDogrulama.Errors.Select(e => e.ErrorMessage));
            return Cevap<EmbedOturumYanitDto>.Hata(hataMesaji);
        }

        // Tenant bilgisi
        var firmaId = HttpContext.Items["ApiKeyFirmaId"] as int?;
        if (firmaId is null or 0)
            return Cevap<EmbedOturumYanitDto>.Hata("API anahtari tenant bilgisi eksik.");

        // Urun varligi ve tenant aidiyeti kontrolu
        var urun = await vt.Urunler
            .AsNoTracking()
            .FirstOrDefaultAsync(u =>
                u.Slug == slug &&
                u.FirmaId == firmaId.Value &&
                !u.SilindiMi &&
                u.AktifMi);

        if (urun is null)
            return Cevap<EmbedOturumYanitDto>.Hata("Urun bulunamadi veya bu firmaya ait degil.");

        // HedefOrigin exact match dogrulamasi: API anahtarindaki izin verilen domainler ile
        var apiKeyDomainlerJson = HttpContext.Items["ApiKeyKapsam"] as string;
        // Izin verilen domainler kontrolu middleware'de yapildi, ama ek guvenlik icin burada da dogrula
        var apiKeyId = HttpContext.Items["ApiKeyId"] as int?;
        if (apiKeyId > 0)
        {
            var anahtarKaydi = await vt.FirmaApiAnahtarlari
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == apiKeyId);

            if (anahtarKaydi?.IzinVerilenDomainler is not null)
            {
                var domainler = System.Text.Json.JsonSerializer.Deserialize<List<string>>(anahtarKaydi.IzinVerilenDomainler);
                if (domainler?.Count > 0)
                {
                    // HedefOrigin'in domain kismini al
                    var hedefUri = new Uri(dto.HedefOrigin);
                    var hedefOrigin = hedefUri.GetLeftPart(UriPartial.Authority);

                    // Domain listesindeki herhangi bir domain ile eslesiyor mu?
                    var eslesme = domainler.Any(d =>
                    {
                        var izinUri = new Uri(d);
                        var izinOrigin = izinUri.GetLeftPart(UriPartial.Authority);
                        return string.Equals(hedefOrigin, izinOrigin, StringComparison.OrdinalIgnoreCase);
                    });

                    if (!eslesme)
                        return Cevap<EmbedOturumYanitDto>.Hata("Hedef origin API anahtari icin yetkilendirilmemis.");
                }
            }
        }

        // Time-limited embed token olustur
        var token = embedToken.TokenOlustur(firmaId.Value, slug, dto.HedefOrigin);

        // Token ASLA loga yazilmaz, storage'a kaydedilmez
        var iframeUrl = $"/konfigurator/embed/{token}";

        return Cevap<EmbedOturumYanitDto>.Basarili(new EmbedOturumYanitDto(
            iframeUrl,
            (int)TimeSpan.FromMinutes(5).TotalSeconds
        ));
    }
}
