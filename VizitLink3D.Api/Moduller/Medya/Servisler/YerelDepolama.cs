namespace VizitLink3D.Api.Moduller.Medya.Servisler;

public class YerelDepolama : IDepolamaAdaptoru
{
    private readonly string _kokDizin;

    public YerelDepolama(IWebHostEnvironment ortam)
    {
        _kokDizin = Path.Combine(ortam.WebRootPath, "medya");
        if (!Directory.Exists(_kokDizin))
            Directory.CreateDirectory(_kokDizin);
    }

    public async Task<string> YukleAsync(Stream dosya, string dosyaAdi, string klasor, CancellationToken iptal = default)
    {
        var hedefKlasor = Path.Combine(_kokDizin, klasor);
        if (!Directory.Exists(hedefKlasor))
            Directory.CreateDirectory(hedefKlasor);

        var yol = Path.Combine(hedefKlasor, dosyaAdi);
        await using var fs = new FileStream(yol, FileMode.Create);
        await dosya.CopyToAsync(fs, iptal);
        return Path.Combine("medya", klasor, dosyaAdi).Replace("\\", "/");
    }

    public Task SilAsync(string dosyaYolu, CancellationToken iptal = default)
    {
        var tamYol = Path.Combine(_kokDizin, dosyaYolu.Replace("medya/", "").Replace("medya\\", ""));
        if (File.Exists(tamYol)) File.Delete(tamYol);
        return Task.CompletedTask;
    }

    public Task<Stream?> GetirAsync(string dosyaYolu, CancellationToken iptal = default)
    {
        var tamYol = Path.Combine(_kokDizin, dosyaYolu.Replace("medya/", "").Replace("medya\\", ""));
        if (!File.Exists(tamYol)) return Task.FromResult<Stream?>(null);
        return Task.FromResult<Stream?>(new FileStream(tamYol, FileMode.Open, FileAccess.Read));
    }

    public string UrlOlustur(string dosyaYolu) => $"/{dosyaYolu.Replace("\\", "/")}";

    public bool Varmi(string dosyaYolu)
    {
        var tamYol = Path.Combine(_kokDizin, dosyaYolu.Replace("medya/", "").Replace("medya\\", ""));
        return File.Exists(tamYol);
    }
}
