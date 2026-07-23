using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace VizitLink3D.Api.Kontrolculer.Sistem;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardKontrolcu(VizitLink3DDbContext vt) : ControllerBase
{
    public record ZiyaretKaydetIstegi(string Sayfa, string? Referer);

    [HttpGet("ozet")]
    public async Task<Cevap<DashboardOzeti>> OzetGetir()
    {
        var simdi = DateTime.UtcNow;
        // AuditLog append-only bir tablodur. En yüksek Id, toplam kaydı verir ve
        // birincil anahtar indeksinden okunduğu için dashboard'u bloklamaz.
        var sonDenetimKaydiId = await vt.AuditLoglar
            .AsNoTracking()
            .MaxAsync(a => (long?)a.Id) ?? 0;
        var ozet = new DashboardOzeti
        {
            ToplamUrun = await vt.Urunler.CountAsync(u => !u.SilindiMi),
            ToplamKapak = await vt.KapakModelleri.CountAsync(),
            ToplamUrunAilesi = await vt.UrunAilesileri.CountAsync(a => !a.SilindiMi),
            ToplamUrunKategori = await vt.UrunKategorileri.CountAsync(k => !k.SilindiMi),
            Toplam3DModel = await vt.UrunUcBoyutModelleri.CountAsync(m => !m.SilindiMi),
            ToplamParca = await vt.UrunUcBoyutParcalari.CountAsync(p => !p.SilindiMi),
            ToplamSlayt = await vt.Slaytlar.CountAsync(),
            ToplamHaber = await vt.Haberler.CountAsync(b => b.AktifMi),
            ToplamSSS = await vt.SikSorulanSorular.CountAsync(),
            ToplamSayfa = await vt.SayfaIcerikleri.CountAsync(),
            ToplamProje = await vt.Projeler.CountAsync(p => p.AktifMi),
            ToplamReferans = await vt.Referanslar.CountAsync(),
            ToplamYorum = await vt.MusteriYorumlari.CountAsync(),
            ToplamMedya = await vt.Medyalar.CountAsync(),
            ToplamKatalog = await vt.Kataloglar.CountAsync(),
            ToplamMesaj = await vt.IletisimMesajlari.CountAsync(),
            BekleyenMesaj = await vt.IletisimMesajlari.CountAsync(m => !m.OkunduMu),
            ToplamTeklif = await vt.TeklifIstekleri.CountAsync(t => !t.SilindiMi),
            ToplamSube = await vt.Subeler.CountAsync(s => s.AktifMi),
            ToplamEkip = await vt.EkipUyeleri.CountAsync(e => e.AktifMi),
            ToplamMenu = await vt.MenuOgeleri.CountAsync(m => !m.SilindiMi),
            ToplamCeviri = await vt.Ceviriler.CountAsync(),
            ToplamDil = await vt.Diller.CountAsync(d => d.AktifMi),
            ToplamKullanici = await vt.Kullanicilar.CountAsync(k => k.AktifMi),
            ToplamAI = await vt.AISaglayicilari.CountAsync(a => a.AktifMi),
            ToplamLog = sonDenetimKaydiId > int.MaxValue ? int.MaxValue : (int)sonDenetimKaydiId,
            BekleyenIs = await vt.IsTakipKayitlari.CountAsync(i => i.Durum != "Tamamlandi" && i.Durum != "Iptal"),
            KritikIs = await vt.IsTakipKayitlari.CountAsync(i => i.Oncelik == "Kritik" && i.Durum != "Tamamlandi"),
            ToplamIs = await vt.IsTakipKayitlari.CountAsync(),
            ToplamBulten = await vt.BultenAboneleri.CountAsync(b => b.AktifMi),
            ToplamEpostaSablon = await vt.EpostaSablonlari.CountAsync(),
            BugunMesaj = await vt.IletisimMesajlari.CountAsync(m => m.Tarih.Date == simdi.Date),
            BugunTeklif = await vt.TeklifIstekleri.CountAsync(t => t.OlusturulmaTarihi.Date == simdi.Date),
            ToplamZiyaret = await vt.ZiyaretKayitlari.CountAsync()
        };

        return Cevap<DashboardOzeti>.Basarili(ozet);
    }

    [HttpGet("komuta-merkezi")]
    public async Task<Cevap<DashboardKomutaMerkezi>> KomutaMerkeziGetir()
    {
        var simdi = DateTime.UtcNow;
        var bugun = simdi.Date;
        var haftaBasi = bugun.AddDays(-6);
        var ayBasi = bugun.AddDays(-29);
        var aktifEsik = simdi.AddMinutes(-30);

        var ozet = (await OzetGetir()).Veri ?? new DashboardOzeti();
        var ziyaretler = await vt.ZiyaretKayitlari
            .AsNoTracking()
            .Where(z => z.Tarih >= ayBasi)
            .OrderByDescending(z => z.Tarih)
            .Take(5000)
            .ToListAsync();

        var bugunZiyaret = ziyaretler.Count(z => z.Tarih.Date == bugun);
        var aktifZiyaretci = ziyaretler
            .Where(z => z.Tarih >= aktifEsik)
            .GroupBy(z => new { Ip = z.IP ?? "", Tarayici = TarayiciAdi(z.Tarayici), Cihaz = CihazAdi(z.Tarayici, z.Cihaz) })
            .Select(g =>
            {
                var son = g.OrderByDescending(z => z.Tarih).First();
                return new ZiyaretciAnlikDto
                {
                    Kimlik = string.IsNullOrWhiteSpace(g.Key.Ip) ? $"{g.Key.Tarayici}-{g.Key.Cihaz}" : IpMaskele(g.Key.Ip),
                    IpAdresi = IpMaskele(g.Key.Ip),
                    Sayfa = SayfaBaslik(son.Sayfa),
                    HamSayfa = son.Sayfa ?? "/",
                    Tarayici = g.Key.Tarayici,
                    Cihaz = g.Key.Cihaz,
                    Konum = KonumMetni(son.Sehir, son.Ulke),
                    SonGorulme = son.Tarih,
                    ZiyaretSayisi = g.Count()
                };
            })
            .OrderByDescending(z => z.SonGorulme)
            .Take(12)
            .ToList();

        var gezilenSayfalar = ziyaretler
            .Where(z => !string.IsNullOrWhiteSpace(z.Sayfa))
            .GroupBy(z => z.Sayfa!)
            .Select(g => new SayfaIlgiDto
            {
                Sayfa = SayfaBaslik(g.Key),
                Url = g.Key,
                Ziyaret = g.Count(),
                TekilZiyaretci = g.Select(x => x.IP ?? $"{x.Tarayici}-{x.Cihaz}").Distinct().Count(),
                SonZiyaret = g.Max(x => x.Tarih)
            })
            .OrderByDescending(s => s.Ziyaret)
            .Take(10)
            .ToList();

        var urunZiyaretleri = ziyaretler
            .Where(z => z.Sayfa?.Contains("/urun/", StringComparison.OrdinalIgnoreCase) == true)
            .GroupBy(z => UrunSlug(z.Sayfa))
            .Where(g => !string.IsNullOrWhiteSpace(g.Key))
            .Select(g => new { Slug = g.Key!, Ziyaret = g.Count(), Tekil = g.Select(x => x.IP ?? $"{x.Tarayici}-{x.Cihaz}").Distinct().Count(), Son = g.Max(x => x.Tarih) })
            .ToList();

        var urunler = await vt.Urunler
            .AsNoTracking()
            .Where(u => !u.SilindiMi && u.AktifMi)
            .OrderByDescending(u => u.OneCikanMi)
            .ThenBy(u => u.SiraNo)
            .Take(40)
            .Select(u => new { u.Id, u.Ad, u.Kod, u.Slug, u.OneCikanMi })
            .ToListAsync();

        var teklifler = await vt.TeklifIstekleri
            .AsNoTracking()
            .Where(t => !t.SilindiMi && t.OlusturulmaTarihi >= ayBasi)
            .GroupBy(t => t.UrunId)
            .Select(g => new { UrunId = g.Key, Adet = g.Count() })
            .ToListAsync();

        var urunIlgileri = urunler
            .Select(u =>
            {
                var ziyaret = urunZiyaretleri.FirstOrDefault(z => z.Slug.Equals(u.Slug, StringComparison.OrdinalIgnoreCase));
                var teklif = teklifler.FirstOrDefault(t => t.UrunId == u.Id)?.Adet ?? 0;
                return new UrunIlgiDto
                {
                    UrunId = u.Id,
                    Ad = u.Ad,
                    Kod = u.Kod,
                    Slug = u.Slug,
                    Ziyaret = ziyaret?.Ziyaret ?? 0,
                    TekilZiyaretci = ziyaret?.Tekil ?? 0,
                    Teklif = teklif,
                    IlgiPuani = ((ziyaret?.Ziyaret ?? 0) * 3) + (teklif * 12) + (u.OneCikanMi ? 8 : 0),
                    SonZiyaret = ziyaret?.Son
                };
            })
            .OrderByDescending(u => u.IlgiPuani)
            .ThenByDescending(u => u.Ziyaret)
            .Take(10)
            .ToList();

        var tarayiciDagilimi = DagilimOlustur(ziyaretler.Select(z => TarayiciAdi(z.Tarayici)), ziyaretler.Count);
        var cihazDagilimi = DagilimOlustur(ziyaretler.Select(z => CihazAdi(z.Tarayici, z.Cihaz)), ziyaretler.Count);
        var trafikKaynaklari = DagilimOlustur(ziyaretler.Select(z => RefererKaynak(z.Referer)), ziyaretler.Count);

        var sonGirisHam = await vt.Kullanicilar
            .AsNoTracking()
            .Where(k => k.AktifMi && k.SonGirisTarihi != null)
            .OrderByDescending(k => k.SonGirisTarihi)
            .Take(10)
            .Select(k => new { k.AdSoyad, k.KullaniciAdi, k.Rol, k.SonGirisIP, k.SonGirisTarihi })
            .ToListAsync();
        var sonGirisler = sonGirisHam
            .Select(k => new GirisKaydiDto
            {
                Kullanici = k.AdSoyad == "" ? k.KullaniciAdi : k.AdSoyad,
                Rol = k.Rol.ToString(),
                IpAdresi = IpMaskele(k.SonGirisIP),
                Zaman = k.SonGirisTarihi!.Value
            })
            .ToList();

        var denetimHam = await vt.AuditLoglar
            .AsNoTracking()
            // AuditLog append-only olduğundan Id sırası, kayıt sırasıdır.
            // Böylece SQLite birincil anahtar indeksini kullanır.
            .OrderByDescending(a => a.Id)
            .Take(12)
            .Select(a => new { a.Eylem, a.KullaniciId, a.IPAdresi, a.Tarayici, a.ZamanDamgasi })
            .ToListAsync();
        var denetimAkisi = denetimHam
            .Select(a => new DenetimAkisiDto
            {
                Eylem = a.Eylem,
                Kullanici = a.KullaniciId ?? "Sistem",
                IpAdresi = IpMaskele(a.IPAdresi),
                Tarayici = TarayiciAdi(a.Tarayici),
                Zaman = a.ZamanDamgasi
            })
            .ToList();

        var haftalikTeklifler = await vt.TeklifIstekleri
            .AsNoTracking()
            .Where(t => !t.SilindiMi && t.OlusturulmaTarihi >= haftaBasi)
            .Select(t => t.OlusturulmaTarihi)
            .ToListAsync();
        var haftalikMesajlar = await vt.IletisimMesajlari
            .AsNoTracking()
            .Where(m => m.Tarih >= haftaBasi)
            .Select(m => m.Tarih)
            .ToListAsync();

        var trend = Enumerable.Range(0, 7)
            .Select(i => haftaBasi.AddDays(i))
            .Select(gun => new TrafikTrendDto
            {
                Etiket = gun.ToString("dd MMM", CultureInfo.GetCultureInfo("tr-TR")),
                Tarih = gun,
                Ziyaret = ziyaretler.Count(z => z.Tarih.Date == gun.Date),
                Teklif = haftalikTeklifler.Count(t => t.Date == gun.Date),
                Mesaj = haftalikMesajlar.Count(m => m.Date == gun.Date)
            })
            .ToList();

        var komuta = new DashboardKomutaMerkezi
        {
            Ozet = ozet,
            BugunZiyaret = bugunZiyaret,
            AktifZiyaretci = aktifZiyaretci.Count,
            AylikZiyaret = ziyaretler.Count,
            DonusumOrani = bugunZiyaret == 0 ? 0 : Math.Round((decimal)(ozet.BugunTeklif + ozet.BugunMesaj) / bugunZiyaret * 100, 1),
            AktifZiyaretciler = aktifZiyaretci,
            EnCokGezilenSayfalar = gezilenSayfalar,
            EnCokIlgiGorenUrunler = urunIlgileri,
            TarayiciDagilimi = tarayiciDagilimi,
            CihazDagilimi = cihazDagilimi,
            TrafikKaynaklari = trafikKaynaklari,
            SonGirisler = sonGirisler,
            DenetimAkisi = denetimAkisi,
            GunlukTrend = trend,
            Sistem = new SistemSagligiDto
            {
                Api = "�evrimi�i",
                Veritabani = "�evrimi�i",
                CanliBaglanti = "SignalR haz�r",
                Lisans = "Aktif",
                SonGuncelleme = simdi
            }
        };

        return Cevap<DashboardKomutaMerkezi>.Basarili(komuta);
    }

    [AllowAnonymous]
    [HttpPost("ziyaret-kaydet")]
    public async Task<Cevap<object>> ZiyaretKaydet([FromBody] ZiyaretKaydetIstegi istek)
    {
        if (string.IsNullOrWhiteSpace(istek.Sayfa))
            return Cevap<object>.Basarili(new { kaydedildi = false });

        var simdi = DateTime.UtcNow;
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var tarayici = Request.Headers.UserAgent.ToString();
        var sayfa = istek.Sayfa.Length > 450 ? istek.Sayfa[..450] : istek.Sayfa;
        var ikiDakikaOnce = simdi.AddMinutes(-2);

        var tekrar = await vt.ZiyaretKayitlari.AnyAsync(z =>
            z.Tarih >= ikiDakikaOnce &&
            z.IP == ip &&
            z.Sayfa == sayfa);

        if (tekrar)
            return Cevap<object>.Basarili(new { kaydedildi = false });

        vt.ZiyaretKayitlari.Add(new ZiyaretKaydi
        {
            Tarih = simdi,
            IP = ip,
            Sayfa = sayfa,
            Referer = string.IsNullOrWhiteSpace(istek.Referer) ? Request.Headers.Referer.ToString() : istek.Referer,
            Tarayici = tarayici,
            Cihaz = CihazAdi(tarayici, null),
            Ulke = "TR"
        });

        await vt.SaveChangesAsync();
        return Cevap<object>.Basarili(new { kaydedildi = true });
    }

    private static List<DagilimDto> DagilimOlustur(IEnumerable<string> degerler, int toplam)
    {
        return degerler
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .GroupBy(d => d)
            .Select(g => new DagilimDto
            {
                Ad = g.Key,
                Adet = g.Count(),
                Oran = toplam == 0 ? 0 : Math.Round(g.Count() * 100m / toplam, 1)
            })
            .OrderByDescending(d => d.Adet)
            .Take(6)
            .ToList();
    }

    private static string IpMaskele(string? ip)
    {
        if (string.IsNullOrWhiteSpace(ip)) return "Bilinmiyor";
        var parcalar = ip.Split('.');
        return parcalar.Length == 4 ? $"{parcalar[0]}.{parcalar[1]}.{parcalar[2]}.***" : ip;
    }

    private static string TarayiciAdi(string? tarayici)
    {
        if (string.IsNullOrWhiteSpace(tarayici)) return "Bilinmiyor";
        if (tarayici.Contains("Edg/", StringComparison.OrdinalIgnoreCase)) return "Microsoft Edge";
        if (tarayici.Contains("Chrome/", StringComparison.OrdinalIgnoreCase)) return "Chrome";
        if (tarayici.Contains("Firefox/", StringComparison.OrdinalIgnoreCase)) return "Firefox";
        if (tarayici.Contains("Safari/", StringComparison.OrdinalIgnoreCase)) return "Safari";
        return "Di�er";
    }

    private static string CihazAdi(string? tarayici, string? cihaz)
    {
        if (!string.IsNullOrWhiteSpace(cihaz)) return cihaz;
        if (string.IsNullOrWhiteSpace(tarayici)) return "Bilinmiyor";
        if (tarayici.Contains("Mobile", StringComparison.OrdinalIgnoreCase)) return "Mobil";
        if (tarayici.Contains("Tablet", StringComparison.OrdinalIgnoreCase) || tarayici.Contains("iPad", StringComparison.OrdinalIgnoreCase)) return "Tablet";
        return "Masa�st�";
    }

    private static string RefererKaynak(string? referer)
    {
        if (string.IsNullOrWhiteSpace(referer)) return "Direkt";
        return Uri.TryCreate(referer, UriKind.Absolute, out var uri) ? uri.Host.Replace("www.", "") : "Site i�i";
    }

    private static string SayfaBaslik(string? sayfa)
    {
        if (string.IsNullOrWhiteSpace(sayfa) || sayfa == "/") return "Ana sayfa";
        var temiz = sayfa.Split('?', '#')[0].Trim('/');
        if (string.IsNullOrWhiteSpace(temiz)) return "Ana sayfa";
        return CultureInfo.GetCultureInfo("tr-TR").TextInfo.ToTitleCase(temiz.Replace("-", " ").Replace("/", " / "));
    }

    private static string? UrunSlug(string? sayfa)
    {
        if (string.IsNullOrWhiteSpace(sayfa)) return null;
        var temiz = sayfa.Split('?', '#')[0].Trim('/');
        var parcalar = temiz.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var indeks = Array.FindIndex(parcalar, p => p.Equals("urun", StringComparison.OrdinalIgnoreCase));
        return indeks >= 0 && indeks + 1 < parcalar.Length ? parcalar[indeks + 1] : null;
    }

    private static string KonumMetni(string? sehir, string? ulke)
    {
        if (!string.IsNullOrWhiteSpace(sehir) && !string.IsNullOrWhiteSpace(ulke)) return $"{sehir}, {ulke}";
        if (!string.IsNullOrWhiteSpace(ulke)) return ulke;
        return "Konum bekleniyor";
    }
}

