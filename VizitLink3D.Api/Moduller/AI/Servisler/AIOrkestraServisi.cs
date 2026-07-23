using System.Text;
using System.Text.RegularExpressions;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller.AI;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Moduller.AI.Servisler;

/// <summary>
/// Çoklu model orkestrasyonu:
/// 1. Planlayıcı görevi analiz eder ve iş planı çıkarır
/// 2. Birden fazla kodlayıcı sağlayıcı görevi paralel çalıştırır
/// 3. Denetleyici sonuçları karşılaştırır, onaylar ve tek çıktı üretir
/// </summary>
public class AIOrkestraServisi
{
    private readonly AISaglayiciFabrikasi _fabrika;
    private readonly IHttpClientFactory _httpFabrika;
    private readonly ILogger<AIOrkestraServisi> _log;

    public AIOrkestraServisi(
        AISaglayiciFabrikasi fabrika,
        IHttpClientFactory httpFabrika,
        ILogger<AIOrkestraServisi> log)
    {
        _fabrika = fabrika;
        _httpFabrika = httpFabrika;
        _log = log;
    }

    public async Task<OrkestraSonucu> KodGorevliCalistirAsync(
        VizitLink3DDbContext db,
        CokluOrkestraIstegi istek,
        CancellationToken iptal = default)
    {
        var sonuc = new OrkestraSonucu
        {
            Gorev = istek.Gorev,
            BaslamaTarihi = DateTime.UtcNow
        };

        var etkinSaglayicilar = await db.AISaglayicilari
            .AsNoTracking()
            .Where(x => x.AktifMi && x.Tip != AISaglayiciTipi.GoogleTranslate)
            .OrderBy(x => x.SiraNo)
            .ToListAsync(iptal);

        if (etkinSaglayicilar.Count == 0)
        {
            sonuc.Hata = "Aktif AI sağlayıcısı bulunamadı.";
            sonuc.BitisTarihi = DateTime.UtcNow;
            return sonuc;
        }

        var planlayiciEntity = PlanlayiciSec(etkinSaglayicilar, istek);
        var denetleyiciEntity = DenetleyiciSec(etkinSaglayicilar, istek, planlayiciEntity);
        var isciHavuzu = IsciHavuzuSec(etkinSaglayicilar, istek, planlayiciEntity, denetleyiciEntity);

        if (isciHavuzu.Count == 0 && planlayiciEntity != null)
        {
            isciHavuzu.Add(planlayiciEntity);
        }

        if (isciHavuzu.Count == 0)
        {
            sonuc.Hata = "Kod yazacak aktif AI işçisi bulunamadı.";
            sonuc.BitisTarihi = DateTime.UtcNow;
            return sonuc;
        }

        var http = _httpFabrika.CreateClient();
        string gorevPlani = istek.Gorev;
        List<string> altGorevler = new();

        if (planlayiciEntity != null)
        {
            var planAdimi = AdimBaslat(sonuc, "Planlama", planlayiciEntity);
            var planlayici = _fabrika.SaglayiciOlustur(planlayiciEntity, http);

            var plan = await planlayici.MetinUretAsync(new AIIstek
            {
                SistemPrompt =
                    "Sen kıdemli bir teknik orkestratörsün. Kullanıcı görevini analiz et ve şu başlıklarla cevap ver: " +
                    "AMAC, ALT_GOREVLER, KABUL_KRITERLERI, RISKLER. ALT_GOREVLER bölümünde kısa madde listesi üret. " +
                    "Kod yazma, sadece uygulanabilir görev planı çıkar.",
                KullaniciPrompt = istek.Gorev,
                Model = ModelBelirle(planlayiciEntity, "claude-opus-4-1"),
                Sicaklik = 0.3f,
                MaksimumToken = 3000
            }, iptal);

            sonuc.ToplamMaliyetUsd += plan.MaliyetUsd;

            if (plan.BasariliMi && !string.IsNullOrWhiteSpace(plan.Metin))
            {
                gorevPlani = plan.Metin.Trim();
                altGorevler = AltGorevleriCikar(gorevPlani);
                sonuc.GorevPlani = gorevPlani;
                planAdimi.CiktiOzeti = OzetKirp(gorevPlani, 320);
                AdimBitir(planAdimi, "tamamlandi");
            }
            else
            {
                planAdimi.Hata = plan.HataMesaji ?? "Plan üretilemedi.";
                AdimBitir(planAdimi, "uyari");
                _log.LogWarning("Orkestra planlama başarısız, görev doğrudan işçi havuzuna veriliyor: {Hata}", plan.HataMesaji);
            }
        }

        if (altGorevler.Count == 0)
        {
            altGorevler.Add(istek.Gorev);
        }

        var isciAdimi = AdimBaslat(sonuc, "Paralel Kodlama", null);
        var isciGorevleri = isciHavuzu
            .Take(Math.Max(1, istek.ParalelIsciSayisi))
            .Select((entity, index) => IsciyiCalistirAsync(entity, index, istek, gorevPlani, altGorevler, iptal))
            .ToList();

        var isciSonuclari = await Task.WhenAll(isciGorevleri);
        sonuc.IsciSonuclari = isciSonuclari
            .OrderBy(x => x.IsciSirasi)
            .ToList();

        foreach (var isciSonucu in sonuc.IsciSonuclari)
        {
            sonuc.ToplamMaliyetUsd += isciSonucu.MaliyetUsd;
        }

        if (!sonuc.IsciSonuclari.Any(x => x.BasariliMi && !string.IsNullOrWhiteSpace(x.Cikti)))
        {
            isciAdimi.Hata = "Hiçbir işçi geçerli çıktı üretemedi.";
            AdimBitir(isciAdimi, "hata");
            sonuc.Hata = "Paralel çalışan modellerden geçerli çıktı alınamadı.";
            sonuc.BitisTarihi = DateTime.UtcNow;
            return sonuc;
        }

        isciAdimi.CiktiOzeti = $"{sonuc.IsciSonuclari.Count(x => x.BasariliMi)} başarılı / {sonuc.IsciSonuclari.Count} toplam işçi";
        AdimBitir(isciAdimi, "tamamlandi");

        if (istek.DenetimYap && denetleyiciEntity != null)
        {
            var denetimAdimi = AdimBaslat(sonuc, "Denetim ve Birleştirme", denetleyiciEntity);
            var denetleyici = _fabrika.SaglayiciOlustur(denetleyiciEntity, http);
            var denetimGirdisi = DenetimIstegiOlustur(istek, gorevPlani, sonuc.IsciSonuclari);

            var denetim = await denetleyici.MetinUretAsync(new AIIstek
            {
                SistemPrompt =
                    "Sen kıdemli teknik denetçisin. Sana verilen çoklu model çıktıları arasından en iyi çözümü seç, " +
                    "gerekirse birleştir ve şu etiketlerle cevap ver: KARAR, SECILEN_ISCI, GEREKCE, BIRLESTIRILMIS_CIKTI. " +
                    "BIRLESTIRILMIS_CIKTI bölümünde kullanıcıya gönderilecek nihai çıktıyı ver.",
                KullaniciPrompt = denetimGirdisi,
                Model = ModelBelirle(denetleyiciEntity, "claude-opus-4-1"),
                Sicaklik = 0.2f,
                MaksimumToken = 5000
            }, iptal);

            sonuc.ToplamMaliyetUsd += denetim.MaliyetUsd;

            if (denetim.BasariliMi && !string.IsNullOrWhiteSpace(denetim.Metin))
            {
                sonuc.SonOnay = BolumCikar(denetim.Metin, "KARAR");
                sonuc.SecilenIsci = BolumCikar(denetim.Metin, "SECILEN_ISCI");
                sonuc.DenetimOzeti = BolumCikar(denetim.Metin, "GEREKCE");
                sonuc.NihaiCikti = BolumCikar(denetim.Metin, "BIRLESTIRILMIS_CIKTI");
                sonuc.DenetimTamMetni = denetim.Metin.Trim();
                denetimAdimi.CiktiOzeti = OzetKirp(sonuc.SonOnay ?? sonuc.DenetimOzeti ?? "Denetim tamamlandı.", 260);
                AdimBitir(denetimAdimi, "tamamlandi");
            }
            else
            {
                denetimAdimi.Hata = denetim.HataMesaji ?? "Denetim başarısız.";
                AdimBitir(denetimAdimi, "uyari");
                VarsayilanNihaiCiktiAta(sonuc);
            }
        }
        else
        {
            VarsayilanNihaiCiktiAta(sonuc);
        }

        sonuc.BasariliMi = true;
        sonuc.BitisTarihi = DateTime.UtcNow;
        return sonuc;
    }

