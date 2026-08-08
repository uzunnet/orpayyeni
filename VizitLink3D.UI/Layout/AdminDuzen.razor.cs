using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;
using VizitLink3D.UI.Servisler;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.UI.Bilesenler;

namespace VizitLink3D.UI.Layout;

public partial class AdminDuzen : LayoutComponentBase, IDisposable
{
    [Inject] private ApiIstemcisi Api { get; set; } = null!;
    [Inject] private FirmaBilgisiServisi FirmaBilgisi { get; set; } = null!;
    [Inject] private BildirimServisi BildirimServisi { get; set; } = null!;
    [Inject] private KimlikServisi Kimlik { get; set; } = null!;
    [Inject] private NavigationManager Nav { get; set; } = null!;
    [Inject] private IJSRuntime JS { get; set; } = null!;
    
    private bool _solCekmeceAcik = true;
    private bool _sagCekmeceAcik = false;
    private List<MenuOgesi> _adminMenuleri = new();
    private KomutPaleti? _komutPaleti;
    private int _bildirimSayisi;

    private string _aktifSayfaBasligi = "";
    private List<Bilesenler.Admin.AdminUstBanner.BreadcrumbItem> _breadcrumbs = [];
    private bool _isSuperAdmin;
    private string _firmaAdi = string.Empty;
    private string _kullaniciAdi = "";
    private string _rol = "";
    private string _aktifTemaModu = "koyu";
    private string _varsayilanDil = "tr";
    private bool KoyuTemaMi => _aktifTemaModu != "acik";

    // FAZ 4: Logo ve favicon API'den firma bilgisiyle dinamik yuklenir.
    // Fallback degerleri kaldirilmistir; _logoUrl null ise UI'da metin tabanli logo gosterilir.
    private string? _logoUrl;
    private string _faviconUrl = "/favicon.png";
    private string LogoTamYolu => MarkaVarligiNormalizeEt(_logoUrl, string.Empty);
    private string FaviconTamYolu => MarkaVarligiNormalizeEt(_faviconUrl, "/favicon.png");

