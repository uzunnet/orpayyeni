using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.AI;
using VizitLink3D.Ortak.Modeller.Medya;
using System.Text.Json;

namespace VizitLink3D.Testler;

/// <summary>
/// JSON serialization ve format testleri.
/// </summary>
public class FormatTestleri
{
    [Fact]
    public void Cevap_Serialize_BasariliFormattaOlmali()
    {
        var c = Cevap<int>.Basarili(42, "Tamam");
        var json = JsonSerializer.Serialize(c);
        Assert.Contains("BasariliMi", json);
        Assert.Contains("42", json);
    }

    [Fact]
    public void Cevap_HataListe_SerializeDogruOlmali()
    {
        var c = Cevap<string>.Hata("Hata", new List<string> { "E1", "E2" });
        var json = JsonSerializer.Serialize(c);
        Assert.Contains("Hatalar", json);
    }

    [Fact]
    public void MedyaTipi_Serialize_DogruSayiOlmali()
    {
        Assert.Equal(0, (int)MedyaTipi.Resim);
        Assert.Equal(1, (int)MedyaTipi.Video);
        Assert.Equal(2, (int)MedyaTipi.Pdf);
        Assert.Equal(3, (int)MedyaTipi.Glb);
        Assert.Equal(4, (int)MedyaTipi.Ses);
        Assert.Equal(5, (int)MedyaTipi.Diger);
    }

    [Fact]
    public void AISaglayiciTipi_Serialize_DogruSayi()
    {
        Assert.Equal(0, (int)AISaglayiciTipi.OpenAI);
        Assert.Equal(1, (int)AISaglayiciTipi.Anthropic);
        Assert.Equal(2, (int)AISaglayiciTipi.Gemini);
    }

    [Fact]
    public void AICagriDurumu_Serialize_DogruSayi()
    {
        Assert.Equal(0, (int)AICagriDurumu.Basarili);
        Assert.Equal(1, (int)AICagriDurumu.Hata);
        Assert.Equal(2, (int)AICagriDurumu.LimitAsildi);
    }

    [Fact]
    public void MedyaKaynagi_Serialize_DogruSayi()
    {
        Assert.Equal(0, (int)MedyaKaynagi.Yerel);
        Assert.Equal(1, (int)MedyaKaynagi.Youtube);
        Assert.Equal(5, (int)MedyaKaynagi.StokFotograf);
    }

    [Fact]
    public void Rol_Serialize_DogruSayi()
    {
        Assert.Equal(0, (int)Rol.Kullanici);
        Assert.Equal(1, (int)Rol.Editor);
        Assert.Equal(2, (int)Rol.Admin);
        Assert.Equal(3, (int)Rol.SuperAdmin);
    }

    [Fact]
    public void KapakModeli_SertifikalarJson_DogruFormatta()
    {
        var m = new VizitLink3D.Api.Modeller.KapakModeli
        {
            ModelAdi = "T", Slug = "t",
            SertifikalarJson = "[\"ISO 9001\",\"TSE\"]"
        };
        Assert.Contains("ISO", m.SertifikalarJson);
    }

    [Fact]
    public void KapakModeli_RenkSecenekleriJson_ArrayFormatta()
    {
        var m = new VizitLink3D.Api.Modeller.KapakModeli
        {
            ModelAdi = "T", Slug = "t",
            RenkSecenekleriJson = "[{\"Ad\":\"Kirmizi\",\"HexKod\":\"#FF0000\"}]"
        };
        Assert.Contains("Kirmizi", m.RenkSecenekleriJson);
    }

    [Fact]
    public void KapakModeli_Nitelikler_DeserializeDogruMu()
    {
        var m = new VizitLink3D.Api.Modeller.KapakModeli
        {
            ModelAdi = "T", Slug = "t",
            NiteliklerJson = "[\"Su Geçirmez\",\"Yangına Dayanıklı\"]"
        };
        Assert.Contains("Su Geçirmez", m.NiteliklerJson);
    }
}
