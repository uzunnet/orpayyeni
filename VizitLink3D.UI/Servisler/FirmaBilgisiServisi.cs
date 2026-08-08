using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.UI.Servisler;

/// <summary>
/// SaaS multi-tenant: Geçerli firma bilgisini (logo, ad, tema, renk) API'den çeker.
/// Layout ve sayfalar bunu kullanarak dinamik UI render eder.
/// </summary>
public class FirmaBilgisiServisi
{
    private sealed class FirmaTemaDto
    {
        public string? Slug { get; set; }
        public string? Ad { get; set; }
        public string? AdminTema { get; set; }
        public string? SiteTema { get; set; }
        public string? TasarimRengi1 { get; set; }
        public string? TasarimRengi2 { get; set; }
        public string? TasarimRengi3 { get; set; }
    }

    private readonly ApiIstemcisi _api;
    private Firma? _cachedFirma;

    public FirmaBilgisiServisi(ApiIstemcisi api)
    {
        _api = api;
    }

    /// <summary>Geçerli firma bilgisini API'den çeker (cache'li).</summary>
    public async Task<Firma?> GetFirmaAsync()
    {
        if (_cachedFirma != null)
            return _cachedFirma;

        try
        {
            var firma = await _api.GetAsync<Firma>("api/firma/guncel");
            if (firma != null)
            {
                if (!string.IsNullOrWhiteSpace(firma.Logo))
                {
                    firma.Logo = MarkaVarligiNormalizeEt(firma.Logo);
                }

                if (!string.IsNullOrWhiteSpace(firma.Favicon))
                {
                    firma.Favicon = MarkaVarligiNormalizeEt(firma.Favicon);
                }

                await SayfaAyarlarindanMarkaVarliklariniTamamlaAsync(firma);
                _cachedFirma = firma;
                return _cachedFirma;
            }

            // Tam firma kaydi gelmezse tema ucundan minimum tenant kimligini kur.
            var tema = await _api.GetAsync<FirmaTemaDto>("api/firma-tema");
            _cachedFirma = new Firma
            {
                Slug = tema?.Slug ?? "varsayilan",
                Ad = string.IsNullOrWhiteSpace(tema?.Ad) ? "VizitLink3D" : tema.Ad,
                AdminTema = tema?.AdminTema,
                SiteTema = string.IsNullOrWhiteSpace(tema?.SiteTema) ? "varsayilan" : tema.SiteTema,
                TasarimRengi1 = tema?.TasarimRengi1,
                TasarimRengi2 = tema?.TasarimRengi2,
                TasarimRengi3 = tema?.TasarimRengi3
            };
            await SayfaAyarlarindanMarkaVarliklariniTamamlaAsync(_cachedFirma);
            return _cachedFirma;
        }
        catch
        {
            return null;
        }
    }

    private async Task SayfaAyarlarindanMarkaVarliklariniTamamlaAsync(Firma firma)
    {
        var sozluk = await _api.GetAsync<Dictionary<string, string>>("api/sayfa-icerigi/ayarlar");
        if (sozluk == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(firma.Logo)
            && sozluk.TryGetValue("LogoUrl", out var logoUrl)
            && !string.IsNullOrWhiteSpace(logoUrl))
        {
            firma.Logo = MarkaVarligiNormalizeEt(logoUrl);
        }

        if (string.IsNullOrWhiteSpace(firma.Favicon)
            && sozluk.TryGetValue("FaviconUrl", out var faviconUrl)
            && !string.IsNullOrWhiteSpace(faviconUrl))
        {
            firma.Favicon = MarkaVarligiNormalizeEt(faviconUrl);
        }
    }

    private static string MarkaVarligiNormalizeEt(string deger)
    {
        // Eski /img/ yolu → yeni /medya/brand/
        if (deger.StartsWith("/img/", StringComparison.OrdinalIgnoreCase))
            return "/medya/brand/" + deger["/img/".Length..];
        if (deger.StartsWith("img/", StringComparison.OrdinalIgnoreCase))
            return "/medya/brand/" + deger["img/".Length..];

        return deger;
    }

    /// <summary>Firma logosu.</summary>
    public async Task<string?> GetLogoAsync()
    {
        var firma = await GetFirmaAsync();
        return firma?.Logo;
    }

    public async Task<string?> GetFaviconAsync()
    {
        var firma = await GetFirmaAsync();
        return firma?.Favicon;
    }

    public async Task<string> GetSlugAsync()
    {
        var firma = await GetFirmaAsync();
        return string.IsNullOrWhiteSpace(firma?.Slug) ? "varsayilan" : firma!.Slug;
    }

    /// <summary>Firma adı.</summary>
    public async Task<string> GetAdAsync()
    {
        var firma = await GetFirmaAsync();
        return firma?.Ad ?? "VizitLink3D";
    }

    /// <summary>Frontend teması.</summary>
    public async Task<string> GetSiteTemaAsync()
    {
        var firma = await GetFirmaAsync();
        return firma?.SiteTema ?? "varsayilan";
    }

    /// <summary>Admin teması (firma-bazlı değil, sistem-çapı).</summary>
    public async Task<string> GetAdminTemaAsync()
    {
        var firma = await GetFirmaAsync();
        return firma?.AdminTema ?? "default";
    }

    /// <summary>Tasarım renkleri.</summary>
    public async Task<(string? Renk1, string? Renk2, string? Renk3)> GetTasarimRenkleriAsync()
    {
        var firma = await GetFirmaAsync();
        return (firma?.TasarimRengi1, firma?.TasarimRengi2, firma?.TasarimRengi3);
    }

    /// <summary>Cache'i temizle (tema değiş olduğunda).</summary>
    public void ClearCache()
    {
        _cachedFirma = null;
    }
}
