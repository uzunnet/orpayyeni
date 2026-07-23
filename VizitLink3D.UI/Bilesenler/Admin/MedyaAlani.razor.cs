using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace VizitLink3D.UI.Bilesenler.Admin;

public partial class MedyaAlani : ComponentBase
{
    [Parameter] public string? Deger { get; set; }
    [Parameter] public EventCallback<string?> DegerDegisti { get; set; }
    [Parameter] public string Etiket { get; set; } = "Dosya";
    [Parameter] public Ortak.Modeller.Medya.MedyaTipi? IzinliTip { get; set; }
    [Parameter] public string YuklemeUcNokta { get; set; } = "api/medya/yukle";

    private bool Is3D() =>
        Deger?.EndsWith(".glb", StringComparison.OrdinalIgnoreCase) == true
        || Deger?.EndsWith(".gltf", StringComparison.OrdinalIgnoreCase) == true;

    private bool IsPdf() =>
        Deger?.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) == true;

    private string DosyaAdi()
    {
        if (string.IsNullOrWhiteSpace(Deger)) return "";
        var slash = Deger.LastIndexOf('/');
        return slash >= 0 ? Deger[(slash + 1)..] : Deger;
    }

    private string OnizlemeSrc()
    {
        if (string.IsNullOrWhiteSpace(Deger)) return "/medya/vizitlink3d_default.png";
        if (Deger.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return Deger;
        return $"{api.ApiBaseUrl}{(Deger.StartsWith('/') ? Deger : "/" + Deger)}";
    }

    private async Task DialogAc()
    {
        var p = new DialogParameters
        {
            [nameof(MedyaAlaniDialogu.IzinliTip)] = IzinliTip,
            [nameof(MedyaAlaniDialogu.YuklemeUcNokta)] = YuklemeUcNokta
        };
        var opts = new DialogOptions
        {
            MaxWidth = MaxWidth.Large,
            FullWidth = true,
            CloseButton = true,
            BackdropClick = false
        };
        var dialog = await dialogService.ShowAsync<MedyaAlaniDialogu>("Medya Seç", p, opts);
        var sonuc = await dialog.Result;
        if (sonuc is { Canceled: false, Data: string yol })
            await DegerDegisti.InvokeAsync(yol);
    }

    private async Task Temizle() => await DegerDegisti.InvokeAsync(null);
}
