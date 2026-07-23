using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.Testler;

/// <summary>
/// Kapsamli veri ve model dogrulama testleri.
/// </summary>
public class KapsamliModelTestleri
{
    [Fact]
    public void IletisimMesaji_BosOncelik_NormalOlmali()
    {
        var m = new IletisimMesaji { AdSoyad = "A", Eposta = "a@a.com", Mesaj = "M" };
        Assert.Equal("Normal", m.OncelikSeviyesi);
    }

    [Fact]
    public void HaberYazisi_VarsayilanOkunma_SifirOlmali()
    {
        var b = new HaberYazisi { Baslik = "T", Slug = "t", Icerik = "i" };
        Assert.Equal(0, b.OkunmaSayisi);
    }

    [Fact]
    public void KapakModeli_Fiyat_VarsayilanNullOlmali()
    {
        var m = new VizitLink3D.Api.Modeller.KapakModeli { ModelAdi = "T", Slug = "t" };
        Assert.Null(m.Fiyat);
    }

    [Fact]
    public void KapakModeli_VarsayilanModelTuru_KapakOlmali()
    {
        var m = new VizitLink3D.Api.Modeller.KapakModeli { ModelAdi = "T", Slug = "t" };
        Assert.Equal("Kapak", m.ModelTuru);
    }

    [Fact]
    public void MobilyaUrunu_OneCikanMi_VarsayilanFalseOlmali()
    {
        var m = new MobilyaUrunu { Ad = "T", Slug = "t" };
        Assert.False(m.OneCikanMi);
    }

    [Fact]
    public void Proje_OneCikanMi_VarsayilanFalseOlmali()
    {
        var p = new Proje { Baslik = "T", Slug = "t" };
        Assert.False(p.OneCikanMi);
    }

    [Fact]
    public void Slayt_MetinHizalama_VarsayilanSolOlmali()
    {
        var s = new Slayt { Baslik = "T" };
        Assert.Equal("sol", s.MetinHizalama);
    }

    [Fact]
    public void Referans_TipVarsayilan_MusteriOlmali()
    {
        var r = new Referans { Ad = "T" };
        Assert.Equal("Musteri", r.Tip);
    }

    [Fact]
    public void EkipUyesi_VarsayilanSiraNo_SifirOlmali()
    {
        var e = new EkipUyesi { AdSoyad = "T" };
        Assert.Equal(0, e.SiraNo);
    }

    [Fact]
    public void Sube_VarsayilanSiraNo_SifirOlmali()
    {
        var s = new Sube { Ad = "T" };
        Assert.Equal(0, s.SiraNo);
    }
}
