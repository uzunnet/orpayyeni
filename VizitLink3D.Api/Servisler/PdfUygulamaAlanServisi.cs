using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller.Medya;
using VizitLink3D.Ortak.Modeller.Urunler;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace VizitLink3D.Api.Servisler;

public class PdfUygulamaAlanServisi(VizitLink3DDbContext vt, IWebHostEnvironment env)
{
    private const float UygulamaYukseklikOrani = 0.62f;
    private const int JpegKalitesi = 87;

    public async Task<List<UygulamaAlanSonucu>> TumSayfalariIsleAsync(CancellationToken iptal = default)
    {
        var sayfalarKlasoru = Path.Combine(env.WebRootPath, "medya", "kataloglar", "sayfalar");
        var hedefKlasor = Path.Combine(env.WebRootPath, "medya", "kataloglar", "uygulama-alanlari");
        Directory.CreateDirectory(hedefKlasor);

        var sonuclar = new List<UygulamaAlanSonucu>();

        var dosyalar = Directory.GetFiles(sayfalarKlasoru, "*_sayfa_*.png")
            .Select(f => new { Yol = f, Ad = Path.GetFileNameWithoutExtension(f) })
            .Where(f => PdfVeSayfaNoAyristir(f.Ad, out _, out _))
            .OrderBy(f => f.Ad)
            .ToList();

        foreach (var dosya in dosyalar)
        {
            PdfVeSayfaNoAyristir(dosya.Ad, out var pdfId, out var sayfaNo);

            using var img = await Image.LoadAsync(dosya.Yol, iptal);
            var yariGenislik = img.Width / 2;
            var uygulamaYukseklik = (int)(img.Height * UygulamaYukseklikOrani);

            var sol = await GorselKaydetAsync(img, pdfId, sayfaNo, "sol",
                new Rectangle(0, 0, yariGenislik, uygulamaYukseklik), hedefKlasor, iptal);
            if (sol != null) sonuclar.Add(sol);

            var sag = await GorselKaydetAsync(img, pdfId, sayfaNo, "sag",
                new Rectangle(yariGenislik, 0, yariGenislik, uygulamaYukseklik), hedefKlasor, iptal);
            if (sag != null) sonuclar.Add(sag);
        }

        return sonuclar;
    }

    public async Task<List<UygulamaAlanSonucu>> TumListeAsync(CancellationToken iptal = default)
    {
        var prefix = "medya/kataloglar/uygulama-alanlari/";

        var medyalar = await vt.Medyalar
            .Where(m => !m.SilindiMi && m.DosyaYolu != null && m.DosyaYolu.StartsWith(prefix))
            .OrderBy(m => m.OlusturulmaTarihi)
            .ToListAsync(iptal);

        var urunMedyalari = await vt.UrunMedyalari
            .Where(um => medyalar.Select(m => m.DosyaYolu!).Contains(um.MedyaUrl))
            .ToListAsync(iptal);

        var eslenenUrunIdler = urunMedyalari.Select(um => um.UrunId).Distinct().ToList();
        var urunler = await vt.Urunler
            .Where(u => eslenenUrunIdler.Contains(u.Id) && !u.SilindiMi)
            .ToDictionaryAsync(u => u.Id, u => u.Ad, iptal);

        return medyalar.Select(m =>
        {
            var um = urunMedyalari.FirstOrDefault(x => x.MedyaUrl == m.DosyaYolu);
            AyristirDosyaAdi(m.OrijinalAd ?? "", out var pdfId, out var sayfaNo, out var konum);
            return new UygulamaAlanSonucu
            {
                MedyaId = m.Id,
                PdfKaynagiId = pdfId,
                SayfaNo = sayfaNo,
                Konum = konum,
                GorselUrl = $"/{m.DosyaYolu}",
                EslenenUrunId = um?.UrunId,
                EslenenUrunAdi = um != null && urunler.TryGetValue(um.UrunId, out var ad) ? ad : null,
                EslenenSiraNo = um?.SiraNo ?? 1
            };
        }).ToList();
    }

