using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace VizitLink3D.UI.Bilesenler.Admin;

public partial class MedyaAlaniDialogu : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    [Parameter] public Ortak.Modeller.Medya.MedyaTipi? IzinliTip { get; set; }
    [Parameter] public string YuklemeUcNokta { get; set; } = "api/medya/yukle";

    private int _aktifSekme;
    private List<Ortak.Modeller.Medya.Medya> _medyaListesi = [];
    private string _arama = "";
    private string _tipFiltre = "";
    private bool _yukleniyor = true;
    private bool _dosyaYukleniyor;
    private string? _secilenYol;
    private Ortak.Modeller.Medya.Medya? _secilenMedya;

    private string KabulEdilenTurler => IzinliTip switch
    {
        Ortak.Modeller.Medya.MedyaTipi.Resim => "image/*",
        Ortak.Modeller.Medya.MedyaTipi.Glb   => ".glb,.gltf",
        Ortak.Modeller.Medya.MedyaTipi.Pdf   => ".pdf",
        Ortak.Modeller.Medya.MedyaTipi.Video => ".mp4,.webm",
        _                                      => "image/*,.glb,.gltf,.pdf,.mp4,.webm"
    };

    private string KabulEdilenAciklama => IzinliTip switch
    {
        Ortak.Modeller.Medya.MedyaTipi.Resim => "JPG, PNG, WebP, SVG",
        Ortak.Modeller.Medya.MedyaTipi.Glb   => "GLB, GLTF (3D model)",
        Ortak.Modeller.Medya.MedyaTipi.Pdf   => "PDF",
        Ortak.Modeller.Medya.MedyaTipi.Video => "MP4, WebM",
        _                                      => "Resim, 3D (GLB), PDF, Video"
    };

    protected override async Task OnInitializedAsync()
    {
        _tipFiltre = IzinliTip switch
        {
            Ortak.Modeller.Medya.MedyaTipi.Resim => "Resim",
            Ortak.Modeller.Medya.MedyaTipi.Glb   => "Glb",
            Ortak.Modeller.Medya.MedyaTipi.Pdf   => "Pdf",
            Ortak.Modeller.Medya.MedyaTipi.Video => "Video",
            _                                      => ""
        };
        await MedyaYukleAsync();
    }

    private async Task MedyaYukleAsync()
    {
        _yukleniyor = true;
        var parcalar = new List<string>();
        if (!string.IsNullOrWhiteSpace(_arama))
            parcalar.Add($"q={Uri.EscapeDataString(_arama)}");
        if (!string.IsNullOrWhiteSpace(_tipFiltre))
            parcalar.Add($"tip={Uri.EscapeDataString(_tipFiltre)}");
        var url = "api/medya" + (parcalar.Count > 0 ? "?" + string.Join("&", parcalar) : "");
        _medyaListesi = await api.GetAsync<List<Ortak.Modeller.Medya.Medya>>(url) ?? [];
        _yukleniyor = false;
    }

    private void MedyaSecildi(Ortak.Modeller.Medya.Medya m)
    {
        _secilenMedya = m;
        _secilenYol = $"/api/medya/dosya/{m.Id}";
    }

    private async Task DosyaYuklendi(IReadOnlyList<IBrowserFile> dosyalar)
    {
        if (dosyalar.Count == 0) return;
        _dosyaYukleniyor = true;
        StateHasChanged();

        var d = dosyalar[0];
        using var ic = new MultipartFormDataContent();
        using var st = d.OpenReadStream(100_000_000);
        ic.Add(new StreamContent(st), "dosya", d.Name);

        var cevap = await api.PostMultipartAsync<Ortak.Modeller.Medya.Medya>(YuklemeUcNokta, ic);
        if (cevap?.BasariliMi == true && cevap.Veri != null)
        {
            _secilenMedya = cevap.Veri;
            _secilenYol = $"/api/medya/dosya/{cevap.Veri.Id}";
            snackbar.Add("Dosya yüklendi.", Severity.Success);
            await MedyaYukleAsync();
            _aktifSekme = 0;
        }
        else
        {
            snackbar.Add(cevap?.Mesaj ?? "Yükleme başarısız.", Severity.Error);
        }

        _dosyaYukleniyor = false;
    }

    private string OnizlemeSrc(string yol)
    {
        if (yol.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return yol;
        return $"{api.ApiBaseUrl}{(yol.StartsWith('/') ? yol : "/" + yol)}";
    }

    private bool SecilenResimMi() =>
        _secilenMedya?.Tip == Ortak.Modeller.Medya.MedyaTipi.Resim
        || (_secilenMedya == null && _secilenYol != null
            && (_secilenYol.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
             || _secilenYol.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
             || _secilenYol.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
             || _secilenYol.EndsWith(".webp", StringComparison.OrdinalIgnoreCase)
             || _secilenYol.EndsWith(".gif", StringComparison.OrdinalIgnoreCase)));

    private bool Secilen3DMi() =>
        _secilenMedya?.Tip == Ortak.Modeller.Medya.MedyaTipi.Glb
        || (_secilenMedya == null && _secilenYol != null
            && (_secilenYol.EndsWith(".glb", StringComparison.OrdinalIgnoreCase)
             || _secilenYol.EndsWith(".gltf", StringComparison.OrdinalIgnoreCase)));

    private bool SecilenPdfMi() =>
        _secilenMedya?.Tip == Ortak.Modeller.Medya.MedyaTipi.Pdf
        || (_secilenMedya == null && _secilenYol != null
            && _secilenYol.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase));

    private void Onayla()
    {
        if (_secilenYol != null)
            MudDialog.Close(DialogResult.Ok(_secilenYol));
    }

    private void Iptal() => MudDialog.Cancel();
}
