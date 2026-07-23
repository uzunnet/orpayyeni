using VizitLink3D.Ortak.Modeller;
using VizitLink3D.UI.Servisler;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;

namespace VizitLink3D.UI.Pages.Admin;

public class DashboardOzeti
{
    public int ToplamUrun { get; set; }
    public int ToplamKapak { get; set; }
    public int ToplamUrunAilesi { get; set; }
    public int ToplamUrunKategori { get; set; }
    public int Toplam3DModel { get; set; }
    public int ToplamParca { get; set; }
    public int ToplamSlayt { get; set; }
    public int ToplamBlog { get; set; }
    public int ToplamSSS { get; set; }
    public int ToplamSayfa { get; set; }
    public int ToplamProje { get; set; }
    public int ToplamReferans { get; set; }
    public int ToplamYorum { get; set; }
    public int ToplamMedya { get; set; }
    public int ToplamKatalog { get; set; }
    public int BekleyenMesaj { get; set; }
    public int ToplamMesaj { get; set; }
    public int ToplamTeklif { get; set; }
    public int ToplamSube { get; set; }
    public int ToplamEkip { get; set; }
    public int ToplamMenu { get; set; }
    public int ToplamCeviri { get; set; }
    public int ToplamDil { get; set; }
    public int ToplamKullanici { get; set; }
    public int ToplamAI { get; set; }
    public int ToplamLog { get; set; }
    public int BekleyenIs { get; set; }
    public int KritikIs { get; set; }
    public int ToplamIs { get; set; }
    public int ToplamBulten { get; set; }
    public int ToplamEpostaSablon { get; set; }
    public int ToplamZiyaret { get; set; }
    public int BugunMesaj { get; set; }
    public int BugunTeklif { get; set; }
}