    // Endüstriyel karanlık tema — degiskenler.css --admin-* token'larından beslenir
    private readonly MudTheme _tema = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#C5A059",
            Secondary = "#1A1A27",
            Tertiary = "#d4a574",
            AppbarBackground = "#111111",
            AppbarText = "#ffffff",
            DrawerBackground = "#0a0a0a",
            DrawerText = "rgba(255,255,255,0.70)",
            Background = "#0a0a0a",
            Surface = "#111111",
            TextPrimary = "#ffffff",
            TextSecondary = "rgba(255,255,255,0.70)",
            TextDisabled = "rgba(255,255,255,0.30)",
            ActionDefault = "rgba(255,255,255,0.50)",
            ActionDisabled = "rgba(255,255,255,0.15)",
            DrawerIcon = "rgba(255,255,255,0.60)",
            Divider = "rgba(255,255,255,0.06)",
            DividerLight = "rgba(255,255,255,0.04)",
            TableLines = "rgba(255,255,255,0.06)",
            TableStriped = "rgba(255,255,255,0.02)",
            TableHover = "rgba(255,255,255,0.04)",
            LinesDefault = "rgba(255,255,255,0.06)",
            LinesInputs = "rgba(255,255,255,0.10)",
            OverlayDark = "rgba(0,0,0,0.50)",
            OverlayLight = "rgba(255,255,255,0.02)",
            Error = "#DC2626",
            Success = "#22C55E",
            Warning = "#F59E0B",
            Info = "#3B82F6"
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#C5A059",
            Secondary = "#d4a574",
            Tertiary = "#a0784c",
            AppbarBackground = "#111111",
            AppbarText = "#ffffff",
            DrawerBackground = "#0a0a0a",
            DrawerText = "rgba(255,255,255,0.70)",
            Background = "#0a0a0a",
            Surface = "#111111",
            TextPrimary = "#ffffff",
            TextSecondary = "rgba(255,255,255,0.70)",
            TextDisabled = "rgba(255,255,255,0.30)",
            ActionDefault = "rgba(255,255,255,0.50)",
            ActionDisabled = "rgba(255,255,255,0.15)",
            DrawerIcon = "rgba(255,255,255,0.60)",
            Divider = "rgba(255,255,255,0.06)",
            DividerLight = "rgba(255,255,255,0.04)",
            TableLines = "rgba(255,255,255,0.06)",
            TableStriped = "rgba(255,255,255,0.02)",
            TableHover = "rgba(255,255,255,0.04)",
            LinesDefault = "rgba(255,255,255,0.06)",
            LinesInputs = "rgba(255,255,255,0.10)",
            OverlayDark = "rgba(0,0,0,0.50)",
            OverlayLight = "rgba(255,255,255,0.02)",
            Error = "#DC2626",
            Success = "#22C55E",
            Warning = "#F59E0B",
            Info = "#3B82F6"
        }
    };

    protected override async Task OnInitializedAsync()
    {
        if (!await Kimlik.GirisliMiAsync())
        {
            Nav.NavigateTo("admin/giris", true);
            return;
        }

        await AyarlariYukleAsync();
        await dil.BaslatAsync(_varsayilanDil);
        BildirimServisi.BildirimGeldi += BildirimGuncelle;
        dil.DilDegisti += OnDilDegisti;
        await KullaniciBilgisiYukleAsync();
        await AdminMenuleriniYukleAsync();
        await BildirimServisi.BaslatAsync();
        _bildirimSayisi = BildirimServisi.BekleyenSayisi;
        Nav.LocationChanged += KonumDegistiginde;
        _ = ZiyaretKaydetAsync(Nav.Uri);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        var oncekiTemaModu = _aktifTemaModu;
        await JS.InvokeVoidAsync("vizitlink3dTema.adminTemaIzoleEt");
        await JS.InvokeVoidAsync("vizitlink3dDil.htmlDiliniAyarla", dil.AktifDil);

        var kayitliTemaModu = await JS.InvokeAsync<string?>("localStorage.getItem", "temaMod");
        _aktifTemaModu = string.IsNullOrWhiteSpace(kayitliTemaModu)
            ? _aktifTemaModu
            : (kayitliTemaModu.Equals("acik", StringComparison.OrdinalIgnoreCase) ? "acik" : "koyu");

        await AdminTemaModuUygulaAsync(_aktifTemaModu);

        if (!string.Equals(oncekiTemaModu, _aktifTemaModu, StringComparison.OrdinalIgnoreCase))
        {
            await InvokeAsync(StateHasChanged);
        }
    }

    private void BildirimGuncelle()
    {
        _bildirimSayisi = BildirimServisi.BekleyenSayisi;
        InvokeAsync(StateHasChanged);
    }

    private async Task KullaniciBilgisiYukleAsync()
    {
        var bilgi = await Kimlik.KullaniciBilgiGetirAsync();
        if (bilgi != null)
        {
            _kullaniciAdi = !string.IsNullOrWhiteSpace(bilgi.AdSoyad) ? bilgi.AdSoyad : bilgi.KullaniciAdi;
            _rol = bilgi.Rol;
            _isSuperAdmin = bilgi.Rol == "SuperAdmin";
        }
        else
        {
            _kullaniciAdi = dil.T("admin.duzen.ziyaretci", "Ziyaretci");
            _rol = "";
            _isSuperAdmin = false;
        }
    }

    private async Task AyarlariYukleAsync()
    {
        try
        {
            var firma = await FirmaBilgisi.GetFirmaAsync();
            if (!string.IsNullOrWhiteSpace(firma?.Ad))
            {
                _firmaAdi = firma.Ad;
            }
            if (!string.IsNullOrWhiteSpace(firma?.Logo))
            {
                _logoUrl = MarkaVarligiNormalizeEt(firma.Logo, string.Empty);
            }
            if (!string.IsNullOrWhiteSpace(firma?.Favicon))
            {
                _faviconUrl = MarkaVarligiNormalizeEt(firma.Favicon, "/favicon.png");
            }

            var sozluk = await Api.GetAsync<Dictionary<string, string>>($"api/sayfa-icerigi/ayarlar?dil={dil.AktifDil}");
            if (sozluk != null)
            {
                if (sozluk.TryGetValue("VarsayilanDil", out var varsayilanDil) && !string.IsNullOrWhiteSpace(varsayilanDil))
                {
                    _varsayilanDil = varsayilanDil.ToLowerInvariant();
                }
                if (sozluk.TryGetValue("TemaModu", out var temaModu) && !string.IsNullOrWhiteSpace(temaModu))
                {
                    _aktifTemaModu = temaModu.ToLowerInvariant() == "acik" ? "acik" : "koyu";
                }
                if (sozluk.TryGetValue("LogoUrl", out var logo) && !string.IsNullOrEmpty(logo))
                {
                    _logoUrl = MarkaVarligiNormalizeEt(logo, string.Empty);
                }
                if (sozluk.TryGetValue("FaviconUrl", out var favicon) && !string.IsNullOrEmpty(favicon))
                {
                    _faviconUrl = MarkaVarligiNormalizeEt(favicon, "/favicon.png");
                }
            }
        }
        catch { /* ayarlar yüklenemezse varsayılanlar kullanılır */ }
    }

    private static string MarkaVarligiNormalizeEt(string? deger, string varsayilanDeger)
    {
        if (string.IsNullOrWhiteSpace(deger))
        {
            return varsayilanDeger;
        }

        if (deger.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || deger.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || deger.StartsWith("/", StringComparison.Ordinal))
        {
            return deger;
        }

        return "/" + deger.TrimStart('~').TrimStart('/');
    }

    private async Task AdminMenuleriniYukleAsync()
    {
        var url = "api/menu/admin";
        var liste = await Api.GetAsync<List<MenuOgesi>>(url);
        
        if (liste != null)
        {
            // FAZ 4.2: Aktif modullere gore menu filtresi
            var aktifModulKodlari = await AktifModulKodlariniAlAsync();
            
            foreach (var menu in liste)
            {
                if (menu.Url != null && menu.Url.StartsWith("/"))
                    menu.Url = menu.Url.TrimStart('/');

                if (menu.AltMenuler != null)
                {
                    menu.AltMenuler = menu.AltMenuler
                        .Where(a => !a.SuperAdminGerekliMi || _isSuperAdmin)
                        .ToList();

                    foreach (var alt in menu.AltMenuler)
                    {
                        if (alt.Url != null && alt.Url.StartsWith("/"))
                            alt.Url = alt.Url.TrimStart('/');
                    }
                }
            }

            // SuperAdmin olmayanlar icin SuperAdmin menulerini gizle
            if (!_isSuperAdmin)
            {
                liste = liste.Where(m => !m.SuperAdminGerekliMi).ToList();
                liste = liste.Where(m => !string.IsNullOrEmpty(m.Url) || m.AltMenuler.Any()).ToList();
            }

            // FAZ 4.2: Modul filtresi — aktif modul yoksa tum menuyu goster
            if (aktifModulKodlari.Count > 0)
            {
                liste = liste.Where(m => MenuModulIleEslesiyor(m, aktifModulKodlari)).ToList();
                foreach (var menu in liste)
                {
                    if (menu.AltMenuler != null && menu.AltMenuler.Any())
                    {
                        menu.AltMenuler = menu.AltMenuler
                            .Where(a => MenuModulIleEslesiyor(a, aktifModulKodlari) || a.SistemMenusuMu)
                            .ToList();
                    }
                }
                // Bos ust menuleri temizle
                liste = liste.Where(m => !string.IsNullOrEmpty(m.Url) || m.AltMenuler.Any()).ToList();
            }

            _adminMenuleri = liste;
        }
        else
        {
            _adminMenuleri = new();
        }

        // Menuler DB'den tam geldigi icin SertifikaMenusunuGarantiEt ve KapakModeliMenusunuGarantiEt kaldirildi
    }

    /// <summary>
    /// Menu ogesi URL/YetkiAnahtari icerigi ile aktif modul kodlarini eslestirir.
    /// </summary>
    private static bool MenuModulIleEslesiyor(MenuOgesi menu, HashSet<string> aktifModuller)
    {
        // Sistem menuleri her zaman gorunur
        if (menu.SistemMenusuMu) return true;
        
        // URL iceriginden modul eslesmesi kontrolu
        var url = (menu.Url ?? "").ToLowerInvariant();
        var yetki = (menu.YetkiAnahtari ?? "").ToLowerInvariant();
        
        // YetkiAnahtari dogrudan modul kodu olabilir
        if (!string.IsNullOrEmpty(yetki) && aktifModuller.Contains(yetki))
            return true;
        
        // URL tabanli eslesme
        if (url.Contains("blog") || url.Contains("haber")) 
            return aktifModuller.Contains("blog") || aktifModuller.Contains("haberler");
        if (url.Contains("galeri")) return aktifModuller.Contains("galeri");
        if (url.Contains("urun")) return aktifModuller.Contains("urunler");
        if (url.Contains("slayt")) return aktifModuller.Contains("slayt_yonetimi");
        if (url.Contains("sayfa")) return aktifModuller.Contains("sayfalar");
        if (url.Contains("menu")) return aktifModuller.Contains("menu_yonetimi");
        if (url.Contains("tema")) return aktifModuller.Contains("tema_yonetimi");
        if (url.Contains("proje")) return aktifModuller.Contains("proje_yonetimi");
        if (url.Contains("referans")) return aktifModuller.Contains("referanslar");
        if (url.Contains("sertifika")) return aktifModuller.Contains("sertifikalar");
        if (url.Contains("sss")) return aktifModuller.Contains("sss");
        if (url.Contains("katalog")) return aktifModuller.Contains("katalog");
        if (url.Contains("bayi")) return aktifModuller.Contains("bayi_yonetimi");
        if (url.Contains("ekip")) return aktifModuller.Contains("ekip_yonetimi");
        if (url.Contains("pwa")) return aktifModuller.Contains("pwa_offline");
        if (url.Contains("audit") || url.Contains("denetim")) return aktifModuller.Contains("audit_log");
        if (url.Contains("lisans")) return aktifModuller.Contains("lisans_yonetimi");
        if (url.Contains("bildirim")) return aktifModuller.Contains("bildirimler");
        if (url.Contains("medya")) return aktifModuller.Contains("medya_havuzu");
        if (url.Contains("iletisim") || url.Contains("sohbet")) 
            return aktifModuller.Contains("iletisim") || aktifModuller.Contains("sohbet");
        if (url.Contains("ceviri") || url.Contains("dil")) return aktifModuller.Contains("ai_asistan");
        
        // Eslesme bulunamazsa varsayilan olarak goster
        return true;
    }

    private async Task<HashSet<string>> AktifModulKodlariniAlAsync()
    {
        try
        {
            var firma = await Api.GetAsync<Firma>("api/firma/guncel");
            if (firma?.AktifModulKodlariJson != null)
            {
                return JsonSerializer.Deserialize<List<string>>(firma.AktifModulKodlariJson)
                    ?.ToHashSet(StringComparer.OrdinalIgnoreCase) 
                    ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
        }
        catch { }
        return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    private void SertifikaMenusunuGarantiEt()
    {
        const string sertifikaUrl = "admin/sertifika-yonetimi";
        var menuVarMi = _adminMenuleri.Any(m => m.Url == sertifikaUrl || m.AltMenuler.Any(a => a.Url == sertifikaUrl));
        if (menuVarMi)
        {
            return;
        }

        var sertifikaMenusu = new MenuOgesi
        {
            Baslik = dil.T("admin.sertifika.baslik", "Sertifika Yönetimi"),
            Url = sertifikaUrl,
            Ikon = "Verified",
            Sira = 80,
            Konum = "AdminSol",
            AktifMi = true
        };

        var icerikMenusu = _adminMenuleri.FirstOrDefault(m =>
            m.Baslik.Contains("icerik", StringComparison.OrdinalIgnoreCase) ||
            m.Baslik.Contains("içerik", StringComparison.OrdinalIgnoreCase) ||
            m.Baslik.Contains("content", StringComparison.OrdinalIgnoreCase));

        if (icerikMenusu is not null)
        {
            icerikMenusu.AltMenuler.Add(sertifikaMenusu);
            icerikMenusu.AltMenuler = icerikMenusu.AltMenuler.OrderBy(m => m.Sira).ToList();
        }
        else
        {
            _adminMenuleri.Add(sertifikaMenusu);
            _adminMenuleri = _adminMenuleri.OrderBy(m => m.Sira).ToList();
        }
    }

    private void KapakModeliMenusunuGarantiEt()
    {
        const string modelUrl = "admin/kapak-modeli-yonetimi";
        var menuVarMi = _adminMenuleri.Any(m => m.Url == modelUrl || m.AltMenuler.Any(a => a.Url == modelUrl));
        if (menuVarMi)
        {
            return;
        }

        var modelMenusu = new MenuOgesi
        {
            Baslik = dil.T("admin.kapakModeli.baslik", "Kapi/Kapak Modelleri"),
            Url = modelUrl,
            Ikon = "DoorFront",
            Sira = 2,
            Konum = "AdminSol",
            AktifMi = true
        };

        var urunMenusu = _adminMenuleri.FirstOrDefault(m =>
            m.Baslik.Contains("urun", StringComparison.OrdinalIgnoreCase) ||
            m.Baslik.Contains("ürün", StringComparison.OrdinalIgnoreCase) ||
            m.Baslik.Contains("3d", StringComparison.OrdinalIgnoreCase));

        if (urunMenusu is not null)
        {
            urunMenusu.AltMenuler.Add(modelMenusu);
            urunMenusu.AltMenuler = urunMenusu.AltMenuler.OrderBy(m => m.Sira).ToList();
        }
        else
        {
            _adminMenuleri.Add(modelMenusu);
            _adminMenuleri = _adminMenuleri.OrderBy(m => m.Sira).ToList();
        }
    }

    private void SolCekmeceyiAcKapat()
    {
        _solCekmeceAcik = !_solCekmeceAcik;
    }

    private void SagCekmeceyiAcKapat()
    {
        _sagCekmeceAcik = !_sagCekmeceAcik;
    }

    private void KlavyeKisayolYakala(KeyboardEventArgs e)
    {
        if (e.Key == "k" && e.CtrlKey)
        {
            _komutPaleti?.Ac();
        }
    }

    private void KomutPaletiAc()
    {
        _komutPaleti?.Ac();
    }

    private void BildirimleriGoster()
    {
        _bildirimSayisi = 0;
    }

    private async Task DilDegistiginde(string kod)
    {
        await AdminMenuleriniYukleAsync();
        await JS.InvokeVoidAsync("vizitlink3dDil.htmlDiliniAyarla", dil.AktifDil);
        await InvokeAsync(StateHasChanged);
    }

    private async Task TemaModuDegistiginde(string mod)
    {
        _aktifTemaModu = mod == "acik" ? "acik" : "koyu";
        await AdminTemaModuUygulaAsync(_aktifTemaModu);
        await InvokeAsync(StateHasChanged);
    }

    private string IkonGetir(string? ikonAdi)
    {
        if (string.IsNullOrEmpty(ikonAdi)) return Icons.Material.Filled.Circle;

        return ikonAdi switch
        {
            "Dashboard" or "Analytics" => Icons.Material.Filled.Analytics,
            "Insights" => Icons.Material.Filled.Insights,
            "Home" => Icons.Material.Filled.Home,
            "HomeRepairService" => Icons.Material.Filled.HomeRepairService,
            "MenuOpen" => Icons.Material.Filled.MenuOpen,
            "DoorFront" => Icons.Material.Filled.DoorFront,
            "MeetingRoom" => Icons.Material.Filled.MeetingRoom,
            "PhotoLibrary" => Icons.Material.Filled.PhotoLibrary,
            "Message" => Icons.Material.Filled.Message,
            "ChatBubbleOutline" => Icons.Material.Filled.ChatBubbleOutline,
            "Palette" => Icons.Material.Filled.Palette,
            "ScreenSearchDesktop" => Icons.Material.Filled.ScreenSearchDesktop,
            "Translate" => Icons.Material.Filled.Translate,
            "Api" => Icons.Material.Filled.Api,
            "Settings" or "Tune" => Icons.Material.Filled.Settings,
            "Slideshow" => Icons.Material.Filled.Slideshow,
            "Quiz" or "QuestionAnswer" => Icons.Material.Filled.Quiz,
            "Timeline" => Icons.Material.Filled.Timeline,
            "GroupWork" => Icons.Material.Filled.GroupWork,
            "RateReview" => Icons.Material.Filled.RateReview,
            "Engineering" => Icons.Material.Filled.Engineering,
            "BookOnline" => Icons.Material.Filled.BookOnline,
            "Article" => Icons.Material.Filled.Article,
            "Store" => Icons.Material.Filled.Store,
            "People" => Icons.Material.Filled.People,
            "Mail" or "Email" => Icons.Material.Filled.Mail,
            "Person" => Icons.Material.Filled.Person,
            "CloudQueue" => Icons.Material.Filled.CloudQueue,
            "VideoLibrary" => Icons.Material.Filled.VideoLibrary,
            "Psychology" => Icons.Material.Filled.Psychology,
            "Category" or "Class" => Icons.Material.Filled.Category,
            "Inventory" => Icons.Material.Filled.Inventory,
            "Inventory2" => Icons.Material.Filled.Inventory2,
            "AccountTree" => Icons.Material.Filled.AccountTree,
            "ViewInAr" or "ThreeDRotation" => Icons.Material.Filled.ViewInAr,
            "Extension" => Icons.Material.Filled.Extension,
            "DesignServices" => Icons.Material.Filled.DesignServices,
            "Rule" => Icons.Material.Filled.Rule,
            "ColorLens" => Icons.Material.Filled.ColorLens,
            "Texture" => Icons.Material.Filled.Texture,
            "Layers" => Icons.Material.Filled.Layers,
            "Verified" => Icons.Material.Filled.Verified,
            "PictureAsPdf" => Icons.Material.Filled.PictureAsPdf,
            "RequestQuote" => Icons.Material.Filled.RequestQuote,
            "Description" or "RssFeed" => Icons.Material.Filled.Description,
            "TrendingUp" => Icons.Material.Filled.TrendingUp,
            "History" => Icons.Material.Filled.History,
            "Delete" => Icons.Material.Filled.Delete,
            "Menu" => Icons.Material.Filled.Menu,
            "Schema" => Icons.Material.Filled.Schema,
            "PermMedia" or "Image" => Icons.Material.Filled.PermMedia,
            "Campaign" => Icons.Material.Filled.Campaign,
            "SupportAgent" or "Support" or "ContactSupport" => Icons.Material.Filled.SupportAgent,
            "Business" or "CorporateFare" => Icons.Material.Filled.Business,
            _ => Icons.Material.Filled.Circle
        };
    }

    private async void OnDilDegisti()
    {
        await AdminMenuleriniYukleAsync();
        await JS.InvokeVoidAsync("vizitlink3dDil.htmlDiliniAyarla", dil.AktifDil);
        await InvokeAsync(StateHasChanged);
    }

    private async Task AdminTemaModuUygulaAsync(string mod)
    {
        _aktifTemaModu = mod == "acik" ? "acik" : "koyu";
        await JS.InvokeVoidAsync("vizitlink3dTema.adminModUygula", _aktifTemaModu);
    }

    private async void KonumDegistiginde(object? sender, LocationChangedEventArgs e)
    {
        await ZiyaretKaydetAsync(e.Location);
        if (e.Location.Contains("admin/ayarlar") || Nav.Uri.Contains("admin/ayarlar"))
        {
            await AyarlariYukleAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task ZiyaretKaydetAsync(string adres)
    {
        var yol = Nav.ToBaseRelativePath(adres);
        if (string.IsNullOrWhiteSpace(yol))
            yol = "/";
        else if (!yol.StartsWith('/'))
            yol = "/" + yol;

        await Api.PostAsync<object>("api/dashboard/ziyaret-kaydet", new ZiyaretKaydetDto(yol, null));
    }

    public void Dispose()
    {
        BildirimServisi.BildirimGeldi -= BildirimGuncelle;
        dil.DilDegisti -= OnDilDegisti;
        Nav.LocationChanged -= KonumDegistiginde;
    }

    private record ZiyaretKaydetDto(string Sayfa, string? Referer);
}