    private async Task<OrkestraIsciSonucu> IsciyiCalistirAsync(
        AISaglayicisi entity,
        int isciSirasi,
        CokluOrkestraIstegi istek,
        string gorevPlani,
        List<string> altGorevler,
        CancellationToken iptal)
    {
        var http = _httpFabrika.CreateClient();
        var saglayici = _fabrika.SaglayiciOlustur(entity, http);
        var gorevMetni = GorevAta(altGorevler, isciSirasi, istek.GoreviBol);
        var prompt = IsciPromptuOlustur(istek, gorevPlani, gorevMetni, entity, isciSirasi);

        var sonuc = new OrkestraIsciSonucu
        {
            IsciSirasi = isciSirasi + 1,
            SaglayiciId = entity.Id,
            SaglayiciAdi = entity.Ad,
            Tip = entity.Tip.ToString(),
            Model = entity.Model,
            AtananGorev = gorevMetni,
            Durum = "calisiyor",
            BaslamaTarihi = DateTime.UtcNow
        };

        try
        {
            var yanit = await saglayici.MetinUretAsync(new AIIstek
            {
                SistemPrompt =
                    "Sen uzman bir yazılım geliştiricisin. Sana verilen görevi eksiksiz uygula. " +
                    "Çıktın uygulanabilir, net ve mümkünse doğrudan kullanılabilir olsun. " +
                    "Varsayım yaparsan bunu en sonda kısa not olarak belirt.",
                KullaniciPrompt = prompt,
                Model = entity.Model,
                Sicaklik = 0.4f,
                MaksimumToken = 7000
            }, iptal);

            sonuc.MaliyetUsd = yanit.MaliyetUsd;
            sonuc.BasariliMi = yanit.BasariliMi;
            sonuc.Cikti = yanit.Metin?.Trim();
            sonuc.Hata = yanit.HataMesaji;
            sonuc.Durum = yanit.BasariliMi ? "tamamlandi" : "hata";
            sonuc.BitisTarihi = DateTime.UtcNow;
            return sonuc;
        }
        catch (Exception ex)
        {
            sonuc.BasariliMi = false;
            sonuc.Hata = ex.Message;
            sonuc.Durum = "hata";
            sonuc.BitisTarihi = DateTime.UtcNow;
            return sonuc;
        }
    }

