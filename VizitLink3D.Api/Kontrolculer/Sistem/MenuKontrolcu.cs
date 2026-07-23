using VizitLink3D.Api.Modeller;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Api.Servisler;

namespace VizitLink3D.Api.Kontrolculer.Sistem;

[ApiController]
[Route("api/menu")]
public class MenuKontrolcu(VizitLink3DDbContext vt, KiraciServisi kiraci) : ControllerBase
{
    [HttpGet("ana")]
    public async Task<Cevap<List<MenuOgesi>>> MenuuGetir() => await KonumdanGetir("PublicHeader");

    [HttpGet("admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<List<MenuOgesi>>> AdminMenusuGetir() => await KonumdanGetir("AdminSol");

    [HttpGet("footer")]
    public async Task<Cevap<List<MenuOgesi>>> FooterMenusuGetir() => await KonumdanGetir("PublicFooterHizli");

    [HttpGet("footer-kategori")]
    public async Task<Cevap<List<MenuOgesi>>> FooterKategorileriGetir() => await KonumdanGetir("PublicFooterKategori");

    [HttpGet("konum/{konum}")]
    public async Task<Cevap<List<MenuOgesi>>> KonumdanGetir(string konum)
    {
        var firmaId = kiraci.MevcutFirmaId;
        var sorgu = vt.MenuOgeleri
            .Where(m => m.AktifMi && m.UstMenuId == null && m.Konum == konum && !m.SilindiMi);

        if (firmaId.HasValue)
            sorgu = sorgu.Where(m => m.FirmaId == firmaId.Value);

        var kokMenuler = await sorgu
            .Include(m => m.AltMenuler.Where(a => a.AktifMi && !a.SilindiMi))
            .OrderBy(m => m.Sira)
            .ToListAsync();
        return Cevap<List<MenuOgesi>>.Basarili(kokMenuler);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<List<MenuOgesi>>> TumunuGetir()
    {
        var firmaId = kiraci.MevcutFirmaId;
        var sorgu = vt.MenuOgeleri
            .Where(m => !m.SilindiMi);

        if (firmaId.HasValue)
            sorgu = sorgu.Where(m => m.FirmaId == firmaId.Value);

        var liste = await sorgu
            .Include(m => m.AltMenuler.Where(a => !a.SilindiMi))
            .OrderBy(m => m.Sira)
            .ToListAsync();
        return Cevap<List<MenuOgesi>>.Basarili(liste);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<MenuOgesi>> Ekle([FromBody] MenuOgesi oge)
    {
        vt.MenuOgeleri.Add(oge);
        await vt.SaveChangesAsync();
        return Cevap<MenuOgesi>.Basarili(oge);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<MenuOgesi>> Guncelle(int id, [FromBody] MenuOgesi oge)
    {
        var mevcut = await vt.MenuOgeleri.FindAsync(id);
        if (mevcut is null) return Cevap<MenuOgesi>.Hata("Men� ��esi bulunamad�.");
        oge.Id = id;
        vt.Entry(mevcut).CurrentValues.SetValues(oge);
        await vt.SaveChangesAsync();
        return Cevap<MenuOgesi>.Basarili(mevcut);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<bool>> Sil(int id)
    {
        var mevcut = await vt.MenuOgeleri.FindAsync(id);
        if (mevcut is null) return Cevap<bool>.Hata("Men� ��esi bulunamad�.");
        mevcut.SilindiMi = true;
        mevcut.SilinmeTarihi = DateTime.UtcNow;
        vt.MenuOgeleri.Update(mevcut);
        await vt.SaveChangesAsync();
        return Cevap<bool>.Basarili(true);
    }
}


