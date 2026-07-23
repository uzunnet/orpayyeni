using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Medya;

namespace VizitLink3D.Testler;

/// <summary>
/// Enum, FK ve son dogrulama testleri.
/// </summary>
public class EnumFkTestleri
{
    [Fact]
    public void MedyaTipi_TumDegerler_Gecerli()
    {
        var tipler = Enum.GetValues<MedyaTipi>();
        Assert.Equal(6, tipler.Length);
    }

    [Fact]
    public void MedyaKaynagi_TumDegerler_Gecerli()
    {
        var kaynaklar = Enum.GetValues<MedyaKaynagi>();
        Assert.Equal(6, kaynaklar.Length);
    }

    [Fact]
    public void AISaglayiciTipi_TumDegerler_Gecerli()
    {
        var tipler = Enum.GetValues<VizitLink3D.Ortak.Modeller.AI.AISaglayiciTipi>();
        Assert.Equal(7, tipler.Length);
    }

    [Fact]
    public void AICagriDurumu_TumDegerler_Gecerli()
    {
        var durumlar = Enum.GetValues<VizitLink3D.Ortak.Modeller.AI.AICagriDurumu>();
        Assert.Equal(3, durumlar.Length);
    }

    [Fact]
    public void Rol_TumDegerler_Gecerli()
    {
        var roller = Enum.GetValues<Rol>();
        Assert.Equal(4, roller.Length);
    }

    [Fact]
    public void KapakModeli_KategoriId_BosOlmali()
    {
        var m = new VizitLink3D.Api.Modeller.KapakModeli { ModelAdi = "T", Slug = "t" };
        Assert.Null(m.KategoriId);
    }

    [Fact]
    public void MobilyaUrunu_KategoriId_VarsayilanSifir()
    {
        var u = new MobilyaUrunu { Ad = "T", Slug = "t" };
        Assert.Equal(0, u.MobilyaKategorisiId);
    }

    [Fact]
    public void Proje_KategoriId_VarsayilanSifir()
    {
        var p = new Proje { Baslik = "T", Slug = "t" };
        Assert.Equal(0, p.KategoriId);
    }

    [Fact]
    public void MusteriYorumu_ProjeId_VarsayilanNull()
    {
        var y = new MusteriYorumu { MusteriAdi = "A", Yorum = "Test" };
        Assert.Null(y.ProjeId);
    }

    [Fact]
    public void Medya_KlasorId_VarsayilanNull()
    {
        var m = new Medya { Ad = "test.jpg" };
        Assert.Null(m.KlasorId);
    }
}
