using Microsoft.AspNetCore.Components;

namespace VizitLink3D.UI.Pages;

public partial class DinamikSayfaGosterici
{
    [Parameter] public string Slug { get; set; } = string.Empty;

    private string Baslik => Slug switch
    {
        "vizyon-misyon" => "Vizyon ve Misyon",
        "kalite-politikasi" => "Kalite Politikasi",
        "cerez-politikasi" => "Cerez Politikasi",
        "gizlilik" => "Gizlilik Politikasi",
        _ => "Sayfa Hazirlaniyor"
    };

    private string Aciklama => "Bu rota yeni ORPAY public yapisinda temiz bir sayfa katmani ile yeniden kurulacak.";
}
