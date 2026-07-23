using VizitLink3D.Api.Servisler;
using VizitLink3D.Api.Servisler.Kimlik;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Kontrolculer.Sistem;

[ApiController]
[Route("api/lisans")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class LisansKontrolcu(VizitLink3DDbContext vt, KiraciServisi kiraci, LisansServisi lisansServisi, IConfiguration konfigurasyon) : ControllerBase
{
    [HttpGet("durum")]
    public async Task<IActionResult> DurumGetir()
    {
        var firma = await AktifFirmaGetirAsync();
        if (firma is null)
        {
            return NotFound(Cevap<bool>.Hata("Firma bulunamad�."));
        }

        var lisans = await AktifLisansGetirAsync(firma.Id);
        var domain = firma.Domain ?? HttpContext.Request.Host.Host;
        var durum = await lisansServisi.DomainKontrolAsync(domain.ToLowerInvariant());

        return Ok(Cevap<LisansDurumDto>.Basarili(LisansDurumDto.Olustur(firma, lisans, durum)));
    }

    [HttpPut("aktif-firma")]
    public async Task<IActionResult> AktifFirmaLisansiniKaydet([FromBody] LisansKaydetDto istek)
    {
        var firma = await AktifFirmaGetirAsync();
        if (firma is null)
        {
            return NotFound(Cevap<bool>.Hata("Firma bulunamad�."));
        }

        var baslangic = TarihUtcYap(istek.BaslangicTarihi ?? DateTime.UtcNow);
        var tip = LisansServisi.TipNormalizeEt(istek.LisansTipi);
        var plan = LisansServisi.PlanCoz(tip, baslangic, istek.BitisTarihi is null ? null : TarihUtcYap(istek.BitisTarihi.Value));
        var domain = DomainTemizle(istek.BirincilDomain) ?? firma.Domain ?? HttpContext.Request.Host.Host;
        var yedekDomain = DomainTemizle(istek.YedekDomain) ?? firma.YedekDomain;

        var lisans = await AktifLisansGetirAsync(firma.Id);
        if (lisans is null)
        {
            lisans = new Lisans
            {
                FirmaId = firma.Id,
                OlusturulmaTarihi = DateTime.UtcNow
            };
            vt.Lisanslar.Add(lisans);
        }

        lisans.BirincilDomain = domain.ToLowerInvariant();
        lisans.YedekDomain = string.IsNullOrWhiteSpace(yedekDomain) ? null : yedekDomain.ToLowerInvariant();
        lisans.BaslangicTarihi = baslangic;
        lisans.BitisTarihi = plan.bitisTarihi;
        lisans.LisansTipi = tip;
        lisans.SureYil = plan.sureYil;
        lisans.DemoMu = plan.demoMu;
        lisans.SuresizMi = plan.suresizMi;
        lisans.AktifMi = istek.AktifMi;
        lisans.SonDogrulamaTarihi = DateTime.UtcNow;
        lisans.GuncellenmeTarihi = DateTime.UtcNow;
        lisans.LisansAnahtari = LisansAnahtariOlustur(firma, lisans);

        firma.DemoMu = plan.demoMu;
        firma.GuncellenmeTarihi = DateTime.UtcNow;

        await vt.SaveChangesAsync();

        var durum = await lisansServisi.DomainKontrolAsync(lisans.BirincilDomain);
        return Ok(Cevap<LisansDurumDto>.Basarili(LisansDurumDto.Olustur(firma, lisans, durum), "Lisans kaydedildi."));
    }

    private async Task<Firma?> AktifFirmaGetirAsync()
    {
        if (kiraci.MevcutFirmaId is int firmaId)
        {
            var firma = await vt.Firmalar.FirstOrDefaultAsync(f => f.Id == firmaId && f.AktifMi);
            if (firma is not null)
            {
                return firma;
            }
        }

        return await vt.Firmalar
            .OrderByDescending(f => f.Slug == "orpay")
            .ThenBy(f => f.Id)
            .FirstOrDefaultAsync(f => f.AktifMi);
    }

    private async Task<Lisans?> AktifLisansGetirAsync(int firmaId)
        => await vt.Lisanslar
            .OrderByDescending(l => l.AktifMi)
            .ThenByDescending(l => l.Id)
            .FirstOrDefaultAsync(l => l.FirmaId == firmaId);

    private string LisansAnahtariOlustur(Firma firma, Lisans lisans)
    {
        var gizliAnahtar = Environment.GetEnvironmentVariable("VIZITLINK3D_LISANS_KEY")
            ?? konfigurasyon["LisansAyarlari:GizliAnahtar"]
            ?? string.Empty;

        return string.IsNullOrWhiteSpace(gizliAnahtar)
            ? string.Empty
            : LisansServisi.AnahtarUret(firma.Slug, lisans.BirincilDomain, lisans.BitisTarihi, gizliAnahtar);
    }

    private static DateTime TarihUtcYap(DateTime tarih)
        => tarih.Kind == DateTimeKind.Utc ? tarih : DateTime.SpecifyKind(tarih, DateTimeKind.Utc);

    private static string? DomainTemizle(string? domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
        {
            return null;
        }

        return domain.Trim()
            .Replace("https://", "", StringComparison.OrdinalIgnoreCase)
            .Replace("http://", "", StringComparison.OrdinalIgnoreCase)
            .Trim('/')
            .ToLowerInvariant();
    }
}

public sealed class LisansKaydetDto
{
    public string LisansTipi { get; set; } = LisansServisi.Yillik;
    public string? BirincilDomain { get; set; }
    public string? YedekDomain { get; set; }
    public DateTime? BaslangicTarihi { get; set; }
    public DateTime? BitisTarihi { get; set; }
    public bool AktifMi { get; set; } = true;
}

public sealed record LisansDurumDto(
    int FirmaId,
    string FirmaAdi,
    string FirmaSlug,
    bool FirmaDemoMu,
    bool LisansVarMi,
    bool GecerliMi,
    bool AktifMi,
    string LisansTipi,
    int? SureYil,
    bool DemoMu,
    bool SuresizMi,
    string BirincilDomain,
    string? YedekDomain,
    DateTime? BaslangicTarihi,
    DateTime? BitisTarihi,
    int? KalanGun,
    bool EkSuredeMi,
    string Mesaj)
{
    public static LisansDurumDto Olustur(Firma firma, Lisans? lisans, LisansDurumu durum)
        => new(
            firma.Id,
            firma.Ad,
            firma.Slug,
            firma.DemoMu,
            lisans is not null,
            durum.GecerliMi,
            lisans?.AktifMi ?? false,
            lisans?.LisansTipi ?? string.Empty,
            lisans?.SureYil,
            lisans?.DemoMu ?? false,
            lisans?.SuresizMi ?? durum.SuresizMi,
            lisans?.BirincilDomain ?? firma.Domain ?? string.Empty,
            lisans?.YedekDomain,
            lisans?.BaslangicTarihi,
            durum.SuresizMi ? null : lisans?.BitisTarihi,
            durum.SuresizMi ? null : durum.KalanGun,
            durum.EkSuredeMi,
            string.IsNullOrWhiteSpace(durum.Sebep) ? "Lisans ge�erli." : durum.Sebep);
}

