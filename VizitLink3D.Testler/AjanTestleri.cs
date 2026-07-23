using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.AI;
using VizitLink3D.Ortak.Modeller.Medya;

namespace VizitLink3D.Testler;

/// <summary>
/// AGENTS.md uyumluluk testleri.
/// </summary>
public class AjanTestleri
{
    [Fact] public void Cevap_VarsayilanMesaj_BosDegil() { var c = Cevap<int>.Hata("H"); Assert.NotEmpty(c.Mesaj); }
    [Fact] public void Cevap_Basarili_VarsayilanMesaj() { var c = Cevap<int>.Basarili(1); Assert.Equal("Islem basarili.", c.Mesaj); }
    [Fact] public void Medya_SureSaniye_Nullable() { Assert.Null(new Medya{Ad="t"}.SureSaniye); }
    [Fact] public void AISaglayicisi_AktifMi_True() { Assert.True(new AISaglayicisi{Ad="T"}.AktifMi); }
    [Fact] public void AICagrisiKaydi_Durum_Basarili() { Assert.Equal(AICagriDurumu.Basarili, new AICagrisiKaydi{SaglayiciId=1}.Durum); }
    [Fact] public void KapiKategorisi_SeoBaslik_Null() { Assert.Null(new KapiKategorisi{Ad="T",Slug="t"}.SeoBaslik); }
    [Fact] public void MobilyaUrunu_SeoBaslik_Null() { Assert.Null(new MobilyaUrunu{Ad="T",Slug="t"}.SeoBaslik); }
    [Fact] public void Proje_KisaAciklama_Null() { Assert.Null(new Proje{Baslik="T",Slug="t"}.KisaAciklama); }
    [Fact] public void HaberYazisi_SeoBaslik_Null() { Assert.Null(new HaberYazisi{Baslik="T",Slug="t",Icerik="i"}.SeoBaslik); }
    [Fact] public void Kullanici_WebAuthn_Null() { Assert.Null(new Kullanici{KullaniciAdi="t",Eposta="t@t.com"}.WebAuthnPublicKey); }
}
