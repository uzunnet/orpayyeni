using MediatR;
using VizitLink3D.Api.Moduller.Konfigurasyon.Dtolar;
using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.Api.Moduller.Konfigurasyon.Komutlar;

/// <summary>
/// Yeni müşteri konfigürasyonu oluştur.
/// FirmaId middleware/claim üzerinden alınır.
/// </summary>
public record KonfigurasyonOlusturKomutu(
    int UrunId,
    string? OturumAnahtari,
    string? Not,
    List<KonfigurasyonParcaDto> Parcalar
) : IRequest<Cevap<KonfigurasyonDetayDto>>;

/// <summary>
/// Mevcut konfigürasyonu güncelle.
/// Gönderilen parça listesi tam set kabul edilir,
/// listede olmayan mevcut parçalar soft-delete yapılır.
/// </summary>
public record KonfigurasyonGuncelleKomutu(
    int Id,
    string? Not,
    List<KonfigurasyonParcaDto> Parcalar
) : IRequest<Cevap<KonfigurasyonDetayDto>>;

/// <summary>
/// Konfigürasyon soft-delete.
/// </summary>
public record KonfigurasyonSilKomutu(int Id) : IRequest<Cevap<bool>>;

/// <summary>
/// Konfigürasyon listeleme sorgusu.
/// FirmaId middleware/claim üzerinden filtrelenir.
/// </summary>
public record KonfigurasyonListeleSorgusu(
    int? UrunId = null,
    int Sayfa = 1,
    int Boyut = 20
) : IRequest<Cevap<List<KonfigurasyonOzetDto>>>;

/// <summary>
/// Konfigürasyon detay sorgusu.
/// </summary>
public record KonfigurasyonDetaySorgusu(int Id) : IRequest<Cevap<KonfigurasyonDetayDto>>;
