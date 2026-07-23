using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VizitLink3D.Api.Moduller.Konfigurasyon.Dtolar;
using VizitLink3D.Api.Moduller.Konfigurasyon.Komutlar;
using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.Api.Moduller.Konfigurasyon.Kontrolcu;

/// <summary>
/// Müşteri konfigürasyonu CRUD endpoint'leri.
/// Tenant izolasyonu: Tüm işlemler MediatR handler üzerinden KiraciServisi ile filtrelenir.
/// Embed endpoint'leri ApiAnahtarDogrulamaMiddleware ile korunur.
/// Admin endpoint'leri JWT + FirmaAdmin/SuperAdmin ile korunur.
/// </summary>
[ApiController]
[Authorize(Roles = "Admin,SuperAdmin")]
[Route("api/konfigurasyon")]
public class KonfigurasyonKontrolcu(IMediator mediator) : ControllerBase
{
    /// <summary>Konfigürasyon listele — tenant filtresi otomatik</summary>
    [HttpGet]
    public async Task<Cevap<List<KonfigurasyonOzetDto>>> Listele(
        [FromQuery] int? urunId = null,
        [FromQuery] int sayfa = 1,
        [FromQuery] int boyut = 20)
        => await mediator.Send(new KonfigurasyonListeleSorgusu(urunId, sayfa, boyut));

    /// <summary>Konfigürasyon detay</summary>
    [HttpGet("{id:int}")]
    public async Task<Cevap<KonfigurasyonDetayDto>> Detay(int id)
        => await mediator.Send(new KonfigurasyonDetaySorgusu(id));

    /// <summary>Yeni konfigürasyon oluştur</summary>
    [HttpPost]
    public async Task<Cevap<KonfigurasyonDetayDto>> Olustur([FromBody] KonfigurasyonOlusturDto dto)
        => await mediator.Send(new KonfigurasyonOlusturKomutu(
            dto.UrunId, dto.OturumAnahtari, dto.Not, dto.Parcalar));

    /// <summary>Konfigürasyon güncelle</summary>
    [HttpPut("{id:int}")]
    public async Task<Cevap<KonfigurasyonDetayDto>> Guncelle(int id, [FromBody] KonfigurasyonGuncelleDto dto)
        => await mediator.Send(new KonfigurasyonGuncelleKomutu(id, dto.Not, dto.Parcalar));

    /// <summary>Konfigürasyon sil (soft delete)</summary>
    [HttpDelete("{id:int}")]
    public async Task<Cevap<bool>> Sil(int id)
        => await mediator.Send(new KonfigurasyonSilKomutu(id));
}
