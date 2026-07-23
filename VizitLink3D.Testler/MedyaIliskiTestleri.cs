using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Medya;

namespace VizitLink3D.Testler;

/// <summary>
/// Medya havuzu iliski ve kenar durum testleri.
/// </summary>
public class MedyaIliskiTestleri
{
    [Fact]
    public void Medya_KlasoreBagli_KlasorIdDogruAtanmali()
    {
        var m = new Medya { Ad = "test.jpg", KlasorId = 3 };
        Assert.Equal(3, m.KlasorId);
    }

    [Fact]
    public void MedyaKlasoru_AltKlasorEkleme_DogruCalismali()
    {
        var kok = new MedyaKlasoru { Ad = "Kök" };
        kok.AltKlasorler.Add(new MedyaKlasoru { Ad = "Alt1" });
        kok.AltKlasorler.Add(new MedyaKlasoru { Ad = "Alt2" });

        Assert.Equal(2, kok.AltKlasorler.Count);
        Assert.Equal("Alt1", kok.AltKlasorler[0].Ad);
    }

    [Fact]
    public void Medya_EtiketlerJson_AktarilabilirOlmali()
    {
        var m = new Medya { Ad = "test.jpg", EtiketlerJson = "[\"kapı\",\"membran\",\"2024\"]" };
        Assert.Contains("membran", m.EtiketlerJson);
    }

    [Fact]
    public void Medya_FarkliKaynaklar_DuzgunAtanabilmeli()
    {
        var y = new Medya { Ad = "yt", Kaynak = MedyaKaynagi.Youtube, KaynakUrl = "https://youtube.com/embed/xyz" };
        var v = new Medya { Ad = "vm", Kaynak = MedyaKaynagi.Vimeo };
        var u = new Medya { Ad = "url", Kaynak = MedyaKaynagi.Url };

        Assert.Equal(MedyaKaynagi.Youtube, y.Kaynak);
        Assert.Equal(MedyaKaynagi.Vimeo, v.Kaynak);
        Assert.Equal(MedyaKaynagi.Url, u.Kaynak);
    }

    [Fact]
    public void Cevap_BosHataListesi_BasarisizOlmali()
    {
        var cevap = Cevap<string>.Hata("Hata");
        Assert.False(cevap.BasariliMi);
        Assert.Empty(cevap.Hatalar);
        Assert.Null(cevap.Veri);
    }

    [Fact]
    public void Kullanici_KilitlenmeSayisi_SifirdanBaslamali()
    {
        var k = new Kullanici { KullaniciAdi = "test", Eposta = "t@t.com" };
        Assert.Equal(0, k.BasarisizGirisDenemesi);
        Assert.False(k.KilitlendiMi);
    }

    [Fact]
    public void Kullanici_IkiAdimDogrulama_VarsayilanKapaliOlmali()
    {
        var k = new Kullanici { KullaniciAdi = "test", Eposta = "t@t.com" };
        Assert.False(k.IkiAdimDogrulamaAktif);
        Assert.False(k.EmailDogrulandiMi);
    }

    [Fact]
    public void Firma_SosyalMedya_DogruAtanabilmeli()
    {
        var f = new Firma
        {
            Ad = "Test",
            Slug = "test",
            Instagram = "https://instagram.com/test",
            Facebook = "https://facebook.com/test",
            Twitter = "https://twitter.com/test"
        };

        Assert.NotEmpty(f.Instagram);
        Assert.NotEmpty(f.Facebook);
        Assert.NotEmpty(f.Twitter);
    }

    [Fact]
    public void Firma_TasarimRenkleri_DogruAtanabilmeli()
    {
        var f = new Firma
        {
            Ad = "Test",
            Slug = "test",
            TasarimRengi1 = "#1A1A27",
            TasarimRengi2 = "#C8952A",
            TasarimRengi3 = "#F5F2ED"
        };

        Assert.NotEmpty(f.TasarimRengi1);
        Assert.NotEmpty(f.TasarimRengi2);
    }

    [Fact]
    public void Katalog_IndirilmeSayisi_SifirdanBaslamali()
    {
        var k = new Katalog { Baslik = "Test Katalog" };
        Assert.Equal(0, k.IndirilmeSayisi);
    }

    [Fact]
    public void MusteriYorumu_Onaylanmamis_VarsayilanFalseOlmali()
    {
        var y = new MusteriYorumu { MusteriAdi = "Test", Yorum = "Test", Puan = 4 };
        Assert.False(y.Onaylandi);
        Assert.False(y.OneCikan);
    }
}
