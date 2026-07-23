using VizitLink3D.UI.Servisler;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace VizitLink3D.UI.Bilesenler.AI;

public partial class AIYazButonu : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;

    [Parameter] public string Amac { get; set; } = "MetinYaz";
    [Parameter] public string? Baglam { get; set; }
    [Parameter] public EventCallback<string> CevapGeldi { get; set; }
    [Parameter] public string ButonMetni { get; set; } = "✨ AI ile Yaz";

    private bool _panelAcik;
    private string _seciliIslem = "MetinYaz";
    private string _ozelPrompt = string.Empty;
    private bool _yukleniyor;

    // Typewriter
    private string _goruntulenMetin = string.Empty;
    private string _tamMetin = string.Empty;
    private bool _yaziyor;
    private CancellationTokenSource? _yazmaIptalKaynagi;

    private void PanelAcKapat()
    {
        _panelAcik = !_panelAcik;
        if (!_panelAcik)
        {
            _goruntulenMetin = string.Empty;
            _tamMetin = string.Empty;
            _yaziyor = false;
            _yukleniyor = false;
            _ozelPrompt = string.Empty;
            _seciliIslem = "MetinYaz";
        }
    }

    private void IslemSec(string islem)
    {
        _seciliIslem = islem;
        _goruntulenMetin = string.Empty;
        _tamMetin = string.Empty;
        _yaziyor = false;
    }

    private async Task Uret()
    {
        _yukleniyor = true;
        _goruntulenMetin = string.Empty;
        _tamMetin = string.Empty;
        StateHasChanged();

        var sistemPrompt = _seciliIslem switch
        {
            "Duzelt" => "Sen bir metin düzeltme asistanısın. Verilen metni yazım ve dilbilgisi açısından düzelt. Sadece düzeltilmiş metni döndür, açıklama yapma.",
            "Kisalt" => "Sen bir metin kısaltma asistanısın. Verilen metni özünü koruyarak kısalt. Sadece kısaltılmış metni döndür, açıklama yapma.",
            "Uzat" => "Sen bir metin genişletme asistanısın. Verilen metni daha detaylı ve zengin hale getir. Sadece genişletilmiş metni döndür, açıklama yapma.",
            "Cevir" => "Sen bir çeviri asistanısın. Verilen metni Türkçe'ye çevir. Sadece çeviriyi döndür, açıklama yapma.",
            _ => "Sen bir içerik üretme asistanısın. Sadece istenen metni üret, açıklama yapma."
        };

        var prompt = string.IsNullOrEmpty(_ozelPrompt) ? (Baglam ?? "") : _ozelPrompt;

        var yanit = await Api.PostAsync<string>("api/ai/yaz", new
        {
            prompt,
            amac = _seciliIslem,
            sistemPrompt
        });

        if (yanit?.BasariliMi == true && !string.IsNullOrEmpty(yanit.Veri))
        {
            _tamMetin = yanit.Veri;
            _yukleniyor = false;
            await MetniYazdir(_tamMetin);
        }
        else
        {
            _yukleniyor = false;
            snackbar.Add(dil.T("ai.hata", "AI yanıt üretemedi"), Severity.Error);
        }
    }

    private async Task MetniYazdir(string metin)
    {
        _yaziyor = true;
        _goruntulenMetin = string.Empty;
        _yazmaIptalKaynagi = new CancellationTokenSource();
        var iptal = _yazmaIptalKaynagi.Token;

        for (int i = 0; i < metin.Length; i++)
        {
            if (iptal.IsCancellationRequested)
                break;

            _goruntulenMetin += metin[i];
            StateHasChanged();
            await Task.Delay(15, iptal);
        }

        _yaziyor = false;
    }

    private void Durdur()
    {
        _yazmaIptalKaynagi?.Cancel();
        _yaziyor = false;
        _goruntulenMetin = _tamMetin;
        StateHasChanged();
    }

    private async Task Onayla()
    {
        await CevapGeldi.InvokeAsync(_tamMetin);
        PanelAcKapat();
    }
}
