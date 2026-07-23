using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.SignalR.Client;
using VizitLink3D.Ortak.Modeller;
using System.Net.Http.Json;

namespace VizitLink3D.UI.Bilesenler;

public partial class CanliSohbetArayuzu
{
    [Inject] public IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] public NavigationManager Navigation { get; set; } = default!;
    [Inject] public VizitLink3D.UI.Servisler.ApiIstemcisi Api { get; set; } = default!;
    [Inject] public IConfiguration Configuration { get; set; } = default!;

    private bool _acikMi = false;
    private string _yeniMesaj = "";
    private List<SohbetMesaji> _mesajlar = new();
    private HubConnection? _hubBaglantisi;
    private string _oturumId = "";

    protected override async Task OnInitializedAsync()
    {
        // LocalStorage'dan oturum ID'sini al veya yeni üret
        try {
            var kaydedilmisId = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "vizitlink3d_sohbet_oturum");
            if (string.IsNullOrEmpty(kaydedilmisId)) {
                _oturumId = Guid.NewGuid().ToString().Substring(0, 8);
                await JSRuntime.InvokeVoidAsync("localStorage.setItem", "vizitlink3d_sohbet_oturum", _oturumId);
            } else {
                _oturumId = kaydedilmisId;
            }
        } catch {
            _oturumId = Guid.NewGuid().ToString().Substring(0, 8);
        }

        var apiBaseUrl = Api.ApiBaseUrl;

        _hubBaglantisi = new HubConnectionBuilder()
            .WithUrl($"{apiBaseUrl}/hubs/sohbet")
            .WithAutomaticReconnect()
            .Build();

        _hubBaglantisi.On<CanliSohbetMesaji>("YeniMesajGeldi", (mesaj) =>
        {
            if (!_mesajlar.Any(m => m.Tarih == mesaj.Tarih && m.Metin == mesaj.MesajMetni))
            {
                _mesajlar.Add(new SohbetMesaji 
                { 
                    Metin = mesaj.MesajMetni, 
                    KullaniciMi = !mesaj.YoneticiMi,
                    Tarih = mesaj.Tarih
                });
                InvokeAsync(StateHasChanged);
            }
        });

        _hubBaglantisi.On<CanliSohbetMesaji>("MesajIletildi", (mesaj) =>
        {
            InvokeAsync(StateHasChanged);
        });

        try
        {
            await _hubBaglantisi.StartAsync();
            await _hubBaglantisi.SendAsync("MusteriOlarakBaglan", _oturumId);
            
            // Geçmişi yükle
            var gecmis = await Api.GetAsync<List<CanliSohbetMesaji>>($"api/sohbet/gecmis/{_oturumId}");
            if (gecmis != null)
            {
                _mesajlar = gecmis.Select(m => new SohbetMesaji {
                    Metin = m.MesajMetni,
                    KullaniciMi = !m.YoneticiMi,
                    Tarih = m.Tarih
                }).ToList();
            }
        }
        catch
        {
            /* Sohbet bağlantısı başarısız olursa sayfa yine de yüklenir */
        }
    }

    private void SohbetGoster() => _acikMi = true;
    private void SohbetGizle() => _acikMi = false;

    private async Task MesajGonder()
    {
        if (string.IsNullOrWhiteSpace(_yeniMesaj) || _hubBaglantisi == null) return;

        string mesajMetni = _yeniMesaj;
        _yeniMesaj = "";
        
        // Önce ekrana ekle (beklemeden)
        _mesajlar.Add(new SohbetMesaji { Metin = mesajMetni, KullaniciMi = true, Tarih = DateTime.UtcNow });
        StateHasChanged();

        try 
        {
            await _hubBaglantisi.SendAsync("MusteriMesajGonder", _oturumId, "Müşteri", mesajMetni);
        }
        catch
        {
            /* Mesaj gönderilemezse kullanıcı tekrar deneyebilir */
        }
    }

    private async Task KlavyeDinle(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await MesajGonder();
        }
    }

    private class SohbetMesaji
    {
        public string Metin { get; set; } = "";
        public bool KullaniciMi { get; set; }
        public DateTime Tarih { get; set; } = DateTime.UtcNow;
    }

    public class CanliSohbetMesaji
    {
        public int Id { get; set; }
        public string OturumId { get; set; } = string.Empty;
        public string GonderenAd { get; set; } = string.Empty;
        public string MesajMetni { get; set; } = string.Empty;
        public bool YoneticiMi { get; set; }
        public DateTime Tarih { get; set; } = DateTime.UtcNow;
        public bool OkunduMu { get; set; }
    }
}

