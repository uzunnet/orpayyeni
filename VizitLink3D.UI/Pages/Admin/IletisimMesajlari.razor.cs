using VizitLink3D.UI.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace VizitLink3D.UI.Pages.Admin;

/// <summary>
/// IletisimMesajlari — Admin panelinde iletişim formundan gelen mesajları
/// gerçek zamanlı olarak veritabanından çekip gösteren sayfa.
/// Okundu işaretleme ve arşivleme işlemleri API üzerinden veritabanına yansır.
/// </summary>
public partial class IletisimMesajlari : ComponentBase
{
    [Inject] private VizitLink3D.UI.Servisler.AnimasyonMotoruServisi _animasyonMotoru { get; set; } = default!;

    private bool _yukleniyor = true;
    private List<IletisimMesajiDto> _mesajlar = [];
    private IletisimMesajiDto? _acikMesaj;
    private int _okunmamisSayi = 0;
    private bool _sadeceOkunmamis = false;
    private string _aramaMetni = string.Empty;

    private List<IletisimMesajiDto> _filtrelenmis =>
        _mesajlar
            .Where(m => !_sadeceOkunmamis || !m.OkunduMu)
            .Where(m => string.IsNullOrWhiteSpace(_aramaMetni)
                        || m.AdSoyad.Contains(_aramaMetni, StringComparison.OrdinalIgnoreCase)
                        || m.Eposta.Contains(_aramaMetni, StringComparison.OrdinalIgnoreCase)
                        || m.Konu.Contains(_aramaMetni, StringComparison.OrdinalIgnoreCase))
            .ToList();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await _animasyonMotoru.ScrollAnimasyonlariniBaslatAsync();
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await MesajlariYukleAsync();
    }

    /// <summary>
    /// API'den iletişim mesajlarını çeker ve okunmamış sayısını hesaplar.
    /// </summary>
    private async Task MesajlariYukleAsync()
    {
        _yukleniyor = true;
        var yanit = await api.GetAsync<MesajListesiDto>("api/iletisim/mesajlar");
        if (yanit != null)
        {
            _mesajlar = yanit.Mesajlar ?? [];
            _okunmamisSayi = _mesajlar.Count(m => !m.OkunduMu);
        }
        _yukleniyor = false;
    }

    /// <summary>
    /// Seçilen mesajı açar ve eğer okunmamışsa API üzerinden okundu olarak işaretler.
    /// Bu işlem anında veritabanına yansır.
    /// </summary>
    private async Task MesajiAc(IletisimMesajiDto mesaj)
    {
        _acikMesaj = mesaj;
        if (!mesaj.OkunduMu)
        {
            var yanit = await api.PatchAsync($"api/iletisim/mesajlar/{mesaj.Id}/okundu");
            if (yanit?.BasariliMi == true)
            {
                mesaj.OkunduMu = true;
                _okunmamisSayi = _mesajlar.Count(m => !m.OkunduMu);
            }
        }
    }

    /// <summary>
    /// Mesajı arşivler (görünümden kaldırır, veritabanında ArsinlenniMi=true olarak işaretlenir).
    /// </summary>
    private async Task MesajiArsiyle(IletisimMesajiDto mesaj)
    {
        var yanit = await api.DeleteAsync($"api/iletisim/mesajlar/{mesaj.Id}");
        if (yanit?.BasariliMi == true)
        {
            _mesajlar.Remove(mesaj);
            if (_acikMesaj?.Id == mesaj.Id) _acikMesaj = null;
            _okunmamisSayi = _mesajlar.Count(m => !m.OkunduMu);
            snackbar.Add("Mesaj arşivlendi.", Severity.Info);
        }
    }
}

