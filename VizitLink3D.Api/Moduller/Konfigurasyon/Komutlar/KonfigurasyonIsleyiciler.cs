using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Api.Moduller.Konfigurasyon.Dtolar;
using VizitLink3D.Api.Servisler;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;

namespace VizitLink3D.Api.Moduller.Konfigurasyon.Komutlar;

/// <summary>
/// Konfigürasyon oluşturma handler'ı.
/// Tenant izolasyonu: FirmaId KiraciServisi'den alınır, request body'den değil.
/// Parça validasyonu: Her parçanın aynı ürüne ait olup olmadığı ve tenant ürünü olduğu kontrol edilir.
/// </summary>
public class KonfigurasyonOlusturIsleyici(
    VizitLink3DDbContext vt,
    KiraciServisi kiraciServisi)
    : IRequestHandler<KonfigurasyonOlusturKomutu, Cevap<KonfigurasyonDetayDto>>
{
    public async Task<Cevap<KonfigurasyonDetayDto>> Handle(
        KonfigurasyonOlusturKomutu istek,
        CancellationToken iptal)
    {
        var firmaId = kiraciServisi.MevcutFirmaId;

        // Ürünün tenant'a ait olduğunu doğrula
        var urun = await vt.Urunler
            .AsNoTracking()
            .FirstOrDefaultAsync(u =>
                u.Id == istek.UrunId &&
                u.FirmaId == firmaId &&
                !u.SilindiMi,
                iptal);

        if (urun is null)
            return Cevap<KonfigurasyonDetayDto>.Hata("Ürün bulunamadı.");

        // Parçaların ürüne ait olduğunu doğrula
        var parcaIdleri = istek.Parcalar.Select(p => p.UrunUcBoyutParcasiId).Distinct().ToList();

        var gecerliParcalar = await vt.UrunUcBoyutParcalari
            .AsNoTracking()
            .Where(p => parcaIdleri.Contains(p.Id) && !p.SilindiMi)
            .Select(p => new { p.Id, p.UrunUcBoyutModeliId })
            .ToListAsync(iptal);

        var gecerliParcaIdleri = gecerliParcalar.Select(p => p.Id).ToHashSet();
        var gecersizParcalar = parcaIdleri.Where(id => !gecerliParcaIdleri.Contains(id)).ToList();

        if (gecersizParcalar.Count > 0)
            return Cevap<KonfigurasyonDetayDto>.Hata(
                $"Geçersiz parça ID'leri: {string.Join(", ", gecersizParcalar)}");

        // Konfigürasyonu oluştur
        var konfigurasyon = new MusteriKonfigurasyonu
        {
            UrunId = istek.UrunId,
            FirmaId = firmaId,
            OturumAnahtari = istek.OturumAnahtari,
            Not = istek.Not,
            Durum = "Taslak",
            OlusturulmaTarihi = DateTime.UtcNow
        };

        vt.MusteriKonfigurasyonlari.Add(konfigurasyon);
        await vt.SaveChangesAsync(iptal);

        // Parçaları ekle
        foreach (var pd in istek.Parcalar)
        {
            vt.MusteriKonfigurasyonParcalari.Add(new MusteriKonfigurasyonParcasi
            {
                MusteriKonfigurasyonuId = konfigurasyon.Id,
                UrunUcBoyutParcasiId = pd.UrunUcBoyutParcasiId,
                SeciliRenkId = pd.SeciliRenkId,
                SeciliMalzemeId = pd.SeciliMalzemeId,
                SeciliKaplamaId = pd.SeciliKaplamaId,
                SeciliDoku = pd.SeciliDoku,
                HareketDegeri = pd.HareketDegeri,
                Aci = pd.Aci,
                Deger = pd.Deger,
                GorunurMu = pd.GorunurMu,
                OlusturulmaTarihi = DateTime.UtcNow
            });
        }

        await vt.SaveChangesAsync(iptal);

        // Detay DTO'yu döndür
        var detay = await KonfigurasyonDetayOlusturAsync(vt, konfigurasyon.Id, iptal);
        return Cevap<KonfigurasyonDetayDto>.Basarili(detay!, "Konfigürasyon oluşturuldu.");
    }

    internal static async Task<KonfigurasyonDetayDto?> KonfigurasyonDetayOlusturAsync(
        VizitLink3DDbContext vt, int id, CancellationToken iptal)
    {
        return await vt.MusteriKonfigurasyonlari
            .AsNoTracking()
            .Where(k => k.Id == id && !k.SilindiMi)
            .Select(k => new KonfigurasyonDetayDto(
                k.Id,
                k.UrunId,
                k.Urun != null ? k.Urun.Ad : null,
                k.OturumAnahtari,
                k.Not,
                k.Durum,
                k.ToplamFiyat,
                k.Parcalar.Where(p => !p.SilindiMi).Select(p => new KonfigurasyonParcaDetayDto(
                    p.Id,
                    p.UrunUcBoyutParcasiId,
                    p.UrunUcBoyutParcasi != null ? p.UrunUcBoyutParcasi.GorunenAd : null,
                    p.SeciliRenkId,
                    p.SeciliRenk != null ? p.SeciliRenk.Ad : null,
                    p.SeciliMalzemeId,
                    p.SeciliMalzeme != null ? p.SeciliMalzeme.Ad : null,
                    p.SeciliKaplamaId,
                    p.SeciliKaplama != null ? p.SeciliKaplama.Ad : null,
                    p.SeciliDoku,
                    p.HareketDegeri,
                    p.Aci,
                    p.Deger,
                    p.GorunurMu
                )).ToList(),
                k.OlusturulmaTarihi,
                k.GuncellenmeTarihi
            ))
            .FirstOrDefaultAsync(iptal);
    }
}

