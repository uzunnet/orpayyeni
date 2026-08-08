using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using VizitLink3D.SuperAdmin.VeriTabani;

namespace VizitLink3D.SuperAdmin.Components.Pages;

[Authorize]
public partial class KullaniciYonetimi : ComponentBase
{
    [Inject] private SuperAdminDbContext Vt { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private IDialogService DialogServisi { get; set; } = null!;

    private bool _yukleniyor = true;
    private bool _ekleniyor;
    private List<SuperAdminKullanici> _kullanicilar = new();
    private YeniKullaniciModel _yeniKullanici = new();

    protected override async Task OnInitializedAsync()
    {
        await YukleAsync();
    }

    private async Task YukleAsync()
    {
        _kullanicilar = await Vt.SuperAdminKullanicilar
            .OrderByDescending(k => k.OlusturulmaTarihi)
            .ToListAsync();
        _yukleniyor = false;
    }

    private async Task YeniKullaniciEkleAsync()
    {
        if (string.IsNullOrWhiteSpace(_yeniKullanici.KullaniciAdi) ||
            string.IsNullOrWhiteSpace(_yeniKullanici.AdSoyad) ||
            string.IsNullOrWhiteSpace(_yeniKullanici.Sifre))
        {
            Snackbar.Add("Tüm alanları doldurun.", Severity.Warning);
            return;
        }

        _ekleniyor = true;

        try
        {
            var mevcut = await Vt.SuperAdminKullanicilar
                .AnyAsync(k => k.KullaniciAdi == _yeniKullanici.KullaniciAdi);

            if (mevcut)
            {
                Snackbar.Add($"\"{_yeniKullanici.KullaniciAdi}\" kullanıcı adı zaten kullanılıyor.", Severity.Error);
                return;
            }

            var kullanici = new SuperAdminKullanici
            {
                KullaniciAdi = _yeniKullanici.KullaniciAdi,
                AdSoyad = _yeniKullanici.AdSoyad,
                SifreHash = BCrypt.Net.BCrypt.HashPassword(_yeniKullanici.Sifre),
                AktifMi = true,
                OlusturulmaTarihi = DateTime.UtcNow
            };

            Vt.SuperAdminKullanicilar.Add(kullanici);
            await Vt.SaveChangesAsync();

            Snackbar.Add($"\"{kullanici.AdSoyad}\" başarıyla eklendi.", Severity.Success);
            _yeniKullanici = new YeniKullaniciModel();
            await YukleAsync();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Ekleme hatası: {ex.Message}", Severity.Error);
        }
        finally
        {
            _ekleniyor = false;
        }
    }

    private async Task SilOnayAsync(SuperAdminKullanici kullanici)
    {
        var sonuc = await DialogServisi.ShowMessageBoxAsync(
            "Onay",
            $"\"{kullanici.AdSoyad}\" kullanıcısını devre dışı bırakmak istediğinize emin misiniz?",
            yesText: "Evet, Devre Dışı Bırak",
            cancelText: "İptal");

        if (sonuc == true)
        {
            kullanici.AktifMi = false;
            await Vt.SaveChangesAsync();
            Snackbar.Add($"\"{kullanici.AdSoyad}\" devre dışı bırakıldı.", Severity.Success);
            await YukleAsync();
        }
    }

    private class YeniKullaniciModel
    {
        public string KullaniciAdi { get; set; } = string.Empty;
        public string AdSoyad { get; set; } = string.Empty;
        public string Sifre { get; set; } = string.Empty;
    }
}
