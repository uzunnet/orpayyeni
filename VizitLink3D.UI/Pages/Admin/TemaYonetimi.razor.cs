using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages.Admin;

public partial class TemaYonetimi : ComponentBase
{
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private ApiIstemcisi Api { get; set; } = default!;

    private readonly List<TemaKatalogDto> _temalar = [];
    private readonly List<FirmaSecimDto> _firmalar = [];

    private FirmaTemaDto? _firmaTema;
    private TemaKatalogDto? _taslakTema;
    private TemaRenkleri _onizlemeRenkleri = TemaRenkleri.Varsayilan;
    private string _aktifTemaSlug = string.Empty;
    private string _aktifSiteTemaSlug = "gold";
    private string _taslakTemaSlug = string.Empty;
    private string _taslakTemaBaslik = string.Empty;
    private string _seciliFirmaAdi = string.Empty;
    private int? _seciliFirmaId;
    private bool _yukleniyor = true;
    private bool _uygulaniyor;
    private bool _firmalarYukleniyor = true;

    private static readonly AdminTemaSablonu[] AdminTemaSablonlari =
    [
        new("endustri-karanlik", "Endüstri Karanlık", "Siyah zemin, altın vurgu — fabrika hissi",
            "#0a0a0a", "#C5A059", "#0a0a0a", "#111111", true),
        new("klasik-aydinlik", "Klasik Aydınlık", "Beyaz ağırlıklı, siyah metin — temiz ofis",
            "#1A1A27", "#C8952A", "#F8F6F2", "#FFFFFF", false),
        new("altin-siyah", "Altın Siyah", "Derin siyah, yoğun altın — premium lüks",
            "#000000", "#D4A843", "#080808", "#151515", true),
        new("modern-gri", "Modern Gri", "Soğuk gri tonlar, çelik mavi — minimal",
            "#1E1E24", "#8BA4BC", "#121218", "#1C1C22", true),
        new("komuta-mavi", "Komuta Mavi", "Koyu lacivert, canlı mavi veri panelleri — operasyon merkezi",
            "#061222", "#2D8CFF", "#050F1D", "#081F37", true),
        new("windows-11", "Windows 11", "Açık akrilik yüzey, mavi vurgu — sade ve tanıdık",
            "#F3F6FB", "#2563EB", "#F3F6FB", "rgba(255,255,255,0.78)", false)
    ];

    protected override async Task OnInitializedAsync()
    {
        _yukleniyor = true;

        await TemaKatalogunuYukleAsync();
        await FirmalariYukleAsync();
        await FirmaTemasiniYukleAsync(_seciliFirmaId);

        var baslangicTema = _firmaTema?.AdminTema;
        if (string.IsNullOrWhiteSpace(baslangicTema))
        {
            baslangicTema = await KayitliAdminTemaGetirAsync();
        }

        if (!string.IsNullOrWhiteSpace(baslangicTema))
        {
            var tema = TemaBul(baslangicTema);
            if (tema is not null)
            {
                await TemaTaslakSecAsync(tema);
                await TemaUygulaAsync(tema);
            }
            else
            {
                var varsayilan = TemaBul("endustri-karanlik");
                if (varsayilan is not null)
                {
                    await TemaTaslakSecAsync(varsayilan);
                    await TemaUygulaAsync(varsayilan);
                }
            }
        }
        else if (_temalar.Count > 0)
        {
            await TemaTaslakSecAsync(_temalar[0]);
        }

        _yukleniyor = false;
    }

    private async Task TemaKatalogunuYukleAsync()
    {
        _temalar.Clear();
        _temalar.AddRange(AdminTemaSablonlari.Select(t => new TemaKatalogDto
        {
            Slug = t.Slug,
            Ad = t.Baslik,
            Aciklama = t.Aciklama,
            GlassmorphismAktif = false,
            Premium = false
        }));

        await Task.CompletedTask;
    }

    private async Task FirmalariYukleAsync()
    {
        _firmalarYukleniyor = true;
        var liste = await Api.GetAsync<List<FirmaSecimDto>>("api/firma-tema/firmalar") ?? [];

        _firmalar.Clear();
        _firmalar.AddRange(liste);

        if (_firmalar.Count > 0)
        {
            _seciliFirmaId = _firmalar[0].FirmaId;
            _seciliFirmaAdi = _firmalar[0].Ad;
        }

        _firmalarYukleniyor = false;
    }

    private async Task FirmaSecildiAsync(int? firmaId)
    {
        _seciliFirmaId = firmaId;
        _seciliFirmaAdi = _firmalar.FirstOrDefault(f => f.FirmaId == firmaId)?.Ad ?? string.Empty;
        await FirmaTemasiniYukleAsync(firmaId);

        var tema = TemaBul(_firmaTema?.AdminTema ?? _aktifTemaSlug);
        if (tema is not null)
        {
            await TemaTaslakSecAsync(tema);
        }
    }

