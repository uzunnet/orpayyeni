namespace VizitLink3D.Api.Moduller.Urunler.Servisler;

/// <summary>
/// 3D model, parça grubu ve sahne önayarı için tenant sahiplik doğrulaması.
/// SuperAdmin çapraz tenant erişebilir; Admin yalnız kendi FirmaId'sine ait kayıtlara erişir.
/// </summary>
public interface IUcBoyutModelSahiplikDogrulayici
{
    /// <summary>
    /// Model → UrunId → ParcaGrubu.FirmaId zinciri üzerinden tenant doğrulaması.
    /// </summary>
    Task<bool> ModelSahibiniDogrulaAsync(int modelId);

    /// <summary>
    /// Grup.FirmaId üzerinden doğrudan tenant doğrulaması.
    /// </summary>
    Task<bool> GrupSahibiniDogrulaAsync(int grupId);

    /// <summary>
    /// Sahne önayarı → ModelId → tenant zinciri üzerinden doğrulama.
    /// </summary>
    Task<bool> SahneOnayariSahibiniDogrulaAsync(int sahneOnayariId);
}
