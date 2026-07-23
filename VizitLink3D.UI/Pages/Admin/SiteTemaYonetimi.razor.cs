using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages.Admin;

public partial class SiteTemaYonetimi : ComponentBase
{
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private ApiIstemcisi Api { get; set; } = default!;

    private readonly List<SiteTemaOzetDto> _siteTemalari = [];
    private readonly List<FirmaSecimDto> _firmalar = [];

    private FirmaTemaDto? _firmaTema;
    private SiteTemaOzetDto? _taslakTema;
    private string _aktifSiteTemaSlug = "gold";
    private string _aktifAdminTemaSlug = "endustri-karanlik";
    private string _taslakTemaSlug = string.Empty;
    private string _taslakTemaBaslik = string.Empty;
    private string _seciliFirmaAdi = string.Empty;
    private int? _seciliFirmaId;
    private bool _yukleniyor = true;
    private bool _uygulaniyor;

    protected override async Task OnInitializedAsync()
    {
        _yukleniyor = true;

        await SiteTemalariniYukleAsync();
        await FirmalariYukleAsync();
        await FirmaTemasiniYukleAsync(_seciliFirmaId);

        _yukleniyor = false;
    }

    private async Task SiteTemalariniYukleAsync()
    {
        var liste = await Api.GetAsync<List<SiteTemaOzetDto>>("api/tema/kapsam?kapsam=site") ?? [];
        _siteTemalari.Clear();
        _siteTemalari.AddRange(liste);
    }

    private async Task FirmalariYukleAsync()
    {
        var liste = await Api.GetAsync<List<FirmaSecimDto>>("api/firma-tema/firmalar") ?? [];

        _firmalar.Clear();
        _firmalar.AddRange(liste);

        if (_firmalar.Count > 0)
        {
            _seciliFirmaId = _firmalar[0].FirmaId;
            _seciliFirmaAdi = _firmalar[0].Ad;
        }
    }

    private async Task FirmaSecildiAsync(int? firmaId)
    {
        _seciliFirmaId = firmaId;
        _seciliFirmaAdi = _firmalar.FirstOrDefault(f => f.FirmaId == firmaId)?.Ad ?? string.Empty;
        await FirmaTemasiniYukleAsync(firmaId);
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
            _aktifAdminTemaSlug = string.IsNullOrWhiteSpace(_firmaTema.AdminTema)
                ? "endustri-karanlik"
                : _firmaTema.AdminTema;
        }
    }

    private void TemaTaslakSec(SiteTemaOzetDto tema)
    {
        _taslakTema = tema;
        _taslakTemaSlug = tema.Slug;
        _taslakTemaBaslik = tema.Ad;
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
            AdminTema = _aktifAdminTemaSlug,
            SiteTema = _taslakTema.Slug
        });

        if (cevap?.BasariliMi == true && cevap.Veri is not null)
        {
            _firmaTema = cevap.Veri;
            _aktifSiteTemaSlug = _taslakTema.Slug;
            await JS.InvokeVoidAsync("localStorage.setItem", "vizitlink3d_site_tema", _taslakTema.Slug);

            try
            {
                await Api.PostAsync<object>("api/tema/aktif", new { temaAd = _taslakTema.Slug });
            }
            catch { }

            Snackbar.Add(
                string.Format(dil.T("admin.siteTema.firmayaAtandi", "{0} teması siteye uygulandı."), _taslakTema.Ad),
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
        var aktif = _siteTemalari.FirstOrDefault(t => t.Slug == _aktifSiteTemaSlug);
        if (aktif is not null)
        {
            TemaTaslakSec(aktif);
        }
        await Task.CompletedTask;
    }

    public sealed record SiteTemaOzetDto(string Slug, string Ad, string Aciklama, bool GlassmorphismAktif, bool Premium, string Etiketler, string? ThumbnailUrl);

    public sealed record FirmaSecimDto(int FirmaId, string Slug, string Ad, string AdminTema, string SiteTema);

    public sealed record FirmaTemaDto(
        int FirmaId,
        string Slug,
        string Ad,
        string AdminTema,
        string SiteTema,
        string? TasarimRengi1,
        string? TasarimRengi2,
        string? TasarimRengi3);

    public sealed class FirmaTemaGuncelleDto
    {
        public int? FirmaId { get; set; }
        public string? AdminTema { get; set; }
        public string SiteTema { get; set; } = string.Empty;
    }
}
