using VizitLink3D.Api.Moduller.Medya.Servisler;
using System.Text;

namespace VizitLink3D.Testler;

public class DepolamaTestleri : IDisposable
{
    private readonly string _testDizini;

    public DepolamaTestleri()
    {
        _testDizini = Path.Combine(Path.GetTempPath(), "vizitlink3d_test_medya_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(Path.Combine(_testDizini, "medya", "genel"));
    }

    [Fact]
    public async Task YerelDepolama_Yukle_DosyaOlusturmali()
    {
        var depo = new YerelDepolama(_testDizini);

        var icerik = "test icerigi";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(icerik));
        var yol = await depo.YukleAsync(stream, "test.txt", "genel");

        Assert.EndsWith("test.txt", yol);
        Assert.True(File.Exists(Path.Combine(_testDizini, yol)));
    }

    [Fact]
    public async Task YerelDepolama_Varmi_DogruCalismali()
    {
        var depo = new YerelDepolama(_testDizini);

        var icerik = "test";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(icerik));
        var yol = await depo.YukleAsync(stream, "varmi.txt", "genel");

        Assert.True(depo.Varmi(yol));
        Assert.False(depo.Varmi("medya/genel/yok.txt"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDizini))
            Directory.Delete(_testDizini, true);
    }
}

internal class YerelDepolama : IDepolamaAdaptoru
{
    private readonly string _kokDizin;

    public YerelDepolama(string kokDizin)
    {
        _kokDizin = kokDizin;
        if (!Directory.Exists(_kokDizin))
            Directory.CreateDirectory(_kokDizin);
    }

    public async Task<string> YukleAsync(Stream dosya, string dosyaAdi, string klasor, CancellationToken iptal = default)
    {
        var hedefKlasor = Path.Combine(_kokDizin, "medya", klasor);
        if (!Directory.Exists(hedefKlasor)) Directory.CreateDirectory(hedefKlasor);
        var yol = Path.Combine(hedefKlasor, dosyaAdi);
        await using var fs = new FileStream(yol, FileMode.Create);
        await dosya.CopyToAsync(fs, iptal);
        return Path.Combine("medya", klasor, dosyaAdi);
    }

    public Task SilAsync(string dosyaYolu, CancellationToken iptal = default)
    {
        var tamYol = Path.Combine(_kokDizin, dosyaYolu);
        if (File.Exists(tamYol)) File.Delete(tamYol);
        return Task.CompletedTask;
    }

    public Task<Stream?> GetirAsync(string dosyaYolu, CancellationToken iptal = default)
    {
        var tamYol = Path.Combine(_kokDizin, dosyaYolu);
        if (!File.Exists(tamYol)) return Task.FromResult<Stream?>(null);
        return Task.FromResult<Stream?>(new FileStream(tamYol, FileMode.Open, FileAccess.Read));
    }

    public string UrlOlustur(string dosyaYolu) => $"/{dosyaYolu.Replace("\\", "/")}";

    public bool Varmi(string dosyaYolu)
    {
        var tamYol = Path.Combine(_kokDizin, dosyaYolu);
        return File.Exists(tamYol);
    }
}
