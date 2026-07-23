using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Malzemeler;
using VizitLink3D.Ortak.Modeller.Renkler;
using VizitLink3D.Ortak.Modeller.Urunler;
using System.Text.Json;

namespace VizitLink3D.Testler;

public class UrunlerModulTestleri
{
    // ═══════════════════════════════════════════════════════════════
    // T.2 — UrunlerKontrolcu Testleri (Liste, Detay, Slug, Olustur, Guncelle, Sil)
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void Urun_VarsayilanDegerler_DogruOlmali()
    {
        var urun = new Urun();

        Assert.Equal(string.Empty, urun.Ad);
        Assert.Equal(string.Empty, urun.Slug);
        Assert.Equal(string.Empty, urun.Kod);
        Assert.True(urun.AktifMi);
        Assert.False(urun.SilindiMi);
        Assert.False(urun.OneCikanMi);
        Assert.False(urun.YeniMi);
        Assert.Equal(0, urun.SiraNo);
    }

    [Fact]
    public void Urun_OlusturulmaTarihi_UtcNowOlmali()
    {
        var once = DateTime.UtcNow.AddSeconds(-1);
        var urun = new Urun();
        var sonra = DateTime.UtcNow.AddSeconds(1);

        Assert.True(urun.OlusturulmaTarihi >= once && urun.OlusturulmaTarihi <= sonra);
        Assert.NotEqual(default, urun.OlusturulmaTarihi);
    }

    [Fact]
    public void Urun_AuditAlanlari_MevcutOlmali()
    {
        var urun = new Urun
        {
            GuncellenmeTarihi = DateTime.UtcNow,
            SilindiMi = false,
            SilinmeTarihi = null
        };

        Assert.NotNull(urun.GuncellenmeTarihi);
        Assert.False(urun.SilindiMi);
        Assert.Null(urun.SilinmeTarihi);
    }

    [Fact]
    public void Urun_JsonIgnore_UrunAilesiNavigationGizlenmeli()
    {
        var urun = new Urun { Ad = "Test", UrunAilesiId = 1 };
        urun.UrunAilesi = new UrunAilesi { Ad = "Aile" };

        var json = JsonSerializer.Serialize(urun);

        Assert.Contains("Test", json);
        Assert.DoesNotContain("\"urunAilesi\"", json.ToLower());
    }

    [Fact]
    public void Urun_Slug_ZorunluAlanOlmali()
    {
        var urun = new Urun();

        Assert.Equal(string.Empty, urun.Slug);
        Assert.NotNull(urun.Slug);
    }

    [Fact]
    public void Urun_SoftDelete_AlanlariMevcutOlmali()
    {
        var urun = new Urun { SilindiMi = true, SilinmeTarihi = DateTime.UtcNow };

        Assert.True(urun.SilindiMi);
        Assert.NotNull(urun.SilinmeTarihi);
        Assert.NotEqual(default, urun.SilinmeTarihi!.Value);
    }

    // ═══════════════════════════════════════════════════════════════
    // T.3 — RalRenkKontrolcu Testleri
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void RalRengi_VarsayilanDegerler_DogruOlmali()
    {
        var renk = new RalRengi();

        Assert.Equal(string.Empty, renk.Kod);
        Assert.Equal(string.Empty, renk.Ad);
        Assert.Equal(string.Empty, renk.YuzeyTipi);
        Assert.True(renk.AktifMi);
        Assert.Equal(0, renk.KatalogId);
        Assert.Equal(0, renk.SiraNo);
    }

    [Fact]
    public void RalRengi_OlusturulmaTarihi_UtcNowOlmali()
    {
        var once = DateTime.UtcNow.AddSeconds(-1);
        var renk = new RalRengi();
        var sonra = DateTime.UtcNow.AddSeconds(1);

        Assert.True(renk.OlusturulmaTarihi >= once && renk.OlusturulmaTarihi <= sonra);
    }

