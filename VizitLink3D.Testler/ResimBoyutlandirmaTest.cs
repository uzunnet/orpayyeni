using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using VizitLink3D.Api.Moduller.Medya.Servisler;

namespace VizitLink3D.Testler;

public class ResimBoyutlandirmaTest
{
    private readonly IResimIslemcisi _islemci = new ResimIslemcisi();

    private static byte[] TestGorseliOlustur(int genislik, int yukseklik)
    {
        using var resim = new Image<Rgba32>(genislik, yukseklik);
        for (var y = 0; y < yukseklik; y++)
        for (var x = 0; x < genislik; x++)
            resim[x, y] = new Rgba32((byte)(x % 256), (byte)(y % 256), 128);
        using var ms = new MemoryStream();
        resim.SaveAsPng(ms);
        return ms.ToArray();
    }

    [Fact]
    public async Task Boyutlandir_BuyukGorsel_UzunKenariMaksimumaIndirir()
    {
        // Arrange: 2000x1500, maks=1000 → 1000x750
        var veri = TestGorseliOlustur(2000, 1500);

        // Act
        var sonuc = await _islemci.OtomatikBoyutlandirAsync(veri, 1000, 85);

        // Assert
        using var sonucResim = Image.Load<Rgba32>(sonuc);
        Assert.Equal(1000, sonucResim.Width);
        Assert.Equal(750, sonucResim.Height);
    }

    [Fact]
    public async Task Boyutlandir_KucukGorsel_WebpYeCevirir()
    {
        // Arrange: 800x600 < 1000, WebP'ye çevrilmeli
        var veri = TestGorseliOlustur(800, 600);

        // Act
        var sonuc = await _islemci.OtomatikBoyutlandirAsync(veri, 1000, 85);

        // Assert: Orijinal boyutlar korunur (küçük olduğu için resize yok)
        using var sonucResim = Image.Load<Rgba32>(sonuc);
        Assert.Equal(800, sonucResim.Width);
        Assert.Equal(600, sonucResim.Height);
        // WebP magic bytes kontrolü (RIFF)
        Assert.Equal((byte)'R', sonuc[0]);
        Assert.Equal((byte)'I', sonuc[1]);
    }

    [Fact]
    public async Task Boyutlandir_WebpKalite_DosyaBoyutuAzalir()
    {
        // Arrange: 2000x2000 PNG, kalite=60 ile WebP boyutu < PNG boyutu
        var veri = TestGorseliOlustur(2000, 2000);
        var pngBoyutu = veri.Length;

        // Act
        var sonuc = await _islemci.OtomatikBoyutlandirAsync(veri, 1000, 60);

        // Assert: WebP boyutu PNG'den küçük olmalı
        Assert.True(sonuc.Length < pngBoyutu,
            $"WebP boyutu ({sonuc.Length}) PNG boyutundan ({pngBoyutu}) küçük olmalıydı");
    }

    [Fact]
    public async Task Boyutlandir_GecersizVeri_OrijinaliDondurur()
    {
        // Arrange: Bozuk byte dizisi
        var bozukVeri = new byte[] { 0x00, 0x01, 0x02, 0x03 };

        // Act
        var sonuc = await _islemci.OtomatikBoyutlandirAsync(bozukVeri, 1000, 85);

        // Assert: Orijinal veri aynen dönmeli (graceful degradation)
        Assert.Same(bozukVeri, sonuc);
    }

    [Fact]
    public async Task Boyutlandir_OranliKucultme_EnBoyOraniniKorar()
    {
        // Arrange: 4000x1000, maks=1000 → 1000x250 (4:1)
        var veri = TestGorseliOlustur(4000, 1000);

        // Act
        var sonuc = await _islemci.OtomatikBoyutlandirAsync(veri, 1000, 85);

        // Assert
        using var sonucResim = Image.Load<Rgba32>(sonuc);
        Assert.Equal(1000, sonucResim.Width);
        Assert.Equal(250, sonucResim.Height);
        // En-boy oranı: 1000/250 = 4.0, orijinal 4000/1000 = 4.0
        var oran = (double)sonucResim.Width / sonucResim.Height;
        Assert.True(Math.Abs(oran - 4.0) < 0.01,
            $"En-boy oranı 4.0 olmalıydı, {oran:F2} bulundu");
    }

    [Fact]
    public async Task Boyutlandir_WebpZorunluFalse_PngFormatiKorur()
    {
        // Arrange: 2000x1500 PNG, webpZorunlu=false → boyut küçülür ama format PNG kalmalı
        var veri = TestGorseliOlustur(2000, 1500);

        // Act
        var sonuc = await _islemci.OtomatikBoyutlandirAsync(veri, 1000, 85, webpZorunlu: false);

        // Assert: PNG magic bytes (89 50 4E 47), WebP (RIFF) DEĞİL
        Assert.Equal(0x89, sonuc[0]);
        Assert.Equal((byte)'P', sonuc[1]);
        Assert.Equal((byte)'N', sonuc[2]);
        Assert.Equal((byte)'G', sonuc[3]);

        using var sonucResim = Image.Load<Rgba32>(sonuc);
        Assert.Equal(1000, sonucResim.Width);
        Assert.Equal(750, sonucResim.Height);
    }

    [Fact]
    public async Task Boyutlandir_WebpZorunluTrue_WebpFormatinaCevirir()
    {
        // Arrange: 2000x1500 PNG, webpZorunlu=true (varsayılan) → WebP'ye çevrilmeli
        var veri = TestGorseliOlustur(2000, 1500);

        // Act
        var sonuc = await _islemci.OtomatikBoyutlandirAsync(veri, 1000, 85, webpZorunlu: true);

        // Assert: WebP magic bytes (RIFF)
        Assert.Equal((byte)'R', sonuc[0]);
        Assert.Equal((byte)'I', sonuc[1]);
    }
}
