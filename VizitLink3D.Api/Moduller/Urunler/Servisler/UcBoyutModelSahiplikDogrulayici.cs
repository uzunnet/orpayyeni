using VizitLink3D.Api.Servisler;
using VizitLink3D.Api.VeriTabani;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Moduller.Urunler.Servisler;

/// <summary>
/// 3D model sahiplik doğrulayıcı — tenant izolasyonu.
/// Zincir: Model → UrunId → Urun.FirmaId (doğrudan ürün sahipliği).
/// SuperAdmin tüm tenant'lara erişebilir; Admin yalnız kendi FirmaId'sine.
/// </summary>
public class UcBoyutModelSahiplikDogrulayici(
    VizitLink3DDbContext vt,
    KiraciServisi kiraciServisi,
    IHttpContextAccessor hca) : IUcBoyutModelSahiplikDogrulayici
{
    public async Task<bool> ModelSahibiniDogrulaAsync(int modelId)
    {
        // SuperAdmin → serbest
        if (hca.HttpContext?.User.IsInRole("SuperAdmin") == true)
            return true;

        var firmaId = kiraciServisi.MevcutFirmaId;
        if (firmaId is null or 0)
            return false;

        // Model var mı?
        var model = await vt.UrunUcBoyutModelleri
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == modelId && !m.SilindiMi);

        if (model is null)
            return false;

        // Model → UrunId → Urun.FirmaId zinciri (doğrudan ürün sahipliği)
        return await vt.Urunler
            .AsNoTracking()
            .AnyAsync(u => u.Id == model.UrunId
                        && u.FirmaId == firmaId.Value
                        && !u.SilindiMi);
    }

    public async Task<bool> GrupSahibiniDogrulaAsync(int grupId)
    {
        if (hca.HttpContext?.User.IsInRole("SuperAdmin") == true)
            return true;

        var firmaId = kiraciServisi.MevcutFirmaId;
        if (firmaId is null or 0)
            return false;

        return await vt.UrunParcaGruplari
            .AsNoTracking()
            .AnyAsync(g => g.Id == grupId
                        && g.FirmaId == firmaId.Value
                        && !g.SilindiMi);
    }

    public async Task<bool> SahneOnayariSahibiniDogrulaAsync(int sahneOnayariId)
    {
        if (hca.HttpContext?.User.IsInRole("SuperAdmin") == true)
            return true;

        var firmaId = kiraciServisi.MevcutFirmaId;
        if (firmaId is null or 0)
            return false;

        // Sahne önayarı → ModelId → tenant zinciri
        var sahneOnayari = await vt.UrunUcBoyutSahneOnayarlari
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sahneOnayariId && !s.SilindiMi);

        if (sahneOnayari is null)
            return false;

        return await ModelSahibiniDogrulaAsync(sahneOnayari.UrunUcBoyutModeliId);
    }
}
