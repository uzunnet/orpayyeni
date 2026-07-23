using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Medya;
using System.Text;

namespace VizitLink3D.Testler;

/// <summary>
/// Kullanim ve davranis testleri.
/// </summary>
public class KullanimTestleri
{
    [Fact]
    public void MedyaKullanim_GerekliAlanlar_DoluOlmali()
    {
        var k = new MedyaKullanim
        {
            MedyaId = 5,
            EntiteAdi = "Slayt",
            EntiteId = 12,
            AlanAdi = "ArkaplanResim"
        };
        Assert.Equal(5, k.MedyaId);
        Assert.Equal("Slayt", k.EntiteAdi);
    }

    [Fact]
    public void MedyaKlasoru_Slug_BenzersizOlmali()
    {
        var k1 = new MedyaKlasoru { Ad = "Kapılar", Slug = "kapilar" };
        var k2 = new MedyaKlasoru { Ad = "Mobilyalar", Slug = "mobilyalar" };
        Assert.NotEqual(k1.Slug, k2.Slug);
    }

    [Fact]
    public void HaberYazisi_BosEtiketler_NullOlmali()
    {
        var b = new HaberYazisi { Baslik = "T", Slug = "t", Icerik = "i" };
        Assert.Null(b.Etiketler);
    }

    [Fact]
    public void HaberYazisi_YayinTarihi_VarsayilanNullOlmali()
    {
        var b = new HaberYazisi { Baslik = "T", Slug = "t", Icerik = "i" };
        Assert.Null(b.YayinTarihi);
    }

    [Fact]
    public void Proje_Tarihi_DefaultDateTimeMinOlmali()
    {
        var p = new Proje { Baslik = "T", Slug = "t" };
        Assert.Equal(default, p.ProjeTarihi);
    }

    [Fact]
    public void MusteriYorumu_ProjeId_VarsayilanNullOlmali()
    {
        var y = new MusteriYorumu { MusteriAdi = "A", Yorum = "Test" };
        Assert.Null(y.ProjeId);
    }

    [Fact]
    public void Slayt_ButonMetni_VarsayilanNullOlmali()
    {
        var s = new Slayt { Baslik = "T" };
        Assert.Null(s.ButonMetni1);
        Assert.Null(s.ButonLink1);
    }

    [Fact]
    public void HizmetAdimi_Aciklama_VarsayilanNullOlmali()
    {
        var h = new HizmetAdimi { Baslik = "Ölçüm", AdimNo = 1 };
        Assert.Null(h.Aciklama);
    }

    [Fact]
    public void Ceviri_BolumAlani_DogruAtanmali()
    {
        var c = new Ceviri { Anahtar = "menu.iletisim", Dil = "tr", Deger = "İletişim", Bolum = "menu" };
        Assert.Equal("menu", c.Bolum);
    }

    [Fact]
    public void Dil_VarsayilanAktif_TrueOlmali()
    {
        var d = new Dil { Kod = "ar", Ad = "Arapça" };
        Assert.True(d.AktifMi);
        Assert.False(d.VarsayilanMi);
    }
}