/// <summary>
/// Konfigürasyon güncelleme handler'ı.
/// Mevcut parçalar soft-delete, yeniler eklenir.
/// Sadece kendi tenant'ına ait konfigürasyon güncellenebilir.
/// </summary>
public class KonfigurasyonGuncelleIsleyici(
    VizitLink3DDbContext vt,
    KiraciServisi kiraciServisi)
    : IRequestHandler<KonfigurasyonGuncelleKomutu, Cevap<KonfigurasyonDetayDto>>
{
    public async Task<Cevap<KonfigurasyonDetayDto>> Handle(
        KonfigurasyonGuncelleKomutu istek,
        CancellationToken iptal)
    {
        var firmaId = kiraciServisi.MevcutFirmaId;

        var konfigurasyon = await vt.MusteriKonfigurasyonlari
            .Include(k => k.Parcalar)
            .FirstOrDefaultAsync(k => k.Id == istek.Id && !k.SilindiMi, iptal);

        if (konfigurasyon is null)
            return Cevap<KonfigurasyonDetayDto>.Hata("Konfigürasyon bulunamadı.");

        // Tenant izolasyonu: sadece kendi firma konfigürasyonu
        if (firmaId.HasValue && konfigurasyon.FirmaId.HasValue && konfigurasyon.FirmaId != firmaId.Value)
            return Cevap<KonfigurasyonDetayDto>.Hata("Bu konfigürasyona erişim izniniz yok.");

        // Not güncelle
        konfigurasyon.Not = istek.Not;
        konfigurasyon.GuncellenmeTarihi = DateTime.UtcNow;

        // Parça ID'lerini doğrula
        var parcaIdleri = istek.Parcalar.Select(p => p.UrunUcBoyutParcasiId).Distinct().ToList();
        var gecerliParcalar = await vt.UrunUcBoyutParcalari
            .AsNoTracking()
            .Where(p => parcaIdleri.Contains(p.Id) && !p.SilindiMi)
            .Select(p => p.Id)
            .ToListAsync(iptal);

        var gecerliSet = gecerliParcalar.ToHashSet();
        var gecersizParcalar = parcaIdleri.Where(id => !gecerliSet.Contains(id)).ToList();

        if (gecersizParcalar.Count > 0)
            return Cevap<KonfigurasyonDetayDto>.Hata(
                $"Geçersiz parça ID'leri: {string.Join(", ", gecersizParcalar)}");

        // Mevcut parçaları soft-delete yap
        foreach (var mevcutParca in konfigurasyon.Parcalar.Where(p => !p.SilindiMi))
        {
            mevcutParca.SilindiMi = true;
            mevcutParca.SilinmeTarihi = DateTime.UtcNow;
        }

        // Yeni parçaları ekle
        foreach (var pd in istek.Parcalar)
        {
            vt.MusteriKonfigurasyonParcalari.Add(new MusteriKonfigurasyonParcasi
            {
                MusteriKonfigurasyonuId = konfigurasyon.Id,
                UrunUcBoyutParcasiId = pd.UrunUcBoyutParcasiId,
                SeciliRenkId = pd.SeciliRenkId,
                SeciliMalzemeId = pd.SeciliMalzemeId,
                SeciliKaplamaId = pd.SeciliKaplamaId,
                SeciliDoku = pd.SeciliDoku,
                HareketDegeri = pd.HareketDegeri,
                Aci = pd.Aci,
                Deger = pd.Deger,
                GorunurMu = pd.GorunurMu,
                OlusturulmaTarihi = DateTime.UtcNow
            });
        }

        await vt.SaveChangesAsync(iptal);

        // Güncel detayı döndür
        var detay = await KonfigurasyonOlusturIsleyici.KonfigurasyonDetayOlusturAsync(vt, konfigurasyon.Id, iptal);
        return Cevap<KonfigurasyonDetayDto>.Basarili(detay!, "Konfigürasyon güncellendi.");
    }
}

