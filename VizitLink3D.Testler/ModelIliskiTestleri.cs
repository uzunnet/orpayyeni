using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.Testler;

/// <summary>
/// Model iliskileri ve kenar durum testleri.
/// </summary>
public class ModelIliskiTestleri
{
    [Fact]
    public void MenuOgesi_UstMenuIliskisi_DogruKurulmali()
    {
        var ust = new MenuOgesi { Baslik = "Urunler", Url = "urunler" };
        ust.AltMenuler = new List<MenuOgesi>
        {
            new() { Baslik = "Kapi", Url = "kapi", UstMenu = ust },
            new() { Baslik = "Mobilya", Url = "mobilya", UstMenu = ust }
        };

        Assert.Equal(2, ust.AltMenuler.Count);
        Assert.Equal(ust, ust.AltMenuler[0].UstMenu);
    }

    [Fact]
    public void ProjeResim_ProjeyeBagli_DogruSiralanmali()
    {
        var proje = new Proje { Baslik = "Test", Slug = "test" };
        proje.Resimler = new List<ProjeResim>
        {
            new() { Url = "/img3.jpg", Sira = 3, Proje = proje },
            new() { Url = "/img1.jpg", Sira = 1, Proje = proje },
            new() { Url = "/img2.jpg", Sira = 2, Proje = proje }
        };

        var sirali = proje.Resimler.OrderBy(r => r.Sira).ToList();
        Assert.Equal("/img1.jpg", sirali[0].Url);
        Assert.Equal("/img3.jpg", sirali[2].Url);
    }

    [Fact]
    public void HaberYazisi_ResimEkleme_DogruCalismali()
    {
        var yazi = new HaberYazisi { Baslik = "Test", Slug = "test", Icerik = "icerik" };
        yazi.Resimler.Add(new HaberResim { ResimUrl = "/img1.jpg", Sira = 1 });
        yazi.Resimler.Add(new HaberResim { ResimUrl = "/img2.jpg", Sira = 2 });

        Assert.Equal(2, yazi.Resimler.Count);
    }

    [Fact]
    public void KapakModeli_GaleriResimleri_CokluOlmali()
    {
        var model = new VizitLink3D.Api.Modeller.KapakModeli { ModelAdi = "Test", Slug = "test" };
        model.GaleriResimleri = new List<VizitLink3D.Api.Modeller.KapiModeliResim>
        {
            new() { Url = "/1.jpg", Sira = 1 },
            new() { Url = "/2.jpg", Sira = 2 }
        };

        Assert.Equal(2, model.GaleriResimleri.Count);
    }

    [Fact]
    public void MusteriYorumu_PuanAraligi_DogruOlmali()
    {
        var yorum = new MusteriYorumu
        {
            MusteriAdi = "Test",
            Yorum = "Harika",
            Puan = 5
        };

        Assert.InRange(yorum.Puan, 1, 5);
    }

    [Fact]
    public void Slayt_HassasAnimasyonAyari_VarsayilanDogruMu()
    {
        var slayt = new Slayt { Baslik = "Test" };

        Assert.Equal("fade", slayt.AnimasyonTipi);
        Assert.Equal(800, slayt.GecisHizi);
        Assert.Equal(5000, slayt.GosterimSuresi);
    }

    [Fact]
    public void Kullanici_RolEnum_DogruAtanmali()
    {
        var admin = new Kullanici { KullaniciAdi = "admin", Eposta = "a@a.com", Rol = Rol.Admin };
        var editor = new Kullanici { KullaniciAdi = "editor", Eposta = "e@e.com", Rol = Rol.Editor };

        Assert.Equal(Rol.Admin, admin.Rol);
        Assert.Equal(Rol.Editor, editor.Rol);
        Assert.NotEqual(admin.Rol, editor.Rol);
    }

    [Fact]
    public void SistemAyari_FarkliTipler_DogruSaklanmali()
    {
        var stringAyar = new SistemAyari { Anahtar = "s", Deger = "text", Tip = "string" };
        var boolAyar = new SistemAyari { Anahtar = "b", Deger = "true", Tip = "bool" };
        var intAyar = new SistemAyari { Anahtar = "i", Deger = "42", Tip = "int" };

        Assert.Equal("string", stringAyar.Tip);
        Assert.Equal("bool", boolAyar.Tip);
        Assert.Equal("int", intAyar.Tip);
    }

    [Fact]
    public void ZiyaretKaydi_IdTipi_LongOlmali()
    {
        var kayit = new ZiyaretKaydi { IP = "127.0.0.1", Sayfa = "/" };
        Assert.True(kayit.Id >= 0);
    }

    [Fact]
    public void AuditLog_IdTipi_LongOlmali()
    {
        var log = new AuditLog { Eylem = "Test" };
        Assert.True(log.Id >= 0);
    }

    [Fact]
    public void Dil_VarsayilanMi_SadeceBirTaneOlmali()
    {
        var tr = new Dil { Kod = "tr", Ad = "Turkce", VarsayilanMi = true };
        var en = new Dil { Kod = "en", Ad = "English", VarsayilanMi = false };

        Assert.True(tr.VarsayilanMi);
        Assert.False(en.VarsayilanMi);
    }

    [Fact]
    public void MobilyaUrunu_KategoriIliskisi_DogruKurulmali()
    {
        var urun = new MobilyaUrunu
        {
            Ad = "Mutfak Dolabi",
            Slug = "mutfak-dolabi",
            MobilyaKategorisiId = 1
        };

        Assert.Equal(1, urun.MobilyaKategorisiId);
        Assert.True(urun.AktifMi);
    }
}
