using Microsoft.AspNetCore.Components;

namespace VizitLink3D.UI.Pages;

public partial class KapakDetay
{
    [Parameter] public int ModelId { get; set; }
    [Parameter] public string? ModelKodu { get; set; }
}
