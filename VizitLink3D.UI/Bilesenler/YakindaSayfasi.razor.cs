using Microsoft.AspNetCore.Components;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Bilesenler;

public partial class YakindaSayfasi : ComponentBase, IDisposable
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private DilServisi DilServisi { get; set; } = default!;

    [Parameter, EditorRequired] public string Bolum { get; set; } = string.Empty;
    [Parameter] public string Ustbaslik { get; set; } = "Kurumsal";
    [Parameter, EditorRequired] public string Baslik { get; set; } = string.Empty;
    [Parameter] public string Aciklama { get; set; } = "Bu bölüm yakında gerçek içerikle güncellenecek.";
    [Parameter] public string Ikon { get; set; } = MudBlazor.Icons.Material.Outlined.Schedule;

    private string SayfaBasligiEtiketi { get; set; } = string.Empty;
    private bool _dinamikIcerikVar;

    protected override void OnInitialized()
    {
        DilServisi.DilDegisti += DilDegistiginde;
    }

    protected override async Task OnParametersSetAsync()
    {
        await IcerigiYukleAsync();
    }

    private async void DilDegistiginde()
    {
        _dinamikIcerikVar = false;
        await IcerigiYukleAsync();
        StateHasChanged();
    }

    private async Task IcerigiYukleAsync()
    {
        SayfaBasligiEtiketi = Baslik;

        if (string.IsNullOrWhiteSpace(Bolum))
            return;

        try
        {
            var icerik = await Api.GetAsync<Dictionary<string, string>>($"api/sayfa-icerigi/{Bolum}?dil={DilServisi.AktifDil}");
            if (icerik is null)
                return;

            // SayfaBasligi tarayici sekmesi basligi icindir (orn. "Hakkimizda | ORPAY"),
            // sayfa H1'ini degil sadece <title>'i belirler.
            if (icerik.TryGetValue("SayfaBasligi", out var baslik) && !string.IsNullOrWhiteSpace(baslik))
                SayfaBasligiEtiketi = baslik;

            // SayfaIcerigi kendi <h2> basligini icerdiginden, dinamik icerik geldiginde
            // ust komponentin statik H1'i tekrar gostermez (bkz. YakindaSayfasi.razor).
            if (icerik.TryGetValue("SayfaIcerigi", out var aciklama) && !string.IsNullOrWhiteSpace(aciklama))
            {
                Aciklama = aciklama;
                _dinamikIcerikVar = true;
            }
        }
        catch
        {
            // API'ye ulaşılamazsa parametre olarak gelen varsayılan metin gösterilmeye devam eder.
        }
    }

    public void Dispose()
    {
        DilServisi.DilDegisti -= DilDegistiginde;
    }
}
