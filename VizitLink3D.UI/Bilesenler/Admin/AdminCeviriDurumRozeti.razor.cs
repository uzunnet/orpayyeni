using VizitLink3D.UI.Servisler;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace VizitLink3D.UI.Bilesenler.Admin;

public partial class AdminCeviriDurumRozeti : ComponentBase
{
    [Parameter] public AdminCeviriKayitDurumu? Durum { get; set; }

    private string Aciklama => Durum is null || Durum.Detaylar.Count == 0
        ? dil.T("admin.ceviri.tumDillerGuncel", "Tum diller guncel.")
        : string.Join(" | ", Durum.Detaylar.Select(x => $"{x.DilKodu.ToUpperInvariant()} {x.AlanAdi}: {DurumEtiketi(x.Durum)}"));

    private string DurumEtiketi(AdminCeviriDurumu durum) => durum switch
    {
        AdminCeviriDurumu.Guncel => dil.T("admin.ceviri.guncel", "Guncel"),
        AdminCeviriDurumu.Eksik => dil.T("admin.ceviri.eksik", "Eksik"),
        AdminCeviriDurumu.KaynakDegisti => dil.T("admin.ceviri.kaynakDegisti", "Kaynak Degisti"),
        AdminCeviriDurumu.Hata => dil.T("admin.ceviri.hata", "Hata"),
        _ => dil.T("ortak.bilinmiyor", "Bilinmiyor")
    };

    private static Color DurumRenk(AdminCeviriDurumu durum) => durum switch
    {
        AdminCeviriDurumu.Guncel => Color.Success,
        AdminCeviriDurumu.Eksik => Color.Info,
        AdminCeviriDurumu.KaynakDegisti => Color.Warning,
        AdminCeviriDurumu.Hata => Color.Error,
        _ => Color.Default
    };
}
