using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Collections.Generic;
using System.Threading.Tasks;
using VizitLink3D.UI.Servisler;
using VizitLink3D.Ortak.Modeller.Istekler;
using VizitLink3D.Ortak.Modeller.Ceviriler;
using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.UI.Bilesenler.Admin;

public partial class AICeviriDialog : ComponentBase
{
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; } = default!;

    [Parameter] public int KayitId { get; set; }
    [Parameter] public string TabloOnEki { get; set; } = string.Empty;
    [Parameter] public Dictionary<string, string> CevrilecekAlanlar { get; set; } = new();

    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private string _seciliHedefDil = string.Empty;
    private bool _cevriliyor = false;
    private bool _dillerYukleniyor = true;
    private string _basariMesaji = string.Empty;
    private string _hataMesaji = string.Empty;
    private List<DilSecenegi> _hedefDiller = [];

    protected override async Task OnInitializedAsync()
    {
        try
        {
            await HedefDilleriYukleAsync();
        }
        finally
        {
            _dillerYukleniyor = false;
        }
    }

    private void Iptal() => MudDialog.Cancel();

    private Task HedefDilDegisti(string deger)
    {
        _seciliHedefDil = deger;
        _basariMesaji = string.Empty;
        _hataMesaji = string.Empty;
        return Task.CompletedTask;
    }

    private async Task HedefDilleriYukleAsync()
    {
        var diller = await Api.GetAsync<List<Dil>>("api/dil/desteklenen") ?? [];

        _hedefDiller = diller
            .Where(d => d.AktifMi && !d.Kod.Equals("tr", StringComparison.OrdinalIgnoreCase))
            .OrderBy(d => d.SiraNo)
            .Select(d => new DilSecenegi(d.Kod, d.Ad, d.Bayrak ?? string.Empty))
            .ToList();

        if (_hedefDiller.Count > 0)
            return;

        _hedefDiller = dil.DesteklenenDiller
            .Where(d => !d.Kod.Equals("tr", StringComparison.OrdinalIgnoreCase))
            .Select(d => new DilSecenegi(d.Kod, d.Ad, d.Bayrak ?? string.Empty))
            .ToList();

        if (_hedefDiller.Count == 0)
        {
            _hedefDiller =
            [
                new("en", "English", "fi fi-gb"),
                new("de", "Deutsch", "fi fi-de"),
                new("fr", "Français", "fi fi-fr"),
                new("ru", "Русский", "fi fi-ru"),
                new("ar", "العربية", "fi fi-sa")
            ];
        }

        _seciliHedefDil = _hedefDiller.FirstOrDefault()?.Kod ?? string.Empty;
    }

    private async Task CevirVeKaydet()
    {
        if (string.IsNullOrEmpty(_seciliHedefDil)) return;
        if (!CevrilecekAlanlar.Any()) return;

        _cevriliyor = true;
        _basariMesaji = string.Empty;
        _hataMesaji = string.Empty;
        StateHasChanged();

        try
        {
            int basariliSayisi = 0;
            foreach (var alan in CevrilecekAlanlar)
            {
                if (string.IsNullOrWhiteSpace(alan.Value)) continue;

                string ceviriAnahtari = $"{TabloOnEki}_{KayitId}_{alan.Key}";

                var istek = new OtomatikCeviriIstegi
                {
                    Metin = alan.Value,
                    KaynakDil = "tr",
                    HedefDil = _seciliHedefDil
                };

                var ceviriYanit = await Api.PostAsync<string>("api/yonetim/ceviri/cevir", istek);
                
                if (ceviriYanit?.BasariliMi == true && !string.IsNullOrEmpty(ceviriYanit.Veri))
                {
                    var kayitIstegi = new CeviriKayitIstegi
                    {
                        Anahtar = ceviriAnahtari,
                        Dil = _seciliHedefDil,
                        Deger = ceviriYanit.Veri
                    };

                    var kayitYanit = await Api.PostAsync<string>("api/yonetim/ceviri/kaydet", kayitIstegi);
                    if (kayitYanit?.BasariliMi == true)
                    {
                        basariliSayisi++;
                    }
                }
            }

            if (basariliSayisi == 0)
            {
                _hataMesaji = dil.T("admin.ai.ceviriBasarisiz", "Çeviri tamamlanamadı. Lütfen API ayarlarını ve hedef dili kontrol edin.");
                Snackbar.Add(_hataMesaji, Severity.Warning);
                return;
            }

            _basariMesaji = string.Format(
                dil.T("admin.ai.ceviriBasarili", "{0} alan başarıyla {1} diline çevrildi ve kaydedildi!"),
                basariliSayisi,
                _seciliHedefDil.ToUpperInvariant());
            Snackbar.Add(_basariMesaji, Severity.Success);
            
            await Task.Delay(1500);
            MudDialog.Close(DialogResult.Ok(true));
        }
        catch (Exception ex)
        {
            _hataMesaji = string.Format(
                dil.T("admin.ai.ceviriHatasi", "Çeviri sırasında hata oluştu: {0}"),
                ex.Message);
            Snackbar.Add(_hataMesaji, Severity.Error);
        }
        finally
        {
            _cevriliyor = false;
            StateHasChanged();
        }
    }

    private sealed record DilSecenegi(string Kod, string Ad, string Bayrak);
}
