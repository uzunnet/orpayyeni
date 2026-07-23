using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VizitLink3D.Api.Moduller.Konfigurasyon.Komutlar;
using VizitLink3D.Api.Moduller.Urunler.Servisler;
using VizitLink3D.Api.Servisler;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Renkler;
using VizitLink3D.Ortak.Modeller.Urunler;

namespace VizitLink3D.Testler;

/// <summary>
/// Multi-Tenant Ürün Güvenlik Testleri.
/// Gerçek SQLite in-memory DbContext ile tenant izolasyonunu doğrular.
/// </summary>
public class MultiTenantUrunGuvenlikTestleri : IDisposable
{
    private readonly SqliteConnection _baglanti;
    private readonly ServiceProvider _servisler;

    public MultiTenantUrunGuvenlikTestleri()
    {
        _baglanti = new SqliteConnection("DataSource=:memory:");
        _baglanti.Open();

        using var pragmaCmd = _baglanti.CreateCommand();
        pragmaCmd.CommandText = "PRAGMA foreign_keys = ON;";
        pragmaCmd.ExecuteNonQuery();

        var servisKoleksiyonu = new ServiceCollection();
        servisKoleksiyonu.AddDbContext<VizitLink3DDbContext>(o =>
            o.UseSqlite(_baglanti));
        servisKoleksiyonu.AddHttpContextAccessor();
        servisKoleksiyonu.AddScoped<KiraciServisi>();
        _servisler = servisKoleksiyonu.BuildServiceProvider();

        using var kapsam = _servisler.CreateScope();
        var vt = kapsam.ServiceProvider.GetRequiredService<VizitLink3DDbContext>();
        vt.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _servisler.Dispose();
        _baglanti.Dispose();
    }

    private (VizitLink3DDbContext vt, KiraciServisi ks) KapsamOlustur(int? firmaId)
    {
        var kapsam = _servisler.CreateScope();
        var vt = kapsam.ServiceProvider.GetRequiredService<VizitLink3DDbContext>();
        var hca = kapsam.ServiceProvider.GetRequiredService<IHttpContextAccessor>();

        if (firmaId.HasValue)
        {
            hca.HttpContext = new DefaultHttpContext();
            hca.HttpContext.Items["FirmaId"] = firmaId.Value;
            hca.HttpContext.Items["FirmaSlug"] = "test-firma";
        }

        var ks = new KiraciServisi(hca);
        return (vt, ks);
    }

    /// <summary>Temel tohum: Firma + UrunAilesi + Urun. Hepsi varsa eklemez.</summary>
    private static async Task TemelVeriHazirla(VizitLink3DDbContext vt, int firmaId, string firmaSlug)
    {
        if (!await vt.Firmalar.AnyAsync(f => f.Id == firmaId))
        {
            vt.Firmalar.Add(new Firma
            {
                Id = firmaId, Ad = firmaSlug, Unvan = firmaSlug, Slug = firmaSlug,
                AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow
            });
        }
        if (!await vt.UrunAilesileri.AnyAsync(a => a.Id == 1))
        {
            vt.UrunAilesileri.Add(new UrunAilesi
            {
                Id = 1, Ad = "Test Ailesi", Slug = "test-ailesi",
                AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow
            });
        }
        await vt.SaveChangesAsync();
    }

    /// <summary>Ürün ekle, UrunAilesi ve Firma mevcut olmalı.</summary>
    private static async Task<int> UrunEkle(VizitLink3DDbContext vt, string slug, int firmaId,
        int urunAilesiId = 1, int id = 0)
    {
        var urun = new Urun
        {
            Slug = slug, Kod = slug.ToUpperInvariant(), Ad = slug + " Ürünü",
            UrunAilesiId = urunAilesiId, FirmaId = firmaId,
            AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow
        };
        if (id > 0) urun.Id = id;
        vt.Urunler.Add(urun);
        await vt.SaveChangesAsync();
        return urun.Id;
    }