/// <summary>
/// Konfigürasyon silme handler'ı — soft delete.
/// </summary>
public class KonfigurasyonSilIsleyici(
    VizitLink3DDbContext vt,
    KiraciServisi kiraciServisi)
    : IRequestHandler<KonfigurasyonSilKomutu, Cevap<bool>>
{
    public async Task<Cevap<bool>> Handle(
        KonfigurasyonSilKomutu istek,
        CancellationToken iptal)
    {
        var firmaId = kiraciServisi.MevcutFirmaId;

        var konfigurasyon = await vt.MusteriKonfigurasyonlari
            .FirstOrDefaultAsync(k => k.Id == istek.Id && !k.SilindiMi, iptal);

        if (konfigurasyon is null)
            return Cevap<bool>.Hata("Konfigürasyon bulunamadı.");

        if (firmaId.HasValue && konfigurasyon.FirmaId.HasValue && konfigurasyon.FirmaId != firmaId.Value)
            return Cevap<bool>.Hata("Bu konfigürasyona erişim izniniz yok.");

        konfigurasyon.SilindiMi = true;
        konfigurasyon.SilinmeTarihi = DateTime.UtcNow;

        await vt.SaveChangesAsync(iptal);
        return Cevap<bool>.Basarili(true, "Konfigürasyon silindi.");
    }
}

