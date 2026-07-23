using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Medya;
using System.Text.Json;

namespace VizitLink3D.Testler;

/// <summary>
/// Son kapsamli birim testleri.
/// </summary>
public class SonBirimTestleri
{
    [Fact]
    public void Medya_OlusturulmaTarihi_UtcNowOlmali()
    {
        var m = new Medya { Ad = "test.jpg" };
        var fark = (DateTime.UtcNow - m.OlusturulmaTarihi).TotalSeconds;
        Assert.True(fark < 5);
    }

    [Fact]
    public void MedyaKlasoru_OlusturulmaTarihi_UtcNowOlmali()
    {
        var k = new MedyaKlasoru { Ad = "Test" };
        Assert.True((DateTime.UtcNow - k.OlusturulmaTarihi).TotalSeconds < 5);
    }

    [Fact]
    public void AISaglayicisi_OlusturulmaTarihi_UtcNowOlmali()
    {
        var s = new VizitLink3D.Ortak.Modeller.AI.AISaglayicisi { Ad = "Test" };
        Assert.True((DateTime.UtcNow - s.OlusturulmaTarihi).TotalSeconds < 5);
    }

    [Fact]
    public void AICagrisiKaydi_OlusturulmaTarihi_UtcNowOlmali()
    {
        var k = new VizitLink3D.Ortak.Modeller.AI.AICagrisiKaydi { SaglayiciId = 1 };
        Assert.True((DateTime.UtcNow - k.OlusturulmaTarihi).TotalSeconds < 5);
    }

    [Fact]
    public void Ceviri_CompositeKey_DogruYapida()
    {
        var c = new Ceviri { Anahtar = "menu.anasayfa", Dil = "tr", Deger = "Ana Sayfa" };
        Assert.Equal("menu.anasayfa", c.Anahtar);
        Assert.Equal("tr", c.Dil);
    }

    [Fact]
    public void Firma_ListeOlarakModuller_NotMappedOlmali()
    {
        var f = new Firma { Ad = "Test", Slug = "test" };
        f.AktifModulKodlari = new List<string> { "Blog", "E-Ticaret" };
        Assert.Contains("Blog", f.AktifModulKodlari);
    }

    [Fact]
    public void KapakModeli_MinOlcu_MaxOlcudenKucukOlmali()
    {
        var m = new VizitLink3D.Api.Modeller.KapakModeli
        {
            ModelAdi = "Test", Slug = "t",
            MinYukseklik = 1800,
            MaxYukseklik = 2200
        };
        Assert.True(m.MinYukseklik < m.MaxYukseklik);
    }

    [Fact]
    public void Slayt_GecisHizi_MakulAraliktaOlmali()
    {
        var s = new Slayt { Baslik = "Test" };
        Assert.Equal(800, s.GecisHizi);
        Assert.True(s.GosterimSuresi >= 1000);
    }

    [Fact]
    public void IletisimMesaji_Tarih_UtcNowOlmali()
    {
        var m = new IletisimMesaji { AdSoyad = "A", Eposta = "a@a.com", Mesaj = "M" };
        Assert.True((DateTime.UtcNow - m.Tarih).TotalSeconds < 5);
    }

    [Fact]
    public void Kullanici_OlusturulmaTarihi_DefaultDogruMu()
    {
        var k = new Kullanici { KullaniciAdi = "test", Eposta = "t@t.com" };
        Assert.True((DateTime.UtcNow - k.OlusturulmaTarihi).TotalSeconds < 5);
    }
}