    private static AISaglayicisi? PlanlayiciSec(List<AISaglayicisi> saglayicilar, CokluOrkestraIstegi istek)
    {
        return SaglayiciSecByIdOrTip(saglayicilar, istek.PlanlayiciSaglayiciId, istek.PlanlayiciTip)
               ?? saglayicilar.FirstOrDefault(x => x.Tip == AISaglayiciTipi.Anthropic)
               ?? saglayicilar.FirstOrDefault(x => x.Tip == AISaglayiciTipi.OpenAI)
               ?? saglayicilar.FirstOrDefault();
    }

    private static AISaglayicisi? DenetleyiciSec(List<AISaglayicisi> saglayicilar, CokluOrkestraIstegi istek, AISaglayicisi? planlayici)
    {
        return SaglayiciSecByIdOrTip(saglayicilar, istek.DenetleyiciSaglayiciId, istek.DenetleyiciTip)
               ?? saglayicilar.FirstOrDefault(x => x.Tip == AISaglayiciTipi.Anthropic && x.Id != planlayici?.Id)
               ?? planlayici
               ?? saglayicilar.FirstOrDefault();
    }

    private static List<AISaglayicisi> IsciHavuzuSec(
        List<AISaglayicisi> saglayicilar,
        CokluOrkestraIstegi istek,
        AISaglayicisi? planlayici,
        AISaglayicisi? denetleyici)
    {
        IEnumerable<AISaglayicisi> adaylar = saglayicilar
            .Where(x => x.Tip != AISaglayiciTipi.GoogleTranslate);

        if (istek.IsciSaglayiciIdleri is { Count: > 0 })
        {
            adaylar = adaylar.Where(x => istek.IsciSaglayiciIdleri.Contains(x.Id));
        }
        else
        {
            adaylar = adaylar.Where(x => x.Id != planlayici?.Id);
        }

        var liste = adaylar
            .OrderBy(x => x.SiraNo)
            .Take(Math.Clamp(istek.ParalelIsciSayisi, 1, 5))
            .ToList();

        if (liste.Count == 0 && denetleyici != null)
        {
            liste.Add(denetleyici);
        }

        return liste;
    }

