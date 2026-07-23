using Microsoft.AspNetCore.Components;

namespace VizitLink3D.UI.Bilesenler.Medya;

public partial class MedyaKart : ComponentBase
{
    [Parameter] public Ortak.Modeller.Medya.Medya MedyaOgesi { get; set; } = default!;
    [Parameter] public bool SeciliMi { get; set; }
    [Parameter] public EventCallback<Ortak.Modeller.Medya.Medya> Tiklandi { get; set; }

    private string MedyaBoyutu()
    {
        if (MedyaOgesi.BoyutByte >= 1_048_576)
            return $"{(MedyaOgesi.BoyutByte / 1_048_576.0):F1} MB";
        if (MedyaOgesi.BoyutByte >= 1024)
            return $"{MedyaOgesi.BoyutByte / 1024} KB";
        return $"{MedyaOgesi.BoyutByte} B";
    }
}
