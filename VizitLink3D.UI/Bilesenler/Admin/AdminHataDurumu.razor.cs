using Microsoft.AspNetCore.Components;

namespace VizitLink3D.UI.Bilesenler.Admin;

public partial class AdminHataDurumu : ComponentBase
{
    [Parameter] public string Baslik { get; set; } = "";
    [Parameter] public string Mesaj { get; set; } = "";
    [Parameter] public string? TekrarDeneMetni { get; set; }
    [Parameter] public EventCallback TekrarDene { get; set; }
}
