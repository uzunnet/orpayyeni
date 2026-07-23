using VizitLink3D.Ortak.Modeller;
using VizitLink3D.UI.Servisler;
using VizitLink3D.Ortak.Yardimcilar;
using Microsoft.AspNetCore.Components;

namespace VizitLink3D.UI.Bilesenler;

public partial class HeroSlider : ComponentBase, IDisposable
{
    [Parameter] public string SayfaKodu { get; set; } = "anasayfa";

    private List<Slayt> _slaytlar = [];
    private readonly List<Slayt> _varsayilanSlaytlar =
    [
        new Slayt
        {
            SayfaKodu = "anasayfa",
            Dil = "tr",
            Baslik = "ORPAY",
            AltBaslik = "Endüstriyel Lüks",
            Aciklama = "Sistem şu anda yerel yedek hero ile çalışıyor. API bağlandığında gerçek slaytlar otomatik olarak devreye girer.",
            ArkaplanResim = "",
            ButonMetni1 = "Koleksiyonu Keşfet",
            ButonLink1 = "/banyo-dolaplari",
            ButonMetni2 = "İletişim",
            ButonLink2 = "/iletisim",
            SiraNo = 1,
            AktifMi = true
        }
    ];
    private bool _yukleniyor = true;
    private int _aktifIndex;
    private Timer? _zamanlayici;

    protected override async Task OnInitializedAsync()
    {
        dil.DilDegisti += DilDegistiginde;
        await SlaytlariYukleAsync();
    }

    private async void DilDegistiginde()
    {
        await SlaytlariYukleAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task SlaytlariYukleAsync()
    {
        _slaytlar = [];
        try
        {
            var slaytListesi = await api.GetAsync<List<Slayt>>($"api/slaytlar?dil={dil.AktifDil}&sayfaKodu={SayfaKodu}");
            if (slaytListesi is { Count: > 0 })
            {
                _slaytlar = slaytListesi.Where(s => s.AktifMi).OrderBy(s => s.SiraNo).ToList();
            }
        }
        catch
        {
            // API gecici olarak yoksa lokal yedek hero kullan.
            _slaytlar = [];
        }
        finally
        {
            if (_slaytlar.Count == 0)
                _slaytlar = _varsayilanSlaytlar.ToList();

            _yukleniyor = false;
            if (_slaytlar.Count > 1)
            {
                _zamanlayici?.Dispose();
                _zamanlayici = new Timer(async _ => await SonrakiSlayt(), null, 5000, 5000);
            }
        }
    }

    private void SlaytaGit(int index)
    {
        _aktifIndex = index;
        StateHasChanged();
    }

    private string ResimUrl(string? yol)
    {
        var guncelYol = AnaSayfaSlaytYolu.Guncelle(yol);
        if (string.IsNullOrWhiteSpace(guncelYol)) return string.Empty;
        if (guncelYol.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return guncelYol;
        return $"{api.ApiBaseUrl}{(guncelYol.StartsWith('/') ? guncelYol : "/" + guncelYol)}";
    }

    private async Task SonrakiSlayt()
    {
        _aktifIndex = (_aktifIndex + 1) % _slaytlar.Count;
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        _zamanlayici?.Dispose();
    }
}

