using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.SuperAdmin.VeriTabani;

namespace VizitLink3D.SuperAdmin.Components.Pages;

[Authorize]
public partial class Home : ComponentBase
{
    [Inject] private SuperAdminDbContext Vt { get; set; } = null!;

    private bool _yukleniyor = true;
    private int _toplamFirma;
    private int _aktifFirma;
    private int _demoFirma;
    private int _toplamModul;
    private int _toplamLisans;
    private int _yaklasanBitis;
    private List<Firma> _sonFirmalar = new();
    private List<LisansBilgisi> _yaklasanLisanslar = new();

    protected override async Task OnInitializedAsync()
    {
        _toplamFirma = await Vt.Firmalar.CountAsync();
        _aktifFirma = await Vt.Firmalar.CountAsync(f => f.AktifMi && !f.DemoMu);
        _demoFirma = await Vt.Firmalar.CountAsync(f => f.DemoMu);
        _toplamModul = await Vt.Moduller.CountAsync();

        var suAn = DateTimeOffset.UtcNow;
        _toplamLisans = await Vt.SuperAdminLisansKayitlari.CountAsync(l => l.AktifMi);
        _yaklasanBitis = await Vt.SuperAdminLisansKayitlari.CountAsync(l =>
            l.AktifMi && l.BitisTarihi > suAn && l.BitisTarihi <= suAn.AddDays(30));

        _sonFirmalar = await Vt.Firmalar
            .OrderByDescending(f => f.OlusturulmaTarihi)
            .Take(10)
            .ToListAsync();

        _yaklasanLisanslar = await Vt.SuperAdminLisansKayitlari
            .Where(l => l.AktifMi && l.BitisTarihi > suAn && l.BitisTarihi <= suAn.AddDays(90))
            .OrderBy(l => l.BitisTarihi)
            .Select(l => new LisansBilgisi
            {
                FirmaAdi = Vt.Firmalar.Where(f => f.Id == l.FirmaId).Select(f => f.Ad).FirstOrDefault() ?? "Bilinmiyor",
                BitisTarihi = l.BitisTarihi,
                KalanGun = (l.BitisTarihi - suAn).Days
            })
            .ToListAsync();

        _yukleniyor = false;
    }

    public class LisansBilgisi
    {
        public string FirmaAdi { get; set; } = string.Empty;
        public DateTimeOffset BitisTarihi { get; set; }
        public int KalanGun { get; set; }
    }
}
