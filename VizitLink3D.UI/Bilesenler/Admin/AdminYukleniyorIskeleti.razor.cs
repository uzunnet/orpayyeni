using Microsoft.AspNetCore.Components;

namespace VizitLink3D.UI.Bilesenler.Admin;

public partial class AdminYukleniyorIskeleti : ComponentBase
{
    [Parameter] public int SatirSayisi { get; set; } = 5;
}
