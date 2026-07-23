using Microsoft.AspNetCore.Components;
using MudBlazor;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages.Admin;

public partial class DenetimLog : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;

    private List<LogDto> _loglar = new();
    private bool _yukleniyor = true;
    private int _sayfa = 1;
    private int _sayfaBoyutu = 20;
    private int _toplam;
    private int _toplamSayfa => (_toplam + _sayfaBoyutu - 1) / _sayfaBoyutu;
    private string _arama = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await YukleAsync();
    }

    private async Task YukleAsync()
    {
        _yukleniyor = true;
        var araParam = string.IsNullOrWhiteSpace(_arama) ? "" : $"&ara={Uri.EscapeDataString(_arama)}";
        var yanit = await Api.GetAsync<SayfaliYanit>($"api/denetim-log?sayfa={_sayfa}&sayfaBoyutu={_sayfaBoyutu}{araParam}");
        if (yanit != null)
        {
            _loglar = yanit.liste ?? new();
            _toplam = yanit.toplam;
        }
        _yukleniyor = false;
    }

    private async Task Ara()
    {
        _sayfa = 1;
        await YukleAsync();
    }

    private async Task SayfayaGit(int sayfa)
    {
        _sayfa = sayfa;
        await YukleAsync();
    }

    private class LogDto
    {
        public long Id { get; set; }
        public DateTime ZamanDamgasi { get; set; }
        public string? KullaniciId { get; set; }
        public string Eylem { get; set; } = string.Empty;
        public string? FirmaId { get; set; }
    }

    private class SayfaliYanit
    {
        public List<LogDto> liste { get; set; } = new();
        public int toplam { get; set; }
        public int sayfa { get; set; }
        public int sayfaBoyutu { get; set; }
    }
}
