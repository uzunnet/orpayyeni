namespace VizitLink3D.Api.Moduller.Konfigurasyon.Dtolar;

/// <summary>
/// Müşteri konfigürasyonu oluşturma/güncelleme için DTO.
/// Tenant FirmaId'si middleware/claim üzerinden alınır, request body'den güvenilmez.
/// </summary>
public record KonfigurasyonOlusturDto(
    int UrunId,
    string? OturumAnahtari,
    string? Not,
    List<KonfigurasyonParcaDto> Parcalar
);

/// <summary>
/// Konfigürasyon güncelleme DTO'su.
/// Gönderilen parça listesi tam set olarak kabul edilir;
/// mevcut olup listede olmayan parçalar soft-delete yapılır.
/// </summary>
public record KonfigurasyonGuncelleDto(
    string? Not,
    List<KonfigurasyonParcaDto> Parcalar
);

/// <summary>
/// Tek bir parça seçimi için DTO.
/// </summary>
public record KonfigurasyonParcaDto(
    int UrunUcBoyutParcasiId,
    int? SeciliRenkId = null,
    int? SeciliMalzemeId = null,
    int? SeciliKaplamaId = null,
    string? SeciliDoku = null,
    double? HareketDegeri = null,
    double? Aci = null,
    double? Deger = null,
    bool GorunurMu = true
);

/// <summary>
/// Konfigürasyon özeti — listeleme için.
/// </summary>
public record KonfigurasyonOzetDto(
    int Id,
    int UrunId,
    string? UrunAdi,
    string Durum,
    decimal? ToplamFiyat,
    int ParcaSayisi,
    DateTime OlusturulmaTarihi,
    DateTime? GuncellenmeTarihi
);

/// <summary>
/// Konfigürasyon detayı — parçalarıyla birlikte.
/// </summary>
public record KonfigurasyonDetayDto(
    int Id,
    int UrunId,
    string? UrunAdi,
    string? OturumAnahtari,
    string? Not,
    string Durum,
    decimal? ToplamFiyat,
    List<KonfigurasyonParcaDetayDto> Parcalar,
    DateTime OlusturulmaTarihi,
    DateTime? GuncellenmeTarihi
);

/// <summary>
/// Parça detayı DTO.
/// </summary>
public record KonfigurasyonParcaDetayDto(
    int Id,
    int UrunUcBoyutParcasiId,
    string? ParcaAdi,
    int? SeciliRenkId,
    string? SeciliRenkAdi,
    int? SeciliMalzemeId,
    string? SeciliMalzemeAdi,
    int? SeciliKaplamaId,
    string? SeciliKaplamaAdi,
    string? SeciliDoku,
    double? HareketDegeri,
    double? Aci,
    double? Deger,
    bool GorunurMu
);
