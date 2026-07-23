using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VizitLink3D.Api.Servisler;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller.Medya;
using VizitLink3D.Ortak.Modeller.Urunler;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Xunit;

namespace VizitLink3D.Testler;

public class TestWebHostEnvironment : IWebHostEnvironment
{
    public string WebRootPath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    public IFileProvider WebRootFileProvider { get; set; } = null!;
    public string ApplicationName { get; set; } = "VizitLink3D.Test";
    public IFileProvider ContentRootFileProvider { get; set; } = null!;
    public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
    public string EnvironmentName { get; set; } = "Development";
}

public class PdfKatalogTestleri : IDisposable
{
    private readonly SqliteConnection _baglanti;
    private readonly VizitLink3DDbContext _vt;
    private readonly TestWebHostEnvironment _env;

    public PdfKatalogTestleri()
    {
        _baglanti = new SqliteConnection("DataSource=:memory:");
        _baglanti.Open();

        var secenekler = new DbContextOptionsBuilder<VizitLink3DDbContext>()
            .UseSqlite(_baglanti)
            .Options;

        _vt = new VizitLink3DDbContext(secenekler, new KiraciServisi(null));
        _vt.Database.EnsureCreated();

        _env = new TestWebHostEnvironment();
        if (!Directory.Exists(_env.WebRootPath))
        {
            Directory.CreateDirectory(_env.WebRootPath);
        }
    }

    public void Dispose()
    {
        _vt.Dispose();
        _baglanti.Close();
        if (Directory.Exists(_env.WebRootPath))
        {
            try { Directory.Delete(_env.WebRootPath, true); } catch { }
        }
    }

    [Fact]
    public async Task GocEtAsync_KaynakKlasorBulunamadi_HataDonmeli()
    {
        var olmayanKaynakDizin = Path.Combine(Path.GetTempPath(), $"vizitlink3d-test-kaynak-{Guid.NewGuid():N}");
        var yapilandirma = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MedyaGoc:KaynakDizin"] = olmayanKaynakDizin
            })
            .Build();
        var servis = new MedyaGocServisi(_vt, _env, yapilandirma);
        
        var cevap = await servis.GocEtAsync();

        Assert.False(cevap.BasariliMi);
        Assert.NotNull(cevap.Hatalar);
        Assert.Contains(cevap.Hatalar, h => h.Contains("Kaynak dizin bulunamadı"));
    }

    [Fact]
    public async Task CozumleAsync_PdfKaynagiYok_SessizceDonmeli()
    {
        var servis = new PdfCozumlemeServisi(_vt, _env);
        
        // Veritabanında hiçbir PDF kaynağı yokken 999 id'sini çağıralım
        await servis.CozumleAsync(999);

        // Hiçbir hata fırlatmadan sessizce dönmeli
        var kaynak = await _vt.UrunPdfKaynaklari.FindAsync(999);
        Assert.Null(kaynak);
    }

    [Fact]
    public async Task CozumleAsync_PdfKaynaginaAitMedyaYok_DurumuHataYapmali()
    {
        // 1. PDF kaynağını ekleyelim (fakat MedyaId'si veritabanında olmasın)
        var pdfKaynagi = new UrunPdfKaynagi
        {
            Id = 10,
            Ad = "Hatalı Medya Katalogu",
            MedyaId = 999, // Olmayan bir Medya ID
            CozumlemeDurumu = "Bekliyor"
        };
        _vt.UrunPdfKaynaklari.Add(pdfKaynagi);
        await _vt.SaveChangesAsync();

        var servis = new PdfCozumlemeServisi(_vt, _env);
        await servis.CozumleAsync(10);

        // 2. Durumunun Hata olarak güncellendiğini doğrulayalım
        var guncelKaynak = await _vt.UrunPdfKaynaklari.FindAsync(10);
        Assert.NotNull(guncelKaynak);
        Assert.Equal("Hata", guncelKaynak.CozumlemeDurumu);
        Assert.Contains("medya kaydı bulunamadı", guncelKaynak.HataMesaji);
    }

    [Fact]
    public async Task CozumleAsync_DiskDosyasiBulunamadi_DurumuHataYapmali()
    {
        // 1. Medya kaydı ekle (dosya diskte yok)
        var medya = new Medya
        {
            Id = 20,
            Ad = "Test PDF",
            DosyaYolu = "medya/kataloglar/olmayan_dosya.pdf",
            Tip = MedyaTipi.Pdf,
            Kaynak = MedyaKaynagi.Yerel
        };
        _vt.Medyalar.Add(medya);

        var pdfKaynagi = new UrunPdfKaynagi
        {
            Id = 20,
            Ad = "Olmayan Dosya Katalogu",
            MedyaId = 20,
            CozumlemeDurumu = "Bekliyor"
        };
        _vt.UrunPdfKaynaklari.Add(pdfKaynagi);
        await _vt.SaveChangesAsync();

        var servis = new PdfCozumlemeServisi(_vt, _env);
        await servis.CozumleAsync(20);

        // 2. Durumunun Hata olarak güncellendiğini ve "diskte bulunamadı" mesajı içerdiğini doğrulayalım
        var guncelKaynak = await _vt.UrunPdfKaynaklari.FindAsync(20);
        Assert.NotNull(guncelKaynak);
        Assert.Equal("Hata", guncelKaynak.CozumlemeDurumu);
        Assert.Contains("disk üzerinde PDF dosyası bulunamadı", guncelKaynak.HataMesaji, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AktarimSonucu_VarsayilanDegerleri_DogruMu()
    {
        var sonuc = new AktarimSonucu();

        Assert.Equal(0, sonuc.Toplam);
        Assert.Equal(0, sonuc.AktarilanPdf);
        Assert.Equal(0, sonuc.AktarilanResim);
        Assert.NotNull(sonuc.Hatalar);
        Assert.Empty(sonuc.Hatalar);
    }
}
