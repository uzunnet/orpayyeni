using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace VizitLink3D.UI.Bilesenler;

public partial class EndustriyelOnayDialogu : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    [Parameter] public string Baslik { get; set; } = "Onay";
    [Parameter] public string Mesaj { get; set; } = "Bu işlemi onaylıyor musunuz?";
    [Parameter] public string Tur { get; set; } = "sil"; // sil, uyari, basarili

    private void Onayla() => MudDialog.Close(DialogResult.Ok(true));
    private void Iptal() => MudDialog.Cancel();
}