    private async Task FirmaTemasiniYukleAsync(int? firmaId)
    {
        var url = firmaId is int id ? $"api/firma-tema?firmaId={id}" : "api/firma-tema";
        _firmaTema = await Api.GetAsync<FirmaTemaDto>(url);

        if (_firmaTema is not null)
        {
            _seciliFirmaId = _firmaTema.FirmaId;
            _seciliFirmaAdi = _firmaTema.Ad;
            _aktifSiteTemaSlug = string.IsNullOrWhiteSpace(_firmaTema.SiteTema)
                ? "gold"
                : _firmaTema.SiteTema;
            _aktifTemaSlug = string.IsNullOrWhiteSpace(_firmaTema.AdminTema)
                ? "endustri-karanlik"
                : _firmaTema.AdminTema;
        }
    }

    private async Task TemaTaslakSecAsync(TemaKatalogDto tema)
    {
        _taslakTema = tema;
        _taslakTemaSlug = tema.Slug;
        _taslakTemaBaslik = TemaAdi(tema);
        _onizlemeRenkleri = await TemaRenkleriGetirAsync(tema.Slug);
    }

    private async Task OnaylaUygulaAsync()
    {
        if (_taslakTema is null)
        {
            Snackbar.Add(dil.T("admin.tema.onceTemaSec", "Önce bir tema seçin."), Severity.Info);
            return;
        }

        _uygulaniyor = true;

        var cevap = await Api.PutAsync<FirmaTemaDto>("api/firma-tema", new FirmaTemaGuncelleDto
        {
            FirmaId = _seciliFirmaId,
            AdminTema = _taslakTema.Slug,
            SiteTema = _aktifSiteTemaSlug
        });

        if (cevap?.BasariliMi == true && cevap.Veri is not null)
        {
            _firmaTema = cevap.Veri;
            _aktifTemaSlug = _taslakTema.Slug;
            await TemaUygulaAsync(_taslakTema);
            await JS.InvokeVoidAsync("localStorage.setItem", "vizitlink3d_admin_tema", _taslakTema.Slug);

            Snackbar.Add(
                string.Format(dil.T("admin.tema.firmayaAtandi", "{0} teması seçili firmaya atandı."), TemaAdi(_taslakTema)),
                Severity.Success);
        }
        else
        {
            Snackbar.Add(dil.T("admin.tema.kayitBasarisiz", "Tema firma ayarına kaydedilemedi. Oturum yetkisini kontrol edin."), Severity.Warning);
        }

        _uygulaniyor = false;
    }

    private async Task AktifTemayaDonAsync()
    {
        var tema = TemaBul(_aktifTemaSlug);
        if (tema is null)
        {
            return;
        }

        await TemaTaslakSecAsync(tema);
    }

    private async Task TemaUygulaAsync(TemaKatalogDto tema)
    {
        var renkler = await TemaRenkleriGetirAsync(tema.Slug);
        await JS.InvokeVoidAsync(
            "vizitlink3dTema.uygula",
            renkler.Birincil,
            renkler.Vurgu,
            renkler.ArkaPlan,
            renkler.Yuzey,
            renkler.KoyuTemaMi,
            tema.Slug);
    }

    private async Task<TemaRenkleri> TemaRenkleriGetirAsync(string slug)
    {
        var adminTema = AdminTemaSablonlari.FirstOrDefault(t => string.Equals(t.Slug, slug, StringComparison.OrdinalIgnoreCase));
        if (adminTema is not null)
        {
            return new TemaRenkleri(
                adminTema.Birincil,
                adminTema.Vurgu,
                adminTema.ArkaPlan,
                adminTema.Yuzey,
                adminTema.KoyuTemaMi);
        }

        var detay = await Api.GetAsync<TemaDetayDto>($"api/tema/{slug}");
        return TemaRenkleriJsondanCoz(detay?.RenklerJson) ?? TemaRenkleri.Varsayilan;
    }

    private async Task<string?> KayitliAdminTemaGetirAsync()
    {
        try
        {
            return await JS.InvokeAsync<string?>("localStorage.getItem", "vizitlink3d_admin_tema");
        }
        catch
        {
            return null;
        }
    }

    private TemaKatalogDto? TemaBul(string? slug)
        => string.IsNullOrWhiteSpace(slug)
            ? null
            : _temalar.FirstOrDefault(t => string.Equals(t.Slug, slug, StringComparison.OrdinalIgnoreCase));

