using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Renkler;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace VizitLink3D.Api.Moduller.Urunler.Kontrolcüler;

[ApiController]
[Route("api/renkler/ral")]
public class RalRenkKontrolcu(VizitLink3DDbContext vt) : ControllerBase
{
    private const string VarsayilanYuzeyTipi = "Mat";
    private static readonly Regex HexKodDeseni = new("^#[0-9A-F]{6}$", RegexOptions.Compiled);

    [HttpGet]
    public async Task<Cevap<List<RalRengi>>> Liste()
    {
        var liste = await vt.RalRenkleri
            .AsNoTracking()
            .OrderBy(r => r.KatalogId)
            .ThenBy(r => r.SiraNo)
            .ToListAsync();

        return Cevap<List<RalRengi>>.Basarili(liste);
    }

    [HttpGet("{id:int}")]
    public async Task<Cevap<RalRengi>> Detay(int id)
    {
        var renk = await vt.RalRenkleri
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (renk is null)
            return Cevap<RalRengi>.Hata("RAL rengi bulunamadi.");

        return Cevap<RalRengi>.Basarili(renk);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<RalRengi>> Olustur([FromBody] RalRengi renk)
    {
        var dogrulama = DogrulaVeDuzenle(renk);
        if (!dogrulama.BasariliMi)
            return dogrulama;

        var mevcut = await vt.RalRenkleri
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Kod == renk.Kod);

        if (mevcut is { SilindiMi: false })
            return Cevap<RalRengi>.Hata("Bu RAL kodu zaten kayitli.");

        if (mevcut is not null)
        {
            mevcut.Ad = renk.Ad;
            mevcut.HexKod = renk.HexKod;
            mevcut.Grup = renk.Grup;
            mevcut.KatalogId = renk.KatalogId;
            mevcut.SiraNo = renk.SiraNo;
            mevcut.YuzeyTipi = renk.YuzeyTipi;
            mevcut.AktifMi = renk.AktifMi;
            mevcut.SilindiMi = false;
            mevcut.SilinmeTarihi = null;
            mevcut.GuncellenmeTarihi = DateTime.UtcNow;

            await vt.SaveChangesAsync();
            return Cevap<RalRengi>.Basarili(mevcut, "RAL rengi yeniden aktif edildi.");
        }

        renk.OlusturulmaTarihi = DateTime.UtcNow;
        renk.GuncellenmeTarihi = null;
        renk.SilindiMi = false;
        renk.SilinmeTarihi = null;

        vt.RalRenkleri.Add(renk);
        await vt.SaveChangesAsync();

        return Cevap<RalRengi>.Basarili(renk, "RAL rengi olusturuldu.");
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<RalRengi>> Guncelle(int id, [FromBody] RalRengi renk)
    {
        var dogrulama = DogrulaVeDuzenle(renk);
        if (!dogrulama.BasariliMi)
            return dogrulama;

        var mevcut = await vt.RalRenkleri.FindAsync(id);
        if (mevcut is null)
            return Cevap<RalRengi>.Hata("RAL rengi bulunamadi.");

        var kodBaskaKayittaVar = await vt.RalRenkleri
            .AsNoTracking()
            .AnyAsync(r => r.Id != id && r.Kod == renk.Kod);

        if (kodBaskaKayittaVar)
            return Cevap<RalRengi>.Hata("Bu RAL kodu baska bir kayitta kullaniliyor.");

        mevcut.Kod = renk.Kod;
        mevcut.Ad = renk.Ad;
        mevcut.HexKod = renk.HexKod;
        mevcut.Grup = renk.Grup;
        mevcut.KatalogId = renk.KatalogId;
        mevcut.SiraNo = renk.SiraNo;
        mevcut.YuzeyTipi = renk.YuzeyTipi;
        mevcut.AktifMi = renk.AktifMi;
        mevcut.GuncellenmeTarihi = DateTime.UtcNow;

        await vt.SaveChangesAsync();
        return Cevap<RalRengi>.Basarili(mevcut, "RAL rengi guncellendi.");
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<bool>> Sil(int id)
    {
        var mevcut = await vt.RalRenkleri.FindAsync(id);
        if (mevcut is null)
            return Cevap<bool>.Hata("RAL rengi bulunamadi.");

        mevcut.SilindiMi = true;
        mevcut.SilinmeTarihi = DateTime.UtcNow;
        mevcut.GuncellenmeTarihi = DateTime.UtcNow;
        await vt.SaveChangesAsync();
        return Cevap<bool>.Basarili(true, "RAL rengi silindi.");
    }

    private static Cevap<RalRengi> DogrulaVeDuzenle(RalRengi renk)
    {
        renk.Kod = renk.Kod.Trim().ToUpperInvariant();
        renk.Ad = renk.Ad.Trim();
        renk.HexKod = renk.HexKod?.Trim().ToUpperInvariant();
        renk.Grup = string.IsNullOrWhiteSpace(renk.Grup) ? null : renk.Grup.Trim();
        renk.YuzeyTipi = string.IsNullOrWhiteSpace(renk.YuzeyTipi)
            ? VarsayilanYuzeyTipi
            : renk.YuzeyTipi.Trim();

        if (string.IsNullOrWhiteSpace(renk.Kod) || string.IsNullOrWhiteSpace(renk.Ad) || string.IsNullOrWhiteSpace(renk.HexKod))
            return Cevap<RalRengi>.Hata("Kod, ad ve hex kod alanlari zorunludur.");

        if (!HexKodDeseni.IsMatch(renk.HexKod))
            return Cevap<RalRengi>.Hata("Hex kod #RRGGBB formatinda olmalidir.");

        return Cevap<RalRengi>.Basarili(renk);
    }
}
