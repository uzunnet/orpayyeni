using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;
using VizitLink3D.UI.Servisler;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace VizitLink3D.UI.Pages.Admin;

public partial class KonfiguratorStudio : ComponentBase, IAsyncDisposable
{
    [Parameter] public int? ModelId { get; set; }

    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private UcBoyutServisi UcBoyut { get; set; } = default!;
    [Inject] private DilServisi DilServisi { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService Dialog { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    // --- Model State ---
    private List<UrunUcBoyutModeli> _modeller = [];
    private int? _seciliModelId;
    private List<UrunUcBoyutParcasi> _parcalar = [];
    private List<UrunUcBoyutParcasi> _filtreliParcalar = [];
    private List<UrunParcaGrubu> _gruplar = [];
    private List<UrunUcBoyutSahneOnayari> _sahneOnayarlari = [];
    private bool _yukleniyor = true;

    // --- Parca Editor State ---
    private UrunUcBoyutParcasi? _seciliParca;
    private string? _parcaFormMeshAdi;
    private string _parcaFormGorunenAd = "";
    private string? _parcaFormMantiksalKod;
    private int? _parcaFormParcaGrubuId;
    private string? _parcaFormParcaTipi;
    private bool _parcaFormRenklenebilirMi = true;
    private bool _parcaFormMalzemeDegisebilirMi = true;
    private bool _parcaFormDokuUygulanabilirMi;
    private bool _parcaFormGizlenebilirMi;
    private bool _parcaFormSecilebilirMi = true;
    private bool _parcaFormHareketliMi;
    private bool _parcaFormAdminOnayliMi;
    private bool _parcaFormAktifMi = true;
    private string? _parcaFormHareketTipi;
    private string? _parcaFormHareketAyarlariJson;
    private string? _parcaFormMalzemeTipiKisiti;
    private int _parcaFormSiraNo;
    private bool _kaydediliyor;

    // --- Preset State ---
    private bool _presetFormAcik;
    private int? _duzenlenenPresetId;
    private string _presetFormAd = "";
    private string _presetFormKod = "";
    private string _presetFormAyarlarJson = "{}";
    private bool _presetFormVarsayilanMi;
    private bool _presetFormAktifMi = true;
    private int _presetFormSiraNo;
    private bool _presetKaydediliyor;

    // --- UI ---
    private int _aktifSekme;
    private string? _modelYolu;
    private string? _arama;

    // --- Lifecycle ---
    protected override async Task OnInitializedAsync()
    {
        await ModelleriYukleAsync();
        _yukleniyor = false;

        if (ModelId.HasValue)
            await ModelSecAsync(ModelId.Value);
    }

    protected override async Task OnParametersSetAsync()
    {
        if (ModelId.HasValue && ModelId.Value != _seciliModelId)
            await ModelSecAsync(ModelId.Value);
    }

    private async Task ModelleriYukleAsync()
    {
        var liste = await Api.GetAsync<List<UrunUcBoyutModeli>>("api/uc-boyut/modeller");
        if (liste != null)
            _modeller = liste;
        else
            Snackbar.Add(DilServisi.T("admin.ks.modelListeHata", "Model listesi alinamadi."), Severity.Error);
    }

    public async Task ModelSecAsync(int? modelId)
    {
        if (!modelId.HasValue) return;

        _seciliModelId = modelId;
        _seciliParca = null;
        ParcaFormTemizle();
        _yukleniyor = true;
        StateHasChanged();

        var cevap = await Api.GetAsync<Cevap<UcBoyutModelKonfigurasyonDtoYerel>>(
            $"api/uc-boyut/admin/modeller/{modelId}/toplu");

        if (cevap?.BasariliMi == true && cevap.Veri != null)
        {
            var veri = cevap.Veri;
            _parcalar = veri.Parcalar;
            _gruplar = veri.Gruplar;
            _sahneOnayarlari = veri.SahneOnayarlari;
            Filtrele();
            _modelYolu = ModelYoluHesapla(veri.ModelDosyaYolu);
        }
        else
        {
            Snackbar.Add(DilServisi.T("admin.ks.modelVeriHata", "Model verisi alinamadi."), Severity.Error);
        }

        _yukleniyor = false;
        StateHasChanged();
    }

    private string? ModelYoluHesapla(string? dosyaYolu)
    {
        if (string.IsNullOrWhiteSpace(dosyaYolu)) return null;
        if (dosyaYolu.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return dosyaYolu;
        var baseUrl = (Api.ApiBaseUrl ?? "").TrimEnd('/');
        return baseUrl + (dosyaYolu.StartsWith('/') ? dosyaYolu : "/" + dosyaYolu);
    }

    // --- Parca islemleri ---
    private void ParcaSec(UrunUcBoyutParcasi p)
    {
        _seciliParca = p;
        _parcaFormMeshAdi = p.MeshAdi;
        _parcaFormGorunenAd = p.GorunenAd;
        _parcaFormMantiksalKod = p.MantiksalKod;
        _parcaFormParcaGrubuId = p.ParcaGrubuId;
        _parcaFormParcaTipi = p.ParcaTipi;
        _parcaFormRenklenebilirMi = p.RenklenebilirMi;
        _parcaFormMalzemeDegisebilirMi = p.MalzemeDegisebilirMi;
        _parcaFormDokuUygulanabilirMi = p.DokuUygulanabilirMi;
        _parcaFormGizlenebilirMi = p.GizlenebilirMi;
        _parcaFormSecilebilirMi = p.SecilebilirMi;
        _parcaFormHareketliMi = p.HareketliMi;
        _parcaFormAdminOnayliMi = p.AdminOnayliMi;
        _parcaFormAktifMi = p.AktifMi;
        _parcaFormHareketTipi = p.HareketTipi;
        _parcaFormHareketAyarlariJson = p.HareketAyarlariJson;
        _parcaFormMalzemeTipiKisiti = p.MalzemeTipiKisiti;
        _parcaFormSiraNo = p.SiraNo;
        _aktifSekme = 0;
        StateHasChanged();
    }

    private void ParcaFormTemizle()
    {
        _seciliParca = null;
        _parcaFormMeshAdi = null;
        _parcaFormGorunenAd = "";
        _parcaFormMantiksalKod = null;
        _parcaFormParcaGrubuId = null;
        _parcaFormParcaTipi = null;
        _parcaFormRenklenebilirMi = true;
        _parcaFormMalzemeDegisebilirMi = true;
        _parcaFormDokuUygulanabilirMi = false;
        _parcaFormGizlenebilirMi = false;
        _parcaFormSecilebilirMi = true;
        _parcaFormHareketliMi = false;
        _parcaFormAdminOnayliMi = false;
        _parcaFormAktifMi = true;
        _parcaFormHareketTipi = null;
        _parcaFormHareketAyarlariJson = null;
        _parcaFormMalzemeTipiKisiti = null;
        _parcaFormSiraNo = 0;
    }

    private async Task ParcaKaydetAsync()
    {
        if (!_seciliModelId.HasValue) return;

        _kaydediliyor = true;
        StateHasChanged();

        var parcaDto = new
        {
            MeshAdi = _parcaFormMeshAdi ?? "",
            MantiksalKod = _parcaFormMantiksalKod,
            GorunenAd = _parcaFormGorunenAd,
            ParcaGrubuId = _parcaFormParcaGrubuId,
            HareketTipi = _parcaFormHareketTipi,
            HareketAyarlariJson = _parcaFormHareketAyarlariJson,
            DokuUygulanabilirMi = _parcaFormDokuUygulanabilirMi,
            GorunurlukDegisebilirMi = _parcaFormGizlenebilirMi,
            RenklenebilirMi = _parcaFormRenklenebilirMi,
            MalzemeDegisebilirMi = _parcaFormMalzemeDegisebilirMi,
            SecilebilirMi = _parcaFormSecilebilirMi,
            HareketliMi = _parcaFormHareketliMi,
            ParcaTipi = _parcaFormParcaTipi,
            MalzemeTipiKisiti = _parcaFormMalzemeTipiKisiti,
            SiraNo = _parcaFormSiraNo,
            AktifMi = _parcaFormAktifMi,
            AdminOnayliMi = _parcaFormAdminOnayliMi
        };

        var istek = new { Parcalar = new[] { parcaDto } };
        var cevap = await Api.PutAsync<Cevap<object>>(
            $"api/uc-boyut/admin/modeller/{_seciliModelId.Value}/parcalar/toplu", istek);

        _kaydediliyor = false;

        if (cevap?.BasariliMi == true)
        {
            Snackbar.Add(DilServisi.T("admin.ks.parcaKaydedildi", "Parca basariyla kaydedildi."), Severity.Success);
            await ModelSecAsync(_seciliModelId.Value);
        }
        else
        {
            Snackbar.Add(DilServisi.T("admin.ks.kaydetmeBasarisiz", "Kaydetme basarisiz oldu."), Severity.Error);
        }

        StateHasChanged();
    }

    // --- Preset islemleri ---
    private void YeniPresetAc()
    {
        _presetFormAcik = true;
        _duzenlenenPresetId = null;
        _presetFormAd = "";
        _presetFormKod = "";
        _presetFormAyarlarJson = "{}";
        _presetFormVarsayilanMi = false;
        _presetFormAktifMi = true;
        _presetFormSiraNo = _sahneOnayarlari.Count + 1;
    }

    private void PresetDuzenleAc(UrunUcBoyutSahneOnayari preset)
    {
        _presetFormAcik = true;
        _duzenlenenPresetId = preset.Id;
        _presetFormAd = preset.Ad;
        _presetFormKod = preset.Kod;
        _presetFormAyarlarJson = preset.AyarlarJson ?? "{}";
        _presetFormVarsayilanMi = preset.VarsayilanMi;
        _presetFormAktifMi = preset.AktifMi;
        _presetFormSiraNo = preset.SiraNo;
    }

    private void PresetFormKapat()
    {
        _presetFormAcik = false;
        _duzenlenenPresetId = null;
    }

    private async Task PresetEkleAsync()
    {
        if (!_seciliModelId.HasValue) return;

        _presetKaydediliyor = true;
        StateHasChanged();

        var dto = new
        {
            Ad = _presetFormAd,
            Kod = _presetFormKod,
            AyarlarJson = _presetFormAyarlarJson,
            VarsayilanMi = _presetFormVarsayilanMi,
            AktifMi = _presetFormAktifMi,
            SiraNo = _presetFormSiraNo
        };

        var cevap = await Api.PostAsync<Cevap<UrunUcBoyutSahneOnayari>>(
            $"api/uc-boyut/admin/modeller/{_seciliModelId.Value}/sahne-onayarlari", dto);

        _presetKaydediliyor = false;

        if (cevap?.BasariliMi == true)
        {
            Snackbar.Add(DilServisi.T("admin.ks.presetEklendi", "Preset basariyla eklendi."), Severity.Success);
            PresetFormKapat();
            await ModelSecAsync(_seciliModelId.Value);
        }
        else
        {
            Snackbar.Add(DilServisi.T("admin.ks.presetEklemeBasarisiz", "Preset eklenemedi."), Severity.Error);
        }

        StateHasChanged();
    }

    private async Task PresetGuncelleAsync()
    {
        if (!_duzenlenenPresetId.HasValue) return;

        _presetKaydediliyor = true;
        StateHasChanged();

        var dto = new
        {
            Ad = _presetFormAd,
            Kod = _presetFormKod,
            AyarlarJson = _presetFormAyarlarJson,
            VarsayilanMi = _presetFormVarsayilanMi,
            AktifMi = _presetFormAktifMi,
            SiraNo = _presetFormSiraNo
        };

        var cevap = await Api.PutAsync<Cevap<UrunUcBoyutSahneOnayari>>(
            $"api/uc-boyut/admin/sahne-onayarlari/{_duzenlenenPresetId.Value}", dto);

        _presetKaydediliyor = false;

        if (cevap?.BasariliMi == true)
        {
            Snackbar.Add(DilServisi.T("admin.ks.presetGuncellendi", "Preset basariyla guncellendi."), Severity.Success);
            PresetFormKapat();
            await ModelSecAsync(_seciliModelId!.Value);
        }
        else
        {
            Snackbar.Add(DilServisi.T("admin.ks.presetGuncellemeBasarisiz", "Preset guncellenemedi."), Severity.Error);
        }

        StateHasChanged();
    }

    private async Task PresetSilAsync(UrunUcBoyutSahneOnayari preset)
    {
        var parametreler = new DialogParameters
        {
            ["Mesaj"] = DilServisi.T("admin.ks.presetSilOnay", $"\"{preset.Ad}\" silinsin mi?")
        };

        var dialog = await Dialog.ShowAsync<SilmeOnayDialogu>(
            DilServisi.T("admin.ks.silmeOnayi", "Silme Onayi"),
            parametreler);

        var dialogSonuc = await dialog.Result;
        if (dialogSonuc?.Data is true)
        {
            var cevap = await Api.DeleteAsync($"api/uc-boyut/admin/sahne-onayarlari/{preset.Id}");
            if (cevap?.BasariliMi == true)
            {
                Snackbar.Add(DilServisi.T("admin.ks.presetSilindi", "Preset silindi."), Severity.Success);
                await ModelSecAsync(_seciliModelId!.Value);
            }
            else
            {
                Snackbar.Add(DilServisi.T("admin.ks.presetSilmeBasarisiz", "Preset silinemedi."), Severity.Error);
            }
        }
    }

    // --- Yardimcilar ---
    private void Filtrele()
    {
        if (string.IsNullOrWhiteSpace(_arama))
            _filtreliParcalar = [.. _parcalar];
        else
            _filtreliParcalar = _parcalar
                .Where(p =>
                    (!string.IsNullOrEmpty(p.MeshAdi) &&
                     p.MeshAdi.Contains(_arama, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(p.GorunenAd) &&
                     p.GorunenAd.Contains(_arama, StringComparison.OrdinalIgnoreCase)))
                .ToList();
    }

    private string GetParcaItemClass(UrunUcBoyutParcasi parca)
    {
        var baseClass = "gb-ks-parca-oge";
        if (_seciliParca != null && _seciliParca.Id == parca.Id)
            return $"{baseClass} secilen";
        return baseClass;
    }

    /// <summary>
    /// 3D viewer'dan gelen mesh tiklamasi callback'i.
    /// UcBoyutGoruntuleyici bilesenine EventCallback eklenince aktif olur.
    /// </summary>
    [JSInvokable]
    public async Task MeshSecildi(string meshAdi)
    {
        var parca = _parcalar.FirstOrDefault(p =>
            string.Equals(p.MeshAdi, meshAdi, StringComparison.OrdinalIgnoreCase));

        if (parca != null)
        {
            ParcaSec(parca);
            return;
        }

        // Bilinmeyen mesh — yeni parca olarak ac
        _seciliParca = null;
        _parcaFormMeshAdi = meshAdi;
        _parcaFormGorunenAd = meshAdi;
        _parcaFormSiraNo = _parcalar.Count + 1;
        _aktifSekme = 0;
        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        _modeller.Clear();
        _parcalar.Clear();
        _filtreliParcalar.Clear();
        _gruplar.Clear();
        _sahneOnayarlari.Clear();
        await Task.CompletedTask;
    }

    /// <summary>
    /// API'den donen aggregate veriyi karsilayan yerel DTO.
    /// UI projesi API DTO'larina referans vermedigi icin burada tanimlanir.
    /// </summary>
    private class UcBoyutModelKonfigurasyonDtoYerel
    {
        public int ModelId { get; set; }
        public string ModelAdi { get; set; } = "";
        public string? ModelDosyaYolu { get; set; }
        public string? ModelTipi { get; set; }
        public int UrunId { get; set; }
        public List<UrunUcBoyutParcasi> Parcalar { get; set; } = [];
        public List<UrunParcaGrubu> Gruplar { get; set; } = [];
        public List<UrunUcBoyutSahneOnayari> SahneOnayarlari { get; set; } = [];
    }
}
