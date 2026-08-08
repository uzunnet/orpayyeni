using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.SuperAdmin.Servisler;
using VizitLink3D.SuperAdmin.VeriTabani;

namespace VizitLink3D.SuperAdmin.Components.Pages;

[Authorize]
public partial class FirmaOlustur : ComponentBase
{
    [Inject] private SuperAdminDbContext Vt { get; set; } = null!;
    [Inject] private FirmaOlusturmaServisi FirmaServisi { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private NavigationManager Gezinme { get; set; } = null!;

    private Firma _model = new() { AktifMi = true, MaxKullaniciSayisi = 5 };
    private List<Modul> _moduller = new();
    private Dictionary<int, bool> _seciliModulIdleri = new();
    private bool _olusuyor;

    protected override async Task OnInitializedAsync()
    {
        _moduller = Vt.Moduller.OrderBy(m => m.Kategori).ThenBy(m => m.Ad).ToList();

        foreach (var modul in _moduller)
        {
            _seciliModulIdleri[modul.Id] = modul.VarsayilanMi || modul.SistemModuluMu;
        }
    }

    private async Task Olustur()
    {
        _olusuyor = true;

        try
        {
            var mevcut = Vt.Firmalar.Any(f => f.Slug == _model.Slug);
            if (mevcut)
            {
                Snackbar.Add($"\"{_model.Slug}\" slug'ı zaten kullanılıyor.", Severity.Error);
                _olusuyor = false;
                return;
            }

            _model.OlusturulmaTarihi = DateTime.UtcNow;
            Vt.Firmalar.Add(_model);
            await Vt.SaveChangesAsync();

            var seciliModuller = _seciliModulIdleri.Where(kv => kv.Value).Select(kv => kv.Key).ToList();
            foreach (var modulId in seciliModuller)
            {
                Vt.FirmaModulAtamalari.Add(new FirmaModulAtama
                {
                    FirmaId = _model.Id,
                    ModulId = modulId,
                    AtanmaTarihi = DateTimeOffset.UtcNow
                });
            }
            await Vt.SaveChangesAsync();

            var sonuc = await FirmaServisi.FirmaAltyapisiniOlustur(
                _model.Slug, _model.Id, _model.Ad,
                _model.Domain ?? $"{_model.Slug}.com",
                _model.PaketTipi ?? "Yillik");

            if (!sonuc)
                Snackbar.Add("Firma kaydedildi ancak klasör/veritabanı oluşturulamadı.", Severity.Warning);

            Snackbar.Add($"\"{_model.Ad}\" firması oluşturuldu!", Severity.Success);
            Gezinme.NavigateTo("/firmalar");
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Hata: {ex.Message}", Severity.Error);
        }
        finally
        {
            _olusuyor = false;
        }
    }
}
