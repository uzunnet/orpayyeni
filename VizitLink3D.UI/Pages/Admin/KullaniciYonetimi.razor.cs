using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages.Admin;

public partial class KullaniciYonetimi : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogServisi { get; set; } = default!;

    private List<Kullanici> _liste = [];
    private List<Kullanici> _filtreliListe = [];
    private bool _yukleniyor = true;
    private bool _formAcik;
    private bool _duzenlemeModu;
    private bool _kaydediliyor;
    private Kullanici _form = new();
    private int? _duzenlenenId;
    private string _arama = string.Empty;
    private string _yeniSifre = string.Empty;

    protected override async Task OnInitializedAsync() => await Yukle();

    async Task Yukle()
    {
        _yukleniyor = true;
        StateHasChanged();
        _liste = await Api.GetAsync<List<Kullanici>>("api/kullanicilar") ?? [];
        AramaUygula();
        _yukleniyor = false;
    }

    void AramaYap(KeyboardEventArgs e) => AramaUygula();

    void AramaUygula()
    {
        var a = _arama?.ToLower() ?? "";
        _filtreliListe = string.IsNullOrWhiteSpace(a) ? _liste :
            _liste.Where(x =>
                (x.KullaniciAdi?.ToLower().Contains(a) ?? false) ||
                (x.AdSoyad?.ToLower().Contains(a) ?? false) ||
                (x.Eposta?.ToLower().Contains(a) ?? false)).ToList();
    }

    void YeniAc()
    {
        _form = new Kullanici { Rol = Rol.Kullanici, AktifMi = true };
        _duzenlenenId = null;
        _duzenlemeModu = false;
        _yeniSifre = string.Empty;
        _formAcik = true;
    }

    void Duzenle(Kullanici k)
    {
        _form = new Kullanici
        {
            Id = k.Id,
            KullaniciAdi = k.KullaniciAdi,
            AdSoyad = k.AdSoyad,
            Eposta = k.Eposta,
            Telefon = k.Telefon,
            Rol = k.Rol,
            AktifMi = k.AktifMi,
            EmailDogrulandiMi = k.EmailDogrulandiMi
        };
        _duzenlenenId = k.Id;
        _duzenlemeModu = true;
        _yeniSifre = string.Empty;
        _formAcik = true;
    }

    async Task SilOnay(Kullanici k)
    {
        var onay = await DialogServisi.ShowMessageBoxAsync(
            "Silme Onayı",
            $"'{k.AdSoyad}' kullanıcısı kalıcı olarak silinecektir. Bu işlem geri alınamaz.\n\nEmin misiniz?",
            yesText: "Evet, Sil",
            cancelText: "İptal");
        if (onay == true) await Sil(k);
    }

    async Task Sil(Kullanici k)
    {
        await Api.DeleteAsync($"api/kullanicilar/{k.Id}");
        Snackbar.Add($"'{k.AdSoyad}' başarıyla silindi.", Severity.Success);
        await Yukle();
    }

    async Task Kaydet()
    {
        if (string.IsNullOrWhiteSpace(_form.KullaniciAdi) || string.IsNullOrWhiteSpace(_form.Eposta))
        {
            Snackbar.Add("Kullanıcı adı ve e-posta zorunludur.", Severity.Warning);
            return;
        }

        if (!_duzenlemeModu && string.IsNullOrWhiteSpace(_yeniSifre))
        {
            Snackbar.Add("Yeni kullanıcı için şifre zorunludur.", Severity.Warning);
            return;
        }

        _kaydediliyor = true;
        try
        {
            if (_duzenlenenId.HasValue)
                await Api.PutAsync<Kullanici>($"api/kullanicilar/{_duzenlenenId.Value}", _form);
            else
                await Api.PostAsync<Kullanici>("api/kullanicilar", _form);

            _formAcik = false;
            Snackbar.Add(_duzenlenenId.HasValue ? "Kullanıcı güncellendi." : "Yeni kullanıcı eklendi.", Severity.Success);
            await Yukle();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Hata: {ex.Message}", Severity.Error);
        }
        finally
        {
            _kaydediliyor = false;
        }
    }

    void FormIptal() { _formAcik = false; }

    string RolAdi(Rol r) => r switch
    {
        Rol.SuperAdmin => "Super Admin",
        Rol.Admin => "Admin",
        Rol.Editor => "Editör",
        _ => "Kullanıcı"
    };

    Color RolRengi(Rol r) => r switch
    {
        Rol.SuperAdmin => Color.Error,
        Rol.Admin => Color.Warning,
        Rol.Editor => Color.Info,
        _ => Color.Default
    };
}