    [Fact]
    public void RalRengi_AktifMi_VarsayilanTrueOlmali()
    {
        var renk = new RalRengi();
        Assert.True(renk.AktifMi);
    }

    [Fact]
    public void RalRengi_HexKod_NullableOlmali()
    {
        var renk = new RalRengi();
        Assert.Null(renk.HexKod);

        renk.HexKod = "#FF0000";
        Assert.Equal("#FF0000", renk.HexKod);
    }

    [Fact]
    public void RalRengi_JsonSerialize_KodVeAdGorunurOlmali()
    {
        var renk = new RalRengi { Kod = "RAL 9005", Ad = "Derin Siyah", HexKod = "#1A1A1A", KatalogId = 1 };

        var json = JsonSerializer.Serialize(renk);

        Assert.Contains("RAL 9005", json);
        Assert.Contains("Derin Siyah", json);
        Assert.Contains("1A1A1A", json);
        Assert.Contains("katalog", json.ToLower());
        Assert.Contains("1", json);
    }

    // ═══════════════════════════════════════════════════════════════
    // T.3 — MalzemeKontrolcu Testleri
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void Malzeme_VarsayilanDegerler_DogruOlmali()
    {
        var m = new Malzeme();

        Assert.Equal(string.Empty, m.Ad);
        Assert.Equal(string.Empty, m.Tip);
        Assert.True(m.AktifMi);
        Assert.False(m.SilindiMi);
        Assert.Equal(0, m.SiraNo);
    }

    [Fact]
    public void Malzeme_OlusturulmaTarihi_UtcNowOlmali()
    {
        var once = DateTime.UtcNow.AddSeconds(-1);
        var m = new Malzeme();
        var sonra = DateTime.UtcNow.AddSeconds(1);

        Assert.True(m.OlusturulmaTarihi >= once && m.OlusturulmaTarihi <= sonra);
    }

    [Fact]
    public void Malzeme_SoftDelete_AlanlariMevcutOlmali()
    {
        var m = new Malzeme { SilindiMi = true, SilinmeTarihi = DateTime.UtcNow };

        Assert.True(m.SilindiMi);
        Assert.NotNull(m.SilinmeTarihi);
    }

    [Fact]
    public void Malzeme_Aciklama_NullableOlmali()
    {
        var m = new Malzeme();

        Assert.Null(m.Aciklama);

        m.Aciklama = "Masif ahsap malzeme";
        Assert.Equal("Masif ahsap malzeme", m.Aciklama);
    }

    [Fact]
    public void Malzeme_ResimUrl_NullableOlmali()
    {
        var m = new Malzeme { ResimUrl = "/medya/malzeme/ahsap.jpg" };

        Assert.Equal("/medya/malzeme/ahsap.jpg", m.ResimUrl);
    }

    // ═══════════════════════════════════════════════════════════════
    // T.3 — KaplamaKontrolcu Testleri
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void KaplamaSecenegi_VarsayilanDegerler_DogruOlmali()
    {
        var k = new KaplamaSecenegi();

        Assert.Equal(string.Empty, k.Ad);
        Assert.True(k.AktifMi);
        Assert.False(k.SilindiMi);
        Assert.Equal(0, k.SiraNo);
        Assert.Null(k.MalzemeId);
    }

    [Fact]
    public void KaplamaSecenegi_OlusturulmaTarihi_UtcNowOlmali()
    {
        var once = DateTime.UtcNow.AddSeconds(-1);
        var k = new KaplamaSecenegi();
        var sonra = DateTime.UtcNow.AddSeconds(1);

        Assert.True(k.OlusturulmaTarihi >= once && k.OlusturulmaTarihi <= sonra);
    }

    [Fact]
    public void KaplamaSecenegi_SoftDelete_AlanlariMevcutOlmali()
    {
        var k = new KaplamaSecenegi { SilindiMi = true, SilinmeTarihi = DateTime.UtcNow };

        Assert.True(k.SilindiMi);
        Assert.NotNull(k.SilinmeTarihi);
    }