    /// <summary>Model ekle.</summary>
    private static async Task ModelEkle(VizitLink3DDbContext vt, int id, int urunId)
    {
        vt.UrunUcBoyutModelleri.Add(new UrunUcBoyutModeli
        {
            Id = id, UrunId = urunId, ModelAdi = "Model " + id,
            ModelDosyaYolu = "/test.glb", AktifMi = true,
            OlusturulmaTarihi = DateTime.UtcNow
        });
        await vt.SaveChangesAsync();
    }

    // ================================================================
    // TEST 1: FirmaId null → fail closed
    // ================================================================
    [Fact]
    public async Task PublicKonfigurator_FirmaIdNull_FailClosedDonmeli()
    {
        var (vt, ks) = KapsamOlustur(null);
        var isleyici = new PublicKonfiguratorIsleyici(vt, ks);
        var sonuc = await isleyici.Handle(new PublicKonfiguratorSorgusu("herhangi"), CancellationToken.None);
        Assert.False(sonuc.BasariliMi);
        Assert.Contains("Firma", sonuc.Mesaj);
    }

    // ================================================================
    // TEST 2: FirmaId 0 → fail closed
    // ================================================================
    [Fact]
    public async Task PublicKonfigurator_FirmaIdSifir_FailClosedDonmeli()
    {
        var (vt, ks) = KapsamOlustur(0);
        var isleyici = new PublicKonfiguratorIsleyici(vt, ks);
        var sonuc = await isleyici.Handle(new PublicKonfiguratorSorgusu("herhangi"), CancellationToken.None);
        Assert.False(sonuc.BasariliMi);
        Assert.Contains("Firma", sonuc.Mesaj);
    }

    // ================================================================
    // TEST 3: Tenant A → kendi ürününü görebilmeli
    // ================================================================
    [Fact]
    public async Task PublicKonfigurator_TenantA_KendiUrununuGorebilmeli()
    {
        var (vt, ks) = KapsamOlustur(1);
        await TemelVeriHazirla(vt, 1, "firma-a");
        await UrunEkle(vt, "test-urun-a", 1);

        var isleyici = new PublicKonfiguratorIsleyici(vt, ks);
        var sonuc = await isleyici.Handle(new PublicKonfiguratorSorgusu("test-urun-a"), CancellationToken.None);

        Assert.True(sonuc.BasariliMi);
        Assert.Equal("test-urun-a", sonuc.Veri!.Slug);
    }

    // ================================================================
    // TEST 4: Tenant B → Tenant A'nın slug'ına erişemez
    // ================================================================
    [Fact]
    public async Task PublicKonfigurator_TenantB_BaskaTenantUrununeErisemez()
    {
        // Tenant A'nın ürününü oluştur
        using var kA = _servisler.CreateScope();
        var vtA = kA.ServiceProvider.GetRequiredService<VizitLink3DDbContext>();
        var hcaA = kA.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        hcaA.HttpContext = new DefaultHttpContext();
        hcaA.HttpContext.Items["FirmaId"] = 1;
        await TemelVeriHazirla(vtA, 1, "firma-a");
        await UrunEkle(vtA, "ortak-slug", 1);

        // Tenant B olarak sorgula
        var (vtB, ksB) = KapsamOlustur(2);
        await TemelVeriHazirla(vtB, 2, "firma-b");

        var isleyici = new PublicKonfiguratorIsleyici(vtB, ksB);
        var sonuc = await isleyici.Handle(new PublicKonfiguratorSorgusu("ortak-slug"), CancellationToken.None);

        Assert.False(sonuc.BasariliMi);
        Assert.Contains("bulunamadı", sonuc.Mesaj);
    }

