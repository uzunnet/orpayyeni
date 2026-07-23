using System.Text.Json;
using System.Text.RegularExpressions;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Tema;

namespace VizitLink3D.Api.Moduller.Tema.Servisler;

public class StitchTemaServisi(
    IWebHostEnvironment ortam,
    CokluTemaServisi cokluTemaServisi,
    ILogger<StitchTemaServisi> log)
{
    private static readonly JsonSerializerOptions JsonAyar = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public async Task<Cevap<StitchTemaTaslakSonucu>> TaslakOlusturAsync(
        StitchTemaTaslakIstek istek,
        CancellationToken iptal = default)
    {
        var icerikSonucu = await DesignMdIcerikGetirAsync(istek, iptal);
        if (!icerikSonucu.BasariliMi || string.IsNullOrWhiteSpace(icerikSonucu.Veri))
            return Cevap<StitchTemaTaslakSonucu>.Hata(icerikSonucu.Mesaj, icerikSonucu.Hatalar);

        var taslak = NormalizeEt(icerikSonucu.Veri, istek);
        var hatalar = cokluTemaServisi.TemaTaslagiDogrula(taslak);
        var manifestJson = cokluTemaServisi.ManifestJsonUret(taslak);

        var sonuc = new StitchTemaTaslakSonucu
        {
            Taslak = taslak,
            GecerliMi = hatalar.Count == 0,
            Hatalar = hatalar,
            ManifestJson = manifestJson,
            TokensCss = cokluTemaServisi.ManifestToTokensCss(manifestJson, taslak.Slug),
            BilesenlerCss = cokluTemaServisi.BilesenlerCssUret(taslak),
            AnimasyonlarCss = cokluTemaServisi.AnimasyonlarCssUret(taslak)
        };

        return Cevap<StitchTemaTaslakSonucu>.Basarili(sonuc, "Stitch tema taslağı hazırlandı.");
    }

    public async Task<Cevap<StitchTemaOnaySonucu>> OnaylaAsync(
        StitchTemaOnayIstek istek,
        CancellationToken iptal = default)
    {
        if (string.IsNullOrWhiteSpace(istek.HamDesignMd))
        {
            var icerikSonucu = await DesignMdIcerikGetirAsync(new StitchTemaTaslakIstek
            {
                FirmaId = istek.FirmaId
            }, iptal);

            if (icerikSonucu.BasariliMi)
                istek.HamDesignMd = icerikSonucu.Veri;
        }

        return await cokluTemaServisi.StitchTaslagiOnaylaAsync(istek, iptal);
    }

    public async Task TokensCssUretAsync(string firmaId = "varsayilan", string prompt = "")
    {
        var taslak = await TaslakOlusturAsync(new StitchTemaTaslakIstek
        {
            FirmaId = firmaId,
            Notlar = string.IsNullOrWhiteSpace(prompt) ? null : "Eski Stitch tetikleyici üzerinden oluşturuldu.",
            AktifEt = true
        });

        if (!taslak.BasariliMi || taslak.Veri is null || !taslak.Veri.GecerliMi)
        {
            log.LogWarning("Stitch tema taslağı üretilemedi. {Hata}", taslak.Mesaj);
            return;
        }

        var onay = await OnaylaAsync(new StitchTemaOnayIstek
        {
            Taslak = taslak.Veri.Taslak,
            FirmaId = firmaId,
            AktifEt = true,
            Notlar = "Eski Stitch tetikleyici CokluTemaServisi hattına yönlendirildi."
        });

        if (!onay.BasariliMi)
            log.LogWarning("Stitch tema onayı başarısız. {Hata}", onay.Mesaj);
    }

    /// <summary>
    /// Stitch proje ID'sinden tema import eder.
    /// TemaSablonuPaketi → CokluTemaServisi.KaydetVeyaGuncelle → FirmaTemaAtama akışını başlatır.
    /// </summary>
    public async Task<Cevap<StitchTemaOnaySonucu>> ImportEtAsync(
        string projectId,
        string firmaId = "varsayilan",
        bool aktifEt = true,
        CancellationToken iptal = default)
    {
        var sonuc = await TaslakOlusturAsync(new StitchTemaTaslakIstek
        {
            FirmaId = firmaId,
            Notlar = $"Stitch import — projeId: {projectId}"
        }, iptal);

        if (!sonuc.BasariliMi || sonuc.Veri is null)
            return Cevap<StitchTemaOnaySonucu>.Hata(sonuc.Mesaj, sonuc.Hatalar);

        if (!sonuc.Veri.GecerliMi)
            return Cevap<StitchTemaOnaySonucu>.Hata("Tema taslağı geçersiz.", sonuc.Veri.Hatalar);

        return await OnaylaAsync(new StitchTemaOnayIstek
        {
            FirmaId = firmaId,
            Taslak = sonuc.Veri.Taslak,
            AktifEt = aktifEt,
            HamDesignMd = null,
            Notlar = $"Stitch import — projeId: {projectId}"
        }, iptal);
    }

    public string DesignMdToCss(string icerik)
    {
        var taslak = NormalizeEt(icerik, new StitchTemaTaslakIstek());
        var manifestJson = cokluTemaServisi.ManifestJsonUret(taslak);
        return cokluTemaServisi.ManifestToTokensCss(manifestJson, taslak.Slug);
    }

    private async Task<Cevap<string>> DesignMdIcerikGetirAsync(StitchTemaTaslakIstek istek, CancellationToken iptal)
    {
        if (!string.IsNullOrWhiteSpace(istek.DesignMdIcerik))
            return Cevap<string>.Basarili(istek.DesignMdIcerik);

        var yol = string.IsNullOrWhiteSpace(istek.DesignMdYolu)
            ? VarsayilanTasarimYoluCoz()
            : GuvenliTasarimYoluCoz(istek.DesignMdYolu);

        if (yol is null || !File.Exists(yol))
            return Cevap<string>.Hata("DESIGN.md bulunamadı.");

        var icerik = await File.ReadAllTextAsync(yol, iptal);
        return Cevap<string>.Basarili(icerik);
    }

    private string? VarsayilanTasarimYoluCoz()
    {
        var tasarimKlasoru = Path.GetFullPath(Path.Combine(ortam.ContentRootPath, "..", "tasarim"));
        var adaylar = new[]
        {
            "DESIGN_orpay_gold.md",
            "DESIGN_orpay.md",
            "DESIGN.md"
        };

        foreach (var aday in adaylar)
        {
            var tamYol = Path.Combine(tasarimKlasoru, aday);
            if (File.Exists(tamYol))
                return tamYol;
        }

        return Path.Combine(tasarimKlasoru, "DESIGN.md");
    }

    private string? GuvenliTasarimYoluCoz(string gelenYol)
    {
        var projeKoku = Path.GetFullPath(Path.Combine(ortam.ContentRootPath, ".."));
        var tamYol = Path.GetFullPath(Path.IsPathRooted(gelenYol)
            ? gelenYol
            : Path.Combine(projeKoku, gelenYol));

        return tamYol.StartsWith(projeKoku, StringComparison.OrdinalIgnoreCase)
            ? tamYol
            : null;
    }

    private static TemaManifestTaslagi NormalizeEt(string icerik, StitchTemaTaslakIstek istek)
    {
        var trimmed = icerik.TrimStart();
        if (trimmed.StartsWith("{"))
            return JsonIceriktenTaslak(trimmed, istek);

        var frontMatter = FrontMatterCikar(icerik);
        return YamlIceriktenTaslak(frontMatter ?? string.Empty, istek);
    }

    private static TemaManifestTaslagi JsonIceriktenTaslak(string json, StitchTemaTaslakIstek istek)
    {
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("renkler", out _))
        {
            var taslak = JsonSerializer.Deserialize<TemaManifestTaslagi>(json, JsonAyar) ?? new TemaManifestTaslagi();
            IstekOverrideUygula(taslak, istek);
            KimlikTamamla(taslak);
            return taslak;
        }

        var tokenlar = JsonTokenDegerleriOku(doc.RootElement);
        var ustDegerler = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (doc.RootElement.TryGetProperty("name", out var ad))
            ustDegerler["name"] = ad.GetString() ?? string.Empty;

        return TokenlardanTaslak(tokenlar, ustDegerler, istek);
    }

    private static TemaManifestTaslagi YamlIceriktenTaslak(string yaml, StitchTemaTaslakIstek istek)
    {
        var (ustDegerler, tokenlar) = YamlTokenDegerleriOku(yaml);
        return TokenlardanTaslak(tokenlar, ustDegerler, istek);
    }

    private static TemaManifestTaslagi TokenlardanTaslak(
        IReadOnlyDictionary<string, string> tokenlar,
        IReadOnlyDictionary<string, string> ustDegerler,
        StitchTemaTaslakIstek istek)
    {
        var ad = Sec(istek.Ad, ustDegerler.GetValueOrDefault("name"), "Stitch Gold Tema");
        var slug = Sec(istek.Slug, SlugOlustur(ad), "stitch-gold-tema");

        var taslak = new TemaManifestTaslagi
        {
            Id = slug,
            Kod = KodOlustur(slug),
            Ad = ad,
            Slug = slug,
            Aciklama = Sec(istek.Aciklama, "Stitch DESIGN.md içeriğinden üretilen tema taslağı.", "Stitch tema taslağı."),
            Kaynak = "stitch",
            Premium = istek.Premium,
            GlassmorphismAktif = true,
            Etiketler = ["stitch", "gold", "lux"],
            Glassmorphism = { Aktif = true, YariSaydam = true },
            Layout = { Header = "glassmorphism", KartStili = "glass", HeroTipi = "fullscreen-slider-overlay" },
            Animasyon = { Hizi = "yavas", HoverYukseklik = 6, MagneticCursor = true, Tip = "stitch-lux" }
        };

        RenkleriUygula(taslak, tokenlar);
        TipografiyiUygula(taslak, tokenlar);
        BosluklariUygula(taslak, tokenlar);
        GolgeleriUygula(taslak, tokenlar);
        AnimasyonuUygula(taslak, tokenlar);
        KimlikTamamla(taslak);

        return taslak;
    }

    private static void RenkleriUygula(TemaManifestTaslagi taslak, IReadOnlyDictionary<string, string> tokenlar)
    {
        taslak.Renkler.Birincil = Token(tokenlar, "color.primary", taslak.Renkler.Birincil);
        taslak.Renkler.Ikincil = Token(tokenlar, "color.secondary", taslak.Renkler.Ikincil);
        taslak.Renkler.Vurgu = Token(tokenlar, "color.accent", Token(tokenlar, "color.gold", taslak.Renkler.Vurgu));
        taslak.Renkler.VurguAcik = Token(tokenlar, "color.accent-light", taslak.Renkler.VurguAcik);
        taslak.Renkler.VurguKoyu = Token(tokenlar, "color.accent-dark", taslak.Renkler.VurguKoyu);
        taslak.Renkler.ArkaPlan = Token(tokenlar, "color.bg-base", taslak.Renkler.ArkaPlan);
        taslak.Renkler.ArkaPlan2 = Token(tokenlar, "color.bg-alt", taslak.Renkler.ArkaPlan2);
        taslak.Renkler.Yuzey = Token(tokenlar, "color.surface", taslak.Renkler.Yuzey);
        taslak.Renkler.YuzeyHover = Token(tokenlar, "color.surface-hover", taslak.Renkler.YuzeyHover);
        taslak.Renkler.Cizgi = Token(tokenlar, "color.border", taslak.Renkler.Cizgi);
        taslak.Renkler.Metin = Token(tokenlar, "color.text", taslak.Renkler.Metin);
        taslak.Renkler.MetinIkincil = Token(tokenlar, "color.text-secondary", taslak.Renkler.MetinIkincil);
        taslak.Renkler.MetinSoluk = Token(tokenlar, "color.text-muted", taslak.Renkler.MetinSoluk);
        taslak.Renkler.MetinTers = Token(tokenlar, "color.text-inverse", taslak.Renkler.MetinTers);
        taslak.Renkler.Basari = Token(tokenlar, "color.success", taslak.Renkler.Basari);
        taslak.Renkler.Uyari = Token(tokenlar, "color.warning", taslak.Renkler.Uyari);
        taslak.Renkler.Hata = Token(tokenlar, "color.error", taslak.Renkler.Hata);
        taslak.Renkler.Bilgi = Token(tokenlar, "color.info", taslak.Renkler.Bilgi);
    }

    private static void TipografiyiUygula(TemaManifestTaslagi taslak, IReadOnlyDictionary<string, string> tokenlar)
    {
        taslak.Tipografi.BaslikAilesi = Token(tokenlar, "typography.heading", taslak.Tipografi.BaslikAilesi);
        taslak.Tipografi.GovdeAilesi = Token(tokenlar, "typography.body", taslak.Tipografi.GovdeAilesi);
        taslak.Tipografi.VurguAilesi = Token(tokenlar, "typography.accent", taslak.Tipografi.VurguAilesi);
        taslak.Tipografi.MonoAilesi = Token(tokenlar, "typography.mono", taslak.Tipografi.MonoAilesi);
    }

    private static void BosluklariUygula(TemaManifestTaslagi taslak, IReadOnlyDictionary<string, string> tokenlar)
    {
        taslak.Bosluklar.Xs = Token(tokenlar, "spacing.xs", taslak.Bosluklar.Xs);
        taslak.Bosluklar.Sm = Token(tokenlar, "spacing.sm", taslak.Bosluklar.Sm);
        taslak.Bosluklar.Md = Token(tokenlar, "spacing.md", taslak.Bosluklar.Md);
        taslak.Bosluklar.Lg = Token(tokenlar, "spacing.lg", taslak.Bosluklar.Lg);
        taslak.Bosluklar.Xl = Token(tokenlar, "spacing.xl", taslak.Bosluklar.Xl);
        taslak.Bosluklar.IkiXl = Token(tokenlar, "spacing.2xl", taslak.Bosluklar.IkiXl);
        taslak.Bosluklar.UcXl = Token(tokenlar, "spacing.3xl", taslak.Bosluklar.UcXl);
    }

    private static void GolgeleriUygula(TemaManifestTaslagi taslak, IReadOnlyDictionary<string, string> tokenlar)
    {
        taslak.Golgeler.Sm = Token(tokenlar, "shadow.sm", taslak.Golgeler.Sm);
        taslak.Golgeler.Md = Token(tokenlar, "shadow.md", taslak.Golgeler.Md);
        taslak.Golgeler.Lg = Token(tokenlar, "shadow.lg", taslak.Golgeler.Lg);
        taslak.Golgeler.Xl = Token(tokenlar, "shadow.xl", taslak.Golgeler.Xl);
        taslak.Golgeler.Vurgu = Token(tokenlar, "shadow.lux", taslak.Golgeler.Vurgu);
    }

    private static void AnimasyonuUygula(TemaManifestTaslagi taslak, IReadOnlyDictionary<string, string> tokenlar)
    {
        taslak.Animasyon.GecisHizli = Token(tokenlar, "animation.fast", taslak.Animasyon.GecisHizli);
        taslak.Animasyon.GecisNormal = Token(tokenlar, "animation.normal", taslak.Animasyon.GecisNormal);
        taslak.Animasyon.GecisYavas = Token(tokenlar, "animation.slow", taslak.Animasyon.GecisYavas);
    }

    private static IReadOnlyDictionary<string, string> JsonTokenDegerleriOku(JsonElement root)
    {
        var sonuc = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!root.TryGetProperty("tokens", out var tokens))
            return sonuc;

        foreach (var grup in tokens.EnumerateObject())
        {
            foreach (var token in grup.Value.EnumerateObject())
            {
                if (token.Value.TryGetProperty("value", out var value))
                    sonuc[$"{grup.Name}.{token.Name}"] = value.ValueKind == JsonValueKind.String ? value.GetString() ?? "" : value.GetRawText();
            }
        }

        return sonuc;
    }

    private static (Dictionary<string, string> UstDegerler, Dictionary<string, string> Tokenlar) YamlTokenDegerleriOku(string yaml)
    {
        var ustDegerler = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var tokenlar = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var yol = new Dictionary<int, string>();

        foreach (var hamSatir in yaml.Replace("\r\n", "\n").Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(hamSatir) || hamSatir.TrimStart().StartsWith("#"))
                continue;

            var girinti = (hamSatir.Length - hamSatir.TrimStart().Length) / 2;
            var satir = hamSatir.Trim();
            var ikiNokta = satir.IndexOf(':');
            if (ikiNokta <= 0)
                continue;

            var anahtar = satir[..ikiNokta].Trim();
            var deger = satir[(ikiNokta + 1)..].Trim();

            foreach (var seviye in yol.Keys.Where(k => k >= girinti).ToArray())
                yol.Remove(seviye);

            if (string.IsNullOrWhiteSpace(deger))
            {
                yol[girinti] = anahtar;
                continue;
            }

            var temizDeger = DegerTemizle(deger);
            if (girinti == 0)
                ustDegerler[anahtar] = temizDeger;

            if (anahtar.Equals("value", StringComparison.OrdinalIgnoreCase)
                && yol.TryGetValue(0, out var kok)
                && kok.Equals("tokens", StringComparison.OrdinalIgnoreCase)
                && yol.TryGetValue(1, out var grup)
                && yol.TryGetValue(2, out var token))
            {
                tokenlar[$"{grup}.{token}"] = temizDeger;
            }
        }

        return (ustDegerler, tokenlar);
    }

    private static void IstekOverrideUygula(TemaManifestTaslagi taslak, StitchTemaTaslakIstek istek)
    {
        if (!string.IsNullOrWhiteSpace(istek.Ad))
            taslak.Ad = istek.Ad;

        if (!string.IsNullOrWhiteSpace(istek.Slug))
            taslak.Slug = SlugOlustur(istek.Slug);

        if (!string.IsNullOrWhiteSpace(istek.Aciklama))
            taslak.Aciklama = istek.Aciklama;

        taslak.Premium = istek.Premium || taslak.Premium;
    }

    private static void KimlikTamamla(TemaManifestTaslagi taslak)
    {
        taslak.Slug = SlugOlustur(Sec(taslak.Slug, taslak.Ad, "stitch-gold-tema"));
        taslak.Id = string.IsNullOrWhiteSpace(taslak.Id) ? taslak.Slug : taslak.Id;
        taslak.Kod = string.IsNullOrWhiteSpace(taslak.Kod) ? KodOlustur(taslak.Slug) : taslak.Kod;
        taslak.Kaynak = string.IsNullOrWhiteSpace(taslak.Kaynak) ? "stitch" : taslak.Kaynak;
        taslak.GlassmorphismAktif = taslak.Glassmorphism.Aktif || taslak.GlassmorphismAktif;
    }

    private static string? FrontMatterCikar(string icerik)
    {
        var match = Regex.Match(icerik, @"^---\s*\n(.*?)\n---", RegexOptions.Singleline);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static string Token(IReadOnlyDictionary<string, string> tokenlar, string anahtar, string varsayilan)
    {
        return tokenlar.TryGetValue(anahtar, out var deger) && !string.IsNullOrWhiteSpace(deger)
            ? deger
            : varsayilan;
    }

    private static string Sec(params string?[] degerler)
    {
        return degerler.FirstOrDefault(d => !string.IsNullOrWhiteSpace(d)) ?? string.Empty;
    }

    private static string DegerTemizle(string deger)
    {
        var temiz = deger.Trim().Trim(',');
        if ((temiz.StartsWith("\"") && temiz.EndsWith("\"")) || (temiz.StartsWith("'") && temiz.EndsWith("'")))
            temiz = temiz[1..^1];

        return temiz.Trim();
    }

    private static string SlugOlustur(string deger)
    {
        var ascii = deger.ToLowerInvariant()
            .Replace("ı", "i")
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ö", "o")
            .Replace("ç", "c");

        ascii = Regex.Replace(ascii, @"[^a-z0-9]+", "-").Trim('-');
        return string.IsNullOrWhiteSpace(ascii) ? "stitch-gold-tema" : ascii;
    }

    private static string KodOlustur(string slug)
    {
        return Regex.Replace(slug.ToUpperInvariant(), @"[^A-Z0-9]+", "_").Trim('_');
    }
}
