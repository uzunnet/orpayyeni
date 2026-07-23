using VizitLink3D.Api.Hubs;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace VizitLink3D.Api.Moduller.Urunler.Kontrolcüler;

[ApiController]
[Route("api/uc-boyut")]
public class UcBoyutModelKontrolcu(VizitLink3DDbContext vt, IWebHostEnvironment env, IHubContext<SahneAyarHub> hub) : ControllerBase
{
    [HttpGet("modeller")]
    public async Task<Cevap<List<UrunUcBoyutModeli>>> TumunuGetir()
    {
        var modeller = await vt.UrunUcBoyutModelleri
            .AsNoTracking()
            .Where(m => !m.SilindiMi)
            .OrderBy(m => m.OlusturulmaTarihi)
            .ToListAsync();

        return Cevap<List<UrunUcBoyutModeli>>.Basarili(modeller);
    }

    [HttpGet("modeller/{id:int}")]
    public async Task<Cevap<UrunUcBoyutModeli>> GetirAsync(int id)
    {
        var model = await vt.UrunUcBoyutModelleri
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id && !m.SilindiMi);

        if (model is null)
            return Cevap<UrunUcBoyutModeli>.Hata("3D model bulunamadi.");

        return Cevap<UrunUcBoyutModeli>.Basarili(model);
    }

    [HttpPost("modeller/yukle")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [RequestSizeLimit(30_000_000)]
    public async Task<Cevap<UrunUcBoyutModeli>> YukleAsync([FromForm] IFormFile dosya, [FromForm] int urunId, [FromForm] string? modelAdi, [FromForm] string? modelTipi)
    {
        if (dosya is null || dosya.Length == 0)
            return Cevap<UrunUcBoyutModeli>.Hata("Dosya gerekli.");

        if (!dosya.FileName.EndsWith(".glb", StringComparison.OrdinalIgnoreCase))
            return Cevap<UrunUcBoyutModeli>.Hata("Sadece .glb dosyalari desteklenir.");

        var medyaYolu = Path.Combine(env.WebRootPath ?? "wwwroot", "medya", "3d");
        if (!Directory.Exists(medyaYolu))
            Directory.CreateDirectory(medyaYolu);

        var dosyaAdi = $"{Guid.NewGuid()}_{dosya.FileName}";
        var tamYol = Path.Combine(medyaYolu, dosyaAdi);

        await using (var stream = new FileStream(tamYol, FileMode.Create))
        {
            await dosya.CopyToAsync(stream);
        }

        var model = new UrunUcBoyutModeli
        {
            UrunId = urunId,
            ModelAdi = modelAdi ?? dosya.FileName,
            ModelDosyaYolu = $"/medya/3d/{dosyaAdi}",
            ModelYolu = $"/medya/3d/{dosyaAdi}",
            ModelTipi = string.IsNullOrWhiteSpace(modelTipi) ? "Glb" : modelTipi,
            DosyaBoyutuByte = dosya.Length,
            Versiyon = 1,
            AktifMi = true,
            VarsayilanMi = true
        };

        vt.UrunUcBoyutModelleri.Add(model);
        await vt.SaveChangesAsync();
        await hub.Clients.Group($"urun_3d_{urunId}").SendAsync("UcBoyutModelGuncellendi", urunId, model.Id);

        return Cevap<UrunUcBoyutModeli>.Basarili(model, "3D model yuklendi.");
    }

    [HttpPost("modeller")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<UrunUcBoyutModeli>> OlusturAsync([FromBody] UrunUcBoyutModeli model)
    {
        if (model.UrunId <= 0)
            return Cevap<UrunUcBoyutModeli>.Hata("Urun ID gerekli.");

        vt.UrunUcBoyutModelleri.Add(model);
        await vt.SaveChangesAsync();
        await hub.Clients.Group($"urun_3d_{model.UrunId}").SendAsync("UcBoyutModelGuncellendi", model.UrunId, model.Id);

        return Cevap<UrunUcBoyutModeli>.Basarili(model, "3D model olusturuldu.");
    }

    [HttpPut("modeller/{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<UrunUcBoyutModeli>> GuncelleAsync(int id, [FromBody] UrunUcBoyutModeli model)
    {
        var mevcut = await vt.UrunUcBoyutModelleri.FindAsync(id);
        if (mevcut is null || mevcut.SilindiMi)
            return Cevap<UrunUcBoyutModeli>.Hata("3D model bulunamadi.");

        mevcut.ModelAdi = model.ModelAdi;
        mevcut.ModelDosyaYolu = model.ModelDosyaYolu;
        mevcut.ModelTipi = model.ModelTipi;
        mevcut.ModelYolu = model.ModelYolu;
        mevcut.MedyaId = model.MedyaId;
        mevcut.OnizlemeMedyaId = model.OnizlemeMedyaId;
        mevcut.DosyaBoyutuByte = model.DosyaBoyutuByte;
        mevcut.Versiyon = model.Versiyon;
        mevcut.KameraAyarJson = model.KameraAyarJson;
        mevcut.IsikAyarJson = model.IsikAyarJson;
        mevcut.CevreAyarJson = model.CevreAyarJson;
        mevcut.ModelAnalizJson = model.ModelAnalizJson;
        mevcut.VarsayilanMi = model.VarsayilanMi;
        mevcut.AktifMi = model.AktifMi;
        mevcut.GuncellenmeTarihi = DateTime.UtcNow;

        await vt.SaveChangesAsync();
        await hub.Clients.Group($"urun_3d_{mevcut.UrunId}").SendAsync("UcBoyutModelGuncellendi", mevcut.UrunId, mevcut.Id);
        return Cevap<UrunUcBoyutModeli>.Basarili(mevcut, "3D model guncellendi.");
    }

    [HttpDelete("modeller/{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<bool>> SilAsync(int id)
    {
        var mevcut = await vt.UrunUcBoyutModelleri.FindAsync(id);
        if (mevcut is null || mevcut.SilindiMi)
            return Cevap<bool>.Hata("3D model bulunamadi.");

        mevcut.SilindiMi = true;
        mevcut.SilinmeTarihi = DateTime.UtcNow;

        await vt.SaveChangesAsync();
        await hub.Clients.Group($"urun_3d_{mevcut.UrunId}").SendAsync("UcBoyutModelGuncellendi", mevcut.UrunId, mevcut.Id);
        return Cevap<bool>.Basarili(true, "3D model silindi.");
    }

    // === PARCA UCLARI ===

    [HttpGet("modeller/{modelId:int}/parcalar")]
    public async Task<Cevap<List<UrunUcBoyutParcasi>>> ParcalariGetir(int modelId)
    {
        var parcalar = await vt.UrunUcBoyutParcalari
            .AsNoTracking()
            .Where(p => p.UrunUcBoyutModeliId == modelId && !p.SilindiMi)
            .OrderBy(p => p.SiraNo)
            .ToListAsync();

        return Cevap<List<UrunUcBoyutParcasi>>.Basarili(parcalar);
    }

    [HttpPost("modeller/{modelId:int}/parcalar")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<UrunUcBoyutParcasi>> ParcaEkle(int modelId, [FromBody] UrunUcBoyutParcasi parca)
    {
        parca.UrunUcBoyutModeliId = modelId;
        vt.UrunUcBoyutParcalari.Add(parca);
        await vt.SaveChangesAsync();
        await hub.Clients.Group($"sahne_{modelId}").SendAsync("UcBoyutParcaGuncellendi", modelId, parca.Id);

        return Cevap<UrunUcBoyutParcasi>.Basarili(parca, "Parca eklendi.");
    }

    [HttpPut("modeller/parcalar/{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<UrunUcBoyutParcasi>> ParcaGuncelle(int id, [FromBody] UrunUcBoyutParcasi parca)
    {
        var mevcut = await vt.UrunUcBoyutParcalari.FindAsync(id);
        if (mevcut is null || mevcut.SilindiMi)
            return Cevap<UrunUcBoyutParcasi>.Hata("Parca bulunamadi.");

        mevcut.GorunenAd = parca.GorunenAd;
        mevcut.MeshAdi = parca.MeshAdi;
        mevcut.ParcaGrubuId = parca.ParcaGrubuId;
        mevcut.SecilebilirMi = parca.SecilebilirMi;
        mevcut.RenklenebilirMi = parca.RenklenebilirMi;
        mevcut.MalzemeDegisebilirMi = parca.MalzemeDegisebilirMi;
        mevcut.HareketliMi = parca.HareketliMi;
        mevcut.GizlenebilirMi = parca.GizlenebilirMi;
        mevcut.HareketTipi = parca.HareketTipi;
        mevcut.MinDeger = parca.MinDeger;
        mevcut.MaxDeger = parca.MaxDeger;
        mevcut.VarsayilanDeger = parca.VarsayilanDeger;
        mevcut.VarsayilanRenkId = parca.VarsayilanRenkId;
        mevcut.VarsayilanMalzemeId = parca.VarsayilanMalzemeId;
        mevcut.SiraNo = parca.SiraNo;
        mevcut.AktifMi = parca.AktifMi;
        mevcut.GuncellenmeTarihi = DateTime.UtcNow;
        // Admin onay sistemi
        mevcut.AdminOnayliMi = parca.AdminOnayliMi;
        mevcut.ParcaTipi = parca.ParcaTipi;
        mevcut.MalzemeTipiKisiti = parca.MalzemeTipiKisiti;

        await vt.SaveChangesAsync();
        await hub.Clients.Group($"sahne_{mevcut.UrunUcBoyutModeliId}").SendAsync("UcBoyutParcaGuncellendi", mevcut.UrunUcBoyutModeliId, mevcut.Id);
        return Cevap<UrunUcBoyutParcasi>.Basarili(mevcut, "Parca guncellendi.");
    }

    [HttpDelete("modeller/parcalar/{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<bool>> ParcaSil(int id)
    {
        var mevcut = await vt.UrunUcBoyutParcalari.FindAsync(id);
        if (mevcut is null || mevcut.SilindiMi)
            return Cevap<bool>.Hata("Parca bulunamadi.");

        mevcut.SilindiMi = true;
        mevcut.SilinmeTarihi = DateTime.UtcNow;
        await vt.SaveChangesAsync();
        await hub.Clients.Group($"sahne_{mevcut.UrunUcBoyutModeliId}").SendAsync("UcBoyutParcaGuncellendi", mevcut.UrunUcBoyutModeliId, mevcut.Id);
        return Cevap<bool>.Basarili(true, "Parca silindi.");
    }

    [HttpPost("modeller/{modelId:int}/analiz-sonucu")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<UrunUcBoyutModeli>> AnalizSonucuKaydet(int modelId, [FromBody] string analizJson)
    {
        var model = await vt.UrunUcBoyutModelleri.FindAsync(modelId);
        if (model is null || model.SilindiMi)
            return Cevap<UrunUcBoyutModeli>.Hata("3D model bulunamadi.");

        model.ModelAnalizJson = analizJson;
        model.GuncellenmeTarihi = DateTime.UtcNow;

        await vt.SaveChangesAsync();
        await hub.Clients.Group($"sahne_{modelId}").SendAsync("UcBoyutModelAnaliziGuncellendi", modelId);
        return Cevap<UrunUcBoyutModeli>.Basarili(model, "Analiz sonucu kaydedildi.");
    }

    // === SAHNE AYARLARI ===

    [HttpGet("modeller/{modelId:int}/sahne-ayarlari")]
    public async Task<Cevap<object>> SahneAyarlariniGetir(int modelId)
    {
        var model = await vt.UrunUcBoyutModelleri
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == modelId && !m.SilindiMi);

        if (model is null)
            return Cevap<object>.Hata("3D model bulunamadi.");

        return Cevap<object>.Basarili(new
        {
            modelId = model.Id,
            kamera = model.KameraAyarJson,
            isik = model.IsikAyarJson,
            cevre = model.CevreAyarJson
        });
    }

    [HttpPut("modeller/{modelId:int}/kamera-ayar")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<UrunUcBoyutModeli>> KameraAyarGuncelle(int modelId, [FromBody] KameraAyar kamera)
    {
        var model = await vt.UrunUcBoyutModelleri.FindAsync(modelId);
        if (model is null || model.SilindiMi)
            return Cevap<UrunUcBoyutModeli>.Hata("3D model bulunamadi.");

        model.KameraAyarJson = JsonSerializer.Serialize(kamera);
        model.GuncellenmeTarihi = DateTime.UtcNow;

        await vt.SaveChangesAsync();
        await hub.Clients.Group($"sahne_{modelId}").SendAsync("SahneAyarGuncellendi", modelId, "kamera", model.KameraAyarJson);
        return Cevap<UrunUcBoyutModeli>.Basarili(model, "Kamera ayarlari guncellendi.");
    }

    [HttpPut("modeller/{modelId:int}/isik-ayar")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<UrunUcBoyutModeli>> IsikAyarGuncelle(int modelId, [FromBody] IsikAyar isik)
    {
        var model = await vt.UrunUcBoyutModelleri.FindAsync(modelId);
        if (model is null || model.SilindiMi)
            return Cevap<UrunUcBoyutModeli>.Hata("3D model bulunamadi.");

        model.IsikAyarJson = JsonSerializer.Serialize(isik);
        model.GuncellenmeTarihi = DateTime.UtcNow;

        await vt.SaveChangesAsync();
        await hub.Clients.Group($"sahne_{modelId}").SendAsync("SahneAyarGuncellendi", modelId, "isik", model.IsikAyarJson);
        return Cevap<UrunUcBoyutModeli>.Basarili(model, "Isik ayarlari guncellendi.");
    }

    [HttpPut("modeller/{modelId:int}/cevre-ayar")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<UrunUcBoyutModeli>> CevreAyarGuncelle(int modelId, [FromBody] CevreAyar cevre)
    {
        var model = await vt.UrunUcBoyutModelleri.FindAsync(modelId);
        if (model is null || model.SilindiMi)
            return Cevap<UrunUcBoyutModeli>.Hata("3D model bulunamadi.");

        model.CevreAyarJson = JsonSerializer.Serialize(cevre);
        model.GuncellenmeTarihi = DateTime.UtcNow;

        await vt.SaveChangesAsync();
        await hub.Clients.Group($"sahne_{modelId}").SendAsync("SahneAyarGuncellendi", modelId, "cevre", model.CevreAyarJson);
        return Cevap<UrunUcBoyutModeli>.Basarili(model, "Cevre ayarlari guncellendi.");
    }
}