    // ================================================================
    // TEST 5: PublicSecimKaydet → başka tenant ürününe kayıt reddi
    // ================================================================
    [Fact]
    public async Task PublicSecimKaydet_BaskaTenantUrunu_Reddedilmeli()
    {
        // Tenant A: ürün + model + parça
        using var kA = _servisler.CreateScope();
        var vtA = kA.ServiceProvider.GetRequiredService<VizitLink3DDbContext>();
        await TemelVeriHazirla(vtA, 1, "firma-a");
        await UrunEkle(vtA, "urun-a", 1, id: 10);
        await ModelEkle(vtA, 1, 10);
        vtA.UrunUcBoyutParcalari.Add(new UrunUcBoyutParcasi
        {
            Id = 100, UrunUcBoyutModeliId = 1, MeshAdi = "m", GorunenAd = "P",
            AdminOnayliMi = true, AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow
        });
        await vtA.SaveChangesAsync();

        // Tenant B: başkasının ürününe seçim yapmaya çalış
        var (vtB, ksB) = KapsamOlustur(2);
        await TemelVeriHazirla(vtB, 2, "firma-b");

        var isleyici = new PublicSecimKaydetIsleyici(vtB, ksB);
        var sonuc = await isleyici.Handle(new PublicSecimKaydetKomutu(10, null,
            [new PublicParcaSecimiDto { ParcaId = 100, GorunurMu = true }]), CancellationToken.None);

        Assert.False(sonuc.BasariliMi);
        Assert.Contains("bulunamadı", sonuc.Mesaj);
    }

    // ================================================================
    // TEST 6: Parça farklı modele ait → red
    // ================================================================
    [Fact]
    public async Task PublicSecimKaydet_ParcaBaskaModel_Reddedilmeli()
    {
        var (vt, ks) = KapsamOlustur(1);
        await TemelVeriHazirla(vt, 1, "orpay");

        var u1 = await UrunEkle(vt, "urun-1", 1, id: 1);
        var u2 = await UrunEkle(vt, "urun-2", 1, id: 2);
        await ModelEkle(vt, 1, u1);
        await ModelEkle(vt, 2, u2);

        // Parça model 2'ye ait
        vt.UrunUcBoyutParcalari.Add(new UrunUcBoyutParcasi
        {
            Id = 200, UrunUcBoyutModeliId = 2, MeshAdi = "m2", GorunenAd = "P2",
            AdminOnayliMi = true, AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow
        });
        await vt.SaveChangesAsync();

        // Ürün 1 için parça 200'ü kullan (model zinciri farklı)
        var isleyici = new PublicSecimKaydetIsleyici(vt, ks);
        var sonuc = await isleyici.Handle(new PublicSecimKaydetKomutu(u1, null,
            [new PublicParcaSecimiDto { ParcaId = 200, GorunurMu = true }]), CancellationToken.None);

        Assert.False(sonuc.BasariliMi);
        Assert.Contains("Geçersiz parça", sonuc.Mesaj);
    }

    // ================================================================
    // TEST 7: Onaysız sahne preset'i dönmez
    // ================================================================
    [Fact]
    public async Task PublicKonfigurator_OnaysizSahneOnayari_Donmemeli()
    {
        var (vt, ks) = KapsamOlustur(1);
        await TemelVeriHazirla(vt, 1, "orpay");
        var urunId = await UrunEkle(vt, "onaysiz-test", 1);
        await ModelEkle(vt, 1, urunId);

        vt.UrunUcBoyutSahneOnayarlari.Add(new UrunUcBoyutSahneOnayari
        {
            Id = 1, UrunUcBoyutModeliId = 1, Ad = "Onaylı", Kod = "onayli",
            AdminOnayliMi = true, AktifMi = true, SiraNo = 1, OlusturulmaTarihi = DateTime.UtcNow
        });
        vt.UrunUcBoyutSahneOnayarlari.Add(new UrunUcBoyutSahneOnayari
        {
            Id = 2, UrunUcBoyutModeliId = 1, Ad = "Onaysız", Kod = "onaysiz",
            AdminOnayliMi = false, AktifMi = true, SiraNo = 2, OlusturulmaTarihi = DateTime.UtcNow
        });
        await vt.SaveChangesAsync();

        var isleyici = new PublicKonfiguratorIsleyici(vt, ks);
        var sonuc = await isleyici.Handle(new PublicKonfiguratorSorgusu("onaysiz-test"), CancellationToken.None);

        Assert.True(sonuc.BasariliMi);
        Assert.Single(sonuc.Veri!.SahneOnayarlari);
        Assert.Equal("onayli", sonuc.Veri.SahneOnayarlari[0].Kod);
    }

