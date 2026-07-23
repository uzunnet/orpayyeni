using VizitLink3D.Api.Moduller.AI.Servisler;
using VizitLink3D.Api.Servisler;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.AI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Moduller.AI.Kontrolcu;

[ApiController]
[Route("api/ai")]
public class AIKontrolcu : ControllerBase
{
    private readonly AISaglayiciFabrikasi _fabrika;
    private readonly IAIMaliyetTakipServisi _maliyetTakip;
    private readonly IPIIFiltreServisi _piiServisi;
    private readonly IApiKeySifrelemeServisi _sifrelemeServisi;
    private readonly IHttpClientFactory _httpFabrikasi;
    private readonly VizitLink3DDbContext _db;

    public AIKontrolcu(
        AISaglayiciFabrikasi fabrika,
        IAIMaliyetTakipServisi maliyetTakip,
        IPIIFiltreServisi piiServisi,
        IApiKeySifrelemeServisi sifrelemeServisi,
        IHttpClientFactory httpFabrikasi,
        VizitLink3DDbContext db)
    {
        _fabrika = fabrika;
        _maliyetTakip = maliyetTakip;
        _piiServisi = piiServisi;
        _sifrelemeServisi = sifrelemeServisi;
        _httpFabrikasi = httpFabrikasi;
        _db = db;
    }

    [HttpPost("yaz")]
    public async Task<Cevap<string?>> AIYaz([FromBody] AIYazIstegi istek)
    {
        var saglayici = await _fabrika.SaglayiciGetirAsync(_db, istek.SaglayiciTip);
        if (saglayici == null)
            return Cevap<string?>.Hata("Aktif AI sağlayıcısı bulunamadı.");

        var saglayiciEntity = await _db.AISaglayicilari.FirstOrDefaultAsync(s => s.AktifMi);
        if (saglayiciEntity != null)
        {
            var limitVar = await _maliyetTakip.LimitKontrolAsync(saglayiciEntity.Id, null);
            if (!limitVar) return Cevap<string?>.Hata("Aylık AI kullanım limiti doldu.");
        }

        var guvenliPrompt = _piiServisi.Filtrele(istek.Prompt);

        var yanit = await saglayici.MetinUretAsync(new AIIstek
        {
            KullaniciPrompt = guvenliPrompt,
            SistemPrompt = istek.SistemPrompt ?? "Sen VizitLink3D kapı ve mobilya sektöründe uzman bir asistansın. Türkçe yanıt ver.",
            Model = saglayiciEntity?.Model ?? "llama3.2-3b"
        }, HttpContext.RequestAborted);

        if (saglayiciEntity != null)
        {
            await _maliyetTakip.KaydetAsync(
                saglayiciEntity.Id, null, istek.Amac ?? "MetinYaz", guvenliPrompt,
                yanit.IstekTokenSayisi, yanit.CevapTokenSayisi, yanit.MaliyetUsd,
                yanit.BasariliMi, yanit.HataMesaji);
        }

        return yanit.BasariliMi
            ? Cevap<string?>.Basarili(yanit.Metin)
            : Cevap<string?>.Hata(yanit.HataMesaji ?? "AI yanıtı alınamadı.");
    }

    [HttpGet("saglayicilar")]
    public async Task<Cevap<List<AISaglayicisi>>> SaglayicilariGetir()
    {
        var liste = await _db.AISaglayicilari.AsNoTracking().OrderBy(s => s.SiraNo).ToListAsync();
        foreach (var s in liste) s.ApiKeyEncrypted = "********";
        return Cevap<List<AISaglayicisi>>.Basarili(liste);
    }

    [HttpPost("saglayici")]
    public async Task<Cevap<AISaglayicisi>> SaglayiciEkle([FromBody] AISaglayicisi saglayici)
    {
        if (!string.IsNullOrWhiteSpace(saglayici.ApiKeyEncrypted))
        {
            saglayici.ApiKeyEncrypted = _sifrelemeServisi.Sifrele(saglayici.ApiKeyEncrypted);
        }

        saglayici.OlusturulmaTarihi = DateTime.UtcNow;
        _db.AISaglayicilari.Add(saglayici);
        await _db.SaveChangesAsync();
        saglayici.ApiKeyEncrypted = "********";
        return Cevap<AISaglayicisi>.Basarili(saglayici, "Sağlayıcı eklendi.");
    }

