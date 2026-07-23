using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Medya;
using VizitLink3D.UI.Servisler;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace VizitLink3D.UI.Bilesenler.Medya;

public partial class MedyaSecici : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    [Parameter] public bool CokluSecim { get; set; }
    [Parameter] public List<long>? MevcutSeciliIdler { get; set; }
    [Parameter] public MedyaTipi? BaslangicTipi { get; set; }

    private List<Ortak.Modeller.Medya.Medya> _medyaListesi = new();
    private HashSet<long> SeciliIdler = new();
    private string _arama = string.Empty;
    private string _filtreTipStr = string.Empty;
    private bool _yukleniyor = true;

    private MedyaTipi? AktifFiltreTip =>
        _filtreTipStr switch
        {
            "resim" => MedyaTipi.Resim,
            "video" => MedyaTipi.Video,
            "pdf" => MedyaTipi.Pdf,
            "glb" => MedyaTipi.Glb,
            _ => null
        };

    protected override async Task OnInitializedAsync()
    {
        _filtreTipStr = BaslangicTipi switch
        {
            MedyaTipi.Resim => "resim",
            MedyaTipi.Video => "video",
            MedyaTipi.Pdf => "pdf",
            MedyaTipi.Glb => "glb",
            _ => ""
        };

        if (MevcutSeciliIdler?.Any() == true)
            foreach (var id in MevcutSeciliIdler)
                SeciliIdler.Add(id);

        await MedyaListesiniYukleAsync();
    }

    private async Task MedyaListesiniYukleAsync()
    {
        _yukleniyor = true;
        StateHasChanged();

        var tip = AktifFiltreTip;
        var sorguParcalari = new List<string>();
        if (!string.IsNullOrWhiteSpace(_arama))
            sorguParcalari.Add($"q={Uri.EscapeDataString(_arama)}");
        if (tip.HasValue)
            sorguParcalari.Add($"tip={tip.Value}"); // API "Resim", "Video" vb. enum adı bekliyor

        var url = "api/medya" + (sorguParcalari.Any() ? $"?{string.Join("&", sorguParcalari)}" : "");
        var liste = await Api.GetAsync<List<Ortak.Modeller.Medya.Medya>>(url);
        _medyaListesi = liste ?? new();
        _yukleniyor = false;
    }

    private async void AramaYapildi()
    {
        await MedyaListesiniYukleAsync();
    }

    private async void MedyaSec(Ortak.Modeller.Medya.Medya medya)
    {
        if (CokluSecim)
        {
            if (SeciliIdler.Contains(medya.Id))
                SeciliIdler.Remove(medya.Id);
            else
                SeciliIdler.Add(medya.Id);
        }
        else
        {
            SeciliIdler.Clear();
            SeciliIdler.Add(medya.Id);
        }
    }

    private async void YeniDosyalarYuklendi(List<long> yeniIdler)
    {
        if (yeniIdler.Any())
        {
            foreach (var id in yeniIdler)
                SeciliIdler.Add(id);

            await MedyaListesiniYukleAsync();
        }
    }

    private void SecimiOnayla()
    {
        if (SeciliIdler.Any() && MudDialog != null)
            MudDialog.Close(DialogResult.Ok(SeciliIdler.ToList()));
    }

    private void Iptal()
    {
        MudDialog?.Cancel();
    }
}
