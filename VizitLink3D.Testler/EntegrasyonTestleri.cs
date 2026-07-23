using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Medya;
using System.Text.Json;

namespace VizitLink3D.Testler;

/// <summary>
/// Entegrasyon ve kapsamli son testler.
/// </summary>
public class EntegrasyonTestleri
{
    [Fact]
    public void Medya_JsonSerialize_JsonIgnoreCalisiyorMu()
    {
        var m = new Ortak.Modeller.Medya.Medya
        {
            Ad = "test.jpg",
            KlasorId = 5,
            YukleyenKullaniciId = "user123"
        };
        var json = JsonSerializer.Serialize(m);
        Assert.Contains("test.jpg", json);
        Assert.Contains("5", json);
    }

    [Fact]
    public void AISaglayicisi_ApiKey_JsonIgnoreCalisiyorMu()
    {
        var s = new VizitLink3D.Ortak.Modeller.AI.AISaglayicisi
        {
            Ad = "OpenAI",
            ApiKeyEncrypted = "super-gizli-key-12345"
        };
        var json = JsonSerializer.Serialize(s);
        Assert.DoesNotContain("super-gizli", json);
        Assert.Contains("OpenAI", json);
    }

    [Fact]
    public void Kullanici_SifreHash_JsonIgnoreCalisiyorMu()
    {
        var k = new Kullanici
        {
            KullaniciAdi = "admin",
            Eposta = "admin@test.com",
            SifreHash = "bcrypt-hash-degeri"
        };
        var json = JsonSerializer.Serialize(k);
        Assert.DoesNotContain("bcrypt-hash", json);
    }

    [Fact]
    public void IletisimMesaji_IPAdresi_JsonIgnoreCalisiyorMu()
    {
        var m = new IletisimMesaji
        {
            AdSoyad = "Test",
            Eposta = "t@t.com",
            Mesaj = "Test",
            IPAdresi = "192.168.1.1"
        };
        var json = JsonSerializer.Serialize(m);
        Assert.DoesNotContain("192.168", json);
    }

    [Fact]
    public void Firma_JsonIgnore_FirmaReferanslariGizlenmeli()
    {
        var l = new Lisans
        {
            BirincilDomain = "test.com",
            LisansAnahtari = "hmac-key",
            Firma = new Firma { Ad = "Test", Slug = "t" }
        };
        var json = JsonSerializer.Serialize(l);
        Assert.DoesNotContain("\"Firma\"", json);
    }

    [Fact]
    public void KapakModeli_GaleriResimleri_BaslangictaNullOlmali()
    {
        var m = new VizitLink3D.Api.Modeller.KapakModeli { ModelAdi = "Test", Slug = "t" };
        Assert.Null(m.GaleriResimleri);
    }

    [Fact]
    public void HaberYazisi_Resimler_BosListeyleBaslamali()
    {
        var b = new HaberYazisi { Baslik = "T", Slug = "t", Icerik = "i" };
        Assert.Empty(b.Resimler);
    }

    [Fact]
    public void Proje_Resimler_BaslangictaNullOlmali()
    {
        var p = new Proje { Baslik = "T", Slug = "t" };
        Assert.Null(p.Resimler);
    }

    [Fact]
    public void MenuOgesi_AltMenuler_BosListeyleBaslamali()
    {
        var m = new MenuOgesi { Baslik = "Test" };
        Assert.Empty(m.AltMenuler);
    }

    [Fact]
    public void MedyaKlasoru_AltKlasorler_BosListeyleBaslamali()
    {
        var k = new MedyaKlasoru { Ad = "Test" };
        Assert.Empty(k.AltKlasorler);
        Assert.Empty(k.Medyalar);
    }
}
