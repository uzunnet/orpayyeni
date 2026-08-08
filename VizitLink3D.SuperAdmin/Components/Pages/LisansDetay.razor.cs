using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using VizitLink3D.SuperAdmin.VeriTabani;

namespace VizitLink3D.SuperAdmin.Components.Pages;

[Authorize]
public partial class LisansDetay : ComponentBase
{
    [Inject] private SuperAdminDbContext Vt { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private NavigationManager Gezinme { get; set; } = null!;

    [Parameter] public int Id { get; set; }

    private bool _yukleniyor = true;
    private bool _kaydediyor;
    private bool _yenileniyor;
    private LisansDuzenlemeModel? _lisans;
    private DateTime? _baslangicTarihi;
    private DateTime? _bitisTarihi;

    protected override async Task OnInitializedAsync()
    {
        await YukleAsync();
    }

    private async Task YukleAsync()
    {
        _yukleniyor = true;

        var kayit = await Vt.Lisanslar
            .Include(l => l.Firma)
            .FirstOrDefaultAsync(l => l.Id == Id);

        if (kayit is null)
        {
            _lisans = null;
        }
        else
        {
            _lisans = new LisansDuzenlemeModel
            {
                Id = kayit.Id,
                FirmaAd = kayit.Firma?.Ad ?? "Bilinmiyor",
                BirincilDomain = kayit.BirincilDomain,
                YedekDomain = kayit.YedekDomain,
                LisansTipi = kayit.LisansTipi,
                SureYil = kayit.SureYil,
                SuresizMi = kayit.SuresizMi,
                DemoMu = kayit.DemoMu,
                Notlar = kayit.Notlar,
                AktifMi = kayit.AktifMi
            };
            _baslangicTarihi = kayit.BaslangicTarihi;
            _bitisTarihi = kayit.BitisTarihi;
        }

        _yukleniyor = false;
    }

    private async Task KaydetAsync()
    {
        if (_lisans is null) return;
        _kaydediyor = true;

        try
        {
            var kayit = await Vt.Lisanslar.FindAsync(Id);
            if (kayit is null)
            {
                Snackbar.Add("Lisans bulunamadı.", Severity.Error);
                return;
            }

            kayit.BirincilDomain = _lisans.BirincilDomain;
            kayit.YedekDomain = _lisans.YedekDomain;
            kayit.LisansTipi = _lisans.LisansTipi;
            kayit.SureYil = _lisans.SureYil;
            kayit.SuresizMi = _lisans.SuresizMi;
            kayit.DemoMu = _lisans.DemoMu;
            kayit.Notlar = _lisans.Notlar;
            kayit.AktifMi = _lisans.AktifMi;
            kayit.GuncellenmeTarihi = DateTime.UtcNow;

            if (_baslangicTarihi.HasValue)
                kayit.BaslangicTarihi = _baslangicTarihi.Value;
            if (_bitisTarihi.HasValue)
                kayit.BitisTarihi = _bitisTarihi.Value;

            await Vt.SaveChangesAsync();
            Snackbar.Add("Lisans başarıyla güncellendi.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Kaydetme hatası: {ex.Message}", Severity.Error);
        }
        finally
        {
            _kaydediyor = false;
        }
    }

    private async Task YenileAsync()
    {
        _yenileniyor = true;
        await YukleAsync();
        _yenileniyor = false;
        Snackbar.Add("Lisans bilgileri yenilendi.", Severity.Info);
    }

    private string HesaplaKalanGun()
    {
        if (_lisans?.SuresizMi == true) return "Süresiz";
        if (!_bitisTarihi.HasValue) return "-";
        var kalan = (_bitisTarihi.Value - DateTime.UtcNow).Days;
        if (kalan <= 0) return "Süresi Doldu";
        return $"{kalan} gün";
    }

    private class LisansDuzenlemeModel
    {
        public int Id { get; set; }
        public string FirmaAd { get; set; } = string.Empty;
        public string BirincilDomain { get; set; } = string.Empty;
        public string? YedekDomain { get; set; }
        public string LisansTipi { get; set; } = string.Empty;
        public int? SureYil { get; set; }
        public bool SuresizMi { get; set; }
        public bool DemoMu { get; set; }
        public string? Notlar { get; set; }
        public bool AktifMi { get; set; }
    }
}
