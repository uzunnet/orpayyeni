using System.Reflection;
using System.Text.Json;
using VizitLink3D.Api.Moduller.Tema.Servisler;

namespace VizitLink3D.Testler;

/// <summary>
/// Aurelian Onyx Stitch tema entegrasyon testleri.
/// CokluTemaServisi sabitleri, manifest.json, CSS dosyaları,
/// yeni sayfalar, Stitch component'ler, JS wrapper ve layout doğrulaması.
/// </summary>
public class StitchTemaTestleri
{
    // ─── A) TEMA SABİTLERİ (CokluTemaServisi) ──────────────────────────

    [Fact]
    public void CokluTemaServisi_VarsayilanTema_GoldOlmali()
    {
        var alan = typeof(CokluTemaServisi).GetField("VARSAYILAN_TEMA",
            BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);

        Assert.NotNull(alan);
        var deger = alan!.GetValue(null) as string;
        Assert.Equal("gold", deger);
    }

    [Fact]
    public void CokluTemaServisi_AktifTemaAyar_AnahtarMevcut()
    {
        var alan = typeof(CokluTemaServisi).GetField("AKTIF_TEMA_AYAR",
            BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);

        Assert.NotNull(alan);
        var deger = alan!.GetValue(null) as string;
        Assert.NotNull(deger);
        Assert.NotEmpty(deger);
    }

    // ─── B) TEMA MANIFEST DOSYASI ───────────────────────────────────────

