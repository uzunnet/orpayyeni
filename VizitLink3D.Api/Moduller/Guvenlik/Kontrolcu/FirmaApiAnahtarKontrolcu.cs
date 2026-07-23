using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Api.Moduller.Guvenlik.Dtolar;
using VizitLink3D.Api.Servisler;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Guvenlik;

namespace VizitLink3D.Api.Moduller.Guvenlik.Kontrolcu;

/// <summary>
/// Firma API anahtarı yönetim endpoint'leri.
/// Sadece Admin ve SuperAdmin yetkisine sahip JWT kullanıcıları erişebilir.
/// Tenant izolasyonu: Admin yalnız kendi firmasının anahtarlarını yönetir;
/// SuperAdmin tüm firmaların anahtarlarını görebilir ve yönetebilir.
/// </summary>
[ApiController]
[Route("api/firma/api-anahtarlari")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class FirmaApiAnahtarKontrolcu(
    VizitLink3DDbContext vt,
    KiraciServisi kiraciServisi,
    IHttpContextAccessor hca) : ControllerBase
{
    /// <summary>Firmaya ait tüm API anahtarlarını listele (düz metin ASLA gösterilmez). SuperAdmin tüm firmaları görür.</summary>
    [HttpGet]
    public async Task<Cevap<List<FirmaApiAnahtariListeDto>>> Listele()
    {
        var superAdminMi = hca.HttpContext?.User.IsInRole("SuperAdmin") == true;
        var firmaId = kiraciServisi.MevcutFirmaId;

        if (!superAdminMi && (firmaId is null or 0))
            return Cevap<List<FirmaApiAnahtariListeDto>>.Hata("Firma bilgisi bulunamadı.");

        var sorgu = vt.FirmaApiAnahtarlari
            .AsNoTracking()
            .Where(a => !a.SilindiMi);

        if (!superAdminMi)
            sorgu = sorgu.Where(a => a.FirmaId == firmaId!.Value);

        var liste = await sorgu
            .OrderByDescending(a => a.OlusturulmaTarihi)
            .Select(a => new FirmaApiAnahtariListeDto(
                a.Id,
                a.AnahtarAd,
                a.AnahtarOnEki,
                a.Kapsam,
                a.IzinVerilenDomainler,
                a.AktifMi,
                a.SonKullanmaTarihi,
                a.SonKullanimTarihi,
                a.OlusturulmaTarihi
            ))
            .ToListAsync();

        return Cevap<List<FirmaApiAnahtariListeDto>>.Basarili(liste);
    }

    /// <summary>Yeni API anahtarı oluştur. SuperAdmin body'den FirmaId alabilir, Admin kendi firmasına oluşturur. Düz metin anahtar SADECE bu yanıtta döner.</summary>
    [HttpPost]
    public async Task<Cevap<FirmaApiAnahtariOlusturYanitDto>> Olustur([FromBody] FirmaApiAnahtariOlusturDto dto)
    {
        var superAdminMi = hca.HttpContext?.User.IsInRole("SuperAdmin") == true;

        // FirmaId belirle: SuperAdmin body'den alabilir, Admin kendi firması
        int? hedefFirmaId;
        if (superAdminMi && dto.FirmaId.HasValue && dto.FirmaId > 0)
            hedefFirmaId = dto.FirmaId;
        else
            hedefFirmaId = kiraciServisi.MevcutFirmaId;

        if (hedefFirmaId is null or 0)
            return Cevap<FirmaApiAnahtariOlusturYanitDto>.Hata("Firma bilgisi bulunamadı.");

        // Firma mevcut mu?
        var firmaVarMi = await vt.Firmalar.AnyAsync(f => f.Id == hedefFirmaId.Value);
        if (!firmaVarMi)
            return Cevap<FirmaApiAnahtariOlusturYanitDto>.Hata("Firma bulunamadı.");

        // Kapsam doğrulaması
        var (kapsamGecerli, kapsamHatasi) = ApiAnahtarUretici.KapsamDogrula(dto.Kapsam);
        if (!kapsamGecerli)
            return Cevap<FirmaApiAnahtariOlusturYanitDto>.Hata(kapsamHatasi!);

        // İzin verilen domain doğrulaması
        var (domainGecerli, domainHatasi, _) = ApiAnahtarUretici.IzinVerilenDomainlerDogrula(dto.IzinVerilenDomainler, dto.Kapsam);
        if (!domainGecerli)
            return Cevap<FirmaApiAnahtariOlusturYanitDto>.Hata(domainHatasi!);

        // SonKullanmaTarihi geçmişte olamaz
        if (dto.SonKullanmaTarihi.HasValue && dto.SonKullanmaTarihi.Value < DateTime.UtcNow)
            return Cevap<FirmaApiAnahtariOlusturYanitDto>.Hata("Son kullanma tarihi geçmiş bir tarih olamaz.");

        // AnahtarAd boş olamaz
        if (string.IsNullOrWhiteSpace(dto.AnahtarAd))
            return Cevap<FirmaApiAnahtariOlusturYanitDto>.Hata("Anahtar adı zorunludur.");

        if (dto.AnahtarAd.Length > 100)
            return Cevap<FirmaApiAnahtariOlusturYanitDto>.Hata("Anahtar adı en fazla 100 karakter olabilir.");

        // Aynı isimde anahtar var mı?
        var ayniIsimVar = await vt.FirmaApiAnahtarlari
            .AnyAsync(a => a.FirmaId == hedefFirmaId.Value && a.AnahtarAd == dto.AnahtarAd && !a.SilindiMi);

        if (ayniIsimVar)
            return Cevap<FirmaApiAnahtariOlusturYanitDto>.Hata("Bu isimde bir anahtar zaten mevcut.");

        // Yeni API anahtarı üret
        var duzMetinAnahtar = ApiAnahtarUretici.AnahtarUret();
        var hash = ApiAnahtarUretici.HashHesapla(duzMetinAnahtar);
        var onEk = duzMetinAnahtar[..8]; // ilk 8 karakter

        var anahtar = new FirmaApiAnahtari
        {
            FirmaId = hedefFirmaId.Value,
            AnahtarAd = dto.AnahtarAd,
            ApiKeyHash = hash,
            AnahtarOnEki = onEk,
            Kapsam = dto.Kapsam,
            IzinVerilenDomainler = dto.IzinVerilenDomainler,
            SonKullanmaTarihi = dto.SonKullanmaTarihi,
            AktifMi = true,
            OlusturulmaTarihi = DateTime.UtcNow
        };

        vt.FirmaApiAnahtarlari.Add(anahtar);
        await vt.SaveChangesAsync();

        // Düz metin anahtarı SADECE bu yanıtta döndür
        var yanit = new FirmaApiAnahtariOlusturYanitDto(
            anahtar.Id,
            anahtar.AnahtarAd,
            anahtar.AnahtarOnEki,
            duzMetinAnahtar,
            anahtar.Kapsam,
            anahtar.IzinVerilenDomainler,
            anahtar.SonKullanmaTarihi,
            anahtar.OlusturulmaTarihi
        );

        return Cevap<FirmaApiAnahtariOlusturYanitDto>.Basarili(
            yanit,
            "API anahtarı oluşturuldu. Bu anahtar değeri SADECE BİR KEZ gösterilir, lütfen kaydedin.");
    }

    /// <summary>API anahtarını güncelle (anahtar değeri değişmez). SuperAdmin tüm firmaların anahtarlarını güncelleyebilir.</summary>
    [HttpPut("{id:int}")]
    public async Task<Cevap<FirmaApiAnahtariListeDto>> Guncelle(int id, [FromBody] FirmaApiAnahtariGuncelleDto dto)
    {
        var superAdminMi = hca.HttpContext?.User.IsInRole("SuperAdmin") == true;
        var firmaId = kiraciServisi.MevcutFirmaId;

        if (!superAdminMi && (firmaId is null or 0))
            return Cevap<FirmaApiAnahtariListeDto>.Hata("Firma bilgisi bulunamadı.");

        var sorgu = vt.FirmaApiAnahtarlari
            .Where(a => a.Id == id && !a.SilindiMi);

        if (!superAdminMi)
            sorgu = sorgu.Where(a => a.FirmaId == firmaId!.Value);

        var anahtar = await sorgu.FirstOrDefaultAsync();

        if (anahtar is null)
            return Cevap<FirmaApiAnahtariListeDto>.Hata("API anahtarı bulunamadı.");

        if (dto.AnahtarAd is not null)
        {
            if (string.IsNullOrWhiteSpace(dto.AnahtarAd))
                return Cevap<FirmaApiAnahtariListeDto>.Hata("Anahtar adı boş olamaz.");
            if (dto.AnahtarAd.Length > 100)
                return Cevap<FirmaApiAnahtariListeDto>.Hata("Anahtar adı en fazla 100 karakter olabilir.");
            anahtar.AnahtarAd = dto.AnahtarAd;
        }

        if (dto.Kapsam is not null)
        {
            var (kapsamGecerli, kapsamHatasi) = ApiAnahtarUretici.KapsamDogrula(dto.Kapsam);
            if (!kapsamGecerli)
                return Cevap<FirmaApiAnahtariListeDto>.Hata(kapsamHatasi!);
            anahtar.Kapsam = dto.Kapsam;
        }

        if (dto.IzinVerilenDomainler is not null)
        {
            var kapsamKontrol = dto.Kapsam ?? anahtar.Kapsam;
            var (domainGecerli, domainHatasi, _) = ApiAnahtarUretici.IzinVerilenDomainlerDogrula(dto.IzinVerilenDomainler, kapsamKontrol);
            if (!domainGecerli)
                return Cevap<FirmaApiAnahtariListeDto>.Hata(domainHatasi!);
            anahtar.IzinVerilenDomainler = dto.IzinVerilenDomainler;
        }

        if (dto.SonKullanmaTarihi is not null)
        {
            if (dto.SonKullanmaTarihi.Value < DateTime.UtcNow)
                return Cevap<FirmaApiAnahtariListeDto>.Hata("Son kullanma tarihi geçmiş bir tarih olamaz.");
            anahtar.SonKullanmaTarihi = dto.SonKullanmaTarihi;
        }

        if (dto.AktifMi.HasValue)
            anahtar.AktifMi = dto.AktifMi.Value;

        anahtar.GuncellenmeTarihi = DateTime.UtcNow;

        await vt.SaveChangesAsync();

        var listeDto = new FirmaApiAnahtariListeDto(
            anahtar.Id,
            anahtar.AnahtarAd,
            anahtar.AnahtarOnEki,
            anahtar.Kapsam,
            anahtar.IzinVerilenDomainler,
            anahtar.AktifMi,
            anahtar.SonKullanmaTarihi,
            anahtar.SonKullanimTarihi,
            anahtar.OlusturulmaTarihi
        );

        return Cevap<FirmaApiAnahtariListeDto>.Basarili(listeDto, "API anahtarı güncellendi.");
    }

    /// <summary>API anahtarını sil (soft delete). SuperAdmin tüm firmaların anahtarlarını silebilir.</summary>
    [HttpDelete("{id:int}")]
    public async Task<Cevap<bool>> Sil(int id)
    {
        var superAdminMi = hca.HttpContext?.User.IsInRole("SuperAdmin") == true;
        var firmaId = kiraciServisi.MevcutFirmaId;

        if (!superAdminMi && (firmaId is null or 0))
            return Cevap<bool>.Hata("Firma bilgisi bulunamadı.");

        var sorgu = vt.FirmaApiAnahtarlari
            .Where(a => a.Id == id && !a.SilindiMi);

        if (!superAdminMi)
            sorgu = sorgu.Where(a => a.FirmaId == firmaId!.Value);

        var anahtar = await sorgu.FirstOrDefaultAsync();

        if (anahtar is null)
            return Cevap<bool>.Hata("API anahtarı bulunamadı.");

        anahtar.SilindiMi = true;
        anahtar.SilinmeTarihi = DateTime.UtcNow;
        anahtar.GuncellenmeTarihi = DateTime.UtcNow;

        await vt.SaveChangesAsync();
        return Cevap<bool>.Basarili(true, "API anahtarı silindi.");
    }
}
