using VizitLink3D.Ortak.Modeller.Urunler;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Bilesenler.Urunler;

public partial class TeklifIstegiFormu : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;

    [Parameter] public int UrunId { get; set; }
    [Parameter] public int? KonfigurasyonId { get; set; }
    [Parameter] public EventCallback OnSubmitted { get; set; }

    private TeklifIstegi _form = new();
    private bool _gonderildi;
    private bool _gonderiliyor;
    private string? _hataMesaji;
    private List<string> _secimOzetleri = [];

    protected override async Task OnParametersSetAsync()
    {
        if (KonfigurasyonId.HasValue && KonfigurasyonId.Value > 0)
        {
            await SecimOzetleriniYukle();
        }
    }

    async Task SecimOzetleriniYukle()
    {
        try
        {
            var konf = await Api.GetAsync<MusteriKonfigurasyonu>($"api/konfigurasyon/{KonfigurasyonId}");
            if (konf != null && konf.Id > 0)
            {
                var parcalar = await Api.GetAsync<List<MusteriKonfigurasyonParcasi>>($"api/konfigurasyon/{KonfigurasyonId}/parcalar");
                if (parcalar != null)
                {
                    foreach (var p in parcalar)
                    {
                        if (p.SeciliRenkId.HasValue)
                        {
                            var renk = await Api.GetAsync<RalRengi>($"api/renkler/ral/{p.SeciliRenkId.Value}");
                            if (renk != null)
                                _secimOzetleri.Add($"{dil.T("ortak.renk", "Renk")}: {renk.Kod} - {renk.Ad}");
                        }
                        if (p.SeciliMalzemeId.HasValue)
                        {
                            var malzeme = await Api.GetAsync<Malzeme>($"api/malzemeler/{p.SeciliMalzemeId.Value}");
                            if (malzeme != null)
                                _secimOzetleri.Add($"{dil.T("ortak.malzeme", "Malzeme")}: {malzeme.Ad}");
                        }
                    }
                }
            }
        }
        catch
        {
            // Seçim özeti yüklenemezse sessizce devam et
        }
    }

    async Task Gonder()
    {
        _gonderiliyor = true;
        _hataMesaji = null;
        StateHasChanged();

        try
        {
            _form.UrunId = UrunId;
            _form.MusteriKonfigurasyonuId = KonfigurasyonId;
            _form.Durum = "Bekliyor";

            var sonuc = await Api.PostAsync<TeklifIstegi>("api/teklifler", _form);
            if (sonuc?.BasariliMi == true)
            {
                _gonderildi = true;
                await OnSubmitted.InvokeAsync();
            }
            else
            {
                _hataMesaji = dil.T("teklif.gonderimHatasi", "Teklif gonderilirken bir hata olustu. Lutfen tekrar deneyin.");
            }
        }
        catch (Exception ex)
        {
            _hataMesaji = $"{dil.T("ortak.hataOlustu", "Bir hata olustu")}: {ex.Message}";
        }
        finally
        {
            _gonderiliyor = false;
        }
    }
}
