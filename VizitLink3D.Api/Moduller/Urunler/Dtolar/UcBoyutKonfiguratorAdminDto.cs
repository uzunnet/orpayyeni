using VizitLink3D.Ortak.Modeller.Urunler;

namespace VizitLink3D.Api.Moduller.Urunler.Dtolar;

// ============================================================
// Admin Toplu (Aggregate) Endpoint DTO'ları
// ============================================================

/// <summary>
/// Bir modele ait tüm konfigürasyon verisini tek seferde döndüren aggregate DTO.
/// </summary>
public record UcBoyutModelKonfigurasyonDto(
    int ModelId,
    string ModelAdi,
    string? ModelDosyaYolu,
    string? ModelTipi,
    int UrunId,
    List<UrunUcBoyutParcasi> Parcalar,
    List<UrunParcaGrubu> Gruplar,
    List<UrunUcBoyutSahneOnayari> SahneOnayarlari
);

// ============================================================
// Parça Upsert (Toplu) DTO'ları
// ============================================================

/// <summary>
/// Admin parça upsert isteği — mesh adı üzerinden eşleşir.
/// MeshAdi eşleşirse günceller, yoksa yeni parça ekler.
/// </summary>
public record UcBoyutParcaUpsertDto(
    string MeshAdi,
    string? MantiksalKod,
    string GorunenAd,
    int? ParcaGrubuId,
    string? HareketTipi,
    string? HareketAyarlariJson,
    bool DokuUygulanabilirMi,
    bool GorunurlukDegisebilirMi,
    bool RenklenebilirMi,
    bool MalzemeDegisebilirMi,
    bool SecilebilirMi,
    bool HareketliMi,
    string? ParcaTipi,
    string? MalzemeTipiKisiti,
    int SiraNo,
    bool AktifMi,
    bool AdminOnayliMi
);

/// <summary>
/// Toplu parça upsert isteği.
/// </summary>
public record UcBoyutParcaTopluUpsertDto(
    List<UcBoyutParcaUpsertDto> Parcalar
);

/// <summary>
/// Toplu parça upsert sonucu.
/// </summary>
public record UcBoyutParcaTopluUpsertSonucDto(
    int Eklendi,
    int Guncellendi,
    List<string> Hatalar
);

// ============================================================
// Grup CRUD DTO'ları
// ============================================================

/// <summary>
/// Parça grubu oluşturma/güncelleme DTO'su.
/// </summary>
public record UcBoyutGrupDto(
    string Ad,
    string? Aciklama,
    int SiraNo,
    bool AktifMi
);

// ============================================================
// Sahne Önayarı CRUD DTO'ları
// ============================================================

/// <summary>
/// Sahne önayarı oluşturma/güncelleme DTO'su.
/// </summary>
public record UcBoyutSahneOnayariDto(
    string Ad,
    string Kod,
    string? AyarlarJson,
    bool VarsayilanMi,
    bool AktifMi,
    int SiraNo
);
