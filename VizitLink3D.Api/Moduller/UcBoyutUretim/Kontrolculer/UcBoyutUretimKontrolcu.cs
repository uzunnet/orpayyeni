using VizitLink3D.Api.Moduller.UcBoyutUretim.Servisler;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VizitLink3D.Api.Moduller.UcBoyutUretim.Kontrolculer;

/// <summary>
/// Resimden AI ile statik (tek parca, sabit dokulu) 3D onizleme mesh'i uretir.
/// FUGA gibi parca-parca renklendirilebilir model URETMEZ - sadece dondurulebilir urun onizlemesi icindir.
/// </summary>
[ApiController]
[Route("api/uc-boyut-uretim")]
public class UcBoyutUretimKontrolcu(
    PythonUcBoyutSaglayici saglayici,
    VizitLink3DDbContext vt,
    IWebHostEnvironment env) : ControllerBase
{
    [HttpGet("saglik")]
    public async Task<Cevap<bool>> SaglikKontrolAsync()
    {
        var calisiyorMu = await saglayici.SaglikTestiAsync();
        return calisiyorMu
            ? Cevap<bool>.Basarili(true, "Python 3D uretim servisi calisiyor.")
            : Cevap<bool>.Hata("Python 3D uretim servisine ulasilamiyor.");
    }

    [HttpPost("resimden-uret")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [RequestSizeLimit(20_000_000)]
    public async Task<Cevap<UrunUcBoyutModeli>> ResimdenUretAsync(
        [FromForm] IFormFile dosya,
        [FromForm] int urunId,
        [FromForm] string? modelAdi)
    {
        if (dosya is null || dosya.Length == 0)
            return Cevap<UrunUcBoyutModeli>.Hata("Resim dosyasi gerekli.");

        if (urunId <= 0)
            return Cevap<UrunUcBoyutModeli>.Hata("Urun ID gerekli.");

        await using var resimStream = dosya.OpenReadStream();
        var (basariliMi, glbVerisi, hataMesaji) = await saglayici.UretAsync(resimStream, dosya.FileName);

        if (!basariliMi || glbVerisi is null)
            return Cevap<UrunUcBoyutModeli>.Hata($"3D uretim basarisiz: {hataMesaji}");

        var medyaYolu = Path.Combine(env.WebRootPath ?? "wwwroot", "medya", "3d");
        if (!Directory.Exists(medyaYolu))
            Directory.CreateDirectory(medyaYolu);

        var dosyaAdi = $"{Guid.NewGuid()}_{Path.GetFileNameWithoutExtension(dosya.FileName)}.glb";
        var tamYol = Path.Combine(medyaYolu, dosyaAdi);
        await System.IO.File.WriteAllBytesAsync(tamYol, glbVerisi);

        var model = new UrunUcBoyutModeli
        {
            UrunId = urunId,
            ModelAdi = modelAdi ?? Path.GetFileNameWithoutExtension(dosya.FileName),
            ModelDosyaYolu = $"/medya/3d/{dosyaAdi}",
            ModelYolu = $"/medya/3d/{dosyaAdi}",
            ModelTipi = "Glb",
            DosyaBoyutuByte = glbVerisi.Length,
            Versiyon = 1,
            AktifMi = true,
            VarsayilanMi = true
        };

        vt.UrunUcBoyutModelleri.Add(model);
        await vt.SaveChangesAsync();

        return Cevap<UrunUcBoyutModeli>.Basarili(
            model,
            "3D model AI ile uretildi. Bu statik bir onizlemedir - parca parca renklendirilemez.");
    }
}