    [Fact]
    public void KaplamaSecenegi_HexKod_NullableOlmali()
    {
        var k = new KaplamaSecenegi { HexKod = "#C8952A" };

        Assert.Equal("#C8952A", k.HexKod);
        Assert.NotNull(k.HexKod);
    }

    [Fact]
    public void KaplamaSecenegi_MalzemeId_BaglantiKurmali()
    {
        var k = new KaplamaSecenegi { MalzemeId = 42, Ad = "Ceviz" };

        Assert.Equal(42, k.MalzemeId);
        Assert.Equal("Ceviz", k.Ad);
    }

    // ═══════════════════════════════════════════════════════════════
    // T.4 — KonfigurasyonKontrolcu Testleri
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void MusteriKonfigurasyonu_VarsayilanDegerler_DogruOlmali()
    {
        var k = new MusteriKonfigurasyonu();

        Assert.Equal(0, k.UrunId);
        Assert.False(k.SilindiMi);
        Assert.Null(k.Not);
    }

    [Fact]
    public void MusteriKonfigurasyonu_OlusturulmaTarihi_UtcNowOlmali()
    {
        var once = DateTime.UtcNow.AddSeconds(-1);
        var k = new MusteriKonfigurasyonu();
        var sonra = DateTime.UtcNow.AddSeconds(1);

        Assert.True(k.OlusturulmaTarihi >= once && k.OlusturulmaTarihi <= sonra);
    }

    [Fact]
    public void MusteriKonfigurasyonu_SoftDelete_AlanlariMevcutOlmali()
    {
        var k = new MusteriKonfigurasyonu { SilindiMi = true, SilinmeTarihi = DateTime.UtcNow };

        Assert.True(k.SilindiMi);
        Assert.NotNull(k.SilinmeTarihi);
    }

    [Fact]
    public void MusteriKonfigurasyonu_UrunId_ZorunluOlmali()
    {
        var k = new MusteriKonfigurasyonu();
        Assert.Equal(0, k.UrunId);

        k.UrunId = 1;
        Assert.Equal(1, k.UrunId);
    }

    [Fact]
    public void MusteriKonfigurasyonu_Not_NullableOlmali()
    {
        var k = new MusteriKonfigurasyonu { Not = "Test konfigurasyon" };

        Assert.Equal("Test konfigurasyon", k.Not);
        Assert.NotNull(k.Not);
    }

    // ═══════════════════════════════════════════════════════════════
    // T.4 — UcBoyutModelKontrolcu Testleri
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void UrunUcBoyutModeli_VarsayilanDegerler_DogruOlmali()
    {
        var m = new UrunUcBoyutModeli();

        Assert.Equal(0, m.UrunId);
        Assert.False(m.SilindiMi);
        Assert.False(m.VarsayilanMi);
    }

    [Fact]
    public void UrunUcBoyutModeli_OlusturulmaTarihi_UtcNowOlmali()
    {
        var once = DateTime.UtcNow.AddSeconds(-1);
        var m = new UrunUcBoyutModeli();
        var sonra = DateTime.UtcNow.AddSeconds(1);

        Assert.True(m.OlusturulmaTarihi >= once && m.OlusturulmaTarihi <= sonra);
    }

    [Fact]
    public void UrunUcBoyutModeli_SoftDelete_AlanlariMevcutOlmali()
    {
        var m = new UrunUcBoyutModeli { SilindiMi = true, SilinmeTarihi = DateTime.UtcNow };

        Assert.True(m.SilindiMi);
        Assert.NotNull(m.SilinmeTarihi);
    }

    [Fact]
    public void UrunUcBoyutModeli_ModelYolu_DogruFormattaOlmali()
    {
        var m = new UrunUcBoyutModeli { ModelYolu = "/medya/ucboyut/test.glb", ModelAdi = "Test" };

        Assert.Equal("/medya/ucboyut/test.glb", m.ModelYolu);
        Assert.Equal("Test", m.ModelAdi);
    }

