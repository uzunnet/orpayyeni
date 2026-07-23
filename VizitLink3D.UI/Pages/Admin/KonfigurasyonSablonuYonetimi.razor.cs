using VizitLink3D.Ortak.Modeller.Urunler;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages.Admin;

public partial class KonfigurasyonSablonuYonetimi : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private List<Urun> _urunListesi = [];
    private UrunKonfigurasyonSablonu _sablon = new();
    private Urun? _seciliUrun;
    private int _seciliUrunId;
    private bool _yukleniyor = true;
    private bool _sablonYukleniyor;
    private bool _kaydediliyor;

    protected override async Task OnInitializedAsync() => await UrunleriYukle();

    async Task UrunleriYukle()
    {
        _yukleniyor = true;
        StateHasChanged();
        _urunListesi = await Api.GetAsync<List<Urun>>("api/urunler") ?? [];
        _yukleniyor = false;
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_seciliUrunId > 0 && (_seciliUrun == null || _seciliUrun.Id != _seciliUrunId))
        {
            await SablonYukle();
        }
        else if (_seciliUrunId == 0)
        {
            _sablon = new UrunKonfigurasyonSablonu();
            _seciliUrun = null;
        }
    }

    async Task SablonYukle()
    {
        _sablonYukleniyor = true;
        StateHasChanged();

        _seciliUrun = _urunListesi.FirstOrDefault(u => u.Id == _seciliUrunId);

        var sablon = await Api.GetAsync<UrunKonfigurasyonSablonu>($"api/urunler/{_seciliUrunId}/konfigurasyon-sablonu");
        if (sablon != null)
        {
            _sablon = sablon;
        }
        else
        {
            _sablon = new UrunKonfigurasyonSablonu
            {
                UrunId = _seciliUrunId,
                DetaySablonu = "Endustriyel3D",
                HeroAktifMi = true,
                TeknikOzellikAktifMi = true,
                PdfKaynakAktifMi = true,
                BenzerUrunlerAktifMi = true,
                TeklifFormuAktifMi = true,
                UcBoyutIlkAcilacakMi = false,
                RenkPaneliKonumu = "sag",
                MobilPanelDavranisi = "sheet"
            };
        }

        _sablonYukleniyor = false;
    }

    async Task Kaydet()
    {
        _kaydediliyor = true;
        StateHasChanged();

        try
        {
            _sablon.UrunId = _seciliUrunId;

            if (_sablon.Id > 0)
            {
                await Api.PutAsync<UrunKonfigurasyonSablonu>($"api/urunler/konfigurasyon-sablonu/{_sablon.Id}", _sablon);
            }
            else
            {
                var sonuc = await Api.PostAsync<UrunKonfigurasyonSablonu>("api/urunler/konfigurasyon-sablonu", _sablon);
                if (sonuc?.Veri != null)
                    _sablon.Id = sonuc.Veri.Id;
            }

            Snackbar.Add("Konfigürasyon şablonu başarıyla kaydedildi.", Severity.Success);
            await SablonYukle();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Hata: {ex.Message}", Severity.Error);
        }
        finally
        {
            _kaydediliyor = false;
        }
    }
}
