using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.Testler;

public class CevapTests
{
    [Fact]
    public void Basarili_Yanit_Olusturulabilmeli()
    {
        var cevap = Cevap<string>.Basarili("test");
        Assert.True(cevap.BasariliMi);
        Assert.Equal("test", cevap.Veri);
        Assert.False(string.IsNullOrEmpty(cevap.Mesaj));
    }

    [Fact]
    public void Hata_Yaniti_Olusturulabilmeli()
    {
        var cevap = Cevap<string>.Hata("hata mesajı");
        Assert.False(cevap.BasariliMi);
        Assert.Null(cevap.Veri);
        Assert.Equal("hata mesajı", cevap.Mesaj);
    }

    [Fact]
    public void Hata_Listesi_Ile_Yanit_Olusturulabilmeli()
    {
        var hatalar = new List<string> { "Hata 1", "Hata 2" };
        var cevap = Cevap<int>.Hata("geçersiz", hatalar);
        Assert.Equal(2, cevap.Hatalar.Count);
    }

    [Fact]
    public void Basarili_Yanit_Varsayilan_Mesaj_Icerir()
    {
        var cevap = Cevap<bool>.Basarili(true);
        Assert.Equal("Islem basarili.", cevap.Mesaj);
    }

    [Fact]
    public void Generic_Tip_Dogru_Korunur()
    {
        var intCevap = Cevap<int>.Basarili(42);
        Assert.IsType<int>(intCevap.Veri);

        var stringCevap = Cevap<string>.Basarili("merhaba");
        Assert.IsType<string>(stringCevap.Veri);
    }
}
