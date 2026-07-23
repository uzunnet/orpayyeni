using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller.Medya;
using VizitLink3D.Ortak.Modeller.Urunler;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using PDFtoImage;

namespace VizitLink3D.Api.Servisler;

/// <summary>
/// PDF Katalog dosyalarını sayfalarına ayırıp her bir sayfayı 
/// resim olarak kaydeden ve veritabanına işleyen %100 C# servisidir.
/// </summary>
public class PdfCozumlemeServisi(VizitLink3DDbContext vt, IWebHostEnvironment env)
{
    private readonly VizitLink3DDbContext _vt = vt;
    private readonly IWebHostEnvironment _env = env;

    /// <summary>
    /// Belirtilen PDF kaynağını sayfa sayfa çözümler.
    /// </summary>
    public async Task CozumleAsync(int pdfKaynagiId, CancellationToken iptal = default)
    {
        var mevcutPdf = await _vt.UrunPdfKaynaklari.FindAsync([pdfKaynagiId], cancellationToken: iptal);
        if (mevcutPdf == null) return;

        try
        {
            // Durumu İşleniyor yap
            mevcutPdf.CozumlemeDurumu = "Isleniyor";
            mevcutPdf.HataMesaji = null;
            mevcutPdf.CozumlemeTarihi = null;
            _vt.UrunPdfKaynaklari.Update(mevcutPdf);
            await _vt.SaveChangesAsync(iptal);

            // İlgili medyayı veritabanından çek
            var medya = await _vt.Medyalar
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Id == mevcutPdf.MedyaId, iptal);

            if (medya == null || string.IsNullOrWhiteSpace(medya.DosyaYolu))
            {
                throw new Exception($"PDF kaynağına ait medya kaydı bulunamadı (MedyaId: {mevcutPdf.MedyaId})");
            }

            var pdfDosyaYolu = Path.Combine(_env.WebRootPath, medya.DosyaYolu.Replace("/", "\\"));
            if (!File.Exists(pdfDosyaYolu))
            {
                throw new Exception($"Disk üzerinde PDF dosyası bulunamadı: {pdfDosyaYolu}");
            }

            if (!PdfCozumlemePlatformuDestekleniyor())
            {
                throw new PlatformNotSupportedException("PDF çözümleme yalnızca Windows, Linux ve macOS üzerinde desteklenir.");
            }

            // Sayfaları kaydedeceğimiz hedef klasör oluşturulur
            var hedefKlasor = Path.Combine(_env.WebRootPath, "medya", "kataloglar", "sayfalar");
            if (!Directory.Exists(hedefKlasor))
            {
                Directory.CreateDirectory(hedefKlasor);
            }

            // Önceki çözümlenmiş sayfaları veritabanından ve diskten temizle
            var eskiSayfalar = await _vt.PdfSayfaGorselleri
                .Where(s => s.PdfKaynagiId == pdfKaynagiId)
                .ToListAsync(iptal);

            foreach (var eskiSayfa in eskiSayfalar)
            {
                var eskiMedya = await _vt.Medyalar
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(m => m.Id == eskiSayfa.MedyaId, iptal);

                if (eskiMedya != null && !string.IsNullOrWhiteSpace(eskiMedya.DosyaYolu))
                {
                    var eskiDosyaYolu = Path.Combine(_env.WebRootPath, eskiMedya.DosyaYolu.Replace("/", "\\"));
                    if (File.Exists(eskiDosyaYolu))
                    {
                        File.Delete(eskiDosyaYolu);
                    }
                    _vt.Medyalar.Remove(eskiMedya);
                }
                _vt.PdfSayfaGorselleri.Remove(eskiSayfa);
            }
            await _vt.SaveChangesAsync(iptal);

            // PDF sayfa sayısını bul
            var sayfaSayisi = PdfSayfaSayisiniGetir(pdfDosyaYolu);

            if (sayfaSayisi <= 0)
            {
                throw new Exception("PDF sayfası okunamadı veya dosya boş.");
            }

            // Her bir sayfayı çözüp kaydet
            for (int i = 0; i < sayfaSayisi; i++)
            {
                var sayfaNo = i + 1;
                var dosyaAdi = $"{pdfKaynagiId}_sayfa_{sayfaNo}.png";
                var relativePath = $"medya/kataloglar/sayfalar/{dosyaAdi}";
                var hedefDosyaYolu = Path.Combine(hedefKlasor, dosyaAdi);

                // Sayfayı PNG olarak kaydet
                PdfSayfayiPngKaydet(pdfDosyaYolu, hedefDosyaYolu, i);

                // Medyaya kaydet
                var sayfaMedya = new Medya
                {
                    Ad = $"{mevcutPdf.Ad} - Sayfa {sayfaNo}",
                    OrijinalAd = dosyaAdi,
                    DosyaYolu = relativePath,
                    Tip = MedyaTipi.Resim,
                    Kaynak = MedyaKaynagi.Yerel,
                    BoyutByte = new FileInfo(hedefDosyaYolu).Length,
                    MimeTipi = "image/png",
                    OlusturulmaTarihi = DateTime.UtcNow
                };

                _vt.Medyalar.Add(sayfaMedya);
                await _vt.SaveChangesAsync(iptal);

                // PdfSayfaGorseli tablosuna ekle
                var sayfaGorsel = new PdfSayfaGorseli
                {
                    PdfKaynagiId = pdfKaynagiId,
                    SayfaNo = sayfaNo,
                    MedyaId = sayfaMedya.Id,
                    Aciklama = $"{mevcutPdf.Ad} sayfa {sayfaNo} görseli.",
                    UruneBaglandiMi = false,
                    OlusturulmaTarihi = DateTime.UtcNow
                };

                _vt.PdfSayfaGorselleri.Add(sayfaGorsel);
                await _vt.SaveChangesAsync(iptal);
            }

            // Durumu Tamamlandı olarak güncelle
            mevcutPdf.CozumlemeDurumu = "Tamamlandi";
            mevcutPdf.SayfaSayisi = sayfaSayisi;
            mevcutPdf.CozumlemeTarihi = DateTime.UtcNow;
            _vt.UrunPdfKaynaklari.Update(mevcutPdf);
            await _vt.SaveChangesAsync(iptal);
        }
        catch (Exception ex)
        {
            mevcutPdf.CozumlemeDurumu = "Hata";
            mevcutPdf.HataMesaji = $"Çözümleme Sırasında C# Hatası: {ex.Message}";
            mevcutPdf.CozumlemeTarihi = DateTime.UtcNow;
            _vt.UrunPdfKaynaklari.Update(mevcutPdf);
            await _vt.SaveChangesAsync(iptal);
        }
    }

    [UnconditionalSuppressMessage(
        "Interoperability",
        "CA1416:Validate platform compatibility",
        Justification = "PDF çözümleme API servisi Windows/Linux/macOS üzerinde çalışır; çağrı öncesinde platform koruması uygulanır.")]
    private static int PdfSayfaSayisiniGetir(string pdfDosyaYolu)
    {
        using var stream = File.OpenRead(pdfDosyaYolu);
        return Conversion.GetPageCount(stream);
    }

    [UnconditionalSuppressMessage(
        "Interoperability",
        "CA1416:Validate platform compatibility",
        Justification = "PDF çözümleme API servisi Windows/Linux/macOS üzerinde çalışır; çağrı öncesinde platform koruması uygulanır.")]
    private static void PdfSayfayiPngKaydet(string pdfDosyaYolu, string hedefDosyaYolu, int sayfaIndeksi)
    {
        using var input = File.OpenRead(pdfDosyaYolu);
        using var output = File.Create(hedefDosyaYolu);
        Conversion.SavePng(output, input, page: sayfaIndeksi);
    }

    private static bool PdfCozumlemePlatformuDestekleniyor() =>
        OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS();
}
