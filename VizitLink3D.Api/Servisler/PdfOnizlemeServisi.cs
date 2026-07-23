using System.Diagnostics.CodeAnalysis;
using PDFtoImage;

namespace VizitLink3D.Api.Servisler;

public class PdfOnizlemeServisi(IWebHostEnvironment ortam)
{
    private readonly IWebHostEnvironment _ortam = ortam;

    public async Task<string?> OnizlemeOlusturAsync(string? pdfYolu, CancellationToken iptal = default)
    {
        if (string.IsNullOrWhiteSpace(pdfYolu) || !PdfMi(pdfYolu))
        {
            return null;
        }

        var guvenliYol = GuvenliMedyaYolu(pdfYolu);
        if (guvenliYol is null)
        {
            return null;
        }

        var kaynakDosya = FizikselBelgeYolu(pdfYolu);
        if (!File.Exists(kaynakDosya) || !PdfCozumlemePlatformuDestekleniyor())
        {
            return null;
        }

        var onizlemeKlasoru = Path.Combine(_ortam.WebRootPath, "medya", "onizlemeler");
        Directory.CreateDirectory(onizlemeKlasoru);

        var dosyaAdi = $"{Path.GetFileNameWithoutExtension(guvenliYol)}.png";
        var hedefDosya = Path.Combine(onizlemeKlasoru, dosyaAdi);
        if (!File.Exists(hedefDosya))
        {
            await Task.Run(() => IlkSayfayiPngAtomikKaydet(kaynakDosya, hedefDosya), iptal);
        }

        return $"/medya/onizlemeler/{dosyaAdi}";
    }

    public string? FizikselGorselYolu(string? belgeYolu)
    {
        if (string.IsNullOrWhiteSpace(belgeYolu))
        {
            return null;
        }

        var guvenliYol = GuvenliMedyaYolu(belgeYolu);
        if (guvenliYol is null)
        {
            return null;
        }

        var fizikselYol = Path.Combine(_ortam.WebRootPath, guvenliYol.Replace('/', Path.DirectorySeparatorChar));
        return File.Exists(fizikselYol) ? fizikselYol : null;
    }

    public string? FizikselBelgeYolu(string? belgeYolu)
    {
        if (string.IsNullOrWhiteSpace(belgeYolu))
        {
            return null;
        }

        var guvenliYol = GuvenliMedyaYolu(belgeYolu);
        if (guvenliYol is null)
        {
            return null;
        }

        var fizikselYol = Path.Combine(_ortam.WebRootPath, guvenliYol.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fizikselYol))
        {
            return fizikselYol;
        }

        return PdfMi(belgeYolu) ? EskiKatalogDosyasiniBul(guvenliYol) : null;
    }

    public static bool GorselMi(string? yol)
    {
        if (string.IsNullOrWhiteSpace(yol))
        {
            return false;
        }

        var uzanti = Path.GetExtension(yol);
        return uzanti.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
            || uzanti.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
            || uzanti.Equals(".png", StringComparison.OrdinalIgnoreCase);
    }

    public static bool PdfMi(string? yol)
    {
        if (string.IsNullOrWhiteSpace(yol))
        {
            return false;
        }

        return Path.GetExtension(yol).Equals(".pdf", StringComparison.OrdinalIgnoreCase);
    }

    private static string? GuvenliMedyaYolu(string yol)
    {
        var temizYol = Uri.UnescapeDataString(yol)
            .Replace('\\', '/')
            .TrimStart('/');

        return temizYol.StartsWith("medya/", StringComparison.OrdinalIgnoreCase)
            ? temizYol
            : null;
    }

    private string? EskiKatalogDosyasiniBul(string guvenliYol)
    {
        var dosyaAdi = Path.GetFileName(guvenliYol);
        if (string.IsNullOrWhiteSpace(dosyaAdi))
        {
            return null;
        }

        var katalogKlasoru = Path.Combine(_ortam.WebRootPath, "medya", "kataloglar");
        if (!Directory.Exists(katalogKlasoru))
        {
            return null;
        }

        var aranan = AnahtarOlustur(Path.GetFileNameWithoutExtension(dosyaAdi));
        return Directory.EnumerateFiles(katalogKlasoru, "*.pdf", SearchOption.TopDirectoryOnly)
            .FirstOrDefault(dosya => AnahtarOlustur(Path.GetFileNameWithoutExtension(dosya)) == aranan);
    }

    private static string AnahtarOlustur(string deger)
    {
        var karakterler = deger
            .ToLowerInvariant()
            .Where(char.IsLetterOrDigit)
            .ToArray();

        return new string(karakterler);
    }

    [UnconditionalSuppressMessage(
        "Interoperability",
        "CA1416:Validate platform compatibility",
        Justification = "PDF onizleme API servisi Windows/Linux/macOS uzerinde calisir; cagri oncesinde platform korumasi uygulanir.")]
    private static void IlkSayfayiPngAtomikKaydet(string pdfDosyaYolu, string hedefDosyaYolu)
    {
        var geciciDosyaYolu = $"{hedefDosyaYolu}.{Guid.NewGuid():N}.tmp";
        try
        {
            using (var input = File.OpenRead(pdfDosyaYolu))
            using (var output = File.Create(geciciDosyaYolu))
            {
                Conversion.SavePng(output, input, page: 0);
            }

            if (File.Exists(hedefDosyaYolu))
            {
                return;
            }

            File.Move(geciciDosyaYolu, hedefDosyaYolu);
        }
        finally
        {
            if (File.Exists(geciciDosyaYolu))
            {
                File.Delete(geciciDosyaYolu);
            }
        }
    }

    private static bool PdfCozumlemePlatformuDestekleniyor() =>
        OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS();
}
