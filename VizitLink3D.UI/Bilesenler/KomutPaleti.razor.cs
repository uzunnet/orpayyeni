using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace VizitLink3D.UI.Bilesenler;

public partial class KomutPaleti : ComponentBase
{
    // dil, nav, js — _Imports.razor'dan geliyor
    private bool _gorunur;
    private string _aramaMetni = "";
    private List<KomutSonucu> _sonuclar = new();
    private KomutSonucu? _seciliSonuc;
    private List<KomutSonucu> _tumKomutlar = new();

    public class KomutSonucu
    {
        public string Baslik { get; set; } = "";
        public string Aciklama { get; set; } = "";
        public string Ikon { get; set; } = Icons.Material.Filled.ChevronRight;
        public Color IkonRengi { get; set; } = Color.Default;
        public string? Url { get; set; }
        public Action? Eylem { get; set; }
    }

    protected override void OnInitialized()
    {
        _tumKomutlar = new List<KomutSonucu>
        {
            new() { Baslik = dil.T("admin.dashboard", "Dashboard"), Aciklama = "/admin/dashboard", Ikon = Icons.Material.Filled.Dashboard, IkonRengi = Color.Primary, Url = "/admin/dashboard" },
            new() { Baslik = dil.T("admin.kapi", "Kapı Yönetimi"), Aciklama = "/admin/urun-yonetimi", Ikon = Icons.Material.Filled.DoorFront, Url = "/admin/urun-yonetimi" },
            new() { Baslik = dil.T("admin.proje", "Proje Yönetimi"), Aciklama = "/admin/proje-yonetimi", Ikon = Icons.Material.Filled.Construction, Url = "/admin/proje-yonetimi" },
            new() { Baslik = dil.T("admin.slayt", "Slayt Yönetimi"), Aciklama = "/admin/slayt-yonetimi", Ikon = Icons.Material.Filled.Slideshow, Url = "/admin/slayt-yonetimi" },
            new() { Baslik = dil.T("admin.menu", "Menü Yönetimi"), Aciklama = "/admin/menu-yonetimi", Ikon = Icons.Material.Filled.Menu, Url = "/admin/menu-yonetimi" },
            new() { Baslik = dil.T("admin.iletisim", "İletişim Mesajları"), Aciklama = "/admin/iletisim-mesajlari", Ikon = Icons.Material.Filled.Email, Url = "/admin/iletisim-mesajlari" },
            new() { Baslik = dil.T("admin.sohbet", "Canlı Sohbet"), Aciklama = "/admin/canli-sohbet", Ikon = Icons.Material.Filled.Chat, Url = "/admin/canli-sohbet" },
            new() { Baslik = dil.T("admin.kullanici", "Kullanıcı Yönetimi"), Aciklama = "/admin/kullanici-yonetimi", Ikon = Icons.Material.Filled.People, Url = "/admin/kullanici-yonetimi" },
            new() { Baslik = dil.T("admin.ayarlar", "Ayarlar"), Aciklama = "/admin/ayarlar", Ikon = Icons.Material.Filled.Settings, Url = "/admin/ayarlar" },
            new() { Baslik = dil.T("admin.tema", "Tema Yönetimi"), Aciklama = "/admin/tema-yonetimi", Ikon = Icons.Material.Filled.Palette, Url = "/admin/tema-yonetimi" },
            new() { Baslik = dil.T("admin.seo", "SEO Yönetimi"), Aciklama = "/admin/seo-yonetimi", Ikon = Icons.Material.Filled.TrendingUp, Url = "/admin/seo-yonetimi" },
            new() { Baslik = dil.T("admin.ceviri", "Çeviri Yönetimi"), Aciklama = "/admin/ceviri-yonetimi", Ikon = Icons.Material.Filled.Translate, Url = "/admin/ceviri-yonetimi" },
            new() { Baslik = dil.T("admin.medya", "Medya Galerisi"), Aciklama = "/admin/medya-galerisi", Ikon = Icons.Material.Filled.PhotoLibrary, Url = "/admin/medya-galerisi" },
            new() { Baslik = dil.T("admin.medya.youtubeEkle", "YouTube Ekle"), Aciklama = "/admin/medya-youtube-ekle", Ikon = Icons.Material.Filled.PlayCircle, Url = "/admin/medya-youtube-ekle" },
            new() { Baslik = dil.T("admin.blog", "Haber Yönetimi"), Aciklama = "/admin/haber-yonetimi", Ikon = Icons.Material.Filled.Article, Url = "/admin/haber-yonetimi" },
            new() { Baslik = dil.T("admin.referans", "Referans Yönetimi"), Aciklama = "/admin/referans-yonetimi", Ikon = Icons.Material.Filled.Handshake, Url = "/admin/referans-yonetimi" },
            new() { Baslik = dil.T("menu.anasayfa", "Anasayfa"), Aciklama = "/", Ikon = Icons.Material.Filled.Home, IkonRengi = Color.Success, Url = "/" },
        };
    }

    public void Ac()
    {
        _gorunur = true;
        _aramaMetni = "";
        _sonuclar = _tumKomutlar;
        _seciliSonuc = _sonuclar.FirstOrDefault();
        StateHasChanged();
    }

    private void Kapat()
    {
        _gorunur = false;
        _aramaMetni = "";
        _sonuclar.Clear();
        _seciliSonuc = null;
    }

    private void AramaYap(KeyboardEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_aramaMetni))
        {
            _sonuclar = _tumKomutlar;
        }
        else
        {
            var arama = _aramaMetni.ToLowerInvariant();
            _sonuclar = _tumKomutlar
                .Where(k => k.Baslik.Contains(arama, StringComparison.OrdinalIgnoreCase) ||
                            k.Aciklama.Contains(arama, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        _seciliSonuc = _sonuclar.FirstOrDefault();
    }

    private void KlavyeYakala(KeyboardEventArgs e)
    {
        switch (e.Key)
        {
            case "Escape":
                Kapat();
                break;
            case "ArrowDown":
                if (_sonuclar.Any() && _seciliSonuc != null)
                {
                    var idx = _sonuclar.IndexOf(_seciliSonuc);
                    _seciliSonuc = _sonuclar[Math.Min(idx + 1, _sonuclar.Count - 1)];
                }
                break;
            case "ArrowUp":
                if (_sonuclar.Any() && _seciliSonuc != null)
                {
                    var idx = _sonuclar.IndexOf(_seciliSonuc);
                    _seciliSonuc = _sonuclar[Math.Max(idx - 1, 0)];
                }
                break;
            case "Enter":
                if (_seciliSonuc != null) Sec(_seciliSonuc);
                break;
        }
    }

    private void Sec(KomutSonucu sonuc)
    {
        if (!string.IsNullOrEmpty(sonuc.Url))
            nav.NavigateTo(sonuc.Url);
        else
            sonuc.Eylem?.Invoke();
        Kapat();
    }
}