    // ================================================================
    // TEST 8: Seed → sadece FirmaId=null olanları atar, mevcutları korur
    // ================================================================
    [Fact]
    public async Task UrunFirmaIdAta_SadeceNullOlanlariAtar_MevcutlariKorur()
    {
        using var kapsam = _servisler.CreateScope();
        var vt = kapsam.ServiceProvider.GetRequiredService<VizitLink3DDbContext>();

        vt.Firmalar.Add(new Firma
        {
            Id = 1, Ad = "Orpay Orman Ürünleri", Unvan = "Orpay Orman Ürünleri", Slug = "orpay",
            AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow
        });
        // FirmaId=5 de FK için mevcut olmalı
        vt.Firmalar.Add(new Firma
        {
            Id = 5, Ad = "Diğer Firma", Unvan = "Diğer", Slug = "diger",
            AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow
        });
        vt.UrunAilesileri.Add(new UrunAilesi
        {
            Id = 1, Ad = "Test", Slug = "test", AktifMi = true,
            OlusturulmaTarihi = DateTime.UtcNow
        });

        vt.Urunler.Add(new Urun
        {
            Id = 1, Slug = "atanmis", Kod = "A1", Ad = "Atanmış", UrunAilesiId = 1,
            FirmaId = 5, AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow
        });
        vt.Urunler.Add(new Urun
        {
            Id = 2, Slug = "atanmamis", Kod = "A2", Ad = "Atanmamış", UrunAilesiId = 1,
            FirmaId = null, AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow
        });
        await vt.SaveChangesAsync();

        var metod = typeof(TohumVerisi).GetMethod("UrunFirmaIdAtaAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
        await (Task)metod.Invoke(null, [vt])!;

        Assert.Equal(5, (await vt.Urunler.FindAsync(1))!.FirmaId);
        Assert.Equal(1, (await vt.Urunler.FindAsync(2))!.FirmaId);
    }

    // ================================================================
    // TEST 9: SuperAdmin → her modele erişir
    // ================================================================
    [Fact]
    public async Task UcBoyutModelSahiplikDogrulayici_SuperAdmin_HepErisir()
    {
        using var kapsam = _servisler.CreateScope();
        var vt = kapsam.ServiceProvider.GetRequiredService<VizitLink3DDbContext>();
        var hca = kapsam.ServiceProvider.GetRequiredService<IHttpContextAccessor>();

        hca.HttpContext = new DefaultHttpContext();
        hca.HttpContext.Items["FirmaId"] = 5;
        hca.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(
                [new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "SuperAdmin")], "test"));

