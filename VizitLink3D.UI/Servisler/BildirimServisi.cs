using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;

namespace VizitLink3D.UI.Servisler;

public class BildirimServisi : IAsyncDisposable
{
    private HubConnection? _baglanti;
    private readonly ISnackbar _snackbar;
    private readonly NavigationManager _nav;
    private readonly string _hubUrl;

    public event Action? BildirimGeldi;
    public int BekleyenSayisi { get; private set; }

    public BildirimServisi(ISnackbar snackbar, NavigationManager nav, ApiIstemcisi api)
    {
        _snackbar = snackbar;
        _nav = nav;
        _hubUrl = $"{api.ApiBaseUrl}/hubs/bildirim";
    }

    public async Task BaslatAsync()
    {
        _baglanti = new HubConnectionBuilder()
            .WithUrl(_hubUrl)
            .WithAutomaticReconnect()
            .Build();

        _baglanti.On<Bildirim>("BildirimGeldi", bildirim =>
        {
            var severity = bildirim.Tur switch
            {
                "hata" => Severity.Error,
                "uyari" => Severity.Warning,
                "basarili" => Severity.Success,
                _ => Severity.Info
            };

            _snackbar.Add($"{bildirim.Baslik}: {bildirim.Mesaj}", severity, cfg =>
            {
                cfg.VisibleStateDuration = 5000;
                cfg.ShowCloseIcon = true;
            });

            BekleyenSayisi++;
            BildirimGeldi?.Invoke();
        });

        await _baglanti.StartAsync();
    }

    public async Task DurdurAsync()
    {
        if (_baglanti != null)
            await _baglanti.DisposeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await DurdurAsync();
    }

    public class Bildirim
    {
        public string Baslik { get; set; } = "";
        public string Mesaj { get; set; } = "";
        public string Tur { get; set; } = "bilgi";
        public DateTime Zaman { get; set; }
    }
}
