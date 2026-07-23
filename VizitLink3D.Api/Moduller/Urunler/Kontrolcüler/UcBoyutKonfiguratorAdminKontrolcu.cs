using VizitLink3D.Api.Hubs;
using VizitLink3D.Api.Moduller.Urunler.Dtolar;
using VizitLink3D.Api.Moduller.Urunler.Servisler;
using VizitLink3D.Api.Servisler;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Moduller.Urunler.Kontrolcüler;

/// <summary>
/// Admin Studio — Multi-Tenant Konfigüratör.
/// Mesh-parça eşleme, grup yönetimi, sahne preset CRUD.
/// Tüm endpointler JWT ile Admin/SuperAdmin korumalı.
/// Tenant izolasyonu: model → ürün zinciri üzerinden doğrulanır.
/// </summary>
[ApiController]
[Route("api/uc-boyut/admin")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class UcBoyutKonfiguratorAdminKontrolcu(
    VizitLink3DDbContext vt,
    IHubContext<SahneAyarHub> hub,
    IUcBoyutModelSahiplikDogrulayici sahiplikDogrulayici,
    KiraciServisi kiraciServisi) : ControllerBase
{
    // ================================================================
    // AGGREGATE — Tek seferde model + parça + grup + preset
    // ================================================================

    /// <summary>
    /// Seçilen model için tüm konfigürasyon verisini döndürür:
    /// model bilgisi, parçalar, gruplar, sahne önayarları.
    /// </summary>
    [HttpGet("modeller/{modelId:int}/toplu")]
    public async Task<Cevap<UcBoyutModelKonfigurasyonDto>> TopluGetir(int modelId)
    {
        if (!await sahiplikDogrulayici.ModelSahibiniDogrulaAsync(modelId))
            return Cevap<UcBoyutModelKonfigurasyonDto>.Hata("Bu modele erişim yetkiniz yok.");

        var model = await vt.UrunUcBoyutModelleri
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == modelId && !m.SilindiMi);

        if (model is null)
            return Cevap<UcBoyutModelKonfigurasyonDto>.Hata("3D model bulunamadı.");

        var parcalar = await vt.UrunUcBoyutParcalari
            .AsNoTracking()
            .Where(p => p.UrunUcBoyutModeliId == modelId && !p.SilindiMi)
            .OrderBy(p => p.SiraNo)
            .ToListAsync();

        var gruplar = await vt.UrunParcaGruplari
            .AsNoTracking()
            .Where(g => g.UrunId == model.UrunId && !g.SilindiMi && g.AktifMi)
            .OrderBy(g => g.SiraNo)
            .ToListAsync();

        var sahneler = await vt.UrunUcBoyutSahneOnayarlari
            .AsNoTracking()
            .Where(s => s.UrunUcBoyutModeliId == modelId && !s.SilindiMi)
            .OrderBy(s => s.SiraNo)
            .ToListAsync();

        var sonuc = new UcBoyutModelKonfigurasyonDto(
            model.Id, model.ModelAdi, model.ModelDosyaYolu, model.ModelTipi,
            model.UrunId, parcalar, gruplar, sahneler);

        return Cevap<UcBoyutModelKonfigurasyonDto>.Basarili(sonuc);
    }

    // ================================================================
    // PARÇA UPSERT — Toplu mesh-parça eşleştirme
    // ================================================================

    /// <summary>
    /// Toplu parça upsert: MeshAdi ile mevcut parçayı bulur,
    /// varsa günceller, yoksa yeni parça ekler.
    /// Eşleşmeyen mevcut parçalara dokunulmaz (silinmez).
    /// </summary>
    [HttpPut("modeller/{modelId:int}/parcalar/toplu")]
    public async Task<Cevap<UcBoyutParcaTopluUpsertSonucDto>> ParcaTopluUpsert(
        int modelId, [FromBody] UcBoyutParcaTopluUpsertDto istek)
    {
        if (!await sahiplikDogrulayici.ModelSahibiniDogrulaAsync(modelId))
            return Cevap<UcBoyutParcaTopluUpsertSonucDto>.Hata("Bu modele erişim yetkiniz yok.");

        // Model mevcut mu?
        var model = await vt.UrunUcBoyutModelleri
            .FirstOrDefaultAsync(m => m.Id == modelId && !m.SilindiMi);

        if (model is null)
            return Cevap<UcBoyutParcaTopluUpsertSonucDto>.Hata("3D model bulunamadı.");

        // Mevcut parçaları yükle (tracking)
        var mevcutParcalar = await vt.UrunUcBoyutParcalari
            .Where(p => p.UrunUcBoyutModeliId == modelId && !p.SilindiMi)
            .ToListAsync();

        // Mantıksal kod unique kontrolü için mevcut kodları topla
        // (gelen batch içindekiler + veritabanındakiler, kendi parçası hariç)
        var mevcutKodlar = mevcutParcalar
            .Where(p => !string.IsNullOrWhiteSpace(p.MantiksalKod))
            .Select(p => p.MantiksalKod!.ToUpperInvariant())
            .ToHashSet();

        int eklendi = 0, guncellendi = 0;
        var hatalar = new List<string>();
        var gelenKodlar = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var pd in istek.Parcalar)
        {
            var meshAdi = pd.MeshAdi.Trim();

            // Aynı batch içinde duplicate kontrol
            if (!gelenKodlar.Add(meshAdi.ToUpperInvariant()))
            {
                hatalar.Add($"Mesh '{meshAdi}' bu istekte birden fazla kez gönderilmiş, atlandı.");
                continue;
            }

            // Mantıksal kod batch içi duplicate
            if (!string.IsNullOrWhiteSpace(pd.MantiksalKod))
            {
                var kod = pd.MantiksalKod.Trim().ToUpperInvariant();
                if (!gelenKodlar.Add(kod))
                {
                    hatalar.Add($"Mantıksal kod '{pd.MantiksalKod}' batch içinde tekrar ediyor, atlandı.");
                    continue;
                }
                // Yeniden ekle (mesh adı için kullanılmıştı)
                gelenKodlar.Remove(kod);
                gelenKodlar.Add(kod);
            }

            var mevcut = mevcutParcalar.FirstOrDefault(p =>
                string.Equals(p.MeshAdi, meshAdi, StringComparison.OrdinalIgnoreCase));

            if (mevcut != null)
            {
                // Mantıksal kod unique kontrolü (kendi kodu hariç)
                if (!string.IsNullOrWhiteSpace(pd.MantiksalKod))
                {
                    var kodUpper = pd.MantiksalKod.Trim().ToUpperInvariant();
                    if (mevcutKodlar.Contains(kodUpper) &&
                        !string.Equals(mevcut.MantiksalKod, pd.MantiksalKod, StringComparison.OrdinalIgnoreCase))
                    {
                        hatalar.Add($"Mantıksal kod '{pd.MantiksalKod}' zaten kullanılıyor (parça: '{mevcut.GorunenAd}'), atlandı.");
                        continue;
                    }
                    if (mevcut.MantiksalKod != null)
                        mevcutKodlar.Remove(mevcut.MantiksalKod.ToUpperInvariant());
                    mevcutKodlar.Add(kodUpper);
                }

                // GÜNCELLE
                mevcut.GorunenAd = pd.GorunenAd;
                mevcut.MantiksalKod = pd.MantiksalKod?.Trim();
                mevcut.ParcaGrubuId = pd.ParcaGrubuId;
                mevcut.HareketTipi = pd.HareketTipi ?? string.Empty;
                mevcut.HareketAyarlariJson = pd.HareketAyarlariJson;
                mevcut.DokuUygulanabilirMi = pd.DokuUygulanabilirMi;
                mevcut.GizlenebilirMi = pd.GorunurlukDegisebilirMi;
                mevcut.RenklenebilirMi = pd.RenklenebilirMi;
                mevcut.MalzemeDegisebilirMi = pd.MalzemeDegisebilirMi;
                mevcut.SecilebilirMi = pd.SecilebilirMi;
                mevcut.HareketliMi = pd.HareketliMi;
                mevcut.ParcaTipi = pd.ParcaTipi;
                mevcut.MalzemeTipiKisiti = pd.MalzemeTipiKisiti;
                mevcut.SiraNo = pd.SiraNo;
                mevcut.AktifMi = pd.AktifMi;
                mevcut.AdminOnayliMi = pd.AdminOnayliMi;
                mevcut.GuncellenmeTarihi = DateTime.UtcNow;

                guncellendi++;
            }
            else
            {
                // Mantıksal kod unique kontrolü (tüm DB)
                if (!string.IsNullOrWhiteSpace(pd.MantiksalKod))
                {
                    var kodUpper = pd.MantiksalKod.Trim().ToUpperInvariant();
                    if (mevcutKodlar.Contains(kodUpper))
                    {
                        hatalar.Add($"Mantıksal kod '{pd.MantiksalKod}' zaten bu modelde kullanılıyor, atlandı.");
                        continue;
                    }
                    mevcutKodlar.Add(kodUpper);
                }

                // YENİ EKLE
                var yeni = new UrunUcBoyutParcasi
                {
                    UrunUcBoyutModeliId = modelId,
                    MeshAdi = meshAdi,
                    GorunenAd = pd.GorunenAd,
                    MantiksalKod = pd.MantiksalKod?.Trim(),
                    ParcaGrubuId = pd.ParcaGrubuId,
                    HareketTipi = pd.HareketTipi ?? string.Empty,
                    HareketAyarlariJson = pd.HareketAyarlariJson,
                    DokuUygulanabilirMi = pd.DokuUygulanabilirMi,
                    GizlenebilirMi = pd.GorunurlukDegisebilirMi,
                    RenklenebilirMi = pd.RenklenebilirMi,
                    MalzemeDegisebilirMi = pd.MalzemeDegisebilirMi,
                    SecilebilirMi = pd.SecilebilirMi,
                    HareketliMi = pd.HareketliMi,
                    ParcaTipi = pd.ParcaTipi,
                    MalzemeTipiKisiti = pd.MalzemeTipiKisiti,
                    SiraNo = pd.SiraNo,
                    AktifMi = pd.AktifMi,
                    AdminOnayliMi = pd.AdminOnayliMi,
                    OlusturulmaTarihi = DateTime.UtcNow
                };

                vt.UrunUcBoyutParcalari.Add(yeni);
                mevcutParcalar.Add(yeni); // batch içi sonraki referanslar için
                eklendi++;
            }
        }

        await vt.SaveChangesAsync();
        await hub.Clients.Group($"sahne_{modelId}").SendAsync("UcBoyutParcaTopluGuncellendi", modelId, eklendi, guncellendi);

        return Cevap<UcBoyutParcaTopluUpsertSonucDto>.Basarili(
            new UcBoyutParcaTopluUpsertSonucDto(eklendi, guncellendi, hatalar),
            $"Toplu parça işlemi: {eklendi} eklendi, {guncellendi} güncellendi.");
    }

    // ================================================================
    // GRUP CRUD
    // ================================================================

    [HttpGet("modeller/{modelId:int}/gruplar")]
    public async Task<Cevap<List<UrunParcaGrubu>>> GruplariGetir(int modelId)
    {
        if (!await sahiplikDogrulayici.ModelSahibiniDogrulaAsync(modelId))
            return Cevap<List<UrunParcaGrubu>>.Hata("Bu modele erişim yetkiniz yok.");

        var model = await vt.UrunUcBoyutModelleri
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == modelId && !m.SilindiMi);

        if (model is null)
            return Cevap<List<UrunParcaGrubu>>.Hata("3D model bulunamadı.");

        var gruplar = await vt.UrunParcaGruplari
            .AsNoTracking()
            .Where(g => g.UrunId == model.UrunId && !g.SilindiMi)
            .OrderBy(g => g.SiraNo)
            .ToListAsync();

        return Cevap<List<UrunParcaGrubu>>.Basarili(gruplar);
    }

    [HttpPost("modeller/{modelId:int}/gruplar")]
    public async Task<Cevap<UrunParcaGrubu>> GrupEkle(int modelId, [FromBody] UcBoyutGrupDto dto)
    {
        if (!await sahiplikDogrulayici.ModelSahibiniDogrulaAsync(modelId))
            return Cevap<UrunParcaGrubu>.Hata("Bu modele erişim yetkiniz yok.");

        var model = await vt.UrunUcBoyutModelleri
            .FirstOrDefaultAsync(m => m.Id == modelId && !m.SilindiMi);

        if (model is null)
            return Cevap<UrunParcaGrubu>.Hata("3D model bulunamadı.");

        var grup = new UrunParcaGrubu
        {
            UrunId = model.UrunId,
            FirmaId = kiraciServisi.MevcutFirmaId,
            Ad = dto.Ad,
            Aciklama = dto.Aciklama,
            SiraNo = dto.SiraNo,
            AktifMi = dto.AktifMi,
            OlusturulmaTarihi = DateTime.UtcNow
        };

        vt.UrunParcaGruplari.Add(grup);
        await vt.SaveChangesAsync();
        await hub.Clients.Group($"sahne_{modelId}").SendAsync("UcBoyutGrupGuncellendi", modelId, grup.Id);

        return Cevap<UrunParcaGrubu>.Basarili(grup, "Grup oluşturuldu.");
    }

    [HttpPut("gruplar/{id:int}")]
    public async Task<Cevap<UrunParcaGrubu>> GrupGuncelle(int id, [FromBody] UcBoyutGrupDto dto)
    {
        if (!await sahiplikDogrulayici.GrupSahibiniDogrulaAsync(id))
            return Cevap<UrunParcaGrubu>.Hata("Bu gruba erişim yetkiniz yok.");

        var mevcut = await vt.UrunParcaGruplari.FindAsync(id);
        if (mevcut is null || mevcut.SilindiMi)
            return Cevap<UrunParcaGrubu>.Hata("Grup bulunamadı.");

        mevcut.Ad = dto.Ad;
        mevcut.Aciklama = dto.Aciklama;
        mevcut.SiraNo = dto.SiraNo;
        mevcut.AktifMi = dto.AktifMi;
        mevcut.GuncellenmeTarihi = DateTime.UtcNow;

        await vt.SaveChangesAsync();
        await hub.Clients.Group($"urun_{mevcut.UrunId}").SendAsync("UcBoyutGrupGuncellendi", mevcut.UrunId, mevcut.Id);

        return Cevap<UrunParcaGrubu>.Basarili(mevcut, "Grup güncellendi.");
    }

    [HttpDelete("gruplar/{id:int}")]
    public async Task<Cevap<bool>> GrupSil(int id)
    {
        if (!await sahiplikDogrulayici.GrupSahibiniDogrulaAsync(id))
            return Cevap<bool>.Hata("Bu gruba erişim yetkiniz yok.");

        var mevcut = await vt.UrunParcaGruplari.FindAsync(id);
        if (mevcut is null || mevcut.SilindiMi)
            return Cevap<bool>.Hata("Grup bulunamadı.");

        mevcut.SilindiMi = true;
        mevcut.SilinmeTarihi = DateTime.UtcNow;

        await vt.SaveChangesAsync();
        await hub.Clients.Group($"urun_{mevcut.UrunId}").SendAsync("UcBoyutGrupSilindi", mevcut.UrunId, id);

        return Cevap<bool>.Basarili(true, "Grup silindi.");
    }

    // ================================================================
    // SAHNE ÖNAYARI CRUD
    // ================================================================

    [HttpGet("modeller/{modelId:int}/sahne-onayarlari")]
    public async Task<Cevap<List<UrunUcBoyutSahneOnayari>>> SahneOnayarlariniGetir(int modelId)
    {
        if (!await sahiplikDogrulayici.ModelSahibiniDogrulaAsync(modelId))
            return Cevap<List<UrunUcBoyutSahneOnayari>>.Hata("Bu modele erişim yetkiniz yok.");

        var model = await vt.UrunUcBoyutModelleri
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == modelId && !m.SilindiMi);

        if (model is null)
            return Cevap<List<UrunUcBoyutSahneOnayari>>.Hata("3D model bulunamadı.");

        var onayarlar = await vt.UrunUcBoyutSahneOnayarlari
            .AsNoTracking()
            .Where(s => s.UrunUcBoyutModeliId == modelId && !s.SilindiMi)
            .OrderBy(s => s.SiraNo)
            .ToListAsync();

        return Cevap<List<UrunUcBoyutSahneOnayari>>.Basarili(onayarlar);
    }

    [HttpPost("modeller/{modelId:int}/sahne-onayarlari")]
    public async Task<Cevap<UrunUcBoyutSahneOnayari>> SahneOnayariEkle(
        int modelId, [FromBody] UcBoyutSahneOnayariDto dto)
    {
        if (!await sahiplikDogrulayici.ModelSahibiniDogrulaAsync(modelId))
            return Cevap<UrunUcBoyutSahneOnayari>.Hata("Bu modele erişim yetkiniz yok.");

        var model = await vt.UrunUcBoyutModelleri
            .FirstOrDefaultAsync(m => m.Id == modelId && !m.SilindiMi);

        if (model is null)
            return Cevap<UrunUcBoyutSahneOnayari>.Hata("3D model bulunamadı.");

        // Kod unique kontrolü (model içinde)
        var kodVar = await vt.UrunUcBoyutSahneOnayarlari
            .AnyAsync(s => s.UrunUcBoyutModeliId == modelId &&
                           s.Kod == dto.Kod && !s.SilindiMi);
        if (kodVar)
            return Cevap<UrunUcBoyutSahneOnayari>.Hata($"'{dto.Kod}' kodu bu modelde zaten kullanılıyor.");

        // Varsayılan seçildiyse diğerlerini kaldır
        if (dto.VarsayilanMi)
        {
            var mevcutVarsayilan = await vt.UrunUcBoyutSahneOnayarlari
                .Where(s => s.UrunUcBoyutModeliId == modelId && s.VarsayilanMi && !s.SilindiMi)
                .ToListAsync();
            foreach (var mv in mevcutVarsayilan)
                mv.VarsayilanMi = false;
        }

        var onayar = new UrunUcBoyutSahneOnayari
        {
            UrunUcBoyutModeliId = modelId,
            Ad = dto.Ad,
            Kod = dto.Kod,
            AyarlarJson = dto.AyarlarJson,
            VarsayilanMi = dto.VarsayilanMi,
            AktifMi = dto.AktifMi,
            SiraNo = dto.SiraNo,
            OlusturulmaTarihi = DateTime.UtcNow
        };

        vt.UrunUcBoyutSahneOnayarlari.Add(onayar);
        await vt.SaveChangesAsync();
        await hub.Clients.Group($"sahne_{modelId}").SendAsync("UcBoyutSahneOnayariGuncellendi", modelId, onayar.Id);

        return Cevap<UrunUcBoyutSahneOnayari>.Basarili(onayar, "Sahne önayarı oluşturuldu.");
    }

    [HttpPut("sahne-onayarlari/{id:int}")]
    public async Task<Cevap<UrunUcBoyutSahneOnayari>> SahneOnayariGuncelle(
        int id, [FromBody] UcBoyutSahneOnayariDto dto)
    {
        if (!await sahiplikDogrulayici.SahneOnayariSahibiniDogrulaAsync(id))
            return Cevap<UrunUcBoyutSahneOnayari>.Hata("Bu sahne önayarına erişim yetkiniz yok.");

        var mevcut = await vt.UrunUcBoyutSahneOnayarlari.FindAsync(id);
        if (mevcut is null || mevcut.SilindiMi)
            return Cevap<UrunUcBoyutSahneOnayari>.Hata("Sahne önayarı bulunamadı.");

        // Kod unique kontrolü (kendi kodu hariç)
        var kodVar = await vt.UrunUcBoyutSahneOnayarlari
            .AnyAsync(s => s.UrunUcBoyutModeliId == mevcut.UrunUcBoyutModeliId &&
                           s.Kod == dto.Kod && s.Id != id && !s.SilindiMi);
        if (kodVar)
            return Cevap<UrunUcBoyutSahneOnayari>.Hata($"'{dto.Kod}' kodu bu modelde zaten kullanılıyor.");

        // Varsayılan değişikliği
        if (dto.VarsayilanMi && !mevcut.VarsayilanMi)
        {
            var digerVarsayilanlar = await vt.UrunUcBoyutSahneOnayarlari
                .Where(s => s.UrunUcBoyutModeliId == mevcut.UrunUcBoyutModeliId &&
                            s.VarsayilanMi && s.Id != id && !s.SilindiMi)
                .ToListAsync();
            foreach (var dv in digerVarsayilanlar)
                dv.VarsayilanMi = false;
        }

        mevcut.Ad = dto.Ad;
        mevcut.Kod = dto.Kod;
        mevcut.AyarlarJson = dto.AyarlarJson;
        mevcut.VarsayilanMi = dto.VarsayilanMi;
        mevcut.AktifMi = dto.AktifMi;
        mevcut.SiraNo = dto.SiraNo;
        mevcut.GuncellenmeTarihi = DateTime.UtcNow;

        await vt.SaveChangesAsync();
        await hub.Clients.Group($"sahne_{mevcut.UrunUcBoyutModeliId}").SendAsync(
            "UcBoyutSahneOnayariGuncellendi", mevcut.UrunUcBoyutModeliId, mevcut.Id);

        return Cevap<UrunUcBoyutSahneOnayari>.Basarili(mevcut, "Sahne önayarı güncellendi.");
    }

    [HttpDelete("sahne-onayarlari/{id:int}")]
    public async Task<Cevap<bool>> SahneOnayariSil(int id)
    {
        if (!await sahiplikDogrulayici.SahneOnayariSahibiniDogrulaAsync(id))
            return Cevap<bool>.Hata("Bu sahne önayarına erişim yetkiniz yok.");

        var mevcut = await vt.UrunUcBoyutSahneOnayarlari.FindAsync(id);
        if (mevcut is null || mevcut.SilindiMi)
            return Cevap<bool>.Hata("Sahne önayarı bulunamadı.");

        // Varsayılan siliniyorsa, sonraki ilk aktif olanı varsayılan yap
        var varsayilanMi = mevcut.VarsayilanMi;
        var modelId = mevcut.UrunUcBoyutModeliId;

        mevcut.SilindiMi = true;
        mevcut.SilinmeTarihi = DateTime.UtcNow;

        if (varsayilanMi)
        {
            var yeniVarsayilan = await vt.UrunUcBoyutSahneOnayarlari
                .Where(s => s.UrunUcBoyutModeliId == modelId && !s.SilindiMi && s.Id != id && s.AktifMi)
                .OrderBy(s => s.SiraNo)
                .FirstOrDefaultAsync();
            if (yeniVarsayilan != null)
                yeniVarsayilan.VarsayilanMi = true;
        }

        await vt.SaveChangesAsync();
        await hub.Clients.Group($"sahne_{modelId}").SendAsync("UcBoyutSahneOnayariSilindi", modelId, id);

        return Cevap<bool>.Basarili(true, "Sahne önayarı silindi.");
    }
}
