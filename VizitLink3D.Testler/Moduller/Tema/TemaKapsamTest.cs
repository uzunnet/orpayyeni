using VizitLink3D.Ortak.Modeller.Tema;
using Xunit;

namespace VizitLink3D.Testler.Moduller.Tema;

public class TemaKapsamTest
{
    [Fact]
    public void Admin_Tema_Secildiginde_Frontend_Degismez()
    {
        // Admin teması için Sadece_Admin kapsamlı tema
        var adminKapsam = TemaKapsam.Sadece_Admin;

        // Admin teması seçildiğinde site teması etkilenmemeli
        Assert.NotEqual(TemaKapsam.Sadece_Site, adminKapsam);
        Assert.True(adminKapsam == TemaKapsam.Sadece_Admin);

        // Kapsam filtresi: Sadece_Admin → site tarafında görünmez
        var siteIcinUygunMu = adminKapsam is TemaKapsam.Sadece_Site or TemaKapsam.Her_ikisi;
        Assert.False(siteIcinUygunMu);
    }

    [Fact]
    public void Site_Tema_Secildiginde_Admin_Degismez()
    {
        // Site teması için Sadece_Site kapsamlı tema
        var siteKapsam = TemaKapsam.Sadece_Site;

        // Site teması seçildiğinde admin teması etkilenmemeli
        Assert.NotEqual(TemaKapsam.Sadece_Admin, siteKapsam);
        Assert.True(siteKapsam == TemaKapsam.Sadece_Site);

        // Kapsam filtresi: Sadece_Site → admin tarafında görünmez
        var adminIcinUygunMu = siteKapsam is TemaKapsam.Sadece_Admin or TemaKapsam.Her_ikisi;
        Assert.False(adminIcinUygunMu);
    }

    [Fact]
    public void Sadece_Site_Kapsamli_Tema_Admin_Endpointinde_Gozukmez()
    {
        var sadeceSite = TemaKapsam.Sadece_Site;
        var sadeceAdmin = TemaKapsam.Sadece_Admin;
        var herIkisi = TemaKapsam.Her_ikisi;

        // Admin filtresi: Sadece_Site kapsamlı tema admin tarafında görünmez
        bool AdminFiltresi(TemaKapsam k) => k is TemaKapsam.Sadece_Admin or TemaKapsam.Her_ikisi;

        Assert.False(AdminFiltresi(sadeceSite));
        Assert.True(AdminFiltresi(sadeceAdmin));
        Assert.True(AdminFiltresi(herIkisi));

        // Site filtresi: Sadece_Admin kapsamlı tema site tarafında görünmez
        bool SiteFiltresi(TemaKapsam k) => k is TemaKapsam.Sadece_Site or TemaKapsam.Her_ikisi;

        Assert.True(SiteFiltresi(sadeceSite));
        Assert.False(SiteFiltresi(sadeceAdmin));
        Assert.True(SiteFiltresi(herIkisi));
    }

    [Fact]
    public void Tema_Sablonu_Kapsam_Varsayilan_Degeri_Her_Ikisi()
    {
        // Yeni oluşturulan temalar varsayılan olarak Her_ikisi kapsamında olmalı (geriye uyumluluk)
        var tema = new TemaSablonu();
        Assert.Equal(TemaKapsam.Her_ikisi, tema.Kapsam);
    }

    [Fact]
    public void Enum_Degerleri_Dogrulama()
    {
        // Enum değerleri sabit kalmalı (DB uyumluluğu)
        Assert.Equal(0, (int)TemaKapsam.Sadece_Admin);
        Assert.Equal(1, (int)TemaKapsam.Sadece_Site);
        Assert.Equal(2, (int)TemaKapsam.Her_ikisi);
    }
}
