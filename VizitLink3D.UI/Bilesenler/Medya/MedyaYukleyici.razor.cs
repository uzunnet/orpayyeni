using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Medya;
using VizitLink3D.UI.Servisler;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace VizitLink3D.UI.Bilesenler.Medya;

public partial class MedyaYukleyici : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    [Parameter] public EventCallback<List<long>> DosyalarYuklendi { get; set; }
    [Parameter] public string HedefUrl { get; set; } = "api/medya/yukle";
    [Parameter] public string KabulEdilenTurler { get; set; } = "image/*,.glb,.gltf,.pdf,.mp4,.webm,.mp3,.wav";

    private bool _yukleniyor;
    private bool _surukleniyorMu;
    private int _ilerlemeYuzdesi;
    private int _yuklenen;
    private int _toplamDosya;
    private readonly List<string> _hatalar = new();

    private async Task DosyalarDegisti(IReadOnlyList<IBrowserFile> dosyalar)
    {
        if (!dosyalar.Any()) return;

        _yukleniyor = true;
        _hatalar.Clear();
        _toplamDosya = dosyalar.Count;
        _yuklenen = 0;

        var yuklenenIdler = new List<long>();

        foreach (var dosya in dosyalar)
        {
            try
            {
                using var icerik = new MultipartFormDataContent();
                using var akis = dosya.OpenReadStream(100_000_000);
                var dosyaIcerigi = new StreamContent(akis);
                icerik.Add(dosyaIcerigi, "dosya", dosya.Name);

                _ilerlemeYuzdesi = (int)((_yuklenen / (double)_toplamDosya) * 100);
                StateHasChanged();

                var cevap = await Api.PostMultipartAsync<Ortak.Modeller.Medya.Medya>(HedefUrl, icerik);

                if (cevap?.BasariliMi == true && cevap.Veri != null)
                {
                    yuklenenIdler.Add(cevap.Veri.Id);
                }
                else
                {
                    _hatalar.Add($"'{dosya.Name}': {cevap?.Mesaj ?? "Yükleme başarısız"}");
                }
            }
            catch (Exception ex)
            {
                _hatalar.Add($"'{dosya.Name}': {ex.Message}");
            }

            _yuklenen++;
        }

        _ilerlemeYuzdesi = _hatalar.Any() ? 0 : 100;

        if (yuklenenIdler.Any())
        {
            Snackbar.Add($"{yuklenenIdler.Count} dosya yüklendi", Severity.Success);
            await DosyalarYuklendi.InvokeAsync(yuklenenIdler);
        }

        if (_hatalar.Any())
        {
            Snackbar.Add($"{_hatalar.Count} dosya yüklenemedi", Severity.Warning);
        }

        _yukleniyor = false;
    }
}
