using VizitLink3D.Ortak.Modeller.Urunler;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages.Admin;

public partial class KonfigurasyonKuraliYonetimi : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogServisi { get; set; } = default!;

    private List<Urun> _urunListesi = [];
    private List<UrunUcBoyutParcasi> _parcaListesi = [];
    private List<RalRengi> _ralListesi = [];
    private List<Malzeme> _malzemeListesi = [];
    private List<UrunKonfigurasyonKurali> _liste = [];
    private List<UrunKonfigurasyonKurali> _filtreliListe = [];
    private bool _yukleniyor = true;
    private bool _kurallarYukleniyor;
    private bool _formAcik;
    private bool _duzenlemeModu;
    private bool _kaydediliyor;
    private UrunKonfigurasyonKurali _form = new();
    private int? _duzenlenenId;
    private int _seciliUrunId;
    private Urun? _seciliUrun;

    protected override async Task OnInitializedAsync()
    {
        _yukleniyor = true;
        StateHasChanged();
        _urunListesi = await Api.GetAsync<List<Urun>>("api/urunler") ?? [];
        _ralListesi = await Api.GetAsync<List<RalRengi>>("api/renkler/ral") ?? [];
        _malzemeListesi = await Api.GetAsync<List<Malzeme>>("api/malzemeler") ?? [];
        _yukleniyor = false;
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_seciliUrunId > 0 && (_seciliUrun == null || _seciliUrun.Id != _seciliUrunId))
        {
            await ParcalariVeKurallariYukle();
        }
        else if (_seciliUrunId == 0)
        {
            _parcaListesi = [];
            _liste = [];
            _filtreliListe = [];
            _seciliUrun = null;
        }
    }

    async Task ParcalariVeKurallariYukle()
    {
        _kurallarYukleniyor = true;
        StateHasChanged();

        _seciliUrun = _urunListesi.FirstOrDefault(u => u.Id == _seciliUrunId);
        _parcaListesi = await Api.GetAsync<List<UrunUcBoyutParcasi>>($"api/urunler/{_seciliUrunId}/uc-boyut-parcalari") ?? [];
        _liste = await Api.GetAsync<List<UrunKonfigurasyonKurali>>($"api/urunler/{_seciliUrunId}/konfigurasyon-kurallari") ?? [];
        _filtreliListe = _liste;

        _kurallarYukleniyor = false;
    }

    Color KuralTipiRengi(string tip) => tip switch
    {
        "Yasak" => Color.Error,
        "Zorunlu" => Color.Warning,
        "Onerilen" => Color.Info,
        _ => Color.Default
    };

    void YeniAc()
    {
        _form = new UrunKonfigurasyonKurali
        {
            UrunId = _seciliUrunId,
            KuralTipi = "Yasak",
            AktifMi = true
        };
        _duzenlenenId = null;
        _duzenlemeModu = false;
        _formAcik = true;
    }

    void Duzenle(UrunKonfigurasyonKurali k)
    {
        _form = new UrunKonfigurasyonKurali
        {
            Id = k.Id,
            UrunId = k.UrunId,
            Parca1Id = k.Parca1Id,
            Parca1RenkId = k.Parca1RenkId,
            Parca1MalzemeId = k.Parca1MalzemeId,
            Parca2Id = k.Parca2Id,
            Parca2RenkId = k.Parca2RenkId,
            Parca2MalzemeId = k.Parca2MalzemeId,
            KuralTipi = k.KuralTipi,
            AktifMi = k.AktifMi
        };
        _duzenlenenId = k.Id;
        _duzenlemeModu = true;
        _formAcik = true;
    }

    async Task SilOnay(UrunKonfigurasyonKurali k)
    {
        var p1 = _parcaListesi.FirstOrDefault(p => p.Id == k.Parca1Id);
        var p2 = _parcaListesi.FirstOrDefault(p => p.Id == k.Parca2Id);
        var onay = await DialogServisi.ShowMessageBoxAsync(
            "Silme Onayı",
            $"'{p1?.GorunenAd ?? k.Parca1Id.ToString()}' → '{p2?.GorunenAd ?? k.Parca2Id.ToString()}' ({k.KuralTipi}) kuralı kalıcı olarak silinecektir. Emin misiniz?",
            yesText: "Evet, Sil",
            cancelText: "İptal");
        if (onay == true) await Sil(k);
    }

    async Task Sil(UrunKonfigurasyonKurali k)
    {
        await Api.DeleteAsync($"api/urunler/{_seciliUrunId}/konfigurasyon-kurallari/{k.Id}");
        Snackbar.Add("Kural başarıyla silindi.", Severity.Success);
        await ParcalariVeKurallariYukle();
    }

    async Task Kaydet()
    {
        _kaydediliyor = true;
        try
        {
            _form.UrunId = _seciliUrunId;

            if (_duzenlenenId.HasValue)
                await Api.PutAsync<UrunKonfigurasyonKurali>($"api/urunler/{_seciliUrunId}/konfigurasyon-kurallari/{_duzenlenenId.Value}", _form);
            else
                await Api.PostAsync<UrunKonfigurasyonKurali>($"api/urunler/{_seciliUrunId}/konfigurasyon-kurallari", _form);

            _formAcik = false;
            Snackbar.Add(_duzenlenenId.HasValue ? "Kural güncellendi." : "Yeni kural eklendi.", Severity.Success);
            await ParcalariVeKurallariYukle();
        }
        catch (Exception ex) { Snackbar.Add($"Hata: {ex.Message}", Severity.Error); }
        finally { _kaydediliyor = false; }
    }

    void FormIptal() { _formAcik = false; }
}
