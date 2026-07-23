using Microsoft.AspNetCore.Components;
using MudBlazor;
using VizitLink3D.UI.Models;
using VizitLink3D.UI.Servisler;
using VizitLink3D.Ortak.Modeller;
using System.Text.Json;
using System.Net.Http.Json;

namespace VizitLink3D.UI.Pages.Admin;

public partial class MenuYonetimi : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogServisi { get; set; } = default!;

    private List<MenuOgesi> _menuler = new();
    private List<MenuOgesi> _kokMenuler = new();
    private bool _yukleniyor = true;
    private string _konumFiltresi = "AdminSol";
    private static readonly HashSet<string> GecerliKonumlar =
    [
        "PublicHeader",
        "PublicMobil",
        "PublicFooterHizli",
        "PublicFooterKategori",
        "AdminSol",
        "AdminUst"
    ];

    private MenuOgesi? _aktifMenu;
    private MudForm _form = default!;
    private bool _kaydediliyor = false;

    protected override async Task OnInitializedAsync()
    {
        await MenuleriYukleAsync();
    }

    private async Task MenuleriYukleAsync()
    {
        _yukleniyor = true;
        var liste = await Api.GetAsync<List<MenuOgesi>>("api/menu");
        if (liste != null)
        {
            _menuler = liste
                .Where(m => GecerliKonumlar.Contains(m.Konum))
                .OrderBy(m => m.Sira)
                .ToList();
            _kokMenuler = _menuler
                .Where(m => m.UstMenuId == null)
                .OrderBy(m => m.Sira)
                .ToList();
        }
        _yukleniyor = false;
    }

    private List<MenuOgesi> FiltreliKokMenuler()
    {
        if (_konumFiltresi == "Tum")
            return _kokMenuler;
        return _kokMenuler.Where(m => m.Konum == _konumFiltresi).ToList();
    }

    private void KonumSec(string konum)
    {
        _konumFiltresi = konum;
        if (_aktifMenu is { Id: 0 })
            _aktifMenu.Konum = konum;
    }

    private Variant KonumButonVaryanti(string konum)
        => _konumFiltresi == konum ? Variant.Filled : Variant.Outlined;

    private string AktifKonumAciklamasi()
        => _konumFiltresi switch
        {
            "PublicHeader" => dil.T("menu.yonetim.frontendUstMenuAciklama", "Frontend üst menü buradan yönetilir. Alt menü oluşturmak için kayıtta 'Üst Menü' alanından ana menüyü seçin."),
            "PublicMobil" => dil.T("menu.yonetim.mobilMenuAciklama", "Mobil görünümde açılan menü kayıtları bu konumdadır."),
            "PublicFooterHizli" => dil.T("menu.yonetim.footerMenuAciklama", "Footer hızlı bağlantı menüsü bu konumdan yönetilir."),
            "AdminSol" => dil.T("menu.yonetim.adminSolMenuAciklama", "Admin panel sol menüsü bu konumdan yönetilir."),
            _ => dil.T("menu.yonetim.tumKonumlarAciklama", "Tüm menü konumları listeleniyor; düzenleme için önce ilgili konumu seçmek daha güvenlidir.")
        };

    private void YeniMenuEkleBaslat()
    {
        var ayniKonumdakiSonSira = _menuler
            .Where(m => m.Konum == _konumFiltresi && m.UstMenuId == null)
            .Select(m => m.Sira)
            .DefaultIfEmpty(0)
            .Max();

        _aktifMenu = new MenuOgesi
        {
            Sira = ayniKonumdakiSonSira + 1,
            AktifMi = true,
            Konum = _konumFiltresi == "Tum" ? "AdminSol" : _konumFiltresi
        };
    }

    private void DuzenleBaslat(MenuOgesi menu)
    {
        _aktifMenu = new MenuOgesi
        {
            Id = menu.Id, Baslik = menu.Baslik, Url = menu.Url,
            AktifMi = menu.AktifMi, Sira = menu.Sira, UstMenuId = menu.UstMenuId,
            Konum = menu.Konum, Ikon = menu.Ikon, YeniSekmede = menu.YeniSekmede,
            GerekliRol = menu.GerekliRol, SuperAdminGerekliMi = menu.SuperAdminGerekliMi,
            YetkiAnahtari = menu.YetkiAnahtari, KilitliMi = menu.KilitliMi,
            SistemMenusuMu = menu.SistemMenusuMu
        };
    }

    private void FormIptal()
    {
        _aktifMenu = null;
    }

    private async Task FormKaydet()
    {
        await _form.ValidateAsync();
        if (!_form.IsValid || _aktifMenu == null) return;

        _kaydediliyor = true;

        if (_aktifMenu.Id == 0)
        {
            var yanit = await Api.PostAsync<MenuOgesi>("api/menu", _aktifMenu);
            if (yanit != null && yanit.BasariliMi)
            {
                Snackbar.Add(dil.T("menu.yonetim.eklendi", "Menu eklendi."), Severity.Success);
                _aktifMenu = null;
                await MenuleriYukleAsync();
            }
            else Snackbar.Add(dil.T("menu.yonetim.eklemeHatasi", "Menu eklenirken hata."), Severity.Error);
        }
        else
        {
            var yanit = await Api.PutAsync<MenuOgesi>($"api/menu/{_aktifMenu.Id}", _aktifMenu);
            if (yanit != null && yanit.BasariliMi)
            {
                Snackbar.Add(dil.T("menu.yonetim.guncellendi", "Menu guncellendi."), Severity.Success);
                _aktifMenu = null;
                await MenuleriYukleAsync();
            }
            else Snackbar.Add(dil.T("menu.yonetim.guncellemeHatasi", "Menu guncellenirken hata."), Severity.Error);
        }

        _kaydediliyor = false;
    }

    private async Task DurumDegistir(MenuOgesi menu)
    {
        var temizMenu = new MenuOgesi
        {
            Id = menu.Id, Baslik = menu.Baslik, Url = menu.Url,
            AktifMi = menu.AktifMi, Sira = menu.Sira, UstMenuId = menu.UstMenuId,
            Konum = menu.Konum, Ikon = menu.Ikon, YeniSekmede = menu.YeniSekmede,
            GerekliRol = menu.GerekliRol, SuperAdminGerekliMi = menu.SuperAdminGerekliMi,
            YetkiAnahtari = menu.YetkiAnahtari, KilitliMi = menu.KilitliMi,
            SistemMenusuMu = menu.SistemMenusuMu
        };

        var yanit = await Api.PutAsync<MenuOgesi>($"api/menu/{temizMenu.Id}", temizMenu);
        if (yanit != null && yanit.BasariliMi)
            Snackbar.Add(dil.T("menu.yonetim.durumGuncellendi", "Durum guncellendi."), Severity.Normal);
    }

    private async Task SilBaslat(MenuOgesi menu)
    {
        if (menu.KilitliMi)
        {
            Snackbar.Add(dil.T("menu.yonetim.kilitliSilinemez", "Bu menu kilitli, silinemez."), Severity.Warning);
            return;
        }

        var onay = await DialogServisi.ShowMessageBoxAsync(
            dil.T("ortak.silmeOnayi", "Silme Onayi"),
            string.Format(dil.T("menu.yonetim.silOnay", "'{0}' menusu devre disi birakilacaktir (soft delete). Emin misiniz?"), menu.Baslik),
            yesText: dil.T("menu.yonetim.devreDisiBirak", "Devre Disi Birak"),
            cancelText: dil.T("ortak.iptal", "İptal"));

        if (onay == true)
        {
            var yanit = await Api.DeleteAsync($"api/menu/{menu.Id}");
            if (yanit != null && yanit.BasariliMi)
            {
                Snackbar.Add(dil.T("menu.yonetim.devreDisiBirakildi", "Menu devre disi birakildi."), Severity.Success);
                if (_aktifMenu?.Id == menu.Id) _aktifMenu = null;
                await MenuleriYukleAsync();
            }
            else Snackbar.Add(dil.T("ortak.hataOlustu", "Hata olustu."), Severity.Error);
        }
    }

    private async Task SiraDegistir(MenuOgesi menu, int degisim)
    {
        menu.Sira = Math.Max(1, menu.Sira + degisim);
        var temizMenu = new MenuOgesi
        {
            Id = menu.Id, Baslik = menu.Baslik, Url = menu.Url,
            AktifMi = menu.AktifMi, Sira = menu.Sira, UstMenuId = menu.UstMenuId,
            Konum = menu.Konum, Ikon = menu.Ikon, YeniSekmede = menu.YeniSekmede,
            GerekliRol = menu.GerekliRol, SuperAdminGerekliMi = menu.SuperAdminGerekliMi,
            YetkiAnahtari = menu.YetkiAnahtari, KilitliMi = menu.KilitliMi,
            SistemMenusuMu = menu.SistemMenusuMu
        };

        var yanit = await Api.PutAsync<MenuOgesi>($"api/menu/{temizMenu.Id}", temizMenu);
        if (yanit != null && yanit.BasariliMi)
            await MenuleriYukleAsync();
    }

    private string IkonCevir(string? ikon)
    {
        if (string.IsNullOrEmpty(ikon)) return Icons.Material.Filled.Circle;
        try
        {
            var alan = typeof(Icons.Material.Filled).GetField(ikon);
            if (alan != null) return alan.GetValue(null)?.ToString() ?? Icons.Material.Filled.Circle;
        }
        catch { }
        return Icons.Material.Filled.Circle;
    }
}
