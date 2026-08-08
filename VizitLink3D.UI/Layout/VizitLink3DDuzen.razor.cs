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

    private string _aktifTema = "orpay-gunduz";
    private string _aktifTemaModu = "acik";
    private string _varsayilanDil = "tr";
    private string _aktifDil = "tr";
    private string _firmaAdi = string.Empty;
    private string _firmaSlug = string.Empty;
    private string? _logoUrl = "/logo.png";
    private List<DilServisi.DilBilgisi> _diller = [];
    private bool _ilkRenderTamamlandi;
    private bool _mobilMenuAcik;

    private List<MenuBaglantisi> _menu = VarsayilanMenuOlustur();
    private bool KoyuTemaMi => _aktifTemaModu != "acik";
    private string MobilMenuClass => MobilMenuGorunumYardimcisi.MenuSinifi(_mobilMenuAcik);
    private string MobilMenuDugmeClass => MobilMenuGorunumYardimcisi.DugmeSinifi(_mobilMenuAcik);

    private readonly MudTheme _tema = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#006d37",
            Secondary = "#4e6073",
            Background = "#f4fbf1",
            Surface = "#ffffff",
            TextPrimary = "#171d17",
            TextSecondary = "#3d4a3f",
            TextDisabled = "#6d7a6e",
            AppbarBackground = "#ffffff",
            DrawerBackground = "#f4fbf1",
            DrawerText = "#171d17",
            Success = "#27ae60",
            Warning = "#e9c349",
            Error = "#ba1a1a",
            Info = "#4a6c8c"
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#27ae60",
            Secondary = "#bccabc",
            Background = "#131313",
            Surface = "#1e2020",
            TextPrimary = "#e5e2e1",
            TextSecondary = "#bccabc",
            TextDisabled = "#879487",
            AppbarBackground = "#131313",
            DrawerBackground = "#1e2020",
            DrawerText = "#e5e2e1",
            Success = "#27ae60",
            Warning = "#e9c349",
            Error = "#ffb4ab",
            Info = "#4a6c8c"
        }
    };

    protected override async Task OnInitializedAsync()
    {
        var ayarlarTask = AyarlariYukleAsync();
        var firmaTask = FirmaBilgisi.GetFirmaAsync();
        // var menuTask = MenuleriYukleAsync(); // API menu kullanilmiyor
        await Task.WhenAll(ayarlarTask, firmaTask);

        await DilServisi.BaslatAsync(_varsayilanDil);
        DilServisi.DilDegisti += DilDegisti;
        _aktifDil = DilServisi.AktifDil;
        _diller = DilServisi.DesteklenenDiller.ToList();

        var firma = await firmaTask;
        if (firma == null) return;

        if (!string.IsNullOrWhiteSpace(firma.Ad))
            _firmaAdi = firma.Ad;
        if (!string.IsNullOrWhiteSpace(firma.Slug))
            _firmaSlug = firma.Slug;
        if (!string.IsNullOrWhiteSpace(firma.SiteTema))
            _aktifTema = firma.SiteTema;
        if (!string.IsNullOrWhiteSpace(firma.Logo))
            _logoUrl = UrlNormalizeEt(firma.Logo);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        await JS.InvokeVoidAsync("localStorage.setItem", "aktif_firma", _firmaSlug);
        await JS.InvokeVoidAsync("localStorage.setItem", "vizitlink3d_site_tema", _aktifTema);

        var kayitliTemaModu = await JS.InvokeAsync<string?>("localStorage.getItem", "temaMod");
        if (string.IsNullOrWhiteSpace(kayitliTemaModu))
        {
            await JS.InvokeVoidAsync("vizitlink3dTema.modUygula", "acik");
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
        new("Ürünler", "/urunler", []),
        new("Katalog", "/katalog", []),
        new("Kurumsal", "/hakkimizda", []),
        new("İletişim", "/iletisim", [])
    ];

    private string MenuBasligi(MenuBaglantisi oge) => oge.Baslik switch
    {
        "Ana Sayfa" => DilServisi.T("menu.anasayfa", oge.Baslik),
        "Ürünler" => DilServisi.T("menu.urunler", oge.Baslik),
        "Katalog" => DilServisi.T("menu.katalog", oge.Baslik),
        "Kurumsal" => DilServisi.T("menu.kurumsal", oge.Baslik),
        "Hakkımızda" => DilServisi.T("menu.hakkimizda", oge.Baslik),
        "İletişim" => DilServisi.T("menu.iletisim", oge.Baslik),
        _ => oge.Baslik
    };

    private async Task AyarlariYukleAsync()
    {
        var ayarlar = await Api.GetAsync<Dictionary<string, string>>("api/sayfa-icerigi/ayarlar?dil=tr");
        if (ayarlar == null) return;

        if (ayarlar.TryGetValue("VarsayilanDil", out var varsayilanDil) && !string.IsNullOrWhiteSpace(varsayilanDil))
            _varsayilanDil = varsayilanDil.ToLowerInvariant();
        if (ayarlar.TryGetValue("TemaModu", out var temaModu) && !string.IsNullOrWhiteSpace(temaModu))
            _aktifTemaModu = temaModu.ToLowerInvariant() == "acik" ? "acik" : "koyu";
    }

    private async Task MenuleriYukleAsync()
    {
        var menuler = await Api.GetAsync<List<MenuOgesi>>("api/menu/ana");
        if (menuler == null || menuler.Count == 0)
        {
            _menu = VarsayilanMenuOlustur();
            return;
        }
        _menu = menuler.OrderBy(m => m.Sira).Select(MenuyeDonustur).ToList();
    }

    private static MenuBaglantisi MenuyeDonustur(MenuOgesi oge)
    {
        var altMenuler = oge.AltMenuler?
            .Where(a => a.AktifMi && !a.SilindiMi)
            .OrderBy(a => a.Sira)
            .Select(MenuyeDonustur)
            .ToList() ?? [];
        return new MenuBaglantisi(oge.Baslik, UrlNormalizeEt(oge.Url), altMenuler);
    }

    private static string UrlNormalizeEt(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return "/";

        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)
            || url.StartsWith("tel:", StringComparison.OrdinalIgnoreCase))
            return url;

        return url.StartsWith('/') ? url : "/" + url.TrimStart('/');
    }

    private async Task DilSecildi(ChangeEventArgs args)
    {
        var yeniDil = args.Value?.ToString()?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(yeniDil) || yeniDil == _aktifDil) return;

        await DilServisi.DilDegistirAsync(yeniDil);
        _aktifDil = DilServisi.AktifDil;
        await JS.InvokeVoidAsync("vizitlink3dDil.htmlDiliniAyarla", _aktifDil);

        if (_ilkRenderTamamlandi)
            Navigasyon.NavigateTo(Navigasyon.Uri, true);
    }

    private async Task TemaModuDegistir(string mod)
    {
        if (string.IsNullOrWhiteSpace(mod) || mod == _aktifTemaModu) return;
        _aktifTemaModu = mod;
        await JS.InvokeVoidAsync("vizitlink3dTema.modUygula", mod);
        StateHasChanged();
    }

    private string TemaModuClass(string mod) => _aktifTemaModu == mod ? "orpay-mod-btn aktif" : "orpay-mod-btn";

    private void MobilMenuDegistir() => _mobilMenuAcik = !_mobilMenuAcik;
    private void MobilMenuKapat() => _mobilMenuAcik = false;

    private void DilDegisti()
    {
        _aktifDil = DilServisi.AktifDil;
        _diller = DilServisi.DesteklenenDiller.ToList();
        InvokeAsync(StateHasChanged);
    }

    public void Dispose() => DilServisi.DilDegisti -= DilDegisti;
}





