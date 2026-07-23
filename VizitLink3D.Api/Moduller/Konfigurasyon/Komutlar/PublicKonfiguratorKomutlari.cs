using MediatR;
using VizitLink3D.Api.Moduller.Konfigurasyon.Dtolar;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;

namespace VizitLink3D.Api.Moduller.Konfigurasyon.Komutlar;

/// <summary>
/// Public konfigüratör sayfası için güvenli ürün yapılandırma sorgusu.
/// Tenant izolasyonu: KiraciServisi üzerinden firma domain filtresi uygulanır.
/// Yalnız AdminOnayliMi=true, AktifMi=true, SilindiMi=false veri döner.
/// </summary>
public record PublicKonfiguratorSorgusu(string Slug) : IRequest<Cevap<PublicKonfiguratorDto>>;

/// <summary>
/// Public konfigüratörden müşteri seçimini kaydetme komutu.
/// FirmaId KiraciServisi'den alınır, OturumAnahtari backend tarafından oluşturulur.
/// </summary>
public record PublicSecimKaydetKomutu(
    int UrunId,
    string? MusteriNotu,
    List<PublicParcaSecimiDto> Secimler
) : IRequest<Cevap<KonfigurasyonDetayDto>>;
