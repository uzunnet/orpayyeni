using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Malzemeler;
using VizitLink3D.Ortak.Modeller.Renkler;
using VizitLink3D.Ortak.Modeller.Urunler;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Api.Servisler;

namespace VizitLink3D.Api.Moduller.Urunler.Kontrolcüler;

[ApiController]
[Route("api/urunler")]
public class UrunlerKontrolcu(VizitLink3DDbContext vt, IOtomatikCeviriServisi otomatikCeviri) : ControllerBase
{
    [HttpGet]
    public async Task<Cevap<List<Urun>>> Liste(
        [FromQuery] int? urunAilesiId = null,
        [FromQuery] int? kategoriId = null,
        [FromQuery] string? arama = null,
        [FromQuery] bool? oneCikan = null,
        [FromQuery] string? dil = null)
    {
        var query = vt.Urunler
            .AsNoTracking()
            .Where(u => !u.SilindiMi);

        if (urunAilesiId.HasValue)
        {
            query = query.Where(u => u.UrunAilesiId == urunAilesiId.Value);
        }

        if (kategoriId.HasValue)
        {
            query = query.Where(u => u.UrunKategoriId == kategoriId.Value);
        }

        if (oneCikan.HasValue)
        {
            query = query.Where(u => u.OneCikanMi == oneCikan.Value);
        }

        if (!string.IsNullOrWhiteSpace(arama))
        {
            var cleanArama = arama.ToLower();
            query = query.Where(u => u.Ad.ToLower().Contains(cleanArama) || u.Kod.ToLower().Contains(cleanArama) || u.Slug.ToLower().Contains(cleanArama));
        }

        var liste = await query
            .OrderByDescending(u => u.OlusturulmaTarihi)
            .ToListAsync();

        var kodlar = liste.Select(u => u.Kod).ToList();
        var kapakUrlleri = await vt.KapakModelleri
            .AsNoTracking()
            .Where(k => !k.SilindiMi && kodlar.Contains(k.ModelKodu) && !string.IsNullOrWhiteSpace(k.AnaGorselUrl))
            .Select(k => new { k.ModelKodu, k.AnaGorselUrl })
            .ToListAsync();
        var kapakUrlSozlugu = kapakUrlleri
            .GroupBy(k => k.ModelKodu)
            .ToDictionary(g => g.Key, g => g.First().AnaGorselUrl!);

        foreach (var urun in liste)
        {
            if (kapakUrlSozlugu.TryGetValue(urun.Kod, out var url) && url.StartsWith("/api/medya/dosya/"))
            {
                if (long.TryParse(url.Split('/').Last(), out var medyaId) && medyaId > 0)
                    urun.AnaGorselMedyaId = medyaId;
            }
        }

        if (!string.IsNullOrWhiteSpace(dil) && !dil.Equals("tr", StringComparison.OrdinalIgnoreCase))
        {
            var aktifDil = dil.ToLowerInvariant();
            var yerellestirmeler = await vt.UrunYerellestirmeleri
                .Where(y => y.Dil == aktifDil)
                .ToListAsync();

            var yerellestirmeSozlugu = yerellestirmeler
                .GroupBy(y => y.UrunId)
                .ToDictionary(g => g.Key, g => g.First());

            var missingProducts = liste.Where(u => !yerellestirmeSozlugu.ContainsKey(u.Id)).ToList();
            if (missingProducts.Any())
            {
                var newYerellestirmeler = new List<UrunYerellestirme>();
                foreach (var urun in missingProducts)
                {
                    var adCeviri = await otomatikCeviri.CevirAsync(urun.Ad, "tr", aktifDil);
                    var kisaAciklamaCeviri = await otomatikCeviri.CevirAsync(urun.KisaAciklama ?? "", "tr", aktifDil);
                    var aciklamaCeviri = await otomatikCeviri.CevirAsync(urun.Aciklama ?? "", "tr", aktifDil);
                    var seoBaslikCeviri = await otomatikCeviri.CevirAsync(urun.SeoBaslik ?? "", "tr", aktifDil);
                    var seoAciklamaCeviri = await otomatikCeviri.CevirAsync(urun.SeoAciklama ?? "", "tr", aktifDil);

                    var y = new UrunYerellestirme
                    {
                        UrunId = urun.Id,
                        Dil = aktifDil,
                        Ad = adCeviri.Veri ?? urun.Ad,
                        KisaAciklama = kisaAciklamaCeviri.Veri ?? urun.KisaAciklama,
                        Aciklama = aciklamaCeviri.Veri ?? urun.Aciklama,
                        SeoBaslik = seoBaslikCeviri.Veri ?? urun.SeoBaslik,
                        SeoAciklama = seoAciklamaCeviri.Veri ?? urun.SeoAciklama
                    };
                    newYerellestirmeler.Add(y);
                    yerellestirmeSozlugu[urun.Id] = y;
                }

                if (newYerellestirmeler.Any())
                {
                    vt.UrunYerellestirmeleri.AddRange(newYerellestirmeler);
                    await vt.SaveChangesAsync();
                }
            }

            foreach (var urun in liste)
            {
                if (yerellestirmeSozlugu.TryGetValue(urun.Id, out var y))
                {
                    if (!string.IsNullOrWhiteSpace(y.Ad)) urun.Ad = y.Ad;
                    if (!string.IsNullOrWhiteSpace(y.KisaAciklama)) urun.KisaAciklama = y.KisaAciklama;
                    if (!string.IsNullOrWhiteSpace(y.Aciklama)) urun.Aciklama = y.Aciklama;
                    if (!string.IsNullOrWhiteSpace(y.SeoBaslik)) urun.SeoBaslik = y.SeoBaslik;
                    if (!string.IsNullOrWhiteSpace(y.SeoAciklama)) urun.SeoAciklama = y.SeoAciklama;
                }
            }
        }

        return Cevap<List<Urun>>.Basarili(liste);
    }

