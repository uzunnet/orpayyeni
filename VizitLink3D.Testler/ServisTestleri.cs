using VizitLink3D.Ortak.Modeller;
using Microsoft.Extensions.Configuration;

namespace VizitLink3D.Testler;

/// <summary>
/// Servis ve is mantigi testleri.
/// </summary>
public class ServisTestleri
{
    [Fact]
    public void LisansUretici_AyniVeri_AyniAnahtarUretmeli()
    {
        var servis = new VizitLink3D.Api.Servisler.Kimlik.LisansUreticiServisi(
            new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["LisansAyarlari:GizliAnahtar"] = "TEST_KEY_32_CHAR_MINIMUM_LENGTH" })
                .Build());

        var anahtar1 = servis.LisansAnahtariUret("test.com", DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddYears(1), "Yillik");
        var anahtar2 = servis.LisansAnahtariUret("test.com", DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddYears(1), "Yillik");

        Assert.Equal(anahtar1, anahtar2);
    }

    [Fact]
    public void LisansUretici_FarkliDomain_FarkliAnahtarUretmeli()
    {
        var servis = new VizitLink3D.Api.Servisler.Kimlik.LisansUreticiServisi(
            new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["LisansAyarlari:GizliAnahtar"] = "TEST_KEY_32_CHAR_MINIMUM_LENGTH" })
                .Build());

        var a1 = servis.LisansAnahtariUret("3dvizitlink.com.tr", DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddYears(1), "Yillik");
        var a2 = servis.LisansAnahtariUret("baska.com.tr", DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddYears(1), "Yillik");

        Assert.NotEqual(a1, a2);
    }

    [Fact]
    public void LisansUretici_DogruVeri_Dogrulamali()
    {
        var servis = new VizitLink3D.Api.Servisler.Kimlik.LisansUreticiServisi(
            new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["LisansAyarlari:GizliAnahtar"] = "TEST_KEY_32_CHAR_MINIMUM_LENGTH" })
                .Build());

        var baslangic = DateTime.UtcNow.Date;
        var bitis = baslangic.AddYears(1);
        var anahtar = servis.LisansAnahtariUret("3dvizitlink.com.tr", baslangic, bitis, "Yillik");

        Assert.True(servis.LisansDogrula("3dvizitlink.com.tr", baslangic, bitis, "Yillik", anahtar));
    }

    [Fact]
    public void LisansUretici_GecersizAnahtar_Dogrulamamali()
    {
        var servis = new VizitLink3D.Api.Servisler.Kimlik.LisansUreticiServisi(
            new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["LisansAyarlari:GizliAnahtar"] = "TEST_KEY_32_CHAR_MINIMUM_LENGTH" })
                .Build());

        Assert.False(servis.LisansDogrula("test.com", DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddYears(1), "Yillik", "GEÇERSİZ_ANAHTAR"));
    }

    [Fact]
    public void LisansUretici_SuresiGecmis_GecerliOlmamali()
    {
        var servis = new VizitLink3D.Api.Servisler.Kimlik.LisansUreticiServisi(
            new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["LisansAyarlari:GizliAnahtar"] = "TEST_KEY_32_CHAR_MINIMUM_LENGTH" })
                .Build());

        Assert.False(servis.GecerliMi("test.com", null, DateTime.UtcNow.AddDays(-1)));
    }

    [Fact]
    public void JwtServisi_VarsayilanDegerler_DogruOlmali()
    {
        var yapilandirma = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Anahtar"] = "test_anahtari_min_32_karakter_uzunlugunda_olmalidir",
                ["Jwt:Yayinci"] = "TestAPI",
                ["Jwt:Izleyici"] = "TestUI",
                ["Jwt:GecerlilikSuresiDakika"] = "120"
            })
            .Build();

        var servis = new VizitLink3D.Api.Servisler.Kimlik.JwtServisi(yapilandirma);

        Assert.Equal("test_anahtari_min_32_karakter_uzunlugunda_olmalidir", servis.Anahtar);
        Assert.Equal("TestAPI", servis.Yayinci);
        Assert.Equal("TestUI", servis.Izleyici);
        Assert.Equal(120, servis.GecerlilikSuresiDakika);
    }

    [Fact]
    public void Cevap_BirdenCokHata_DogruListelenmeli()
    {
        var cevap = Cevap<int>.Hata("Coklu hata", new List<string> { "Hata1", "Hata2", "Hata3" });
        Assert.Equal(3, cevap.Hatalar.Count);
        Assert.Contains("Hata2", cevap.Hatalar);
        Assert.Equal(0, cevap.Veri);
    }

    [Fact]
    public void Rol_Enum_Degerleri_DogruMu()
    {
        Assert.Equal(0, (int)Rol.Kullanici);
        Assert.Equal(1, (int)Rol.Editor);
        Assert.Equal(2, (int)Rol.Admin);
        Assert.Equal(3, (int)Rol.SuperAdmin);
    }

    [Fact]
    public void KapakModeli_YeniMiVarsayilan_FalseOlmali()
    {
        var m = new VizitLink3D.Api.Modeller.KapakModeli { ModelAdi = "Test", Slug = "test" };
        Assert.False(m.YeniMi);
        Assert.False(m.OneCikanMi);
        Assert.False(m.SilindiMi);
    }

    [Fact]
    public void KapakModeli_RenkSecenekleri_NotMapped_DeserializeEdilebilmeli()
    {
        var m = new VizitLink3D.Api.Modeller.KapakModeli
        {
            RenkSecenekleriJson = "[{\"Ad\":\"Beyaz\",\"HexKod\":\"#FFFFFF\"}]"
        };
        Assert.NotEmpty(m.RenkSecenekleriJson);
    }
}
