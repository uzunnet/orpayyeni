using Microsoft.Extensions.Caching.Memory;

namespace VizitLink3D.Api.Servisler;

public interface IOnbellekYonetici
{
    Task<T?> GetirAsync<T>(string anahtar) where T : class;
    Task YazAsync<T>(string anahtar, T veri, TimeSpan? gecerlilik = null);
    Task SilAsync(string anahtar);
    Task TemizleAsync(string desen = "*");
}

public class OnbellekYonetici : IOnbellekYonetici
{
    private readonly IMemoryCache _cache;
    private readonly HashSet<string> _anahtarlar = new();
    private readonly object _kilit = new();

    public OnbellekYonetici(IMemoryCache cache) => _cache = cache;

    public Task<T?> GetirAsync<T>(string anahtar) where T : class
    {
        _cache.TryGetValue(anahtar, out T? deger);
        return Task.FromResult(deger);
    }

    public Task YazAsync<T>(string anahtar, T veri, TimeSpan? gecerlilik = null)
    {
        var sure = gecerlilik ?? TimeSpan.FromMinutes(30);
        _cache.Set(anahtar, veri, sure);
        lock (_kilit)
        {
            _anahtarlar.Add(anahtar);
        }
        return Task.CompletedTask;
    }

    public Task SilAsync(string anahtar)
    {
        _cache.Remove(anahtar);
        lock (_kilit)
        {
            _anahtarlar.Remove(anahtar);
        }
        return Task.CompletedTask;
    }

    public Task TemizleAsync(string desen = "*")
    {
        List<string> silinecekler;
        lock (_kilit)
        {
            silinecekler = desen == "*"
                ? _anahtarlar.ToList()
                : _anahtarlar.Where(k => k.StartsWith(desen.TrimEnd('*'))).ToList();
        }

        foreach (var anahtar in silinecekler)
        {
            _cache.Remove(anahtar);
            lock (_kilit)
            {
                _anahtarlar.Remove(anahtar);
            }
        }

        return Task.CompletedTask;
    }
}