    public async Task UrunEsleAsync(UrunEslemeIstegi istek, CancellationToken iptal = default)
    {
        var medya = await vt.Medyalar.FindAsync([istek.MedyaId], cancellationToken: iptal)
            ?? throw new InvalidOperationException("Medya bulunamadı.");

        var mevcutEsleme = await vt.UrunMedyalari
            .FirstOrDefaultAsync(um => um.UrunId == istek.UrunId && um.MedyaUrl == medya.DosyaYolu, iptal);

        if (mevcutEsleme == null)
        {
            vt.UrunMedyalari.Add(new UrunMedya
            {
                UrunId = istek.UrunId,
                MedyaUrl = medya.DosyaYolu!,
                MedyaTuru = "UygulamaAlani",
                SiraNo = istek.SiraNo,
                AnaGosterim = false
            });
        }
        else
        {
            mevcutEsleme.SiraNo = istek.SiraNo;
        }

        await vt.SaveChangesAsync(iptal);
    }

    public async Task EslemeKaldir(int medyaId, int urunId, CancellationToken iptal = default)
    {
        var medya = await vt.Medyalar.FindAsync([medyaId], cancellationToken: iptal);
        if (medya == null) return;

        var esleme = await vt.UrunMedyalari
            .FirstOrDefaultAsync(um => um.UrunId == urunId && um.MedyaUrl == medya.DosyaYolu, iptal);

        if (esleme != null)
        {
            vt.UrunMedyalari.Remove(esleme);
            await vt.SaveChangesAsync(iptal);
        }
    }

    private async Task<UygulamaAlanSonucu?> GorselKaydetAsync(
        Image kaynak, int pdfId, int sayfaNo, string konum,
        Rectangle kesim, string hedefKlasor, CancellationToken iptal)
    {
        var dosyaAdi = $"{pdfId}_s{sayfaNo}_{konum}_uygulama.jpg";
        var relativePath = $"medya/kataloglar/uygulama-alanlari/{dosyaAdi}";
        var hedefDosya = Path.Combine(hedefKlasor, dosyaAdi);

        var mevcutMedya = await vt.Medyalar
            .FirstOrDefaultAsync(m => m.DosyaYolu == relativePath && !m.SilindiMi, iptal);

        if (mevcutMedya != null)
        {
            return new UygulamaAlanSonucu
            {
                MedyaId = mevcutMedya.Id,
                PdfKaynagiId = pdfId,
                SayfaNo = sayfaNo,
                Konum = konum == "sol" ? "Sol" : "Sağ",
                GorselUrl = $"/{relativePath}"
            };
        }

        using var krop = kaynak.Clone(ctx => ctx.Crop(kesim));
        var encoder = new JpegEncoder { Quality = JpegKalitesi };
        await krop.SaveAsJpegAsync(hedefDosya, encoder, iptal);

        var bilgi = new FileInfo(hedefDosya);
        var yeniMedya = new Medya
        {
            Ad = $"Katalog PDF{pdfId} S{sayfaNo} {(konum == "sol" ? "Sol" : "Sağ")} Uygulama",
            OrijinalAd = dosyaAdi,
            DosyaYolu = relativePath,
            Tip = MedyaTipi.Resim,
            Kaynak = MedyaKaynagi.Yerel,
            BoyutByte = bilgi.Length,
            MimeTipi = "image/jpeg",
            Genislik = kesim.Width,
            Yukseklik = kesim.Height,
            OlusturulmaTarihi = DateTime.UtcNow
        };

        vt.Medyalar.Add(yeniMedya);
        await vt.SaveChangesAsync(iptal);

        return new UygulamaAlanSonucu
        {
            MedyaId = yeniMedya.Id,
            PdfKaynagiId = pdfId,
            SayfaNo = sayfaNo,
            Konum = konum == "sol" ? "Sol" : "Sağ",
            GorselUrl = $"/{relativePath}"
        };
    }

    private static bool PdfVeSayfaNoAyristir(string dosyaAdi, out int pdfId, out int sayfaNo)
    {
        pdfId = 0;
        sayfaNo = 0;
        var p = dosyaAdi.Split('_');
        if (p.Length < 3) return false;
        return int.TryParse(p[0], out pdfId) && int.TryParse(p[2], out sayfaNo);
    }

    private static void AyristirDosyaAdi(string dosyaAdi, out int pdfId, out int sayfaNo, out string konum)
    {
        pdfId = 0; sayfaNo = 0; konum = "";
        var p = Path.GetFileNameWithoutExtension(dosyaAdi).Split('_');
        if (p.Length < 3) return;
        int.TryParse(p[0], out pdfId);
        int.TryParse(p[1].TrimStart('s'), out sayfaNo);
        konum = p.Length > 2 && p[2] == "sol" ? "Sol" : "Sağ";
    }
}
