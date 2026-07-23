using MediatR;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Api.Moduller.Konfigurasyon.Dtolar;
using VizitLink3D.Api.Servisler;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;

namespace VizitLink3D.Api.Moduller.Konfigurasyon.Komutlar;

/// <summary>
/// Public konfigüratör sorgu handler'ı.
/// Tenant domain'ine göre ürün slug ile bulunur, yalnız public güvenli veri döner.
/// Teknik mesh/HDR/kamera/JSON/admin audit alanları dönmez.
/// Multi-tenant: KiraciServisi.MevcutFirmaId null/0 ise fail closed.
/// </summary>
public class PublicKonfiguratorIsleyici(
    VizitLink3DDbContext vt,
    KiraciServisi kiraciServisi)
    : IRequestHandler<PublicKonfiguratorSorgusu, Cevap<PublicKonfiguratorDto>>
{
    public async Task<Cevap<PublicKonfiguratorDto>> Handle(
        PublicKonfiguratorSorgusu sorgu,
        CancellationToken iptal)
    {
        // Tenant güvenlik: FirmaId yoksa fail closed
        var firmaId = kiraciServisi.MevcutFirmaId;
        if (firmaId is null or 0)
            return Cevap<PublicKonfiguratorDto>.Hata("Firma tanımlanamadı.");

        // Ürünü slug ile bul — tenant filtresi + aktif + silinmemiş
        var urun = await vt.Urunler
            .AsNoTracking()
            .FirstOrDefaultAsync(u =>
                u.Slug == sorgu.Slug &&
                u.FirmaId == firmaId.Value &&
                !u.SilindiMi &&
                u.AktifMi,
                iptal);

        if (urun is null)
            return Cevap<PublicKonfiguratorDto>.Hata("Ürün bulunamadı.");

        // Varsayılan 3D modeli bul
        var model = await VarsayilanModelGetir(vt, urun, iptal);
        int? modelId = model?.Id;

        var dto = new PublicKonfiguratorDto
        {
            UrunId = urun.Id,
            Slug = urun.Slug,
            Ad = urun.Ad,
            Fiyat = urun.Fiyat,
            FirmaId = urun.FirmaId ?? 0,
            ModelId = modelId,
            ModelYolu = model != null
                ? (model.ModelYolu ?? model.ModelDosyaYolu)
                : null
        };

        if (modelId.HasValue)
        {
            dto.Parcalar = await OnayliParcalarGetir(vt, modelId.Value, iptal);
            dto.SahneOnayarlari = await OnayliSahneOnayarlariGetir(vt, modelId.Value, iptal);
        }

        return Cevap<PublicKonfiguratorDto>.Basarili(dto);
    }

    private static async Task<UrunUcBoyutModeli?> VarsayilanModelGetir(
        VizitLink3DDbContext vt, Urun urun, CancellationToken iptal)
    {
        if (urun.VarsayilanUcBoyutModeliId.HasValue)
        {
            var varsayilan = await vt.UrunUcBoyutModelleri
                .AsNoTracking()
                .FirstOrDefaultAsync(m =>
                    m.Id == urun.VarsayilanUcBoyutModeliId.Value &&
                    m.UrunId == urun.Id &&
                    m.AktifMi &&
                    !m.SilindiMi,
                    iptal);

            if (varsayilan != null) return varsayilan;
        }

        return await vt.UrunUcBoyutModelleri
            .AsNoTracking()
            .Where(m => m.UrunId == urun.Id && m.AktifMi && !m.SilindiMi)
            .OrderByDescending(m => m.VarsayilanMi)
            .ThenBy(m => m.OlusturulmaTarihi)
            .FirstOrDefaultAsync(iptal);
    }

    private static async Task<List<PublicParcaDto>> OnayliParcalarGetir(
        VizitLink3DDbContext vt, int modelId, CancellationToken iptal)
    {
        // Admin onaylı, aktif, silinmemiş parçalar
        var parcalar = await vt.UrunUcBoyutParcalari
            .AsNoTracking()
            .Where(p =>
                p.UrunUcBoyutModeliId == modelId &&
                p.AdminOnayliMi &&
                p.AktifMi &&
                !p.SilindiMi)
            .OrderBy(p => p.SiraNo)
            .ThenBy(p => p.GorunenAd)
            .ToListAsync(iptal);

        if (parcalar.Count == 0) return [];

        var parcaIdleri = parcalar.Select(p => p.Id).ToList();

        // Parça renk seçenekleri: UrunParcaRenkSecenegi → RalRengi (join ile)
        var renkSecenekleri = await (
            from prs in vt.UrunParcaRenkSecenekleri
            join ral in vt.RalRenkleri on prs.RalRengiId equals ral.Id into ralGrup
            from ral in ralGrup.DefaultIfEmpty()
            where prs.AktifMi && parcaIdleri.Contains(prs.UrunUcBoyutParcasiId) && ral != null
            select new { prs.Id, prs.UrunUcBoyutParcasiId, prs.RalRengiId, ral.Kod, ral.Ad, ral.HexKod }
        ).ToListAsync(iptal);

        var renkGruplari = renkSecenekleri
            .GroupBy(r => r.UrunUcBoyutParcasiId)
            .ToDictionary(g => g.Key, g => g.Select(r => new PublicParcaRenkDto
            {
                RenkId = r.Id,
                RalRengiId = r.RalRengiId ?? 0,
                RalKodu = r.Kod,
                RalAdi = r.Ad,
                HexKodu = r.HexKod ?? "#808080",
                EginliResimUrl = null
            }).ToList());

        // Parça malzeme seçenekleri: UrunParcaMalzemeSecenegi → Malzeme (join ile)
        var malzemeSecenekleri = await (
            from pms in vt.UrunParcaMalzemeSecenekleri
            join m in vt.Malzemeler on pms.MalzemeId equals m.Id into mGrup
            from m in mGrup.DefaultIfEmpty()
            where pms.AktifMi && parcaIdleri.Contains(pms.UrunUcBoyutParcasiId) && m != null
            select new { pms.Id, pms.UrunUcBoyutParcasiId, pms.MalzemeId, m.Ad, m.ResimUrl }
        ).ToListAsync(iptal);

        var malzemeGruplari = malzemeSecenekleri
            .GroupBy(m => m.UrunUcBoyutParcasiId)
            .ToDictionary(g => g.Key, g => g.Select(m => new PublicParcaMalzemeDto
            {
                MalzemeId = m.MalzemeId ?? 0,
                MalzemeAdi = m.Ad,
                TeksurResimUrl = m.ResimUrl
            }).ToList());

        // Doku/kaplama seçenekleri: DokuUygulanabilirMi=true parçalar için,
        // UrunParcaMalzemeSecenegi üzerinden KaplamaSecenegiId join'lendiğinde malzemeden bağımsız kaplamalar
        var dokuParcaIdleri = parcalar.Where(p => p.DokuUygulanabilirMi).Select(p => p.Id).ToList();
        var dokuGruplari = new Dictionary<int, List<PublicParcaDokuDto>>();

        if (dokuParcaIdleri.Count > 0)
        {
            var kaplamalar = await (
                from pms in vt.UrunParcaMalzemeSecenekleri
                join k in vt.KaplamaSecenekleri on pms.KaplamaSecenegiId equals k.Id into kGrup
                from k in kGrup.DefaultIfEmpty()
                where pms.AktifMi && dokuParcaIdleri.Contains(pms.UrunUcBoyutParcasiId) && k != null && !k.SilindiMi
                select new { pms.UrunUcBoyutParcasiId, k.Id, k.Ad, k.ResimUrl }
            ).ToListAsync(iptal);

            dokuGruplari = kaplamalar
                .DistinctBy(k => new { k.UrunUcBoyutParcasiId, k.Id })
                .GroupBy(k => k.UrunUcBoyutParcasiId)
                .ToDictionary(g => g.Key, g => g.Select(k => new PublicParcaDokuDto
                {
                    KaplamaId = k.Id,
                    Ad = k.Ad,
                    TeksurResimUrl = k.ResimUrl
                }).ToList());
        }

        // Public DTO'ları oluştur — teknik alanları hariç tut
        return parcalar.Select(p =>
        {
            var dto = new PublicParcaDto
            {
                Id = p.Id,
                ParcaGrubuId = p.ParcaGrubuId,
                GorunenAd = p.GorunenAd,
                ParcaTipi = p.ParcaTipi,
                RenklenebilirMi = p.RenklenebilirMi,
                MalzemeDegisebilirMi = p.MalzemeDegisebilirMi,
                DokuUygulanabilirMi = p.DokuUygulanabilirMi,
                GizlenebilirMi = p.GizlenebilirMi,
                SecilebilirMi = p.SecilebilirMi,
                HareketliMi = p.HareketliMi,
                HareketTipi = p.HareketTipi,
                VarsayilanRenkId = p.VarsayilanRenkId,
                VarsayilanMalzemeId = p.VarsayilanMalzemeId,
                MinDeger = p.MinDeger,
                MaxDeger = p.MaxDeger,
                VarsayilanDeger = p.VarsayilanDeger,
                SiraNo = p.SiraNo
            };

            if (renkGruplari.TryGetValue(p.Id, out var renkler))
                dto.Renkler = renkler;

            if (malzemeGruplari.TryGetValue(p.Id, out var malzemeler))
                dto.Malzemeler = malzemeler;

            if (dokuGruplari.TryGetValue(p.Id, out var dokular))
                dto.Dokular = dokular;

            return dto;
        }).ToList();
    }

    private static async Task<List<PublicSahneOnayariDto>> OnayliSahneOnayarlariGetir(
        VizitLink3DDbContext vt, int modelId, CancellationToken iptal)
    {
        return await vt.UrunUcBoyutSahneOnayarlari
            .AsNoTracking()
            .Where(s =>
                s.UrunUcBoyutModeliId == modelId &&
                s.AdminOnayliMi &&
                s.AktifMi &&
                !s.SilindiMi)
            .OrderBy(s => s.SiraNo)
            .Select(s => new PublicSahneOnayariDto
            {
                Id = s.Id,
                Ad = s.Ad,
                Kod = s.Kod,
                VarsayilanMi = s.VarsayilanMi,
                SiraNo = s.SiraNo
            })
            .ToListAsync(iptal);
    }
}

