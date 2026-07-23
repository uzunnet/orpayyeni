using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Servisler;

public interface ICeviriServisi
{
    Task<string> CeviriGetirAsync(string anahtar, string dil, string varsayilan);
    Task<Dictionary<string, string>> TumCevirileriGetirAsync(string dil);
    Task OnbellekTemizleAsync();
}

public class CeviriServisi : ICeviriServisi
{
    private readonly VizitLink3DDbContext _db;
    private readonly IOnbellekYonetici _onbellek;
    private readonly IOtomatikCeviriServisi _otomatikCeviri;
    private const string CACHE_ON_EK = "ceviri:";

    public CeviriServisi(VizitLink3DDbContext db, IOnbellekYonetici onbellek, IOtomatikCeviriServisi otomatikCeviri)
    {
        _db = db;
        _onbellek = onbellek;
        _otomatikCeviri = otomatikCeviri;
    }

    public async Task<string> CeviriGetirAsync(string anahtar, string dil, string varsayilan)
    {
        var cacheAnahtar = $"{CACHE_ON_EK}{dil}:{anahtar}";
        var ceviri = await _onbellek.GetirAsync<string>(cacheAnahtar);
        if (ceviri != null) return ceviri;

        var kayit = await _db.Ceviriler
            .FirstOrDefaultAsync(c => c.Anahtar == anahtar && c.Dil == dil);

        if (kayit != null)
        {
            ceviri = kayit.Deger;
        }
        else
        {
            if (dil.Equals("tr", StringComparison.OrdinalIgnoreCase))
            {
                ceviri = varsayilan;
            }
            else
            {
                var trKayit = await _db.Ceviriler
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Anahtar == anahtar && c.Dil == "tr");

                var metin = trKayit?.Deger ?? varsayilan;
                var response = await _otomatikCeviri.CevirAsync(metin, "tr", dil);
                if (response.BasariliMi && !string.IsNullOrEmpty(response.Veri))
                {
                    ceviri = response.Veri;
                    _db.Ceviriler.Add(new Ceviri
                    {
                        Anahtar = anahtar,
                        Dil = dil,
                        Deger = ceviri,
                        Bolum = "otomatik",
                        OlusturulmaTarihi = DateTime.UtcNow
                    });
                    await _db.SaveChangesAsync();
                }
                else
                {
                    ceviri = metin;
                }
            }
        }

        await _onbellek.YazAsync(cacheAnahtar, ceviri, TimeSpan.FromMinutes(30));
        return ceviri;
    }

    public async Task<Dictionary<string, string>> TumCevirileriGetirAsync(string dil)
    {
        var cacheAnahtar = $"{CACHE_ON_EK}{dil}:tumu";
        var tumu = await _onbellek.GetirAsync<Dictionary<string, string>>(cacheAnahtar);
        if (tumu != null) return tumu;

        var trListe = await _db.Ceviriler
            .AsNoTracking()
            .Where(c => c.Dil == "tr")
            .ToListAsync();
        var trSozluk = trListe.ToDictionary(c => c.Anahtar, c => c.Deger);

        if (dil.Equals("tr", StringComparison.OrdinalIgnoreCase))
        {
            await _onbellek.YazAsync(cacheAnahtar, trSozluk, TimeSpan.FromMinutes(30));
            return trSozluk;
        }

        var liste = await _db.Ceviriler
            .Where(c => c.Dil == dil)
            .ToListAsync();
        var sozluk = liste.ToDictionary(c => c.Anahtar, c => c.Deger);

        var missingKeys = trSozluk.Keys.Except(sozluk.Keys).ToList();

        if (missingKeys.Any())
        {
            var translationTasks = missingKeys.Select(async key =>
            {
                var trText = trSozluk[key];
                var response = await _otomatikCeviri.CevirAsync(trText, "tr", dil);
                return new { Key = key, Text = response.Veri, Success = response.BasariliMi };
            }).ToList();

            var results = await Task.WhenAll(translationTasks);

            var newCeviriler = new List<Ceviri>();
            foreach (var r in results)
            {
                if (r.Success && !string.IsNullOrEmpty(r.Text))
                {
                    newCeviriler.Add(new Ceviri
                    {
                        Anahtar = r.Key,
                        Dil = dil,
                        Deger = r.Text,
                        Bolum = "otomatik",
                        OlusturulmaTarihi = DateTime.UtcNow
                    });
                    sozluk[r.Key] = r.Text;
                }
                else
                {
                    sozluk[r.Key] = trSozluk[r.Key];
                }
            }

            if (newCeviriler.Any())
            {
                _db.Ceviriler.AddRange(newCeviriler);
                await _db.SaveChangesAsync();
            }
        }

        foreach (var kv in trSozluk)
        {
            if (!sozluk.ContainsKey(kv.Key))
            {
                sozluk[kv.Key] = kv.Value;
            }
        }

        await _onbellek.YazAsync(cacheAnahtar, sozluk, TimeSpan.FromMinutes(30));
        return sozluk;
    }

    public async Task OnbellekTemizleAsync()
    {
        await _onbellek.TemizleAsync($"{CACHE_ON_EK}*");
    }
}