    [Fact]
    public void UrunUcBoyutModeli_DosyaBoyutuByte_DogruSaklanmali()
    {
        var m = new UrunUcBoyutModeli { DosyaBoyutuByte = 1024000 };

        Assert.Equal(1024000, m.DosyaBoyutuByte);
    }

    // ═══════════════════════════════════════════════════════════════
    // T.5 — Seed verisi testleri
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void UrunAilesi_VarsayilanDegerler_DogruOlmali()
    {
        var aile = new UrunAilesi { Ad = "Premium Seri", Slug = "premium" };

        Assert.Equal("Premium Seri", aile.Ad);
        Assert.Equal("premium", aile.Slug);
        Assert.True(aile.AktifMi);
        Assert.False(aile.SilindiMi);
    }

    [Fact]
    public void UrunKategori_VarsayilanDegerler_DogruOlmali()
    {
        var k = new UrunKategori { Ad = "Ic Kapi", Slug = "ic-kapi" };

        Assert.Equal("Ic Kapi", k.Ad);
        Assert.Equal("ic-kapi", k.Slug);
        Assert.True(k.AktifMi);
    }

    [Fact]
    public void RalRengi_SeedDegerleri_DogruFormattaOlmali()
    {
        var renkler = new List<RalRengi>
        {
            new() { Kod = "RAL 9005", Ad = "Derin Siyah", KatalogId = 1 },
            new() { Kod = "RAL 9010", Ad = "Saf Beyaz", KatalogId = 1 },
            new() { Kod = "RAL 7016", Ad = "Antrasit Gri", KatalogId = 1 },
            new() { Kod = "RAL 8017", Ad = "Cikolata Kahve", KatalogId = 1 },
            new() { Kod = "RAL 3003", Ad = "Yakut Kirmizi", KatalogId = 1 }
        };

        Assert.Equal(5, renkler.Count);
        Assert.All(renkler, r => Assert.NotEmpty(r.Kod));
        Assert.All(renkler, r => Assert.NotEmpty(r.Ad));
        Assert.All(renkler, r => Assert.True(r.AktifMi));
    }

    [Fact]
    public void Malzeme_SeedDegerleri_DogruFormattaOlmali()
    {
        var malzemeler = new List<Malzeme>
        {
            new() { Ad = "Masif Ahsap", Tip = "ahsap" },
            new() { Ad = "MDF Lam", Tip = "mdf" },
            new() { Ad = "PVC Membran", Tip = "pvc" },
            new() { Ad = "Lake Boyalı", Tip = "lake" },
            new() { Ad = "Aluminyum", Tip = "metal" }
        };

        Assert.Equal(5, malzemeler.Count);
        Assert.All(malzemeler, m => Assert.NotEmpty(m.Ad));
        Assert.All(malzemeler, m => Assert.NotEmpty(m.Tip));
        Assert.All(malzemeler, m => Assert.True(m.AktifMi));
    }

    [Fact]
    public void KaplamaSecenegi_SeedDegerleri_DogruFormattaOlmali()
    {
        var kaplamalar = new List<KaplamaSecenegi>
        {
            new() { Ad = "Ceviz", MalzemeId = 1 },
            new() { Ad = "Mese", MalzemeId = 1 },
            new() { Ad = "Beyaz Mat", MalzemeId = 2 },
            new() { Ad = "Antrasit Parlak", MalzemeId = 4 },
            new() { Ad = "Altin Fircali", MalzemeId = 5 }
        };

        Assert.Equal(5, kaplamalar.Count);
        Assert.All(kaplamalar, k => Assert.NotEmpty(k.Ad));
        Assert.All(kaplamalar, k => Assert.NotNull(k.MalzemeId));
        Assert.All(kaplamalar, k => Assert.True(k.AktifMi));
    }
}
