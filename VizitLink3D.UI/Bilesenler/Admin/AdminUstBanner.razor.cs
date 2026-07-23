using Microsoft.AspNetCore.Components;
using MudBlazor;
using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.UI.Bilesenler.Admin;

public partial class AdminUstBanner
{
    [Inject] private NavigationManager NavigasyonYoneticisi { get; set; } = default!;
    [Inject] private Servisler.DilServisi DilServisi { get; set; } = default!;
    [Inject] private IDialogService DialogServisi { get; set; } = default!;

    [Parameter] public string? SayfaBasligi { get; set; }
    [Parameter] public List<BreadcrumbItem> Breadcrumb { get; set; } = [];
    [Parameter] public int BildirimSayisi { get; set; } = 0;
    [Parameter] public bool IsSuperAdmin { get; set; } = false;
    [Parameter] public string KullaniciAdi { get; set; } = "Yönetici";
    [Parameter] public string Rol { get; set; } = "Admin";
    [Parameter] public string? LogoUrl { get; set; }
    [Parameter] public string AktifTemaModu { get; set; } = "koyu";
    [Parameter] public EventCallback OnMenuToggle { get; set; }
    [Parameter] public EventCallback OnNotificationClick { get; set; }
    [Parameter] public EventCallback<string> OnLanguageChanged { get; set; }
    [Parameter] public EventCallback<string> OnThemeModeChanged { get; set; }

    private bool _bildirimDropdownAcik = false;
    private List<MudBlazor.BreadcrumbItem> _breadcrumbItems = [];

    public record BreadcrumbItem(string Ad, string? Href = null);

    protected override void OnInitialized()
    {
        if (string.IsNullOrEmpty(SayfaBasligi))
        {
            SayfaBasligi = DilServisi.T("admin.dashboard", "Yönetim Paneli");
        }
    }

    protected override void OnParametersSet()
    {
        _breadcrumbItems = Breadcrumb
            .Select(b => new MudBlazor.BreadcrumbItem(b.Ad, b.Href, b.Href != null))
            .ToList();
    }

    private async Task MenuTugKeySilidi()
    {
        await OnMenuToggle.InvokeAsync();
    }

    private async Task BildirimTiklandi()
    {
        _bildirimDropdownAcik = !_bildirimDropdownAcik;
        await OnNotificationClick.InvokeAsync();
    }

    private void ProfilMenuAc()
    {
        NavigasyonYoneticisi.NavigateTo("/admin/profil");
    }

    private void AyarlarAc()
    {
        NavigasyonYoneticisi.NavigateTo("/admin/ayarlar");
    }

    private async Task DilDegistir(string kod)
    {
        if (string.IsNullOrEmpty(kod) || kod == DilServisi.AktifDil) return;
        await DilServisi.DilDegistirAsync(kod);
        if (OnLanguageChanged.HasDelegate)
            await OnLanguageChanged.InvokeAsync(kod);
    }

    private async Task TemaModuDegistir(string mod)
    {
        if (string.IsNullOrWhiteSpace(mod) || mod == AktifTemaModu)
        {
            return;
        }

        if (OnThemeModeChanged.HasDelegate)
        {
            await OnThemeModeChanged.InvokeAsync(mod);
        }
    }

    private string TemaDugmeSinifi(string mod) =>
        AktifTemaModu == mod ? "admin-banner-tema-btn aktif" : "admin-banner-tema-btn";
}