    private static AISaglayicisi? SaglayiciSecByIdOrTip(List<AISaglayicisi> saglayicilar, int? id, AISaglayiciTipi? tip)
    {
        if (id.HasValue)
        {
            return saglayicilar.FirstOrDefault(x => x.Id == id.Value);
        }

        if (tip.HasValue)
        {
            return saglayicilar.FirstOrDefault(x => x.Tip == tip.Value);
        }

        return null;
    }

    private static string ModelBelirle(AISaglayicisi saglayici, string varsayilan)
    {
        return string.IsNullOrWhiteSpace(saglayici.Model) ? varsayilan : saglayici.Model;
    }

    private static string GorevAta(List<string> altGorevler, int isciSirasi, bool goreviBol)
    {
        if (!goreviBol || altGorevler.Count == 0)
        {
            return string.Join(Environment.NewLine, altGorevler);
        }

        return altGorevler[isciSirasi % altGorevler.Count];
    }

    private static string IsciPromptuOlustur(CokluOrkestraIstegi istek, string gorevPlani, string gorevMetni, AISaglayicisi entity, int isciSirasi)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"ISCI: {isciSirasi + 1}");
        sb.AppendLine($"SAGLAYICI: {entity.Ad}");
        sb.AppendLine();
        sb.AppendLine("UST GOREV:");
        sb.AppendLine(istek.Gorev);
        sb.AppendLine();
        sb.AppendLine("PLAN:");
        sb.AppendLine(gorevPlani);
        sb.AppendLine();
        sb.AppendLine("SANA ATANAN ALT GOREV:");
        sb.AppendLine(gorevMetni);
        sb.AppendLine();
        sb.AppendLine("BEKLENEN CIKTI:");
        sb.AppendLine("1. Kısa yaklaşım");
        sb.AppendLine("2. Uygulanabilir çözüm / kod / akış");
        sb.AppendLine("3. Riskler veya eksik varsayımlar");
        return sb.ToString();
    }

    private static string DenetimIstegiOlustur(CokluOrkestraIstegi istek, string gorevPlani, List<OrkestraIsciSonucu> isciSonuclari)
    {
        var sb = new StringBuilder();
        sb.AppendLine("UST GOREV:");
        sb.AppendLine(istek.Gorev);
        sb.AppendLine();
        sb.AppendLine("PLAN:");
        sb.AppendLine(gorevPlani);
        sb.AppendLine();
        sb.AppendLine("ISCI CIKTILARI:");

        foreach (var isci in isciSonuclari.OrderBy(x => x.IsciSirasi))
        {
            sb.AppendLine($"### ISCI {isci.IsciSirasi} - {isci.SaglayiciAdi} ({isci.Model})");
            sb.AppendLine($"DURUM: {isci.Durum}");
            sb.AppendLine($"ATANAN_GOREV: {isci.AtananGorev}");
            sb.AppendLine("CIKTI:");
            sb.AppendLine(isci.Cikti ?? isci.Hata ?? "Bos");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static List<string> AltGorevleriCikar(string plan)
    {
        var satirlar = plan
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        var altGorevler = satirlar
            .Where(x =>
                x.StartsWith("- ") ||
                x.StartsWith("* ") ||
                Regex.IsMatch(x, @"^\d+\.\s"))
            .Select(x => Regex.Replace(x, @"^(- |\* |\d+\.\s)", "").Trim())
            .Where(x => x.Length > 4)
            .Take(8)
            .ToList();

        return altGorevler;
    }

    private static string? BolumCikar(string metin, string etiket)
    {
        var desen = $"{etiket}\\s*:\\s*(.*?)(?=\\n[A-ZCDEGIKLMNOPRSTU_]+\\s*:|\\z)";
        var eslesme = Regex.Match(metin, desen, RegexOptions.Singleline);
        if (!eslesme.Success)
        {
            return null;
        }

        var deger = eslesme.Groups[1].Value.Trim();
        return string.IsNullOrWhiteSpace(deger) ? null : deger;
    }

    private static void VarsayilanNihaiCiktiAta(OrkestraSonucu sonuc)
    {
        var secilen = sonuc.IsciSonuclari
            .Where(x => x.BasariliMi && !string.IsNullOrWhiteSpace(x.Cikti))
            .OrderBy(x => x.IsciSirasi)
            .FirstOrDefault();

        if (secilen == null)
        {
            return;
        }

        sonuc.SecilenIsci = $"{secilen.SaglayiciAdi} ({secilen.Model})";
        sonuc.SonOnay = "Otomatik kabul";
        sonuc.DenetimOzeti = "Denetleyici yanıtı alınamadığı için ilk başarılı işçi çıktısı seçildi.";
        sonuc.NihaiCikti = secilen.Cikti;
    }

    private static OrkestraAdimi AdimBaslat(OrkestraSonucu sonuc, string ad, AISaglayicisi? saglayici)
    {
        var adim = new OrkestraAdimi
        {
            Ad = ad,
            Durum = "calisiyor",
            Saglayici = saglayici?.Ad,
            BaslamaTarihi = DateTime.UtcNow
        };
        sonuc.Adimlar.Add(adim);
        return adim;
    }

    private static void AdimBitir(OrkestraAdimi adim, string durum)
    {
        adim.Durum = durum;
        adim.BitisTarihi = DateTime.UtcNow;
    }

    private static string OzetKirp(string metin, int uzunluk)
    {
        if (string.IsNullOrWhiteSpace(metin))
        {
            return string.Empty;
        }

        var temiz = Regex.Replace(metin, "\\s+", " ").Trim();
        return temiz.Length <= uzunluk ? temiz : temiz[..uzunluk] + "...";
    }
}

public class CokluOrkestraIstegi
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

public class OrkestraSonucu
{
    public string Gorev { get; set; } = "";
    public string? GorevPlani { get; set; }
    public string? SonOnay { get; set; }
    public string? SecilenIsci { get; set; }
    public string? DenetimOzeti { get; set; }
    public string? DenetimTamMetni { get; set; }
    public string? NihaiCikti { get; set; }
    public decimal ToplamMaliyetUsd { get; set; }
    public bool BasariliMi { get; set; }
    public string? Hata { get; set; }
    public DateTime BaslamaTarihi { get; set; }
    public DateTime? BitisTarihi { get; set; }
    public List<OrkestraAdimi> Adimlar { get; set; } = new();
    public List<OrkestraIsciSonucu> IsciSonuclari { get; set; } = new();
}

public class OrkestraAdimi
{
    public string Ad { get; set; } = "";
    public string Durum { get; set; } = "";
    public string? Saglayici { get; set; }
    public string? CiktiOzeti { get; set; }
    public string? Hata { get; set; }
    public DateTime BaslamaTarihi { get; set; }
    public DateTime? BitisTarihi { get; set; }
}

public class OrkestraIsciSonucu
{
    public int IsciSirasi { get; set; }
    public int? SaglayiciId { get; set; }
    public string SaglayiciAdi { get; set; } = "";
    public string Tip { get; set; } = "";
    public string Model { get; set; } = "";
    public string Durum { get; set; } = "";
    public bool BasariliMi { get; set; }
    public string? AtananGorev { get; set; }
    public string? Cikti { get; set; }
    public string? Hata { get; set; }
    public decimal MaliyetUsd { get; set; }
    public DateTime BaslamaTarihi { get; set; }
    public DateTime? BitisTarihi { get; set; }
}
