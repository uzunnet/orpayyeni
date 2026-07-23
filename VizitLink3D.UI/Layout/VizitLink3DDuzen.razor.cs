using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using VizitLink3D.Ortak.Yardimcilar;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Layout;

public partial class VizitLink3DDuzen : IDisposable
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private FirmaBilgisiServisi FirmaBilgisi { get; set; } = default!;
    [Inject] private DilServisi DilServisi { get; set; } = default!;
    [Inject] private NavigationManager Navigasyon { get; set; } = default!;

    private string _aktifTema = "gold";
    private string _aktifTemaModu = "koyu";
    private string _varsayilanDil = "tr";
    private string _aktifDil = "tr";
    private string _firmaAdi = "ORPAY";
    private string _firmaSlug = "orpay";
    private string? _logoUrl = "/medya/brand/vizitlink3d-logo.svg";
    private List<DilServisi.DilBilgisi> _diller = [];
    private bool _ilkRenderTamamlandi;
    private bool _mobilMenuAcik;

    private List<MenuBaglantisi> _menu = VarsayilanMenuOlustur();
    private bool KoyuTemaMi => _aktifTemaModu != "acik";
    private string MobilMenuClass => MobilMenuGorunumYardimcisi.MenuSinifi(_mobilMenuAcik);
    private string MobilMenuDugmeClass => MobilMenuGorunumYardimcisi.DugmeSinifi(_mobilMenuAcik);

    private readonly MudTheme _tema = new()
    {
        PaletteDark = new PaletteDark
        {
            Primary = "#d4af37",
            Secondary = "#c4c7c7",
            Background = "#121414",
            Surface = "#1e2020",
            TextPrimary = "#e2e2e2",
            TextSecondary = "#c4c7c7",
            TextDisabled = "#8e9192",
            AppbarBackground = "#121414",
            DrawerBackground = "#1e2020",
            DrawerText = "#e2e2e2",
            Success = "#4a7c59",
            Warning = "#c9a449",
            Error = "#ffb4ab",
            Info = "#4a6c8c"
        }
    };

    protected override async Task OnInitializedAsync()
    {
        // Ayarlar/firma/menu birbirine bagimli degil - paralel cekilir,
        // boylece ilk render'a kadar gecen sure (ve "ORPAY" varsayilaninin
        // gorunme suresi) art arda 3 istek yerine tek round-trip'e iner.
        var ayarlarTask = AyarlariYukleAsync();
        var firmaTask = FirmaBilgisi.GetFirmaAsync();
        var menuTask = MenuleriYukleAsync();
        await Task.WhenAll(ayarlarTask, firmaTask, menuTask);

        // DilServisi, ayarlardan gelen _varsayilanDil'e ihtiyac duydugu icin
        // paralel grubun tamamlanmasini bekler.
        await DilServisi.BaslatAsync(_varsayilanDil);
        DilServisi.DilDegisti += DilDegisti;
        _aktifDil = DilServisi.AktifDil;
        _diller = DilServisi.DesteklenenDiller.ToList();

        var firma = await firmaTask;
        if (firma == null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(firma.Ad))
        {
            _firmaAdi = firma.Ad;
        }

        if (!string.IsNullOrWhiteSpace(firma.Slug))
        {
            _firmaSlug = firma.Slug;
        }

        if (!string.IsNullOrWhiteSpace(firma.SiteTema))
        {
            _aktifTema = firma.SiteTema;
        }

        if (!string.IsNullOrWhiteSpace(firma.Logo))
        {
            _logoUrl = UrlNormalizeEt(firma.Logo);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        await JS.InvokeVoidAsync("localStorage.setItem", "aktif_firma", _firmaSlug);
        await JS.InvokeVoidAsync("localStorage.setItem", "vizitlink3d_site_tema", _aktifTema);
        var kayitliDil = await JS.InvokeAsync<string?>("localStorage.getItem", "vizitlink3dil");
        if (string.IsNullOrWhiteSpace(kayitliDil) && !string.Equals(_varsayilanDil, _aktifDil, StringComparison.OrdinalIgnoreCase))
        {
            await DilServisi.DilDegistirAsync(_varsayilanDil);
            _aktifDil = DilServisi.AktifDil;
        }

        var kayitliTemaModu = await JS.InvokeAsync<string?>("localStorage.getItem", "temaMod");
        if (string.IsNullOrWhiteSpace(kayitliTemaModu))
        {
            await JS.InvokeVoidAsync("vizitlink3dTema.modUygula", _aktifTemaModu);
        }
        else
        {
            _aktifTemaModu = kayitliTemaModu;
            await JS.InvokeVoidAsync("vizitlink3dTema.modUygula", kayitliTemaModu);
        }

        await JS.InvokeVoidAsync("vizitlink3dTema.siteUygula", _aktifTema);
        await JS.InvokeVoidAsync("vizitlink3dDil.htmlDiliniAyarla", _aktifDil);
        _ilkRenderTamamlandi = true;
        StateHasChanged();
    }

    private sealed record MenuBaglantisi(string Baslik, string Url, List<MenuBaglantisi> AltMenuler);

    private static List<MenuBaglantisi> VarsayilanMenuOlustur() =>
    [
        new("Ana Sayfa", "/", []),
        new("Ürünler", "/banyo-dolaplari", []),
        new("Katalog", "/katalog", []),
        new("Projeler", "/projeler", []),
        new("Kurumsal", "/hakkimizda", []),
        new("Referanslar", "/referanslar", []),
        new("Haber", "/haber", []),
        new("SSS", "/sss", []),
        new("İletişim", "/iletisim", [])
    ];

    private string MenuBasligi(MenuBaglantisi oge) => oge.Baslik switch
    {
        "Ana Sayfa" => DilServisi.T("menu.anasayfa", oge.Baslik),
        "Ürünler" => DilServisi.T("menu.urunler", oge.Baslik),
        "Gold Exclusive" => DilServisi.T("menu.goldExclusive", oge.Baslik),
        "Gold Premium" => DilServisi.T("menu.goldPremium", oge.Baslik),
        "Gold Trend" => DilServisi.T("menu.goldTrend", oge.Baslik),
        "Gold Standart" => DilServisi.T("menu.goldStandart", oge.Baslik),
        "Tüm Ürünler" => DilServisi.T("menu.tumUrunler", oge.Baslik),
        "Katalog" => DilServisi.T("menu.katalog", oge.Baslik),
        "Projeler" => DilServisi.T("menu.projeler", oge.Baslik),
        "Kurumsal" => DilServisi.T("menu.kurumsal", oge.Baslik),
        "Hakkımızda" => DilServisi.T("menu.hakkimizda", oge.Baslik),
        "Vizyon & Misyon" => DilServisi.T("menu.vizyonMisyon", oge.Baslik),
        "Ekibimiz" => DilServisi.T("menu.ekibimiz", oge.Baslik),
        "Sertifikalarımız" => DilServisi.T("menu.sertifikalarimiz", oge.Baslik),
        "Bayiler" => DilServisi.T("menu.bayiler", oge.Baslik),
        "Fabrikamız" => DilServisi.T("menu.fabrikamiz", oge.Baslik),
        "Referanslar" => DilServisi.T("menu.referanslar", oge.Baslik),
        "Haber" => DilServisi.T("menu.haber", oge.Baslik),
        "SSS" => DilServisi.T("menu.sss", oge.Baslik),
        "İletişim" => DilServisi.T("menu.iletisim", oge.Baslik),
        _ => oge.Baslik
    };

    private async Task AyarlariYukleAsync()
    {
        var ayarlar = await Api.GetAsync<Dictionary<string, string>>("api/sayfa-icerigi/ayarlar?dil=tr");
        if (ayarlar == null)
        {
            return;
        }

        if (ayarlar.TryGetValue("VarsayilanDil", out var varsayilanDil) && !string.IsNullOrWhiteSpace(varsayilanDil))
        {
            _varsayilanDil = varsayilanDil.ToLowerInvariant();
        }

        if (ayarlar.TryGetValue("TemaModu", out var temaModu) && !string.IsNullOrWhiteSpace(temaModu))
        {
            _aktifTemaModu = temaModu.ToLowerInvariant() == "acik" ? "acik" : "koyu";
        }
    }

    private async Task MenuleriYukleAsync()
    {
        var menuler = await Api.GetAsync<List<MenuOgesi>>("api/menu/ana");
        if (menuler == null || menuler.Count == 0)
        {
            _menu = VarsayilanMenuOlustur();
            return;
        }

        _menu = menuler
            .OrderBy(m => m.Sira)
            .Select(MenuyeDonustur)
            .ToList();
    }

    private static MenuBaglantisi MenuyeDonustur(MenuOgesi oge)
    {
        var altMenuler = oge.AltMenuler?
            .Where(a => a.AktifMi && !a.SilindiMi)
            .OrderBy(a => a.Sira)
            .Select(MenuyeDonustur)
            .ToList() ?? [];

        return new MenuBaglantisi(
            oge.Baslik,
            UrlNormalizeEt(oge.Url),
            altMenuler);
    }

    private static string UrlNormalizeEt(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return "/";
        }

        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)
            || url.StartsWith("tel:", StringComparison.OrdinalIgnoreCase))
        {
            return url;
        }

        // Eski/yasak yollardan yeni medya havuzu yollarina normalize et
        var normalizeEdilecekYollar = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "/medya/brand/vizitlink3d-logo.svg",
            "medya/brand/vizitlink3d-logo.svg",
            "/img/vizitlink3d-logo.svg",
            "img/vizitlink3d-logo.svg",
            "/img/vizitlink3d-logo.svg",
            "img/vizitlink3d-logo.svg",
            "/vizitlink3d-logo.svg",
            "vizitlink3d-logo.svg",
            "/vizitlink3d-logo.svg",
            "vizitlink3d-logo.svg"
        };

        if (normalizeEdilecekYollar.Contains(url))
        {
            return "/medya/brand/vizitlink3d-logo.svg";
        }

        return url.StartsWith('/') ? url : "/" + url.TrimStart('/');
    }

    private async Task DilSecildi(ChangeEventArgs args)
    {
        var yeniDil = args.Value?.ToString()?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(yeniDil) || yeniDil == _aktifDil)
        {
            return;
        }

        await DilServisi.DilDegistirAsync(yeniDil);
        _aktifDil = DilServisi.AktifDil;
        await JS.InvokeVoidAsync("vizitlink3dDil.htmlDiliniAyarla", _aktifDil);

        if (_ilkRenderTamamlandi)
        {
            Navigasyon.NavigateTo(Navigasyon.Uri, true);
        }
    }

    private async Task TemaModuDegistir(string mod)
    {
        if (string.IsNullOrWhiteSpace(mod) || mod == _aktifTemaModu)
        {
            return;
        }

        _aktifTemaModu = mod;
        await JS.InvokeVoidAsync("vizitlink3dTema.modUygula", mod);
        StateHasChanged();
    }

    private string TemaModuClass(string mod) => _aktifTemaModu == mod ? "gb-mod-btn aktif" : "gb-mod-btn";

    private void MobilMenuDegistir() => _mobilMenuAcik = !_mobilMenuAcik;

    private void MobilMenuKapat() => _mobilMenuAcik = false;

    private void DilDegisti()
    {
        _aktifDil = DilServisi.AktifDil;
        _diller = DilServisi.DesteklenenDiller.ToList();
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        DilServisi.DilDegisti -= DilDegisti;
    }
}

