using VizitLink3D.Ortak.Yardimcilar;

namespace VizitLink3D.Testler;

public class KatalogYoluTestleri
{
    [Fact]
    public void BosYol_Reddedilir()
    {
        Assert.Null(KatalogYolu.GuvenliGenelKatalogYolu(null));
    }

    [Fact]
    public void GecerliKatalogYolu_KabulEdilir()
    {
        const string yol = "medya/orpay-katalog/GOLD-2026-KATALOG.pdf";
        Assert.Equal(yol, KatalogYolu.GuvenliGenelKatalogYolu(yol));
    }

    [Fact]
    public void BasindaEgitikCizgiOlanYol_NormalizeEdilir()
    {
        Assert.Equal(
            "medya/orpay-katalog/katalog.pdf",
            KatalogYolu.GuvenliGenelKatalogYolu("/medya/orpay-katalog/katalog.pdf"));
    }

    [Fact]
    public void UstKlasoreCikmaDenemesi_Reddedilir()
    {
        Assert.Null(KatalogYolu.GuvenliGenelKatalogYolu("medya/orpay-katalog/../gizli.pdf"));
    }

    [Fact]
    public void PdfOlmayanDosya_Reddedilir()
    {
        Assert.Null(KatalogYolu.GuvenliGenelKatalogYolu("medya/orpay-katalog/katalog.exe"));
    }

    [Fact]
    public void FarkliMedyaKlasoru_Reddedilir()
    {
        Assert.Null(KatalogYolu.GuvenliGenelKatalogYolu("medya/diger/katalog.pdf"));
    }
}
