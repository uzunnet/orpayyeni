using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace VizitLink3D.UI.Bilesenler;

public partial class GaleriDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    [Parameter] public string GorselUrl { get; set; } = "";
    [Parameter] public int Index { get; set; }
    [Parameter] public int Toplam { get; set; }

    private void Kapat() => MudDialog.Close();
}
