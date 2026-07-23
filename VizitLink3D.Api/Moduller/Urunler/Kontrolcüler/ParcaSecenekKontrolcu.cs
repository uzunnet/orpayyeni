using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Moduller.Urunler.Kontrolcüler;

[ApiController]
[Route("api/uc-boyut/parcalar")]
public class ParcaSecenekKontrolcu(VizitLink3DDbContext vt) : ControllerBase
{
    // === RENK SECENEKLERI ===

    [HttpGet("{parcaId:int}/renk-secenekleri")]
    public async Task<Cevap<List<UrunParcaRenkSecenegi>>> RenkSecenekleri(int parcaId)
    {
        var secenekler = await vt.UrunParcaRenkSecenekleri
            .AsNoTracking()
            .Where(s => s.UrunUcBoyutParcasiId == parcaId && s.AktifMi)
            .ToListAsync();

        return Cevap<List<UrunParcaRenkSecenegi>>.Basarili(secenekler);
    }

    [HttpPost("{parcaId:int}/renk-secenekleri")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<UrunParcaRenkSecenegi>> RenkSecenegiEkle(int parcaId, [FromBody] UrunParcaRenkSecenegi secenek)
    {
        secenek.UrunUcBoyutParcasiId = parcaId;
        vt.UrunParcaRenkSecenekleri.Add(secenek);
        await vt.SaveChangesAsync();

        return Cevap<UrunParcaRenkSecenegi>.Basarili(secenek, "Renk secenegi eklendi.");
    }

    [HttpDelete("renk-secenekleri/{id:int}")]
    [HttpDelete("{parcaId:int}/renk-secenekleri/{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<bool>> RenkSecenegiSil(int id, int? parcaId = null)
    {
        var mevcut = await vt.UrunParcaRenkSecenekleri.FindAsync(id);
        if (mevcut is null)
            return Cevap<bool>.Hata("Renk secenegi bulunamadi.");

        mevcut.AktifMi = false;
        await vt.SaveChangesAsync();
        return Cevap<bool>.Basarili(true, "Renk secenegi silindi.");
    }

    // === MALZEME SECENEKLERI ===

    [HttpGet("{parcaId:int}/malzeme-secenekleri")]
    public async Task<Cevap<List<UrunParcaMalzemeSecenegi>>> MalzemeSecenekleri(int parcaId)
    {
        var secenekler = await vt.UrunParcaMalzemeSecenekleri
            .AsNoTracking()
            .Where(s => s.UrunUcBoyutParcasiId == parcaId && s.AktifMi)
            .ToListAsync();

        return Cevap<List<UrunParcaMalzemeSecenegi>>.Basarili(secenekler);
    }

    [HttpPost("{parcaId:int}/malzeme-secenekleri")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<UrunParcaMalzemeSecenegi>> MalzemeSecenegiEkle(int parcaId, [FromBody] UrunParcaMalzemeSecenegi secenek)
    {
        secenek.UrunUcBoyutParcasiId = parcaId;
        vt.UrunParcaMalzemeSecenekleri.Add(secenek);
        await vt.SaveChangesAsync();

        return Cevap<UrunParcaMalzemeSecenegi>.Basarili(secenek, "Malzeme secenegi eklendi.");
    }

    [HttpDelete("malzeme-secenekleri/{id:int}")]
    [HttpDelete("{parcaId:int}/malzeme-secenekleri/{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<bool>> MalzemeSecenegiSil(int id, int? parcaId = null)
    {
        var mevcut = await vt.UrunParcaMalzemeSecenekleri.FindAsync(id);
        if (mevcut is null)
            return Cevap<bool>.Hata("Malzeme secenegi bulunamadi.");

        mevcut.AktifMi = false;
        await vt.SaveChangesAsync();
        return Cevap<bool>.Basarili(true, "Malzeme secenegi silindi.");
    }
}