public class DashboardOzeti
{
    public int ToplamUrun { get; set; }
    public int ToplamKapak { get; set; }
    public int ToplamUrunAilesi { get; set; }
    public int ToplamUrunKategori { get; set; }
    public int Toplam3DModel { get; set; }
    public int ToplamParca { get; set; }
    public int ToplamSlayt { get; set; }
    public int ToplamHaber { get; set; }
    public int ToplamSSS { get; set; }
    public int ToplamSayfa { get; set; }
    public int ToplamProje { get; set; }
    public int ToplamReferans { get; set; }
    public int ToplamYorum { get; set; }
    public int ToplamMedya { get; set; }
    public int ToplamKatalog { get; set; }
    public int ToplamMesaj { get; set; }
    public int BekleyenMesaj { get; set; }
    public int ToplamTeklif { get; set; }
    public int ToplamSube { get; set; }
    public int ToplamEkip { get; set; }
    public int ToplamMenu { get; set; }
    public int ToplamCeviri { get; set; }
    public int ToplamDil { get; set; }
    public int ToplamKullanici { get; set; }
    public int ToplamAI { get; set; }
    public int ToplamLog { get; set; }
    public int BekleyenIs { get; set; }
    public int KritikIs { get; set; }
    public int ToplamIs { get; set; }
    public int ToplamBulten { get; set; }
    public int ToplamEpostaSablon { get; set; }
    public int BugunMesaj { get; set; }
    public int BugunTeklif { get; set; }
    public int ToplamZiyaret { get; set; }
}