    [HttpGet("{id:int}")]
    public async Task<Cevap<Urun>> Detay(int id, [FromQuery] string? dil = null)
    {
        var urun = await vt.Urunler
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id && !u.SilindiMi);

        if (urun is null)
            return Cevap<Urun>.Hata("Urun bulunamadi.");

        if (!string.IsNullOrWhiteSpace(dil) && !dil.Equals("tr", StringComparison.OrdinalIgnoreCase))
        {
            await UrunYerellestirAsync(urun, dil);
        }

        return Cevap<Urun>.Basarili(urun);
    }

    [HttpGet("slug/{slug}")]
    public async Task<Cevap<Urun>> SlugIle(string slug, [FromQuery] string? dil = null)
    {
        var urun = await vt.Urunler
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Slug == slug && !u.SilindiMi);

        if (urun is null)
            return Cevap<Urun>.Hata("Urun bulunamadi.");

        if (!string.IsNullOrWhiteSpace(dil) && !dil.Equals("tr", StringComparison.OrdinalIgnoreCase))
        {
            await UrunYerellestirAsync(urun, dil);
        }

        return Cevap<Urun>.Basarili(urun);
    }

    private async Task UrunYerellestirAsync(Urun urun, string dil)
    {
        var aktifDil = dil.ToLowerInvariant();
        var y = await vt.UrunYerellestirmeleri
            .FirstOrDefaultAsync(x => x.UrunId == urun.Id && x.Dil == aktifDil);

        if (y == null)
        {
            var adCeviri = await otomatikCeviri.CevirAsync(urun.Ad, "tr", aktifDil);
            var kisaAciklamaCeviri = await otomatikCeviri.CevirAsync(urun.KisaAciklama ?? "", "tr", aktifDil);
            var aciklamaCeviri = await otomatikCeviri.CevirAsync(urun.Aciklama ?? "", "tr", aktifDil);
            var seoBaslikCeviri = await otomatikCeviri.CevirAsync(urun.SeoBaslik ?? "", "tr", aktifDil);
            var seoAciklamaCeviri = await otomatikCeviri.CevirAsync(urun.SeoAciklama ?? "", "tr", aktifDil);

            y = new UrunYerellestirme
            {
                UrunId = urun.Id,
                Dil = aktifDil,
                Ad = adCeviri.Veri ?? urun.Ad,
                KisaAciklama = kisaAciklamaCeviri.Veri ?? urun.KisaAciklama,
                Aciklama = aciklamaCeviri.Veri ?? urun.Aciklama,
                SeoBaslik = seoBaslikCeviri.Veri ?? urun.SeoBaslik,
                SeoAciklama = seoAciklamaCeviri.Veri ?? urun.SeoAciklama
            };

            vt.UrunYerellestirmeleri.Add(y);
            await vt.SaveChangesAsync();
        }

        if (!string.IsNullOrWhiteSpace(y.Ad)) urun.Ad = y.Ad;
        if (!string.IsNullOrWhiteSpace(y.KisaAciklama)) urun.KisaAciklama = y.KisaAciklama;
        if (!string.IsNullOrWhiteSpace(y.Aciklama)) urun.Aciklama = y.Aciklama;
        if (!string.IsNullOrWhiteSpace(y.SeoBaslik)) urun.SeoBaslik = y.SeoBaslik;
        if (!string.IsNullOrWhiteSpace(y.SeoAciklama)) urun.SeoAciklama = y.SeoAciklama;
    }

    [HttpGet("{id:int}/uc-boyut-modelleri")]
    public async Task<Cevap<List<UrunUcBoyutModeli>>> UcBoyutModelleri(int id)
    {
        var modeller = await vt.UrunUcBoyutModelleri
            .AsNoTracking()
            .Where(m => m.UrunId == id && m.AktifMi && !m.SilindiMi)
            .OrderByDescending(m => m.VarsayilanMi)
            .ThenBy(m => m.OlusturulmaTarihi)
            .ToListAsync();

        return Cevap<List<UrunUcBoyutModeli>>.Basarili(modeller);
    }

    [HttpGet("{id:int}/parcalar")]
    public async Task<Cevap<List<UrunUcBoyutParcasi>>> UcBoyutParcalari(int id)
    {
        var modelId = await VarsayilanModelIdGetir(id);
        if (modelId is null)
            return Cevap<List<UrunUcBoyutParcasi>>.Basarili([]);

        var parcalar = await vt.UrunUcBoyutParcalari
            .AsNoTracking()
            .Where(p => p.UrunUcBoyutModeliId == modelId && p.AktifMi && !p.SilindiMi)
            .OrderBy(p => p.SiraNo)
            .ThenBy(p => p.GorunenAd)
            .ToListAsync();

        return Cevap<List<UrunUcBoyutParcasi>>.Basarili(parcalar);
    }

    [HttpGet("{id:int}/renkler")]
    public async Task<Cevap<List<RalRengi>>> UcBoyutRenkleri(int id)
    {
        var modelId = await VarsayilanModelIdGetir(id);
        if (modelId is null)
            return await AktifRalRenkleriGetir();

        var parcaIdleri = await vt.UrunUcBoyutParcalari
            .AsNoTracking()
            .Where(p => p.UrunUcBoyutModeliId == modelId && p.AktifMi && !p.SilindiMi && p.RenklenebilirMi)
            .Select(p => p.Id)
            .ToListAsync();

        var renkIdleri = await vt.UrunParcaRenkSecenekleri
            .AsNoTracking()
            .Where(s => s.AktifMi && s.RalRengiId.HasValue && parcaIdleri.Contains(s.UrunUcBoyutParcasiId))
            .Select(s => s.RalRengiId!.Value)
            .Distinct()
            .ToListAsync();

        if (renkIdleri.Count == 0)
            return await AktifRalRenkleriGetir();

        var renkler = await vt.RalRenkleri
            .AsNoTracking()
            .Where(r => r.AktifMi && renkIdleri.Contains(r.Id))
            .OrderBy(r => r.SiraNo)
            .ThenBy(r => r.Kod)
            .ToListAsync();

        return Cevap<List<RalRengi>>.Basarili(renkler);
    }

    [HttpGet("{id:int}/medyalar")]
    public async Task<Cevap<List<UrunMedya>>> Medyalar(int id)
    {
        var medyalar = await vt.UrunMedyalari
            .AsNoTracking()
            .Where(m => m.UrunId == id && !m.SilindiMi)
            .OrderBy(m => m.SiraNo)
            .ToListAsync();

        return Cevap<List<UrunMedya>>.Basarili(medyalar);
    }

    [HttpPost("{id:int}/medya")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<UrunMedya>> MedyaEkle(int id, [FromBody] UrunMedyaEkleIstek istek)
    {
        var urunVarMi = await vt.Urunler.AnyAsync(u => u.Id == id && !u.SilindiMi);
        if (!urunVarMi)
            return Cevap<UrunMedya>.Hata("Urun bulunamadi.");

        if (string.IsNullOrWhiteSpace(istek.MedyaUrl))
            return Cevap<UrunMedya>.Hata("Medya havuz yolu zorunludur.");

        var havuzYolu = istek.MedyaUrl.Trim();
        var havuzdanMi = havuzYolu.StartsWith("/api/medya/dosya/", StringComparison.OrdinalIgnoreCase)
            || havuzYolu.StartsWith("api/medya/dosya/", StringComparison.OrdinalIgnoreCase);

        if (!havuzdanMi)
            return Cevap<UrunMedya>.Hata("Medya sadece havuzdan eklenebilir.");

        var mevcut = await vt.UrunMedyalari
            .FirstOrDefaultAsync(m =>
                m.UrunId == id &&
                m.MedyaUrl == havuzYolu &&
                m.MedyaTuru == istek.MedyaTuru &&
                !m.SilindiMi);

        if (mevcut is not null)
            return Cevap<UrunMedya>.Basarili(mevcut, "Medya zaten ekli.");

        var medya = new UrunMedya
        {
            UrunId = id,
            MedyaUrl = havuzYolu,
            MedyaTuru = istek.MedyaTuru,
            SiraNo = istek.SiraNo > 0
                ? istek.SiraNo
                : await vt.UrunMedyalari.CountAsync(m => m.UrunId == id && !m.SilindiMi) + 1
        };
        vt.UrunMedyalari.Add(medya);
        await vt.SaveChangesAsync();

        return Cevap<UrunMedya>.Basarili(medya, "Medya eklendi.");
    }

    [HttpPut("{id:int}/medyalar/{medyaId:int}/sira")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<UrunMedya>> MedyaSirasiGuncelle(int id, int medyaId, [FromBody] UrunMedya istek)
    {
        var medya = await vt.UrunMedyalari.FirstOrDefaultAsync(m => m.Id == medyaId && m.UrunId == id);
        if (medya is null)
            return Cevap<UrunMedya>.Hata("Urun medyasi bulunamadi.");

        medya.SiraNo = Math.Max(1, istek.SiraNo);

        await vt.SaveChangesAsync();
        return Cevap<UrunMedya>.Basarili(medya, "Medya sirası guncellendi.");
    }

    [HttpDelete("{id:int}/medyalar/{medyaId:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<bool>> MedyaSil(int id, int medyaId)
    {
        var medya = await vt.UrunMedyalari.FirstOrDefaultAsync(m => m.Id == medyaId && m.UrunId == id);
        if (medya is null)
            return Cevap<bool>.Hata("Urun medyasi bulunamadi.");

        medya.SilindiMi = true;
        await vt.SaveChangesAsync();
        return Cevap<bool>.Basarili(true, "Medya kaldirildi.");
    }

    private async Task<int?> VarsayilanModelIdGetir(int urunId)
    {
        var urun = await vt.Urunler
            .AsNoTracking()
            .Where(u => u.Id == urunId && u.AktifMi)
            .Select(u => new { u.Id, u.VarsayilanUcBoyutModeliId })
            .FirstOrDefaultAsync();

        if (urun is null)
            return null;

        if (urun.VarsayilanUcBoyutModeliId.HasValue)
        {
            var varsayilanVarMi = await vt.UrunUcBoyutModelleri
                .AsNoTracking()
                .AnyAsync(m => m.Id == urun.VarsayilanUcBoyutModeliId.Value && m.UrunId == urunId && m.AktifMi && !m.SilindiMi);

            if (varsayilanVarMi)
                return urun.VarsayilanUcBoyutModeliId.Value;
        }

        return await vt.UrunUcBoyutModelleri
            .AsNoTracking()
            .Where(m => m.UrunId == urunId && m.AktifMi && !m.SilindiMi)
            .OrderByDescending(m => m.VarsayilanMi)
            .ThenBy(m => m.OlusturulmaTarihi)
            .Select(m => (int?)m.Id)
            .FirstOrDefaultAsync();
    }

    private async Task<Cevap<List<RalRengi>>> AktifRalRenkleriGetir()
    {
        var renkler = await vt.RalRenkleri
            .AsNoTracking()
            .Where(r => r.AktifMi)
            .OrderBy(r => r.SiraNo)
            .ThenBy(r => r.Kod)
            .ToListAsync();

        return Cevap<List<RalRengi>>.Basarili(renkler);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<Urun>> Olustur([FromBody] Urun urun)
    {
        vt.Urunler.Add(urun);
        await vt.SaveChangesAsync();

        return Cevap<Urun>.Basarili(urun, "Urun olusturuldu.");
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<Urun>> Guncelle(int id, [FromBody] Urun urun)
    {
        var mevcut = await vt.Urunler.FindAsync(id);
        if (mevcut is null || mevcut.SilindiMi)
            return Cevap<Urun>.Hata("Urun bulunamadi.");

        mevcut.Ad = urun.Ad;
        mevcut.Slug = urun.Slug;
        mevcut.Kod = urun.Kod;
        mevcut.KisaAciklama = urun.KisaAciklama;
        mevcut.Aciklama = urun.Aciklama;
        mevcut.UrunAilesiId = urun.UrunAilesiId;
        mevcut.UrunKategoriId = urun.UrunKategoriId;
        mevcut.AktifMi = urun.AktifMi;
        mevcut.OneCikanMi = urun.OneCikanMi;
        mevcut.YeniMi = urun.YeniMi;
        mevcut.Fiyat = urun.Fiyat;
        mevcut.Birim = urun.Birim;
        mevcut.SiraNo = urun.SiraNo;
        mevcut.AnaGorselMedyaId = urun.AnaGorselMedyaId;
        mevcut.VarsayilanUcBoyutModeliId = urun.VarsayilanUcBoyutModeliId;
        mevcut.SeoBaslik = urun.SeoBaslik;
        mevcut.SeoAciklama = urun.SeoAciklama;
        mevcut.GuncellenmeTarihi = DateTime.UtcNow;

        await vt.SaveChangesAsync();
        return Cevap<Urun>.Basarili(mevcut, "Urun guncellendi.");
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<bool>> Sil(int id)
    {
        var mevcut = await vt.Urunler.FindAsync(id);
        if (mevcut is null || mevcut.SilindiMi)
            return Cevap<bool>.Hata("Urun bulunamadi.");

        mevcut.SilindiMi = true;
        mevcut.SilinmeTarihi = DateTime.UtcNow;
        mevcut.GuncellenmeTarihi = DateTime.UtcNow;

        await vt.SaveChangesAsync();
        return Cevap<bool>.Basarili(true, "Urun silindi.");
    }

    /// <summary>
    /// Benzer ürünler — aynı kategori veya aynı ürün ailesi, maks {adet} adet.
    /// </summary>
    [HttpGet("{id:int}/benzer")]
    public async Task<Cevap<List<Urun>>> BenzerUrunler(int id, [FromQuery] int adet = 8, [FromQuery] string? dil = null)
    {
        var kaynak = await vt.Urunler
            .AsNoTracking()
            .Where(u => u.Id == id && !u.SilindiMi)
            .Select(u => new { u.Kod, u.UrunKategoriId, u.UrunAilesiId })
            .FirstOrDefaultAsync();

        if (kaynak is null)
            return Cevap<List<Urun>>.Basarili([]);

        // Ayni urun ailesi/kategori tum katalog urunlerinde ozdes oldugundan (orn. hepsi
        // UrunAilesiId=3), gercek benzerlik icin Kod'un sondaki "-<boy>" ekini atarak
        // model adini (orn. "HERMES-120" -> "HERMES") cikarip ayni model olanlari eslestiriyoruz.
        var modelOneki = System.Text.RegularExpressions.Regex.Replace(
            kaynak.Kod ?? string.Empty, @"-\d+[A-Z]?$", string.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        var benzerler = !string.IsNullOrWhiteSpace(modelOneki)
            ? await vt.Urunler
                .AsNoTracking()
                .Where(u => !u.SilindiMi && u.AktifMi && u.Id != id && u.Kod.StartsWith(modelOneki + "-"))
                .OrderByDescending(u => u.OneCikanMi)
                .ThenBy(u => u.SiraNo)
                .Take(adet)
                .ToListAsync()
            : [];

        if (!string.IsNullOrWhiteSpace(dil) && !dil.Equals("tr", StringComparison.OrdinalIgnoreCase))
        {
            var aktifDil = dil.ToLowerInvariant();
            var yIds = benzerler.Select(u => u.Id).ToList();
            var yer = await vt.UrunYerellestirmeleri
                .AsNoTracking()
                .Where(y => yIds.Contains(y.UrunId) && y.Dil == aktifDil)
                .ToDictionaryAsync(y => y.UrunId);

            foreach (var u in benzerler)
            {
                if (yer.TryGetValue(u.Id, out var y))
                {
                    if (!string.IsNullOrWhiteSpace(y.Ad)) u.Ad = y.Ad;
                    if (!string.IsNullOrWhiteSpace(y.KisaAciklama)) u.KisaAciklama = y.KisaAciklama;
                }
            }
        }

        return Cevap<List<Urun>>.Basarili(benzerler);
    }

    /// <summary>
    /// En çok gezilen ürünler — ZiyaretKaydi tablosundan /urun/* URL'lerini sayar.
    /// Hiç kayıt yoksa öne çıkan ürünleri döndürür.
    /// </summary>
    [HttpGet("en-cok-gezilen")]
    public async Task<Cevap<List<Urun>>> EnCokGezilen([FromQuery] int adet = 6, [FromQuery] string? dil = null)
    {
        var enCokGezilen = await vt.ZiyaretKayitlari
            .AsNoTracking()
            .Where(z => z.Sayfa != null && z.Sayfa.StartsWith("/banyo-dolabi/"))
            .GroupBy(z => z.Sayfa!)
            .OrderByDescending(g => g.Count())
            .Take(adet * 2)
            .Select(g => g.Key)
            .ToListAsync();

        var sluglar = enCokGezilen
            .Select(url => url.Replace("/banyo-dolabi/", "").Trim('/'))
            .Where(s => !string.IsNullOrEmpty(s))
            .Distinct()
            .ToList();

        List<Urun> urunler;
        if (sluglar.Any())
        {
            urunler = await vt.Urunler
                .AsNoTracking()
                .Where(u => !u.SilindiMi && u.AktifMi && sluglar.Contains(u.Slug))
                .Take(adet)
                .ToListAsync();
        }
        else
        {
            urunler = await vt.Urunler
                .AsNoTracking()
                .Where(u => !u.SilindiMi && u.AktifMi && u.OneCikanMi)
                .Take(adet)
                .ToListAsync();
        }

        if (!string.IsNullOrWhiteSpace(dil) && !dil.Equals("tr", StringComparison.OrdinalIgnoreCase))
        {
            var aktifDil = dil.ToLowerInvariant();
            var yIds = urunler.Select(u => u.Id).ToList();
            var yer = await vt.UrunYerellestirmeleri
                .AsNoTracking()
                .Where(y => yIds.Contains(y.UrunId) && y.Dil == aktifDil)
                .ToDictionaryAsync(y => y.UrunId);

            foreach (var u in urunler)
            {
                if (yer.TryGetValue(u.Id, out var y))
                {
                    if (!string.IsNullOrWhiteSpace(y.Ad)) u.Ad = y.Ad;
                    if (!string.IsNullOrWhiteSpace(y.KisaAciklama)) u.KisaAciklama = y.KisaAciklama;
                }
            }
        }

        return Cevap<List<Urun>>.Basarili(urunler);
    }

    [HttpGet("one-cikanlar")]
    public async Task<Cevap<List<Urun>>> OneCikanlar([FromQuery] int adet = 6, [FromQuery] string? dil = "tr")
    {
        var urunler = await vt.Urunler
            .AsNoTracking()
            .Where(u => !u.SilindiMi && u.AktifMi && u.OneCikanMi)
            .OrderByDescending(u => u.GuncellenmeTarihi)
            .Take(adet)
            .ToListAsync();

        if (!string.IsNullOrWhiteSpace(dil) && !dil.Equals("tr", StringComparison.OrdinalIgnoreCase))
        {
            var aktifDil = dil.ToLowerInvariant();
            var yIds = urunler.Select(u => u.Id).ToList();
            var yer = await vt.UrunYerellestirmeleri
                .AsNoTracking()
                .Where(y => yIds.Contains(y.UrunId) && y.Dil == aktifDil)
                .ToDictionaryAsync(y => y.UrunId);

            foreach (var u in urunler)
            {
                if (yer.TryGetValue(u.Id, out var y))
                {
                    if (!string.IsNullOrWhiteSpace(y.Ad)) u.Ad = y.Ad;
                    if (!string.IsNullOrWhiteSpace(y.KisaAciklama)) u.KisaAciklama = y.KisaAciklama;
                }
            }
        }

        return Cevap<List<Urun>>.Basarili(urunler);
    }

    /// <summary>
    /// Admin onaylı parçaları getirir — kullanıcı konfigüratör arayüzü için.
    /// </summary>
    [HttpGet("{id:int}/parcalar/onayli")]
    public async Task<Cevap<List<UrunUcBoyutParcasi>>> OnayliParcalar(int id)
    {
        var modelId = await VarsayilanModelIdGetir(id);
        if (modelId is null)
            return Cevap<List<UrunUcBoyutParcasi>>.Basarili([]);

        var parcalar = await vt.UrunUcBoyutParcalari
            .AsNoTracking()
            .Where(p => p.UrunUcBoyutModeliId == modelId && p.AktifMi && !p.SilindiMi && p.AdminOnayliMi)
            .OrderBy(p => p.SiraNo)
            .ThenBy(p => p.GorunenAd)
            .ToListAsync();

        return Cevap<List<UrunUcBoyutParcasi>>.Basarili(parcalar);
    }

    /// <summary>
    /// Parça tipine veya parça ID'sine göre filtrelenmiş malzemeleri döndürür.
    /// Admin MalzemeTipiKisiti JSON'ından okunan tiplere göre filtreler.
    /// </summary>
    [HttpGet("malzemeler-parca-tipi")]
    public async Task<Cevap<List<Malzeme>>> MalzemeleriParcaTipineGore(
        [FromQuery] string? parcaTipi = null,
        [FromQuery] int? parcaId = null)
    {
        if (parcaId.HasValue)
        {
            var parca = await vt.UrunUcBoyutParcalari
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == parcaId.Value && p.AktifMi && !p.SilindiMi);

            if (parca?.MalzemeTipiKisiti != null)
            {
                try
                {
                    var kisitlar = System.Text.Json.JsonSerializer
                        .Deserialize<List<string>>(parca.MalzemeTipiKisiti) ?? [];

                    if (kisitlar.Count > 0)
                    {
                        var malzemeler = await vt.Malzemeler
                            .AsNoTracking()
                            .Where(m => m.AktifMi && kisitlar.Contains(m.Tip))
                            .OrderBy(m => m.SiraNo)
                            .ToListAsync();
                        return Cevap<List<Malzeme>>.Basarili(malzemeler);
                    }
                }
                catch { /* JSON parse hatasi — tumumunu dondur */ }
            }

            // Fallback: If MalzemeTipiKisiti is empty/null, use the part's native ParcaTipi
            if (!string.IsNullOrWhiteSpace(parca?.ParcaTipi))
            {
                var malzemeler = await vt.Malzemeler
                    .AsNoTracking()
                    .Where(m => m.AktifMi && m.Tip == parca.ParcaTipi)
                    .OrderBy(m => m.SiraNo)
                    .ToListAsync();
                
                if (malzemeler.Count > 0)
                    return Cevap<List<Malzeme>>.Basarili(malzemeler);
            }
        }

        if (!string.IsNullOrWhiteSpace(parcaTipi))
        {
            var malzemeler = await vt.Malzemeler
                .AsNoTracking()
                .Where(m => m.AktifMi && m.Tip == parcaTipi)
                .OrderBy(m => m.SiraNo)
                .ToListAsync();
            return Cevap<List<Malzeme>>.Basarili(malzemeler);
        }

        var tumMalzemeler = await vt.Malzemeler
            .AsNoTracking()
            .Where(m => m.AktifMi)
            .OrderBy(m => m.SiraNo)
            .ToListAsync();
        return Cevap<List<Malzeme>>.Basarili(tumMalzemeler);
    }
}

public sealed record UrunMedyaEkleIstek(string MedyaUrl, string MedyaTuru, int SiraNo);