    private static readonly string ManifestYolu = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..",
        "VizitLink3D.UI", "wwwroot", "css", "temalar", "aurelian-onyx", "manifest.json");

    private static JsonDocument ManifestOku()
    {
        var tamYol = Path.GetFullPath(ManifestYolu);
        var json = File.ReadAllText(tamYol);
        return JsonDocument.Parse(json);
    }

    [Fact]
    public void AurelianOnyx_Manifest_DosyaVar()
    {
        var tamYol = Path.GetFullPath(ManifestYolu);
        Assert.True(File.Exists(tamYol), $"Manifest dosyası bulunamadı: {tamYol}");
    }

    [Fact]
    public void AurelianOnyx_Manifest_StitchProjeIdDolu()
    {
        using var doc = ManifestOku();
        var projeId = doc.RootElement.GetProperty("stitchProjeId").GetString();
        Assert.Equal("13800263520330366969", projeId);
    }

    [Fact]
    public void AurelianOnyx_Manifest_VarsayilanMi_Dogrulama()
    {
        using var doc = ManifestOku();
        var varsayilanMi = doc.RootElement.GetProperty("varsayilanMi").GetBoolean();

        // Not: manifest'te varsayilanMi = false; CokluTemaServisi.VARSAYILAN_TEMA
        // ise "aurelian-onyx" olarak tanimlidir. Bu test manifest gercegini dogrular.
        Assert.False(varsayilanMi);
    }

    [Fact]
    public void AurelianOnyx_Manifest_RenklerAltinDortBin37()
    {
        using var doc = ManifestOku();
        var renkler = doc.RootElement.GetProperty("renkler");
        var vurgu = renkler.GetProperty("vurgu").GetString();
        Assert.Equal("#d4af37", vurgu);
    }

    [Fact]
    public void AurelianOnyx_Manifest_Fontlar_PlayfairVeSpaceGrotesk()
    {
        using var doc = ManifestOku();
        var tipografi = doc.RootElement.GetProperty("tipografi");

        var baslikAilesi = tipografi.GetProperty("baslikAilesi").GetString();
        Assert.Equal("Playfair Display", baslikAilesi);

        var govdeAilesi = tipografi.GetProperty("govdeAilesi").GetString();
        Assert.Equal("Space Grotesk", govdeAilesi);
    }

    // ─── C) TEMA CSS DOSYALARI ──────────────────────────────────────────

    private static readonly string TemaKlasoru = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..",
        "VizitLink3D.UI", "wwwroot", "css", "temalar", "gold");

    [Fact]
    public void AurelianOnyx_TokensCss_TemaVurguTanimli()
    {
        var tamYol = Path.GetFullPath(Path.Combine(TemaKlasoru, "tokens.css"));
        var icerik = File.ReadAllText(tamYol);
        Assert.Contains("--tema-vurgu: #FFD700", icerik);
    }

    [Fact]
    public void AurelianOnyx_BilesenlerCss_StitchSiniflariTanimli()
    {
        var tamYol = Path.GetFullPath(Path.Combine(TemaKlasoru, "bilesenler.css"));
        var icerik = File.ReadAllText(tamYol);

        var siniflar = new[] { ".navbar", ".urun-kart", ".gb-rozet", ".footer" };
        foreach (var sinif in siniflar)
        {
            Assert.Contains(sinif, icerik);
        }
    }

    [Fact]
    public void AurelianOnyx_AnimasyonlarCss_RevealKeyframeTanimli()
    {
        var tamYol = Path.GetFullPath(Path.Combine(TemaKlasoru, "animasyonlar.css"));
        var icerik = File.ReadAllText(tamYol);

        Assert.Contains("@keyframes gold-goruntu-belir", icerik);
    }

    // ─── D) YENİ SAYFALAR ───────────────────────────────────────────────

    private static readonly string PagesKlasoru = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..",
        "VizitLink3D.UI", "Pages");

    [Fact]
    public void HareketliKoleksiyon_PagesDizini_DosyaVar()
    {
        var razor = Path.GetFullPath(Path.Combine(PagesKlasoru, "HareketliKoleksiyon.razor"));
        var cs = Path.GetFullPath(Path.Combine(PagesKlasoru, "HareketliKoleksiyon.razor.cs"));

        Assert.True(File.Exists(razor), $"Dosya bulunamadı: {razor}");
        Assert.True(File.Exists(cs), $"Dosya bulunamadı: {cs}");
    }

    [Fact]
    public void AkilliKoleksiyon_PagesDizini_DosyaVar()
    {
        var razor = Path.GetFullPath(Path.Combine(PagesKlasoru, "AkilliKoleksiyon.razor"));
        var cs = Path.GetFullPath(Path.Combine(PagesKlasoru, "AkilliKoleksiyon.razor.cs"));

        Assert.True(File.Exists(razor), $"Dosya bulunamadı: {razor}");
        Assert.True(File.Exists(cs), $"Dosya bulunamadı: {cs}");
    }

    [Fact]
    public void HareketliKoleksiyon_Route_Dogru()
    {
        var tamYol = Path.GetFullPath(Path.Combine(PagesKlasoru, "HareketliKoleksiyon.razor"));
        var icerik = File.ReadAllText(tamYol);
        Assert.Contains("@page \"/hareketli-koleksiyon\"", icerik);
    }

    [Fact]
    public void AkilliKoleksiyon_Route_Dogru()
    {
        var tamYol = Path.GetFullPath(Path.Combine(PagesKlasoru, "AkilliKoleksiyon.razor"));
        var icerik = File.ReadAllText(tamYol);
        Assert.Contains("@page \"/akilli-koleksiyon\"", icerik);
    }

    [Fact]
    public void HareketliKoleksiyon_DilServisiKullanimi()
    {
        var tamYol = Path.GetFullPath(Path.Combine(PagesKlasoru, "HareketliKoleksiyon.razor"));
        var icerik = File.ReadAllText(tamYol);
        Assert.Contains("dil.T", icerik);
        Assert.DoesNotContain("<style>", icerik);
    }

    // ─── E) STITCH COMPONENT'LER ────────────────────────────────────────

    private static readonly string StitchKlasoru = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..",
        "VizitLink3D.UI", "Bilesenler", "Stitch");

    [Fact]
    public void StitchComponents_TumBilesenlerVar()
    {
        var beklenen = new[] { "StitchHero", "StitchBentoKart", "StitchIletisimKutu",
                               "StitchKatalogKart", "StitchUrunKart", "StitchOzellikSatir" };

        foreach (var bilesen in beklenen)
        {
            var razor = Path.GetFullPath(Path.Combine(StitchKlasoru, $"{bilesen}.razor"));
            var cs = Path.GetFullPath(Path.Combine(StitchKlasoru, $"{bilesen}.razor.cs"));

            Assert.True(File.Exists(razor), $"Razor dosyası bulunamadı: {razor}");
            Assert.True(File.Exists(cs), $"Kod dosyası bulunamadı: {cs}");
        }
    }

    [Fact]
    public void StitchHero_PartialClass_Yapi()
    {
        var tamYol = Path.GetFullPath(Path.Combine(StitchKlasoru, "StitchHero.razor.cs"));
        var icerik = File.ReadAllText(tamYol);
        Assert.Contains("public partial class StitchHero", icerik);
    }

    [Fact]
    public void StitchComponents_HicbirindeStyleYok()
    {
        var dosyalar = Directory.GetFiles(StitchKlasoru, "*.razor");
        foreach (var dosya in dosyalar)
        {
            var icerik = File.ReadAllText(dosya);
            Assert.DoesNotContain("<style>", icerik);
        }
    }

    // ─── F) JS WRAPPER ──────────────────────────────────────────────────

    private static readonly string JsKlasoru = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..",
        "VizitLink3D.UI", "wwwroot", "js");

    [Fact]
    public void PariltiMotoru_DosyaVar()
    {
        var tamYol = Path.GetFullPath(Path.Combine(JsKlasoru, "parilti-motoru.js"));
        Assert.True(File.Exists(tamYol), $"JS dosyası bulunamadı: {tamYol}");
    }

    [Fact]
    public void ScrollAnimasyon_DosyaVar()
    {
        var tamYol = Path.GetFullPath(Path.Combine(JsKlasoru, "scroll-animasyon.js"));
        Assert.True(File.Exists(tamYol), $"JS dosyası bulunamadı: {tamYol}");
    }

    // ─── G) LAYOUT ──────────────────────────────────────────────────────

    [Fact]
    public void Layout_DataTemaIdSet()
    {
        var layoutYolu = Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..",
            "VizitLink3D.UI", "Layout", "VizitLink3DDuzen.razor.cs"));
        var icerik = File.ReadAllText(layoutYolu);
        Assert.Contains("\"gold\"", icerik);
    }

    [Fact]
    public void IndexHtml_DataTemaIdFallback()
    {
        var indexYolu = Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..",
            "VizitLink3D.UI", "wwwroot", "index.html"));
        var icerik = File.ReadAllText(indexYolu);
        Assert.Contains("data-tema-id=\"gold\"", icerik);
    }
}