public class DashboardKomutaMerkezi
{
    public DashboardOzeti Ozet { get; set; } = new();
    public int BugunZiyaret { get; set; }
    public int AktifZiyaretci { get; set; }
    public int AylikZiyaret { get; set; }
    public decimal DonusumOrani { get; set; }
    public List<ZiyaretciAnlikDto> AktifZiyaretciler { get; set; } = [];
    public List<SayfaIlgiDto> EnCokGezilenSayfalar { get; set; } = [];
    public List<UrunIlgiDto> EnCokIlgiGorenUrunler { get; set; } = [];
    public List<DagilimDto> TarayiciDagilimi { get; set; } = [];
    public List<DagilimDto> CihazDagilimi { get; set; } = [];
    public List<DagilimDto> TrafikKaynaklari { get; set; } = [];
    public List<GirisKaydiDto> SonGirisler { get; set; } = [];
    public List<DenetimAkisiDto> DenetimAkisi { get; set; } = [];
    public List<TrafikTrendDto> GunlukTrend { get; set; } = [];
    public SistemSagligiDto Sistem { get; set; } = new();
}

public class ZiyaretciAnlikDto
{
    public string Kimlik { get; set; } = "";
    public string IpAdresi { get; set; } = "";
    public string Sayfa { get; set; } = "";
    public string HamSayfa { get; set; } = "";
    public string Tarayici { get; set; } = "";
    public string Cihaz { get; set; } = "";
    public string Konum { get; set; } = "";
    public DateTime SonGorulme { get; set; }
    public int ZiyaretSayisi { get; set; }
}