        var ks = new KiraciServisi(hca);
        var dogrulayici = new UcBoyutModelSahiplikDogrulayici(vt, ks, hca);
        Assert.True(await dogrulayici.ModelSahibiniDogrulaAsync(99999));
    }

    // ================================================================
    // TEST 10: Admin → başka tenant modeline erişemez
    // ================================================================
    [Fact]
    public async Task UcBoyutModelSahiplikDogrulayici_Admin_BaskaTenantModelineErisemez()
    {
        using var kapsam = _servisler.CreateScope();
        var vt = kapsam.ServiceProvider.GetRequiredService<VizitLink3DDbContext>();
        var hca = kapsam.ServiceProvider.GetRequiredService<IHttpContextAccessor>();

        await TemelVeriHazirla(vt, 1, "f1");
        await UrunEkle(vt, "f1-urun", 1, id: 1);
        await ModelEkle(vt, 1, 1);

        hca.HttpContext = new DefaultHttpContext();
        hca.HttpContext.Items["FirmaId"] = 2;
        hca.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(
                [new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Admin")], "test"));

        var ks = new KiraciServisi(hca);
        var dogrulayici = new UcBoyutModelSahiplikDogrulayici(vt, ks, hca);
        Assert.False(await dogrulayici.ModelSahibiniDogrulaAsync(1));
    }

    // ================================================================
    // TEST 11: Renk farklı parçaya ait → red
    // ================================================================
    [Fact]
    public async Task PublicSecimKaydet_RenkBaskaParcayaAit_Reddetmeli()
    {
        var (vt, ks) = KapsamOlustur(1);
        await TemelVeriHazirla(vt, 1, "orpay");
        var urunId = await UrunEkle(vt, "renk-test", 1, id: 1);
        await ModelEkle(vt, 1, urunId);

        vt.UrunUcBoyutParcalari.Add(new UrunUcBoyutParcasi
        {
            Id = 10, UrunUcBoyutModeliId = 1, MeshAdi = "m", GorunenAd = "P",
            AdminOnayliMi = true, AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow
        });
        vt.RalRenkleri.Add(new RalRengi
        {
            Id = 1, Kod = "RAL-1000", Ad = "Yeşil", HexKod = "#CDBA88",
            OlusturulmaTarihi = DateTime.UtcNow
        });
        // Renk seçeneği FARKLI parçaya ait (99 ≠ 10)
        vt.UrunParcaRenkSecenekleri.Add(new UrunParcaRenkSecenegi
        {
            Id = 500, UrunUcBoyutParcasiId = 99, RalRengiId = 1, AktifMi = true
        });
        await vt.SaveChangesAsync();

        var isleyici = new PublicSecimKaydetIsleyici(vt, ks);
        var sonuc = await isleyici.Handle(new PublicSecimKaydetKomutu(urunId, null,
            [new PublicParcaSecimiDto { ParcaId = 10, SeciliRenkId = 500, GorunurMu = true }]),
            CancellationToken.None);

        Assert.False(sonuc.BasariliMi);
        Assert.Contains("renk", sonuc.Mesaj, StringComparison.OrdinalIgnoreCase);
    }

    // ================================================================
    // TEST 12: SahneOnayariAdminOnay → sadece varsayılanları onaylar
    // ================================================================
    [Fact]
    public async Task SahneOnayariAdminOnay_SadeceVarsayilanlariOnaylar()
    {
        using var kapsam = _servisler.CreateScope();
        var vt = kapsam.ServiceProvider.GetRequiredService<VizitLink3DDbContext>();

        // FK için gerekli: UrunAilesi, Urun, UrunUcBoyutModeli
        vt.UrunAilesileri.Add(new UrunAilesi
        {
            Id = 1, Ad = "Test", Slug = "test", AktifMi = true,
            OlusturulmaTarihi = DateTime.UtcNow
        });
        vt.Firmalar.Add(new Firma
        {
            Id = 1, Ad = "F1", Unvan = "F1", Slug = "f1", AktifMi = true,
            OlusturulmaTarihi = DateTime.UtcNow
        });
        vt.Urunler.Add(new Urun
        {
            Id = 1, Slug = "t", Kod = "T", Ad = "T", UrunAilesiId = 1,
            FirmaId = 1, AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow
        });
        vt.UrunUcBoyutModelleri.Add(new UrunUcBoyutModeli
        {
            Id = 1, UrunId = 1, ModelAdi = "M", ModelDosyaYolu = "/m.glb",
            AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow
        });
        await vt.SaveChangesAsync();

        vt.UrunUcBoyutSahneOnayarlari.Add(new UrunUcBoyutSahneOnayari
        {
            Id = 1, UrunUcBoyutModeliId = 1, Ad = "Varsayılan", Kod = "v",
            VarsayilanMi = true, AdminOnayliMi = false, AktifMi = true,
            OlusturulmaTarihi = DateTime.UtcNow
        });
        vt.UrunUcBoyutSahneOnayarlari.Add(new UrunUcBoyutSahneOnayari
        {
            Id = 2, UrunUcBoyutModeliId = 1, Ad = "Normal", Kod = "n",
            VarsayilanMi = false, AdminOnayliMi = false, AktifMi = true,
            OlusturulmaTarihi = DateTime.UtcNow
        });
        await vt.SaveChangesAsync();

        var metod = typeof(TohumVerisi).GetMethod("SahneOnayariAdminOnayAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
        await (Task)metod.Invoke(null, [vt])!;

        Assert.True((await vt.UrunUcBoyutSahneOnayarlari.FindAsync(1))!.AdminOnayliMi);
        Assert.False((await vt.UrunUcBoyutSahneOnayarlari.FindAsync(2))!.AdminOnayliMi);
    }
}