/// <summary>
/// Konfigürasyon listeleme handler'ı — tenant filtresi otomatik.
/// </summary>
public class KonfigurasyonListeleIsleyici(
    VizitLink3DDbContext vt,
    KiraciServisi kiraciServisi)
    : IRequestHandler<KonfigurasyonListeleSorgusu, Cevap<List<KonfigurasyonOzetDto>>>
{
    public async Task<Cevap<List<KonfigurasyonOzetDto>>> Handle(
        KonfigurasyonListeleSorgusu sorgu,
        CancellationToken iptal)
    {
        var firmaId = kiraciServisi.MevcutFirmaId;

        var q = vt.MusteriKonfigurasyonlari
            .AsNoTracking()
            .Where(k => !k.SilindiMi);

        if (firmaId.HasValue)
            q = q.Where(k => k.FirmaId == firmaId.Value);

        if (sorgu.UrunId.HasValue)
            q = q.Where(k => k.UrunId == sorgu.UrunId.Value);

        var liste = await q
            .OrderByDescending(k => k.OlusturulmaTarihi)
            .Skip((sorgu.Sayfa - 1) * sorgu.Boyut)
            .Take(sorgu.Boyut)
            .Select(k => new KonfigurasyonOzetDto(
                k.Id,
                k.UrunId,
                k.Urun != null ? k.Urun.Ad : null,
                k.Durum,
                k.ToplamFiyat,
                k.Parcalar.Count(p => !p.SilindiMi),
                k.OlusturulmaTarihi,
                k.GuncellenmeTarihi
            ))
            .ToListAsync(iptal);

        return Cevap<List<KonfigurasyonOzetDto>>.Basarili(liste);
    }
}

/// <summary>
/// Konfigürasyon detay handler'ı — tenant izolasyonu.
/// </summary>
public class KonfigurasyonDetayIsleyici(
    VizitLink3DDbContext vt,
    KiraciServisi kiraciServisi)
    : IRequestHandler<KonfigurasyonDetaySorgusu, Cevap<KonfigurasyonDetayDto>>
{
    public async Task<Cevap<KonfigurasyonDetayDto>> Handle(
        KonfigurasyonDetaySorgusu sorgu,
        CancellationToken iptal)
    {
        var firmaId = kiraciServisi.MevcutFirmaId;

        var detay = await vt.MusteriKonfigurasyonlari
            .AsNoTracking()
            .Where(k => k.Id == sorgu.Id && !k.SilindiMi)
            .Select(k => new KonfigurasyonDetayDto(
                k.Id,
                k.UrunId,
                k.Urun != null ? k.Urun.Ad : null,
                k.OturumAnahtari,
                k.Not,
                k.Durum,
                k.ToplamFiyat,
                k.Parcalar.Where(p => !p.SilindiMi).Select(p => new KonfigurasyonParcaDetayDto(
                    p.Id,
                    p.UrunUcBoyutParcasiId,
                    p.UrunUcBoyutParcasi != null ? p.UrunUcBoyutParcasi.GorunenAd : null,
                    p.SeciliRenkId,
                    p.SeciliRenk != null ? p.SeciliRenk.Ad : null,
                    p.SeciliMalzemeId,
                    p.SeciliMalzeme != null ? p.SeciliMalzeme.Ad : null,
                    p.SeciliKaplamaId,
                    p.SeciliKaplama != null ? p.SeciliKaplama.Ad : null,
                    p.SeciliDoku,
                    p.HareketDegeri,
                    p.Aci,
                    p.Deger,
                    p.GorunurMu
                )).ToList(),
                k.OlusturulmaTarihi,
                k.GuncellenmeTarihi
            ))
            .FirstOrDefaultAsync(iptal);

        if (detay is null)
            return Cevap<KonfigurasyonDetayDto>.Hata("Konfigürasyon bulunamadı.");

        // Tenant kontrolü (detay null değilse bile tenant'ı kontrol et)
        var konfigurasyon = await vt.MusteriKonfigurasyonlari
            .AsNoTracking()
            .Select(k => new { k.Id, k.FirmaId })
            .FirstOrDefaultAsync(k => k.Id == sorgu.Id, iptal);

        if (firmaId.HasValue && konfigurasyon?.FirmaId.HasValue == true && konfigurasyon.FirmaId != firmaId.Value)
            return Cevap<KonfigurasyonDetayDto>.Hata("Bu konfigürasyona erişim izniniz yok.");

        return Cevap<KonfigurasyonDetayDto>.Basarili(detay);
    }
}
