using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace VizitLink3D.UI.Bilesenler.Admin;

public partial class AdminBosDurum : ComponentBase
{
    [Parameter] public string Icon { get; set; } = Icons.Material.Filled.Inbox;
    [Parameter] public string Baslik { get; set; } = "";
    [Parameter] public string? Aciklama { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
}
