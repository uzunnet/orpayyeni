using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.AI;
using VizitLink3D.Ortak.Modeller.Medya;

namespace VizitLink3D.Testler;

/// <summary>
/// Hizli son testler.
/// </summary>
public class HizliTestler
{
    [Fact] public void Kullanici_RefreshToken_VarsayilanNull() { Assert.Null(new Kullanici{KullaniciAdi="t",Eposta="t@t.com"}.RefreshToken); }
    [Fact] public void Kullanici_RefreshTokenBitis_Null() { Assert.Null(new Kullanici{KullaniciAdi="t",Eposta="t@t.com"}.RefreshTokenBitisTarihi); }
    [Fact] public void Kullanici_SonGirisIP_Null() { Assert.Null(new Kullanici{KullaniciAdi="t",Eposta="t@t.com"}.SonGirisIP); }
    [Fact] public void IletisimMesaji_Tarayici_Null() { Assert.Null(new IletisimMesaji{AdSoyad="A",Eposta="a@a.com",Mesaj="M"}.Tarayici); }
    [Fact] public void IletisimMesaji_Cihaz_Null() { Assert.Null(new IletisimMesaji{AdSoyad="A",Eposta="a@a.com",Mesaj="M"}.Cihaz); }
    [Fact] public void AISaglayicisi_Guncellenme_Null() { Assert.Null(new AISaglayicisi{Ad="T"}.GuncellenmeTarihi); }
    [Fact] public void Medya_Genislik_Null() { Assert.Null(new Medya{Ad="t.jpg"}.Genislik); }
    [Fact] public void Medya_Yukseklik_Null() { Assert.Null(new Medya{Ad="t.jpg"}.Yukseklik); }
    [Fact] public void Medya_MimeTipi_Null() { Assert.Null(new Medya{Ad="t.jpg"}.MimeTipi); }
    [Fact] public void Kullanici_TelefonDogrulandi_False() { Assert.False(new Kullanici{KullaniciAdi="t",Eposta="t@t.com"}.TelefonDogrulandiMi); }
}
