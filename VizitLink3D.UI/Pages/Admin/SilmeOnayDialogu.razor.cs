using Microsoft.AspNetCore.Components;
using MudBlazor;
using VizitLink3D.UI.Models;

namespace VizitLink3D.UI.Pages.Admin;

public partial class SilmeOnayDialogu : ComponentBase
{

    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public string Mesaj { get; set; } = string.Empty;

    private void Onayla() => MudDialog.Close(DialogResult.Ok(true));
    private void Iptal() => MudDialog.Cancel();

}