    [HttpPut("saglayici/{id:int}")]
    public async Task<Cevap<AISaglayicisi>> SaglayiciGuncelle(int id, [FromBody] AISaglayicisi istek)
    {
        var mevcut = await _db.AISaglayicilari.FirstOrDefaultAsync(x => x.Id == id);
        if (mevcut == null)
            return Cevap<AISaglayicisi>.Hata("Sağlayıcı bulunamadı.");

        mevcut.Ad = istek.Ad;
        mevcut.Tip = istek.Tip;
        mevcut.Model = istek.Model;
        mevcut.AylikLimitUsd = istek.AylikLimitUsd;
        mevcut.AktifMi = istek.AktifMi;
        mevcut.SiraNo = istek.SiraNo;
        mevcut.EkBaslik = istek.EkBaslik;
        mevcut.GuncellenmeTarihi = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(istek.ApiKeyEncrypted))
        {
            mevcut.ApiKeyEncrypted = _sifrelemeServisi.Sifrele(istek.ApiKeyEncrypted);
        }

        await _db.SaveChangesAsync();
        mevcut.ApiKeyEncrypted = "********";
        return Cevap<AISaglayicisi>.Basarili(mevcut, "Sağlayıcı güncellendi.");
    }

    [HttpPost("saglayici/{id:int}/test")]
    public async Task<Cevap<bool>> SaglayiciTest(int id)
    {
        var saglayiciEntity = await _db.AISaglayicilari.FindAsync(id);
        if (saglayiciEntity == null)
            return Cevap<bool>.Hata("Sağlayıcı bulunamadı.");

        var saglayici = _fabrika.SaglayiciOlustur(saglayiciEntity, _httpFabrikasi.CreateClient());
        var sonuc = await saglayici.SaglikTestiAsync();
        return Cevap<bool>.Basarili(sonuc, sonuc ? "Bağlantı başarılı" : "Bağlantı başarısız");
    }

    [HttpGet("maliyet")]
    public async Task<Cevap<object>> AylikMaliyet()
    {
        var buAy = await _db.AICagrisiKayitlari
            .Where(k => k.OlusturulmaTarihi.Month == DateTime.UtcNow.Month)
            .GroupBy(k => 1)
            .Select(g => new { ToplamCagri = g.Count(), ToplamMaliyet = g.Sum(k => k.ToplamMaliyetUsd) })
            .FirstOrDefaultAsync();

        return Cevap<object>.Basarili(buAy ?? new { ToplamCagri = 0, ToplamMaliyet = 0m }!);
    }

    [HttpGet("cagrilar")]
    public async Task<Cevap<List<AICagrisiKaydi>>> CagrilariGetir([FromQuery] int sayfa = 1)
    {
        var liste = await _db.AICagrisiKayitlari
            .AsNoTracking()
            .Include(k => k.Saglayici)
            .OrderByDescending(k => k.OlusturulmaTarihi)
            .Skip((sayfa - 1) * 20)
            .Take(20)
            .ToListAsync();
        return Cevap<List<AICagrisiKaydi>>.Basarili(liste);
    }

    /// <summary>
    /// AI ile çeviri. Eksik çevirileri tamamlamak için kullanılır.
    /// Hedef dilde karşılığı olmayan anahtarları AI otomatik çevirir.
    /// </summary>
    [HttpPost("cevir")]
    public async Task<Cevap<string?>> AICevir([FromBody] AICeviriIstegi istek)
    {
        if (string.IsNullOrWhiteSpace(istek.Metin))
            return Cevap<string?>.Hata("Çevrilecek metin boş.");

        var saglayici = await _fabrika.SaglayiciGetirAsync(_db, istek.SaglayiciTip);
        if (saglayici == null)
            return Cevap<string?>.Hata("Aktif AI sağlayıcısı bulunamadı.");

        var saglayiciEntity = await _db.AISaglayicilari.FirstOrDefaultAsync(s => s.AktifMi);

        var dilHaritasi = new Dictionary<string, string>
        {
            ["tr"] = "Turkce", ["en"] = "Ingilizce", ["de"] = "Almanca",
            ["fr"] = "Fransizca", ["ru"] = "Rusca", ["ar"] = "Arapca",
            ["es"] = "Ispanyolca", ["zh"] = "Cince"
        };

        var kaynakAd = istek.KaynakDil != null && dilHaritasi.ContainsKey(istek.KaynakDil) ? dilHaritasi[istek.KaynakDil] : "Turkce";
        var hedefAd = dilHaritasi.ContainsKey(istek.HedefDil) ? dilHaritasi[istek.HedefDil] : istek.HedefDil;

        var sistemPrompt = $"Sen profesyonel bir cevirmensin. Verilen metni {kaynakAd}'den {hedefAd}'ye cevir. SADECE ceviriyi ver, aciklama yapma. Kisa ve net ol.";

        var yanit = await saglayici.MetinUretAsync(new AIIstek
        {
            KullaniciPrompt = istek.Metin,
            SistemPrompt = sistemPrompt,
            Model = saglayiciEntity?.Model ?? "llama3.2:3b",
            Sicaklik = 0.2f,
            MaksimumToken = 500
        }, HttpContext.RequestAborted);

        return yanit.BasariliMi
            ? Cevap<string?>.Basarili(yanit.Metin.Trim())
            : Cevap<string?>.Hata(yanit.HataMesaji ?? "AI ceviri yapamadi.");
    }

    /// <summary>
    /// AI Orkestrasyon: Claude görevi planlar (emir veren), DeepSeek kodu yazar,
    /// Claude sonucu denetler. Anthropic kayıtlı değilse DeepSeek tek başına çalışır.
    /// </summary>
    [HttpPost("orkestra")]
    public async Task<Cevap<OrkestraSonucu>> Orkestra(
        [FromBody] OrkestraIstegi istek,
        [FromServices] AIOrkestraServisi orkestra)
    {
        if (string.IsNullOrWhiteSpace(istek.Gorev))
            return Cevap<OrkestraSonucu>.Hata("Görev boş olamaz.");

        var guvenliGorev = _piiServisi.Filtrele(istek.Gorev);
        var sonuc = await orkestra.KodGorevliCalistirAsync(_db, new CokluOrkestraIstegi
        {
            Gorev = guvenliGorev,
            DenetimYap = istek.DenetimYap,
            GoreviBol = istek.GoreviBol,
            ParalelIsciSayisi = istek.ParalelIsciSayisi,
            PlanlayiciTip = istek.PlanlayiciTip,
            DenetleyiciTip = istek.DenetleyiciTip,
            PlanlayiciSaglayiciId = istek.PlanlayiciSaglayiciId,
            DenetleyiciSaglayiciId = istek.DenetleyiciSaglayiciId,
            IsciSaglayiciIdleri = istek.IsciSaglayiciIdleri
        }, HttpContext.RequestAborted);

        return sonuc.BasariliMi
            ? Cevap<OrkestraSonucu>.Basarili(sonuc)
            : Cevap<OrkestraSonucu>.Hata(sonuc.Hata ?? "Orkestrasyon başarısız.");
    }

    public class OrkestraIstegi
    {
        public string Gorev { get; set; } = "";
        public bool DenetimYap { get; set; } = true;
        public bool GoreviBol { get; set; } = true;
        public int ParalelIsciSayisi { get; set; } = 3;
        public AISaglayiciTipi? PlanlayiciTip { get; set; }
        public AISaglayiciTipi? DenetleyiciTip { get; set; }
        public int? PlanlayiciSaglayiciId { get; set; }
        public int? DenetleyiciSaglayiciId { get; set; }
        public List<int> IsciSaglayiciIdleri { get; set; } = new();
    }

    public class AICeviriIstegi
    {
        public string Metin { get; set; } = "";
        public string HedefDil { get; set; } = "en";
        public string? KaynakDil { get; set; }
        public AISaglayiciTipi? SaglayiciTip { get; set; }
    }

    public class AIYazIstegi
    {
        public string Prompt { get; set; } = "";
        public string? SistemPrompt { get; set; }
        public string? Amac { get; set; }
        public AISaglayiciTipi? SaglayiciTip { get; set; }
    }
}