/// <summary>
/// Public konfigüratörden müşteri seçimini kaydetme handler'ı.
/// Tenant FirmaId'si KiraciServisi'den alınır, OturumAnahtari backend oluşturur.
/// Çapraz tenant/ürün/parça validasyonu: her seçim zincirleme doğrulanır.
/// </summary>
public class PublicSecimKaydetIsleyici(
    VizitLink3DDbContext vt,
    KiraciServisi kiraciServisi)
    : IRequestHandler<PublicSecimKaydetKomutu, Cevap<KonfigurasyonDetayDto>>
{
    public async Task<Cevap<KonfigurasyonDetayDto>> Handle(
        PublicSecimKaydetKomutu istek,
        CancellationToken iptal)
    {
        // Tenant güvenlik: FirmaId yoksa fail closed
        var firmaId = kiraciServisi.MevcutFirmaId;
        if (firmaId is null or 0)
            return Cevap<KonfigurasyonDetayDto>.Hata("Firma tanımlanamadı.");

        // Ürün varlığını ve tenant sahipliğini doğrula
        var urun = await vt.Urunler
            .AsNoTracking()
            .FirstOrDefaultAsync(u =>
                u.Id == istek.UrunId &&
                u.FirmaId == firmaId.Value &&
                !u.SilindiMi &&
                u.AktifMi,
                iptal);

        if (urun is null)
            return Cevap<KonfigurasyonDetayDto>.Hata("Ürün bulunamadı.");

        // Seçilen parçaların admin onaylı, aktif ve silinmemiş olduğunu doğrula
        var parcaIdleri = istek.Secimler.Select(s => s.ParcaId).Distinct().ToList();

        // Parça → Model → Ürün zinciri ile tenant ve ürün doğrulaması
        var parcaZincirleri = await vt.UrunUcBoyutParcalari
            .AsNoTracking()
            .Where(p => parcaIdleri.Contains(p.Id) && p.AdminOnayliMi && p.AktifMi && !p.SilindiMi)
            .Join(vt.UrunUcBoyutModelleri,
                p => p.UrunUcBoyutModeliId, m => m.Id,
                (p, m) => new { ParcaId = p.Id, ModelId = m.Id, m.UrunId })
            .Join(vt.Urunler,
                pm => pm.UrunId, u => u.Id,
                (pm, u) => new { pm.ParcaId, pm.ModelId, pm.UrunId, u.FirmaId })
            .Where(x => x.FirmaId == firmaId.Value && x.UrunId == istek.UrunId)
            .ToListAsync(iptal);

        var gecerliParcaSet = parcaZincirleri.Select(x => x.ParcaId).ToHashSet();
        var gecersizler = parcaIdleri.Where(id => !gecerliParcaSet.Contains(id)).ToList();
        if (gecersizler.Count > 0)
            return Cevap<KonfigurasyonDetayDto>.Hata("Geçersiz parça seçimi.");

        // Seçili renk doğrulaması (parçaya ait + aktif)
        foreach (var secim in istek.Secimler)
        {
            if (secim.SeciliRenkId.HasValue)
            {
                var renkGecerli = await vt.UrunParcaRenkSecenekleri
                    .AsNoTracking()
                    .AnyAsync(r =>
                        r.Id == secim.SeciliRenkId.Value &&
                        r.UrunUcBoyutParcasiId == secim.ParcaId &&
                        r.AktifMi,
                        iptal);

                if (!renkGecerli)
                    return Cevap<KonfigurasyonDetayDto>.Hata("Geçersiz renk seçimi.");
            }

            if (secim.SeciliMalzemeId.HasValue)
            {
                var malzemeGecerli = await vt.UrunParcaMalzemeSecenekleri
                    .AsNoTracking()
                    .AnyAsync(m =>
                        m.Id == secim.SeciliMalzemeId.Value &&
                        m.UrunUcBoyutParcasiId == secim.ParcaId &&
                        m.AktifMi,
                        iptal);

                if (!malzemeGecerli)
                    return Cevap<KonfigurasyonDetayDto>.Hata("Geçersiz malzeme seçimi.");
            }

            if (secim.SeciliKaplamaId.HasValue)
            {
                var kaplamaGecerli = await vt.UrunParcaMalzemeSecenekleri
                    .AsNoTracking()
                    .AnyAsync(k =>
                        k.KaplamaSecenegiId == secim.SeciliKaplamaId.Value &&
                        k.UrunUcBoyutParcasiId == secim.ParcaId &&
                        k.AktifMi,
                        iptal);

                if (!kaplamaGecerli)
                    return Cevap<KonfigurasyonDetayDto>.Hata("Geçersiz kaplama seçimi.");
            }
        }

        // Anonim oturum anahtarı oluştur
        var oturumAnahtari = Guid.NewGuid().ToString("N")[..16];

        // Konfigürasyon oluştur
        var konfigurasyon = new MusteriKonfigurasyonu
        {
            UrunId = istek.UrunId,
            FirmaId = firmaId,
            OturumAnahtari = oturumAnahtari,
            Not = istek.MusteriNotu,
            Durum = "Taslak",
            OlusturulmaTarihi = DateTime.UtcNow
        };

        vt.MusteriKonfigurasyonlari.Add(konfigurasyon);
        await vt.SaveChangesAsync(iptal);

        // Parçaları ekle
        foreach (var secim in istek.Secimler)
        {
            vt.MusteriKonfigurasyonParcalari.Add(new MusteriKonfigurasyonParcasi
            {
                MusteriKonfigurasyonuId = konfigurasyon.Id,
                UrunUcBoyutParcasiId = secim.ParcaId,
                SeciliRenkId = secim.SeciliRenkId,
                SeciliMalzemeId = secim.SeciliMalzemeId,
                SeciliKaplamaId = secim.SeciliKaplamaId,
                SeciliDoku = secim.SeciliDoku,
                HareketDegeri = secim.HareketDegeri,
                Aci = secim.Aci,
                GorunurMu = secim.GorunurMu,
                OlusturulmaTarihi = DateTime.UtcNow
            });
        }

        await vt.SaveChangesAsync(iptal);

        // Detay DTO döndür
        var detay = await KonfigurasyonOlusturIsleyici.KonfigurasyonDetayOlusturAsync(vt, konfigurasyon.Id, iptal);
        if (detay != null)
        {
            detay = detay with { OturumAnahtari = oturumAnahtari };
        }

        return Cevap<KonfigurasyonDetayDto>.Basarili(detay!, "Konfigürasyon kaydedildi.");
    }
}
