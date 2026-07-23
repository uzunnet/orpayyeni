using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.Testler;

/// <summary>
/// Son hiz testleri.
/// </summary>
public class SonHizTestleri
{
    [Fact] public void Cevap_Basarili_NullVeri() { var c=Cevap<object>.Basarili(null!); Assert.True(c.BasariliMi); Assert.Null(c.Veri); }
    [Fact] public void Slayt_AnimasyonTipi_Fade() { Assert.Equal("fade", new Slayt{Baslik="T"}.AnimasyonTipi); }
    [Fact] public void Slayt_GecisHizi_800() { Assert.Equal(800, new Slayt{Baslik="T"}.GecisHizi); }
    [Fact] public void Firma_MenuYatay_30() { Assert.Equal(30, new Firma{Ad="T",Slug="t"}.MenuYatayAralik); }
    [Fact] public void Firma_MenuDikey_20() { Assert.Equal(20, new Firma{Ad="T",Slug="t"}.MenuDikeyPadding); }
    [Fact] public void MusteriYorumu_Puan1() { Assert.True(new MusteriYorumu{MusteriAdi="A",Yorum="T",Puan=1}.Puan>=1); }
    [Fact] public void Kullanici_Rol_Kullanici0() { Assert.Equal(0,(int)Rol.Kullanici); }
    [Fact] public void KapakModeliDto_Kategori_Ozel() { Assert.Equal("Ozel",new VizitLink3D.Ortak.Modeller.Sektorler.KapakModeliDto().Kategori); }
    [Fact] public void KapakModeliDto_SiraNo_100() { Assert.Equal(100,new VizitLink3D.Ortak.Modeller.Sektorler.KapakModeliDto().SiraNo); }
    [Fact] public void HaberYazisi_AktifMi_True() { Assert.True(new HaberYazisi{Baslik="T",Slug="t",Icerik="i"}.AktifMi); }
}
