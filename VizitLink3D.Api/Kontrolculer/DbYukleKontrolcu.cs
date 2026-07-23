using System.IO.Compression;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace VizitLink3D.Api.Kontrolculer;

[ApiController]
[Route("api/db-yukle")]
[Authorize(Roles = "SuperAdmin")]
public class DbYukleKontrolcu(IWebHostEnvironment env) : ControllerBase
{
    private const long MaksimumDosyaBoyutu = 100L * 1024 * 1024;

    [HttpPost]
    [EnableRateLimiting("Genel")]
    [RequestSizeLimit(MaksimumDosyaBoyutu)]
    public async Task<IActionResult> Yukle(IFormFile dosya)
    {
        if (dosya is null || dosya.Length == 0)
            return BadRequest("Dosya yok.");

        if (dosya.Length > MaksimumDosyaBoyutu)
            return BadRequest("Dosya cok buyuk.");

        var uzanti = Path.GetExtension(dosya.FileName);
        if (!uzanti.Equals(".db", StringComparison.OrdinalIgnoreCase) && !uzanti.Equals(".zip", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Sadece .db veya .zip kabul edilir.");

        var hedefYolu = Path.Combine(env.ContentRootPath, "vizitlink3d.db");
        var yedekYolu = hedefYolu + ".backup_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var geciciKlasor = Path.Combine(Path.GetTempPath(), "vizitlink3d-db-upload");
        Directory.CreateDirectory(geciciKlasor);

        if (System.IO.File.Exists(hedefYolu))
            System.IO.File.Copy(hedefYolu, yedekYolu, true);

        var geciciKaynak = Path.Combine(geciciKlasor, Guid.NewGuid().ToString("N") + uzanti);
        await using (var s = new FileStream(geciciKaynak, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            await dosya.CopyToAsync(s);

        var uygulanacakKaynak = geciciKaynak;

        if (uzanti.Equals(".zip", StringComparison.OrdinalIgnoreCase))
        {
            using var archive = ZipFile.OpenRead(geciciKaynak);
            var dbEntry = archive.Entries.FirstOrDefault(e => e.Name.EndsWith(".db", StringComparison.OrdinalIgnoreCase));
            if (dbEntry is null)
                return BadRequest("ZIP icinde .db dosyasi bulunamadi.");

            uygulanacakKaynak = Path.Combine(geciciKlasor, Guid.NewGuid().ToString("N") + ".db");
            dbEntry.ExtractToFile(uygulanacakKaynak, overwrite: true);
        }

        var kaynakBoyut = new FileInfo(uygulanacakKaynak).Length;
        if (kaynakBoyut == 0)
            return BadRequest("DB bos olamaz.");

        System.IO.File.Copy(uygulanacakKaynak, hedefYolu, true);

        var boyut = new FileInfo(hedefYolu).Length;
        return Ok($"DB yuklendi: {boyut} byte. Yedek: {Path.GetFileName(yedekYolu)}");
    }
}
