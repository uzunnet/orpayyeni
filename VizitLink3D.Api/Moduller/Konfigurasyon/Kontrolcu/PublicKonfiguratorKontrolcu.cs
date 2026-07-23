using MediatR;
using Microsoft.AspNetCore.Mvc;
using VizitLink3D.Api.Moduller.Konfigurasyon.Dogrulayicilar;
using VizitLink3D.Api.Moduller.Konfigurasyon.Dtolar;
using VizitLink3D.Api.Moduller.Konfigurasyon.Komutlar;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;

namespace VizitLink3D.Api.Moduller.Konfigurasyon.Kontrolcu;

/// <summary>
/// Public konfigüratör endpoint'leri.
/// Anonim erişime açık, tenant domain middleware ile izole edilir.
/// Yalnız admin-onaylı, aktif, silinmemiş veri döndürür.
/// Teknik mesh/HDR/kamera/JSON/admin audit alanları dönmez.
/// CORS/AllowAnyOrigin eklenmez — yalnız first-party tenant domain.
/// Embed API key gereksinimi Paket-4'e bırakıldı.
/// </summary>
[ApiController]
[Route("api/konfigurasyon/public")]
public class PublicKonfiguratorKontrolcu(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Ürün slug'ına göre public konfigüratör verisini getirir.
    /// Yalnız AdminOnayliMi=true parçalar, kamuya açık seçenekler döner.
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

        return await mediator.Send(new PublicKonfiguratorSorgusu(slug));
    }

    /// <summary>
    /// Müşteri konfigürasyon seçimini kaydeder.
    /// Tenant FirmaId'si backend'den alınır, OturumAnahtari otomatik oluşturulur.
    /// Anonim kullanıcılar için güvenli taslak oluşturur.
    /// </summary>
    [HttpPost("secim-kaydet")]
    public async Task<Cevap<KonfigurasyonDetayDto>> SecimKaydet([FromBody] PublicSecimKaydetDto dto)
    {
        var dogrulayici = new PublicSecimKaydetDogrulayici();
        var dogrulamaSonucu = await dogrulayici.ValidateAsync(dto);
        if (!dogrulamaSonucu.IsValid)
        {
            var hataMesaji = string.Join("; ", dogrulamaSonucu.Errors.Select(e => e.ErrorMessage));
            return Cevap<KonfigurasyonDetayDto>.Hata(hataMesaji);
        }

        return await mediator.Send(new PublicSecimKaydetKomutu(
            dto.UrunId, dto.MusteriNotu, dto.Secimler));
    }
}
