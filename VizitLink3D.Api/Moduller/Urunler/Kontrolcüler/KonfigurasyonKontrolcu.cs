using System;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Moduller.Urunler.Kontrolcüler;

// ESKI KONTROLCU — Yerini Moduller/Konfigurasyon/Kontrolcu/KonfigurasyonKontrolcu aldi.
// Cakismayi onlemek icin pasif. Paket-1 ile yeni CQRS yapisi kullaniliyor.
[NonController]
[Obsolete("Paket-1: Yeni KonfigurasyonKontrolcu kullaniliyor.")]
[ApiController]
[Route("api/konfigurasyon-eski")]
public class KonfigurasyonKontrolcuEski(VizitLink3DDbContext vt) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<Cevap<MusteriKonfigurasyonu>> KonfigurasyonGetir(int id)
    {
        var konfigurasyonu = await vt.MusteriKonfigurasyonlari
            .Where(m => m.Id == id && !m.SilindiMi)
            .FirstOrDefaultAsync();

        if (konfigurasyonu is null)
            return Cevap<MusteriKonfigurasyonu>.Hata("Konfigürasyon bulunamadı.");

        return Cevap<MusteriKonfigurasyonu>.Basarili(konfigurasyonu);
    }

    [HttpGet("{id:int}/parcalar")]
    public async Task<Cevap<List<MusteriKonfigurasyonParcasi>>> ParcalarGetir(int id)
    {
        var parcalar = await vt.MusteriKonfigurasyonParcalari
            .Where(p => p.MusteriKonfigurasyonuId == id)
            .ToListAsync();

        return Cevap<List<MusteriKonfigurasyonParcasi>>.Basarili(parcalar);
    }

    [HttpPost]
    public async Task<Cevap<MusteriKonfigurasyonu>> OlusturAsync([FromBody] MusteriKonfigurasyonu oge)
    {
        if (oge.UrunId <= 0)
            return Cevap<MusteriKonfigurasyonu>.Hata("Ürün ID gerekli.");

        vt.MusteriKonfigurasyonlari.Add(oge);
        await vt.SaveChangesAsync();
        return Cevap<MusteriKonfigurasyonu>.Basarili(oge, "Konfigürasyon oluşturuldu.");
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<MusteriKonfigurasyonu>> GuncelleAsync(int id, [FromBody] MusteriKonfigurasyonu oge)
    {
        var mevcut = await vt.MusteriKonfigurasyonlari.FindAsync(id);
        if (mevcut is null || mevcut.SilindiMi)
            return Cevap<MusteriKonfigurasyonu>.Hata("Konfigürasyon bulunamadı.");

        mevcut.Not = oge.Not;
        mevcut.GuncellenmeTarihi = DateTime.UtcNow;

        await vt.SaveChangesAsync();
        return Cevap<MusteriKonfigurasyonu>.Basarili(mevcut, "Konfigürasyon güncellendi.");
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<bool>> SilAsync(int id)
    {
        var mevcut = await vt.MusteriKonfigurasyonlari.FindAsync(id);
        if (mevcut is null || mevcut.SilindiMi)
            return Cevap<bool>.Hata("Konfigürasyon bulunamadı.");

        mevcut.SilindiMi = true;
        mevcut.SilinmeTarihi = DateTime.UtcNow;

        await vt.SaveChangesAsync();
        return Cevap<bool>.Basarili(true, "Konfigürasyon silindi.");
    }
}
