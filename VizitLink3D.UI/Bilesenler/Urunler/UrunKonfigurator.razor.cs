using VizitLink3D.Ortak.Modeller.Urunler;
using VizitLink3D.Ortak.Modeller.Renkler;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Bilesenler.Urunler;

public partial class UrunKonfigurator : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    [Parameter, EditorRequired] public int UrunId { get; set; }

    private List<UrunUcBoyutParcasi> _parcalar = [];
    private List<RalRengi> _renkler = [];
    private int? _seciliParcaId;
    private string _secilenRenkKodu = "#E8E4DF";
    private string? _modelYolu;
    private int? _modelId;
    private string? _kameraAyarJson;
    private string? _isikAyarJson;
    private string? _cevreAyarJson;
    private bool _yukleniyor = true;

    protected override async Task OnInitializedAsync()
    {
        _yukleniyor = true;

        try
        {
            await Task.WhenAll(
                ParcalariYukleAsync(),
                RenkleriYukleAsync(),
                ModelYoluYukleAsync()
            );
        }
        finally
        {
            _yukleniyor = false;
        }
    }

    private async Task ParcalariYukleAsync()
    {
        try
        {
            var sonuc = await Api.GetAsync<List<UrunUcBoyutParcasi>>($"api/urunler/{UrunId}/parcalar");
            if (sonuc != null)
                _parcalar = sonuc.Where(p => p.AktifMi && p.SecilebilirMi).ToList();
        }
        catch { /* parça listesi yüklenemezse konfigüratör çalışmaz */ Snackbar.Add("Parça listesi yüklenemedi", Severity.Warning); }
    }

    private async Task RenkleriYukleAsync()
    {
        try
        {
            var sonuc = await Api.GetAsync<List<RalRengi>>($"api/urunler/{UrunId}/renkler");
            if (sonuc != null && sonuc.Any())
            {
                _renkler = sonuc.Where(r => r.AktifMi).ToList();
                _secilenRenkKodu = _renkler.FirstOrDefault()?.HexKod ?? "#E8E4DF";
            }
        }
        catch { /* renk listesi yüklenemezse varsayılan renk kullanılır */ Snackbar.Add("Renk listesi yüklenemedi", Severity.Warning); }
    }

    private async Task ModelYoluYukleAsync()
    {
        try
        {
            var sonuc = await Api.GetAsync<List<UrunUcBoyutModeli>>($"api/urunler/{UrunId}/uc-boyut-modelleri");
            if (sonuc != null)
            {
                var model = sonuc.FirstOrDefault(m => m.VarsayilanMi) ?? sonuc.FirstOrDefault();
                if (model != null)
                {
                    _modelId = model.Id;
                    var yol = string.IsNullOrWhiteSpace(model.ModelYolu)
                        ? model.ModelDosyaYolu
                        : model.ModelYolu;

                    _modelYolu = yol.StartsWith('/')
                        ? Api.ApiBaseUrl + yol
                        : yol;
                    _kameraAyarJson = model.KameraAyarJson;
                    _isikAyarJson = model.IsikAyarJson;
                    _cevreAyarJson = model.CevreAyarJson;
                }
            }
        }
        catch { /* 3D model yolu yüklenemezse görüntüleyici boş kalır */ Snackbar.Add("3D model yolu yüklenemedi", Severity.Warning); }
    }

    private async Task ParcaDegisti(UrunUcBoyutParcasi parca)
    {
        _seciliParcaId = parca.Id;
        await Task.CompletedTask;
    }

    private async Task RenkDegisti(RalRengi renk)
    {
        _secilenRenkKodu = renk.HexKod ?? "#E8E4DF";
        StateHasChanged();
        await Task.CompletedTask;
    }
}
