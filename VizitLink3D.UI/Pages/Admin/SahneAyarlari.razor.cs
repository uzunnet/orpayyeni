using System.Text.Json;
using VizitLink3D.Ortak.Modeller.Urunler;
using VizitLink3D.UI.Servisler;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Utilities;

namespace VizitLink3D.UI.Pages.Admin;

public partial class SahneAyarlari : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private DilServisi DilServisi { get; set; } = default!;

    private List<UrunAilesi> _aileler = [];
    private List<Urun> _urunler = [];
    private List<UrunUcBoyutModeli> _modeller = [];
    private List<UrunUcBoyutModeli> _filtreliModeller = [];
    private int? _seciliAileId;
    private int? _seciliModelId;
    private UrunUcBoyutModeli? _seciliModel;
    private bool _yukleniyor = true;

    private KameraAyar _kamera = new();
    private IsikAyar _isik = new();
    private CevreAyar _cevre = new();

    private MudColor _zeminRengi = new("#CCCCCC");
    private MudColor _arkaPlanRengi = new("#F5F5F0");

    public MudColor ZeminRengi
    {
        get => _zeminRengi;
        set
        {
            _zeminRengi = value;
            _cevre.ZeminRengi = value.ToString(MudColorOutputFormats.Hex);
        }
    }

    public MudColor ArkaPlanRengi
    {
        get => _arkaPlanRengi;
        set
        {
            _arkaPlanRengi = value;
            _cevre.ArkaPlanRengi = value.ToString(MudColorOutputFormats.Hex);
        }
    }

    private static readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true };
    private string KameraOnizlemeJson => JsonSerializer.Serialize(_kamera, _jsonOpts);
    private string IsikOnizlemeJson => JsonSerializer.Serialize(_isik, _jsonOpts);
    private string CevreOnizlemeJson => JsonSerializer.Serialize(_cevre, _jsonOpts);

    private string? GetModelYolu()
    {
        if (_seciliModel == null) return null;
        var yol = string.IsNullOrWhiteSpace(_seciliModel.ModelYolu) ? _seciliModel.ModelDosyaYolu : _seciliModel.ModelYolu;
        if (string.IsNullOrWhiteSpace(yol)) return null;
        
        if (yol.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            return yol;

        return Api.ApiBaseUrl + (yol.StartsWith("/") ? yol : "/" + yol);
    }

    protected override async Task OnInitializedAsync()
    {
        _aileler = await Api.GetAsync<List<UrunAilesi>>("api/urun-ailesi") ?? [];
        _urunler = await Api.GetAsync<List<Urun>>("api/urunler") ?? [];
        _modeller = await Api.GetAsync<List<UrunUcBoyutModeli>>("api/uc-boyut/modeller") ?? [];
        FiltreliModelleriYenile();

        if (_filtreliModeller.Count > 0)
        {
            await ModelSec(_filtreliModeller.First().Id);
        }

        _yukleniyor = false;
    }

    private async Task AileDegisti(int? aileId)
    {
        _seciliAileId = aileId;
        FiltreliModelleriYenile();

        if (_filtreliModeller.Count == 0)
        {
            _seciliModelId = null;
            _seciliModel = null;
            _kamera = new();
            _isik = new();
            _cevre = new();
            return;
        }

        await ModelSec(_filtreliModeller.First().Id);
    }

    private async Task ModelDegisti(int? modelId)
    {
        if (!modelId.HasValue)
            return;

        await ModelSec(modelId.Value);
    }

    private void FiltreliModelleriYenile()
    {
        if (!_seciliAileId.HasValue)
        {
            _filtreliModeller = _modeller
                .Where(m => m.AktifMi && !m.SilindiMi)
                .OrderBy(m => m.ModelAdi)
                .ToList();
            return;
        }

        var urunIds = _urunler
            .Where(u => u.UrunAilesiId == _seciliAileId.Value)
            .Select(u => u.Id)
            .ToHashSet();

        _filtreliModeller = _modeller
            .Where(m => m.AktifMi && !m.SilindiMi && urunIds.Contains(m.UrunId))
            .OrderBy(m => m.ModelAdi)
            .ToList();
    }

    private async Task ModelSec(int modelId)
    {
        _seciliModelId = modelId;
        _seciliModel = _modeller.FirstOrDefault(m => m.Id == modelId);
        if (_seciliModel == null) return;

        var ayarlar = await Api.GetAsync<SahneAyarlariYaniti>($"api/uc-boyut/modeller/{modelId}/sahne-ayarlari");
        if (ayarlar != null)
        {
            _kamera = JsonOku<KameraAyar>(ayarlar.Kamera);
            _isik = JsonOku<IsikAyar>(ayarlar.Isik);
            _cevre = JsonOku<CevreAyar>(ayarlar.Cevre);

            _zeminRengi = new MudColor(string.IsNullOrWhiteSpace(_cevre.ZeminRengi) ? "#CCCCCC" : _cevre.ZeminRengi);
            _arkaPlanRengi = new MudColor(string.IsNullOrWhiteSpace(_cevre.ArkaPlanRengi) ? "#F5F5F0" : _cevre.ArkaPlanRengi);

            _seciliModel.KameraAyarJson = ayarlar.Kamera;
            _seciliModel.IsikAyarJson = ayarlar.Isik;
            _seciliModel.CevreAyarJson = ayarlar.Cevre;
        }

        StateHasChanged();
    }

    private async Task KameraKaydet()
    {
        if (_seciliModel == null) return;
        var cevap = await Api.PutAsync<UrunUcBoyutModeli>($"api/uc-boyut/modeller/{_seciliModel.Id}/kamera-ayar", _kamera);
        if (cevap?.Veri is not null)
            _seciliModel = cevap.Veri;

        Snackbar.Add(cevap?.BasariliMi == true
                ? DilServisi.T("admin.sahne.kameraUygulandi", "Kamera ayarlari uygulandi")
                : DilServisi.T("admin.sahne.kameraKaydedilemedi", "Kamera ayarlari kaydedilemedi"),
            cevap?.BasariliMi == true ? Severity.Success : Severity.Error);
    }

    private async Task IsikKaydet()
    {
        if (_seciliModel == null) return;
        var cevap = await Api.PutAsync<UrunUcBoyutModeli>($"api/uc-boyut/modeller/{_seciliModel.Id}/isik-ayar", _isik);
        if (cevap?.Veri is not null)
            _seciliModel = cevap.Veri;

        Snackbar.Add(cevap?.BasariliMi == true
                ? DilServisi.T("admin.sahne.isikUygulandi", "Isik ayarlari uygulandi")
                : DilServisi.T("admin.sahne.isikKaydedilemedi", "Isik ayarlari kaydedilemedi"),
            cevap?.BasariliMi == true ? Severity.Success : Severity.Error);
    }

    private async Task CevreKaydet()
    {
        if (_seciliModel == null) return;
        _cevre.ZeminRengi = _zeminRengi.ToString(MudColorOutputFormats.Hex);
        _cevre.ArkaPlanRengi = _arkaPlanRengi.ToString(MudColorOutputFormats.Hex);
        var cevap = await Api.PutAsync<UrunUcBoyutModeli>($"api/uc-boyut/modeller/{_seciliModel.Id}/cevre-ayar", _cevre);
        if (cevap?.Veri is not null)
            _seciliModel = cevap.Veri;

        Snackbar.Add(cevap?.BasariliMi == true
                ? DilServisi.T("admin.sahne.cevreUygulandi", "Cevre ayarlari uygulandi")
                : DilServisi.T("admin.sahne.cevreKaydedilemedi", "Cevre ayarlari kaydedilemedi"),
            cevap?.BasariliMi == true ? Severity.Success : Severity.Error);
    }

    private static T JsonOku<T>(string? json) where T : new()
    {
        if (string.IsNullOrWhiteSpace(json))
            return new T();

        try
        {
            return JsonSerializer.Deserialize<T>(json, _jsonOpts) ?? new T();
        }
        catch
        {
            return new T();
        }
    }

    private sealed class SahneAyarlariYaniti
    {
        public int ModelId { get; set; }
        public string? Kamera { get; set; }
        public string? Isik { get; set; }
        public string? Cevre { get; set; }
    }
}