    private string TemaAdi(TemaKatalogDto tema)
    {
        var varsayilan = string.IsNullOrWhiteSpace(tema.Ad) ? tema.Slug : tema.Ad;
        return dil.T($"tema.{tema.Slug}.ad", varsayilan);
    }

    private string TemaAciklama(TemaKatalogDto tema)
    {
        var varsayilan = string.IsNullOrWhiteSpace(tema.Aciklama)
            ? dil.T("admin.tema.aciklamaYok", "Açıklama girilmemiş.")
            : tema.Aciklama;

        return dil.T($"tema.{tema.Slug}.aciklama", varsayilan);
    }

    private static string RenkStili(string renk)
        => RenkGecerliMi(renk) ? $"background:{renk};" : "background:var(--admin-accent);";

    private static TemaRenkleri? TemaRenkleriJsondanCoz(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var belge = JsonDocument.Parse(json);
            var kok = belge.RootElement;
            var birincil = JsonMetinOku(kok, "birincil", "primary", "anaRenk");
            var vurgu = JsonMetinOku(kok, "vurgu", "accent", "vurguRenk", "ikincil");
            var arkaPlan = JsonMetinOku(kok, "arkaPlan", "background", "arkaplan");
            var yuzey = JsonMetinOku(kok, "yuzey", "surface", "arkaPlan2", "arkaplan2");

            if (!RenkGecerliMi(birincil) || !RenkGecerliMi(vurgu) || !RenkGecerliMi(arkaPlan) || !RenkGecerliMi(yuzey))
            {
                return null;
            }

            return new TemaRenkleri(birincil!, vurgu!, arkaPlan!, yuzey!, RenkKoyuMu(arkaPlan!));
        }
        catch
        {
            return null;
        }
    }

    private static string? JsonMetinOku(JsonElement kok, params string[] adlar)
    {
        foreach (var ad in adlar)
        {
            if (kok.TryGetProperty(ad, out var deger) && deger.ValueKind == JsonValueKind.String)
            {
                return deger.GetString();
            }
        }

        return null;
    }

    private static bool RenkGecerliMi(string? renk)
    {
        if (string.IsNullOrWhiteSpace(renk) || renk[0] != '#')
        {
            return false;
        }

        return renk.Length is 4 or 7 && renk.Skip(1).All(Uri.IsHexDigit);
    }

    private static bool RenkKoyuMu(string renk)
    {
        if (!RenkGecerliMi(renk))
        {
            return true;
        }

        var hex = renk.Length == 4
            ? string.Concat(renk[1], renk[1], renk[2], renk[2], renk[3], renk[3])
            : renk[1..];
        var r = Convert.ToInt32(hex[..2], 16);
        var g = Convert.ToInt32(hex[2..4], 16);
        var b = Convert.ToInt32(hex[4..6], 16);
        return (r * 0.299 + g * 0.587 + b * 0.114) < 140;
    }

    private sealed class TemaKatalogDto
    {
        public string Slug { get; set; } = string.Empty;
        public string Ad { get; set; } = string.Empty;
        public string Aciklama { get; set; } = string.Empty;
        public bool GlassmorphismAktif { get; set; }
        public bool Premium { get; set; }
        public string Etiketler { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
    }

    private sealed record AdminTemaSablonu(
        string Slug,
        string Baslik,
        string Aciklama,
        string Birincil,
        string Vurgu,
        string ArkaPlan,
        string Yuzey,
        bool KoyuTemaMi);

    private sealed class TemaDetayDto
    {
        public string RenklerJson { get; set; } = string.Empty;
    }

    private sealed class FirmaTemaDto
    {
        public int FirmaId { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string Ad { get; set; } = string.Empty;
        public string AdminTema { get; set; } = string.Empty;
        public string SiteTema { get; set; } = string.Empty;
        public string? TasarimRengi1 { get; set; }
        public string? TasarimRengi2 { get; set; }
        public string? TasarimRengi3 { get; set; }
    }

    private sealed class FirmaTemaGuncelleDto
    {
        public int? FirmaId { get; set; }
        public string AdminTema { get; set; } = string.Empty;
        public string SiteTema { get; set; } = string.Empty;
    }

    private sealed class FirmaSecimDto
    {
        public int FirmaId { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string Ad { get; set; } = string.Empty;
        public string AdminTema { get; set; } = string.Empty;
        public string SiteTema { get; set; } = string.Empty;
    }

    private sealed record TemaRenkleri(
        string Birincil,
        string Vurgu,
        string ArkaPlan,
        string Yuzey,
        bool KoyuTemaMi)
    {
        public static TemaRenkleri Varsayilan { get; } = new("#1A1A27", "#C8952A", "#0a0a0a", "#111111", true);
    }
}
