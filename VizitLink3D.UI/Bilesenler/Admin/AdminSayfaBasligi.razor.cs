using Microsoft.AspNetCore.Components;

namespace VizitLink3D.UI.Bilesenler.Admin;

public partial class AdminSayfaBasligi : ComponentBase
{
    [Parameter] public string Baslik { get; set; } = "";
    [Parameter] public string? Ikon { get; set; }
    [Parameter] public string? Aciklama { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
}
