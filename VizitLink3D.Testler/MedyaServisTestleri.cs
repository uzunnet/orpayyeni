using VizitLink3D.Api.Moduller.Medya.Servisler;
using System.Text;

namespace VizitLink3D.Testler;

public class MedyaServisTestleri
{
    private readonly ResimIslemcisi _resimIslemcisi = new();

    [Fact]
    public void HashHesapla_AyniIcerik_AyniHashUretmeli()
    {
        var icerik1 = new MemoryStream(Encoding.UTF8.GetBytes("test icerik"));
        var hash1 = _resimIslemcisi.HashHesapla(icerik1);

        var icerik2 = new MemoryStream(Encoding.UTF8.GetBytes("test icerik"));
        var hash2 = _resimIslemcisi.HashHesapla(icerik2);

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void HashHesapla_FarkliIcerik_FarkliHashUretmeli()
    {
        var icerik1 = new MemoryStream(Encoding.UTF8.GetBytes("icerik A"));
        var hash1 = _resimIslemcisi.HashHesapla(icerik1);

        var icerik2 = new MemoryStream(Encoding.UTF8.GetBytes("icerik B"));
        var hash2 = _resimIslemcisi.HashHesapla(icerik2);

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void HashHesapla_HexFormatindaOlmali()
    {
        var icerik = new MemoryStream(Encoding.UTF8.GetBytes("test"));
        var hash = _resimIslemcisi.HashHesapla(icerik);

        Assert.Equal(64, hash.Length);
        Assert.Matches("^[0-9a-f]+$", hash);
    }

    [Fact]
    public void HashHesapla_StreamPozisyonu_Sifirlanmali()
    {
        var icerik = new MemoryStream(Encoding.UTF8.GetBytes("test icerik"));
        _resimIslemcisi.HashHesapla(icerik);

        Assert.Equal(0, icerik.Position);
    }

    [Fact]
    public void HashHesapla_BosIcerik_DuzgunHashUretmeli()
    {
        var icerik = new MemoryStream(Array.Empty<byte>());
        var hash = _resimIslemcisi.HashHesapla(icerik);
        Assert.Equal(64, hash.Length);
    }
}
