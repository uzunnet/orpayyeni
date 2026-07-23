using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace VizitLink3D.Api.Moduller.Medya.Kontrolcu;

[ApiController]
[Route("api/medya/goruntu")]
public class GoruntuIslemeKontrolcu : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public GoruntuIslemeKontrolcu(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpGet("{**dosyaAdi}")]
    public async Task<IActionResult> Getir(
        string dosyaAdi,
        [FromQuery] int w = 0,
        [FromQuery] int h = 0,
        [FromQuery] int q = 80,
        [FromQuery] string fmt = "")
    {
        var dosyaYolu = Path.Combine(_env.WebRootPath, "medya", dosyaAdi);
        if (!System.IO.File.Exists(dosyaYolu))
            return NotFound();

        // Eger islem parametresi yoksa, ham dosyayi dondur
        if (w == 0 && h == 0 && string.IsNullOrEmpty(fmt))
        {
            var mime = Path.GetExtension(dosyaAdi).ToLower() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
            return PhysicalFile(dosyaYolu, mime);
        }

        // Resize + format donusumu
        using var gorsel = await Image.LoadAsync(dosyaYolu);

        if (w > 0 || h > 0)
        {
            gorsel.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(w > 0 ? w : 0, h > 0 ? h : 0),
                Mode = ResizeMode.Max
            }));
        }

        var ms = new MemoryStream();
        var ciktiFormati = fmt.ToLower();

        if (ciktiFormati == "webp")
        {
            await gorsel.SaveAsWebpAsync(ms, new WebpEncoder { Quality = q });
            ms.Position = 0;
            return File(ms, "image/webp");
        }
        else
        {
            // Orijinal formatta kaydet (JPEG/PNG)
            await gorsel.SaveAsJpegAsync(ms, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder { Quality = q });
            ms.Position = 0;
            return File(ms, "image/jpeg");
        }
    }
}
