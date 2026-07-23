using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System.Security.Cryptography;

namespace VizitLink3D.Api.Moduller.Medya.Servisler;

public interface IResimIslemcisi
{
    Task<string> KucukBoyutOlusturAsync(string kaynakYol, string hedefKlasor, int genislik = 200, int yukseklik = 200);
    string HashHesapla(Stream dosya);
    Task<string> WebpDonusturAsync(string kaynakYol, string hedefYol);

    /// <summary>
    /// Verilen görseli maksimum kenar sınırına göre boyutlandırır.
    /// webpZorunlu=true (varsayılan) ise çıktı her zaman WebP olur.
    /// webpZorunlu=false ise ve girdi zaten WebP değilse, orijinal format (PNG/JPEG/…) korunarak sadece boyut küçültülür.
    /// </summary>
    Task<byte[]> OtomatikBoyutlandirAsync(
        byte[] veri,
        int maksimumKenar,
        int kalite,
        bool webpZorunlu = true,
        CancellationToken iptalToken = default);
}

public class ResimIslemcisi : IResimIslemcisi
{
    public async Task<string> KucukBoyutOlusturAsync(string kaynakYol, string hedefKlasor, int genislik = 200, int yukseklik = 200)
    {
        if (!Directory.Exists(hedefKlasor)) Directory.CreateDirectory(hedefKlasor);
        var dosyaAdi = Path.GetFileName(kaynakYol);
        var hedefYol = Path.Combine(hedefKlasor, $"thumb_{dosyaAdi}");

        if (!File.Exists(kaynakYol)) return hedefYol;

        using var resim = await Image.LoadAsync(kaynakYol);
        resim.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(genislik, yukseklik),
            Mode = ResizeMode.Max
        }));
        await resim.SaveAsync(hedefYol);

        return hedefYol;
    }

    public string HashHesapla(Stream dosya)
    {
        dosya.Position = 0;
        var hash = SHA256.HashData(dosya);
        dosya.Position = 0;
        return Convert.ToHexStringLower(hash);
    }

    public async Task<string> WebpDonusturAsync(string kaynakYol, string hedefYol)
    {
        if (!File.Exists(kaynakYol)) return hedefYol;

        using var resim = await Image.LoadAsync(kaynakYol);
        var webpYol = Path.ChangeExtension(hedefYol, ".webp");
        await resim.SaveAsWebpAsync(webpYol);
        return webpYol;
    }

    public async Task<byte[]> OtomatikBoyutlandirAsync(byte[] veri, int maksimumKenar, int kalite, bool webpZorunlu = true, CancellationToken iptalToken = default)
    {
        try
        {
            using var girisAkisi = new MemoryStream(veri);
            using var resim = await Image.LoadAsync(girisAkisi, iptalToken);
            var orijinalFormat = resim.Metadata.DecodedImageFormat;

            var (genislik, yukseklik) = (resim.Width, resim.Height);
            var uzunKenar = Math.Max(genislik, yukseklik);

            if (uzunKenar > maksimumKenar)
            {
                var oran = (double)maksimumKenar / uzunKenar;
                var yeniGenislik = (int)Math.Round(genislik * oran);
                var yeniYukseklik = (int)Math.Round(yukseklik * oran);

                resim.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(yeniGenislik, yeniYukseklik),
                    Mode = ResizeMode.Max,
                    Sampler = KnownResamplers.Bicubic
                }));
            }

            using var ciktiAkisi = new MemoryStream();

            var zatenWebpMi = orijinalFormat is WebpFormat;
            if (webpZorunlu || zatenWebpMi)
            {
                await resim.SaveAsync(ciktiAkisi, new WebpEncoder { Quality = kalite }, iptalToken);
            }
            else if (orijinalFormat is JpegFormat)
            {
                await resim.SaveAsync(ciktiAkisi, new JpegEncoder { Quality = kalite }, iptalToken);
            }
            else if (orijinalFormat is PngFormat)
            {
                await resim.SaveAsync(ciktiAkisi, new PngEncoder { CompressionLevel = PngCompressionLevel.BestCompression }, iptalToken);
            }
            else
            {
                // Bilinmeyen/desteklenmeyen format: orijinal formatiyla (veya varsayilan kodlayicisiyla) kaydet
                await resim.SaveAsync(ciktiAkisi, orijinalFormat ?? PngFormat.Instance, iptalToken);
            }

            return ciktiAkisi.ToArray();
        }
        catch
        {
            // Graceful degradation: hata durumunda orijinal veriyi döndür
            return veri;
        }
    }
}