public partial class Dashboard : ComponentBase, IAsyncDisposable
{
    [Inject] private ApiIstemcisi Api { get; set; } = null!;
    [Inject] private BildirimServisi BildirimServisi { get; set; } = null!;
    [Inject] private KimlikServisi Kimlik { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private HubConnection? _hub;
    private CancellationTokenSource? _yenilemeIptal;
    private bool _canliBagli;
    private readonly List<EtkinlikOgesi> _etkinlikler = new();

    private DashboardOzeti? _ozet;
    private DashboardKomutaMerkezi? _komuta;
    private List<IsTakipKaydi> _isTakipListe = new();
    private bool _yukleniyor = true;
    private DateTime _sonYenileme = DateTime.UtcNow;

    private bool _isSuperAdmin;
    private string _rol = "";

    protected override async Task OnInitializedAsync()
    {
        var bilgi = await Kimlik.KullaniciBilgiGetirAsync();
        if (bilgi != null)
        {
            _rol = bilgi.Rol;
            _isSuperAdmin = bilgi.Rol == "SuperAdmin";
        }

        await VeriYukleAsync();
        await SignalRBaglanAsync();
        BildirimServisi.BildirimGeldi += CanliGuncelle;
        _yenilemeIptal = new CancellationTokenSource();
        _ = PeriyodikYenileAsync(_yenilemeIptal.Token);
    }

    private async Task VeriYukleAsync(bool sessiz = false)
    {
        if (!sessiz)
            _yukleniyor = true;

        try
        {
            _komuta = await Api.GetAsync<DashboardKomutaMerkezi>("api/dashboard/komuta-merkezi");
            _ozet = _komuta?.Ozet ?? await Api.GetAsync<DashboardOzeti>("api/dashboard/ozet");
            var isListe = await Api.GetAsync<List<IsTakipKaydi>>("api/is-takip?durum=Tumu");
            _isTakipListe = isListe?.Where(i => i.Durum != "Tamamlandi").OrderByDescending(i => i.Oncelik == "Kritik").Take(7).ToList() ?? [];
            _sonYenileme = DateTime.UtcNow;
        }
        catch { }

        if (!_etkinlikler.Any(e => e.Eylem == dil.T("admin.dashboard.yuklendi", "Dashboard yüklendi")))
            _etkinlikler.Add(new EtkinlikOgesi { Eylem = dil.T("admin.dashboard.yuklendi", "Dashboard yüklendi"), Kullanici = dil.T("admin.dashboard.sistem", "Sistem"), Tarih = DateTime.UtcNow, Tip = dil.T("admin.dashboard.bilgi", "bilgi") });

        _yukleniyor = false;
        await InvokeAsync(StateHasChanged);
    }

    private async Task PeriyodikYenileAsync(CancellationToken iptal)
    {
        using var zamanlayici = new PeriodicTimer(TimeSpan.FromSeconds(30));
        while (await zamanlayici.WaitForNextTickAsync(iptal))
            await VeriYukleAsync(true);
    }

    private async Task SignalRBaglanAsync()
    {
        try
        {
            _hub = new HubConnectionBuilder()
                .WithUrl($"{Api.ApiBaseUrl}/hubs/tema")
                .WithAutomaticReconnect()
                .Build();

            _hub.On<string>("TemaGuncellendi", _ => InvokeAsync(StateHasChanged));
            _hub.On<string, string, string>("AktiviteGeldi", (eylem, kullanici, tip) =>
            {
                _etkinlikler.Insert(0, new EtkinlikOgesi { Eylem = eylem, Kullanici = kullanici, Tarih = DateTime.UtcNow, Tip = tip, YeniMi = true });
                if (_etkinlikler.Count > 50) _etkinlikler.RemoveAt(_etkinlikler.Count - 1);
                InvokeAsync(StateHasChanged);
            });

            _hub.Reconnecting += _ => { _canliBagli = false; InvokeAsync(StateHasChanged); return Task.CompletedTask; };
            _hub.Reconnected += _ => { _canliBagli = true; InvokeAsync(StateHasChanged); return Task.CompletedTask; };

            await _hub.StartAsync();
            _canliBagli = true;
            StateHasChanged();
        }
        catch { _canliBagli = false; }
    }

    private async void CanliGuncelle() => await InvokeAsync(StateHasChanged);
    private int Maksimum(IEnumerable<int> degerler)
    {
        var liste = degerler.ToList();
        return liste.Count == 0 ? 1 : Math.Max(liste.Max(), 1);
    }

    private string BarGenisligi(int deger, int maksimum)
    {
        var oran = maksimum <= 0 ? 0 : Math.Clamp(deger * 100 / maksimum, 4, 100);
        return $"{oran}%";
    }

    private string ZamanMetni(DateTime tarih)
    {
        var fark = DateTime.UtcNow - tarih;
        if (fark.TotalMinutes < 1) return dil.T("admin.azOnce", "Az önce");
        if (fark.TotalHours < 1) return $"{(int)fark.TotalMinutes} {dil.T("admin.dakikaOnce", "dk önce")}";
        if (fark.TotalDays < 1) return $"{(int)fark.TotalHours} {dil.T("admin.saatOnce", "saat önce")}";
        return $"{(int)fark.TotalDays} {dil.T("admin.gunOnce", "gün önce")}";
    }

    private string YuzdeMetni(decimal deger) => $"{deger:0.#}%";

    private int IsTamamlamaOrani()
    {
        var toplam = Math.Max(_ozet?.ToplamIs ?? 0, 1);
        var bekleyen = _ozet?.BekleyenIs ?? 0;
        return Math.Clamp((toplam - bekleyen) * 100 / toplam, 0, 100);
    }

    private IEnumerable<HaritaNoktasi> HaritaNoktalari()
    {
        var liste = _komuta?.AktifZiyaretciler.Take(10).ToList() ?? [];
        if (liste.Count == 0)
        {
            return
            [
                new("48%", "42%"),
                new("55%", "46%"),
                new("33%", "50%"),
                new("69%", "57%")
            ];
        }

        return liste.Select((_, i) => new HaritaNoktasi($"{22 + (i * 13 % 60)}%", $"{24 + (i * 17 % 50)}%"));
    }

    private string DonutStili()
    {
        var liste = _komuta?.TrafikKaynaklari.Take(5).ToList() ?? [];
        if (liste.Count == 0)
            return "background: conic-gradient(#2f80ed 0 42%, #56cc7b 42% 70%, #f2a33a 70% 86%, #ef5b86 86% 100%);";

        var renkler = new[] { "#2f80ed", "#56cc7b", "#f2a33a", "#ef5b86", "#8b5cf6" };
        decimal baslangic = 0;
        var dilimler = new List<string>();
        for (var i = 0; i < liste.Count; i++)
        {
            var bitis = baslangic + liste[i].Oran;
            dilimler.Add($"{renkler[i % renkler.Length]} {baslangic:0.#}% {bitis:0.#}%");
            baslangic = bitis;
        }

        if (baslangic < 100)
            dilimler.Add($"rgba(255,255,255,0.08) {baslangic:0.#}% 100%");

        return $"background: conic-gradient({string.Join(", ", dilimler)});";
    }

    private string TrendNoktaStili(int indeks, int deger, int maksimum)
    {
        var sol = indeks * 16;
        var yukseklik = maksimum <= 0 ? 20 : Math.Clamp(deger * 78 / maksimum, 10, 78);
        return $"left:{sol}%;bottom:{yukseklik}%;";
    }

    public async ValueTask DisposeAsync()
    {
        BildirimServisi.BildirimGeldi -= CanliGuncelle;
        if (_yenilemeIptal != null)
        {
            await _yenilemeIptal.CancelAsync();
            _yenilemeIptal.Dispose();
        }
        if (_hub != null) await _hub.DisposeAsync();
    }

    public class EtkinlikOgesi
    {
        public string Eylem { get; set; } = "";
        public string Kullanici { get; set; } = "";
        public DateTime Tarih { get; set; }
        public string Tip { get; set; } = "bilgi";
        public bool YeniMi { get; set; }
        // Zaman metni razor'da hesaplanir (dil.T() gerektirir)
    }

    public class DashboardKomutaMerkezi
    {
        public DashboardOzeti Ozet { get; set; } = new();
        public int BugunZiyaret { get; set; }
        public int AktifZiyaretci { get; set; }
        public int AylikZiyaret { get; set; }
        public decimal DonusumOrani { get; set; }
        public List<ZiyaretciAnlikDto> AktifZiyaretciler { get; set; } = [];
        public List<SayfaIlgiDto> EnCokGezilenSayfalar { get; set; } = [];
        public List<UrunIlgiDto> EnCokIlgiGorenUrunler { get; set; } = [];
        public List<DagilimDto> TarayiciDagilimi { get; set; } = [];
        public List<DagilimDto> CihazDagilimi { get; set; } = [];
        public List<DagilimDto> TrafikKaynaklari { get; set; } = [];
        public List<GirisKaydiDto> SonGirisler { get; set; } = [];
        public List<DenetimAkisiDto> DenetimAkisi { get; set; } = [];
        public List<TrafikTrendDto> GunlukTrend { get; set; } = [];
        public SistemSagligiDto Sistem { get; set; } = new();
    }

    public class ZiyaretciAnlikDto
    {
        public string Kimlik { get; set; } = "";
        public string IpAdresi { get; set; } = "";
        public string Sayfa { get; set; } = "";
        public string HamSayfa { get; set; } = "";
        public string Tarayici { get; set; } = "";
        public string Cihaz { get; set; } = "";
        public string Konum { get; set; } = "";
        public DateTime SonGorulme { get; set; }
        public int ZiyaretSayisi { get; set; }
    }

    public class SayfaIlgiDto
    {
        public string Sayfa { get; set; } = "";
        public string Url { get; set; } = "";
        public int Ziyaret { get; set; }
        public int TekilZiyaretci { get; set; }
        public DateTime SonZiyaret { get; set; }
    }

    public class UrunIlgiDto
    {
        public int UrunId { get; set; }
        public string Ad { get; set; } = "";
        public string Kod { get; set; } = "";
        public string Slug { get; set; } = "";
        public int Ziyaret { get; set; }
        public int TekilZiyaretci { get; set; }
        public int Teklif { get; set; }
        public int IlgiPuani { get; set; }
        public DateTime? SonZiyaret { get; set; }
    }

    public class DagilimDto
    {
        public string Ad { get; set; } = "";
        public int Adet { get; set; }
        public decimal Oran { get; set; }
    }

    public class GirisKaydiDto
    {
        public string Kullanici { get; set; } = "";
        public string Rol { get; set; } = "";
        public string IpAdresi { get; set; } = "";
        public DateTime Zaman { get; set; }
    }

    public class DenetimAkisiDto
    {
        public string Eylem { get; set; } = "";
        public string Kullanici { get; set; } = "";
        public string IpAdresi { get; set; } = "";
        public string Tarayici { get; set; } = "";
        public DateTime Zaman { get; set; }
    }

    public class TrafikTrendDto
    {
        public string Etiket { get; set; } = "";
        public DateTime Tarih { get; set; }
        public int Ziyaret { get; set; }
        public int Teklif { get; set; }
        public int Mesaj { get; set; }
    }

    public class SistemSagligiDto
    {
        public string Api { get; set; } = "";
        public string Veritabani { get; set; } = "";
        public string CanliBaglanti { get; set; } = "";
        public string Lisans { get; set; } = "";
        public DateTime SonGuncelleme { get; set; }
    }

    private record HaritaNoktasi(string Sol, string Ust);
}
