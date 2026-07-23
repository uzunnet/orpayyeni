using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace VizitLink3D.UI.Bilesenler.AI;

public partial class AIStreamMetinKutusu : ComponentBase
{
    [Parameter] public string? Metin { get; set; }
    [Parameter] public bool YaziyorMu { get; set; }
    [Parameter] public EventCallback TekrarUret { get; set; }

    private string _goruntulenMetin = string.Empty;
    private bool _yaziyaBasladi;
    private CancellationTokenSource? _iptalKaynagi;
    private string? _oncekiMetin;

    protected override async Task OnParametersSetAsync()
    {
        if (Metin != _oncekiMetin)
        {
            _oncekiMetin = Metin;
            _goruntulenMetin = string.Empty;
            _yaziyaBasladi = true;

            if (!string.IsNullOrEmpty(Metin) && YaziyorMu)
            {
                await MetniYazdir(Metin);
            }
            else if (!string.IsNullOrEmpty(Metin) && !YaziyorMu)
            {
                _goruntulenMetin = Metin;
            }
        }

        await base.OnParametersSetAsync();
    }

    private async Task MetniYazdir(string metin)
    {
        _iptalKaynagi?.Cancel();
        _iptalKaynagi = new CancellationTokenSource();
        var iptal = _iptalKaynagi.Token;

        for (int i = 0; i < metin.Length; i++)
        {
            if (iptal.IsCancellationRequested)
                break;

            _goruntulenMetin += metin[i];
            StateHasChanged();
            await Task.Delay(20, iptal);
        }

        if (!iptal.IsCancellationRequested)
        {
            _goruntulenMetin = metin;
        }
    }

    private void Durdur()
    {
        _iptalKaynagi?.Cancel();
    }
}