public class SayfaIlgiDto
{
    public string Sayfa { get; set; } = "";
    public string Url { get; set; } = "";
    public int Ziyaret { get; set; }
    public int TekilZiyaretci { get; set; }
    public DateTime SonZiyaret { get; set; }
}

public class UrunIlgiDto
{
    public int UrunId { get; set; }
    public string Ad { get; set; } = "";
    public string Kod { get; set; } = "";
    public string Slug { get; set; } = "";
    public int Ziyaret { get; set; }
    public int TekilZiyaretci { get; set; }
    public int Teklif { get; set; }
    public int IlgiPuani { get; set; }
    public DateTime? SonZiyaret { get; set; }
}

public class DagilimDto
{
    public string Ad { get; set; } = "";
    public int Adet { get; set; }
    public decimal Oran { get; set; }
}

public class GirisKaydiDto
{
    public string Kullanici { get; set; } = "";
    public string Rol { get; set; } = "";
    public string IpAdresi { get; set; } = "";
    public DateTime Zaman { get; set; }
}

public class DenetimAkisiDto
{
    public string Eylem { get; set; } = "";
    public string Kullanici { get; set; } = "";
    public string IpAdresi { get; set; } = "";
    public string Tarayici { get; set; } = "";
    public DateTime Zaman { get; set; }
}

public class TrafikTrendDto
{
    public string Etiket { get; set; } = "";
    public DateTime Tarih { get; set; }
    public int Ziyaret { get; set; }
    public int Teklif { get; set; }
    public int Mesaj { get; set; }
}

public class SistemSagligiDto
{
    public string Api { get; set; } = "";
    public string Veritabani { get; set; } = "";
    public string CanliBaglanti { get; set; } = "";
    public string Lisans { get; set; } = "";
    public DateTime SonGuncelleme { get; set; }
}

