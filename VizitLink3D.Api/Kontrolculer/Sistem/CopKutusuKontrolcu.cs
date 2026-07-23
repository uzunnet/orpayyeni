using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Kontrolculer.Sistem;

[ApiController]
[Route("api/cop-kutusu")]
[Authorize(Roles = "SuperAdmin")]
public class CopKutusuKontrolcu(VizitLink3DDbContext vt) : ControllerBase
{
    [HttpGet("urunler")]
    public async Task<Cevap<List<object>>> SilinmisUrunler()
    {
        var liste = await vt.Urunler
            .IgnoreQueryFilters()
            .Where(u => u.SilindiMi)
            .OrderByDescending(u => u.SilinmeTarihi)
            .Select(u => new { u.Id, u.Ad, u.Kod, u.Slug, u.SilinmeTarihi, Tur = "Urun" })
            .ToListAsync();

        return Cevap<List<object>>.Basarili(liste.Cast<object>().ToList());
    }

    [HttpGet("menu-ogeleri")]
    public async Task<Cevap<List<object>>> SilinmisMenuler()
    {
        var liste = await vt.MenuOgeleri
            .IgnoreQueryFilters()
            .Where(m => m.SilindiMi)
            .OrderByDescending(m => m.SilinmeTarihi)
            .Select(m => new { m.Id, m.Baslik, m.Url, m.Konum, m.SilinmeTarihi, Tur = "MenuOgesi" })
            .ToListAsync();

        return Cevap<List<object>>.Basarili(liste.Cast<object>().ToList());
    }

    [HttpGet("modeller")]
    public async Task<Cevap<List<object>>> SilinmisModeller()
    {
        var liste = await vt.UrunUcBoyutModelleri
            .IgnoreQueryFilters()
            .Where(m => m.SilindiMi)
            .OrderByDescending(m => m.SilinmeTarihi)
            .Select(m => new { m.Id, m.ModelAdi, m.ModelYolu, m.SilinmeTarihi, Tur = "UcBoyutModel" })
            .ToListAsync();

        return Cevap<List<object>>.Basarili(liste.Cast<object>().ToList());
    }

    [HttpPost("urun/{id:int}/geri-al")]
    public async Task<Cevap<bool>> UrunGeriAl(int id)
    {
        var urun = await vt.Urunler.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id);
        if (urun is null) return Cevap<bool>.Hata("Urun bulunamadi.");

        urun.SilindiMi = false;
        urun.SilinmeTarihi = null;
        await vt.SaveChangesAsync();

        return Cevap<bool>.Basarili(true, "Urun geri alindi.");
    }

    [HttpPost("menu/{id:int}/geri-al")]
    public async Task<Cevap<bool>> MenuGeriAl(int id)
    {
        var menu = await vt.MenuOgeleri.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.Id == id);
        if (menu is null) return Cevap<bool>.Hata("Menu bulunamadi.");
        if (menu.KilitliMi) return Cevap<bool>.Hata("Kilitli menu geri alinamaz.");

        menu.SilindiMi = false;
        menu.SilinmeTarihi = null;
        await vt.SaveChangesAsync();

        return Cevap<bool>.Basarili(true, "Menu geri alindi.");
    }

    [HttpPost("model/{id:int}/geri-al")]
    public async Task<Cevap<bool>> ModelGeriAl(int id)
    {
        var model = await vt.UrunUcBoyutModelleri.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.Id == id);
        if (model is null) return Cevap<bool>.Hata("Model bulunamadi.");

        model.SilindiMi = false;
        model.SilinmeTarihi = null;
        await vt.SaveChangesAsync();

        return Cevap<bool>.Basarili(true, "Model geri alindi.");
    }
}

