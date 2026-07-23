using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.SignalR.Client;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages.Admin
{
    public partial class CanliSohbet : ComponentBase
    {
        private List<SohbetKullanicisi> _kullanicilar = new();
        private List<SohbetMesaji> _mesajlar = new();
        private SohbetKullanicisi? _seciliKullanici;
        private string _yeniMesaj = string.Empty;

        [Inject] IJSRuntime JS { get; set; } = default!;
        [Inject] NavigationManager Navigation { get; set; } = default!;
        [Inject] ApiIstemcisi Api { get; set; } = default!;
        [Inject] IConfiguration Konfig { get; set; } = default!;

        // 'dil' ve 'api' zaten _Imports.razor üzerinden inject ediliyor.
        // Buradaki 'Api' büyük harfli olduğu için çakışma yapmaz ve koddaki kullanımları destekler.

        private HubConnection? _hubBaglantisi;

        protected override async Task OnInitializedAsync()
        {
            await OturumlariYukle();

            // API Base Address'i ApiIstemcisi üzerinden alıyoruz
            var apiBaseUrl = Api.BaseAddress?.ToString() ?? "http://localhost:5015"; 
            
            _hubBaglantisi = new HubConnectionBuilder()
                .WithUrl($"{apiBaseUrl.TrimEnd('/')}/hubs/sohbet")
                .WithAutomaticReconnect()
                .Build();

            _hubBaglantisi.On<SohbetMesaji>("YeniMesajGeldi", async (mesaj) =>
            {
                var kullanici = _kullanicilar.FirstOrDefault(k => k.OturumId == mesaj.OturumId);
                if (kullanici == null)
                {
                    kullanici = new SohbetKullanicisi 
                    { 
                        OturumId = mesaj.OturumId, 
                        Ad = mesaj.GonderenAd, 
                        SonMesaj = mesaj.MesajMetni, 
                        CevrimiciMi = true, 
                        OkunmayanSayisi = 1 
                    };
                    _kullanicilar.Insert(0, kullanici);
                }
                else
                {
                    kullanici.SonMesaj = mesaj.MesajMetni;
                    if (_seciliKullanici?.OturumId != mesaj.OturumId)
                        kullanici.OkunmayanSayisi++;
                    
                    // Listede en üste taşı
                    _kullanicilar.Remove(kullanici);
                    _kullanicilar.Insert(0, kullanici);
                }

                if (_seciliKullanici?.OturumId == mesaj.OturumId)
                {
                    _mesajlar.Add(mesaj);
                    await AsagiKaydir();
                }
                
                await InvokeAsync(StateHasChanged);
            });

            _hubBaglantisi.On<SohbetMesaji>("MesajIletildi", async (mesaj) =>
            {
                if (_seciliKullanici?.OturumId == mesaj.OturumId)
                {
                    if (!_mesajlar.Any(m => m.Id == mesaj.Id))
                    {
                        _mesajlar.Add(mesaj);
                        await AsagiKaydir();
                        await InvokeAsync(StateHasChanged);
                    }
                }
            });

            try
            {
                await _hubBaglantisi.StartAsync();
                await _hubBaglantisi.SendAsync("YoneticiOlarakBaglan");
            }
            catch
            {
                /* Sohbet Hub bağlantısı başarısız olursa sayfa yine de yüklenir */
            }
        }

        private async Task OturumlariYukle()
        {
            try
            {
                var veriler = await Api.GetAsync<List<SohbetKullanicisi>>("api/sohbet/oturumlar");
                if (veriler != null)
                {
                    _kullanicilar = veriler;
                }
            }
            catch
            {
                /* Oturum verisi çekilemezse boş listeyle devam edilir */
            }
        }

        private async Task SohbetSec(SohbetKullanicisi kullanici)
        {
            _seciliKullanici = kullanici;
            kullanici.OkunmayanSayisi = 0;
            
            try
            {
                var veriler = await Api.GetAsync<List<SohbetMesaji>>($"api/sohbet/gecmis/{kullanici.OturumId}");
                if (veriler != null)
                {
                    _mesajlar = veriler;
                }
            }
            catch
            {
                /* Geçmiş verisi çekilemezse boş listeyle devam edilir */
            }
            
            await AsagiKaydir();
        }

        private async Task MesajGonder()
        {
            if (string.IsNullOrWhiteSpace(_yeniMesaj) || _seciliKullanici == null || _hubBaglantisi == null) return;

            var gidenMesaj = _yeniMesaj;
            _yeniMesaj = string.Empty;

            try
            {
                await _hubBaglantisi.SendAsync("YoneticiMesajGonder", _seciliKullanici.OturumId, gidenMesaj);
                _seciliKullanici.SonMesaj = gidenMesaj;
            }
            catch
            {
                /* Mesaj gönderilemezse kullanıcı tekrar deneyebilir */
            }

            await AsagiKaydir();
        }

        private async Task KlavyeDinle(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await MesajGonder();
            }
        }

        private async Task AsagiKaydir()
        {
            await Task.Delay(50);
            try {
                await JS.InvokeVoidAsync("sohbetAlaniEnAltaKaydir");
            } catch { /* JS scroll animasyonu başarısız olursa sohbet işlevselliği etkilenmez */ }
        }

        public class SohbetKullanicisi
        {
            public string OturumId { get; set; } = string.Empty;
            public string Ad { get; set; } = string.Empty;
            public string SonMesaj { get; set; } = string.Empty;
            public bool CevrimiciMi { get; set; }
            public int OkunmayanSayisi { get; set; }
        }

        public class SohbetMesaji
        {
            public int Id { get; set; }
            public string OturumId { get; set; } = string.Empty;
            public string GonderenAd { get; set; } = string.Empty;
            public string MesajMetni { get; set; } = string.Empty;
            public bool YoneticiMi { get; set; }
            public DateTime Tarih { get; set; } = DateTime.UtcNow;
            public bool OkunduMu { get; set; }
            
            // UI helper
            public bool BenimMi => YoneticiMi;
            public string Metin => MesajMetni;
        }
    }
}
