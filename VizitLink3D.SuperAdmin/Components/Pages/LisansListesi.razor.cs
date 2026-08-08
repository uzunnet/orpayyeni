using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.SuperAdmin.VeriTabani;

namespace VizitLink3D.SuperAdmin.Components.Pages;

[Authorize]
public partial class LisansListesi : ComponentBase
{
    [Inject] private SuperAdminDbContext Vt { get; set; } = null!;

    private bool _yukleniyor = true;
    private string _firmaFiltresi = string.Empty;
    private string _tipFiltresi = "Tümü";
    private List<LisansSatir> _tumLisanslar = new();
    private List<LisansSatir> _filtrelenmisLisanslar = new();

    protected override async Task OnInitializedAsync()
    {
        var lisanslar = await Vt.Lisanslar
            .Include(l => l.Firma)
            .OrderByDescending(l => l.OlusturulmaTarihi)
            .ToListAsync();

        _tumLisanslar = lisanslar.Select(l => new LisansSatir
        {
            Id = l.Id,
            FirmaAd = l.Firma?.Ad ?? "Bilinmiyor",
            Domain = l.BirincilDomain,
            LisansTipi = l.LisansTipi,
            SureYil = l.SureYil,
            SuresizMi = l.SuresizMi,
            BaslangicTarihi = l.BaslangicTarihi,
            BitisTarihi = l.BitisTarihi,
            AktifMi = l.AktifMi
        }).ToList();

        _filtrelenmisLisanslar = new List<LisansSatir>(_tumLisanslar);
        _yukleniyor = false;
    }

    private void FiltreUygula()
    {
        var sonuc = _tumLisanslar.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(_firmaFiltresi))
        {
            sonuc = sonuc.Where(l =>
                l.FirmaAd.Contains(_firmaFiltresi, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(_tipFiltresi) && _tipFiltresi != "Tümü")
        {
            sonuc = sonuc.Where(l =>
                l.LisansTipi.Equals(_tipFiltresi, StringComparison.OrdinalIgnoreCase));
        }

        _filtrelenmisLisanslar = sonuc.ToList();
        StateHasChanged();
    }

    private class LisansSatir
    {
        public int Id { get; set; }
        public string FirmaAd { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string LisansTipi { get; set; } = string.Empty;
        public int? SureYil { get; set; }
        public bool SuresizMi { get; set; }
        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }
        public bool AktifMi { get; set; }
    }
}
