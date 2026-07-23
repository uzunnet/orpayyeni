namespace VizitLink3D.Ortak.Modeller.Tema;

public sealed class StitchTemaTaslakIstek
{
    public string? DesignMdIcerik { get; set; }
    public string? DesignMdYolu { get; set; }
    public string? Slug { get; set; }
    public string? Ad { get; set; }
    public string? Aciklama { get; set; }
    public bool Premium { get; set; }
    public bool AktifEt { get; set; }
    public string FirmaId { get; set; } = "varsayilan";
    public string? Notlar { get; set; }
}

public sealed class StitchTemaTaslakSonucu
{
    public TemaManifestTaslagi Taslak { get; set; } = new();
    public bool GecerliMi { get; set; }
    public List<string> Hatalar { get; set; } = [];
    public string ManifestJson { get; set; } = "{}";
    public string TokensCss { get; set; } = string.Empty;
    public string BilesenlerCss { get; set; } = string.Empty;
    public string AnimasyonlarCss { get; set; } = string.Empty;
}

public sealed class StitchTemaOnayIstek
{
    public TemaManifestTaslagi Taslak { get; set; } = new();
    public string? HamDesignMd { get; set; }
    public string FirmaId { get; set; } = "varsayilan";
    public bool AktifEt { get; set; }
    public string? Notlar { get; set; }
}

public sealed class StitchTemaOnaySonucu
{
    public int TemaSablonuId { get; set; }
    public int RevizyonId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public int Versiyon { get; set; }
    public string TemaKlasoru { get; set; } = string.Empty;
    public bool AktifEdildiMi { get; set; }
}

public sealed class TemaManifestTaslagi
{
    public string Id { get; set; } = string.Empty;
    public string Kod { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Aciklama { get; set; } = string.Empty;
    public string Kaynak { get; set; } = "stitch";
    public string? StitchProjeId { get; set; }
    public bool Aktif { get; set; } = true;
    public bool VarsayilanMi { get; set; }
    public bool Premium { get; set; }
    public decimal Fiyat { get; set; }
    public string ParaBirimi { get; set; } = "TRY";
    public string? ThumbnailUrl { get; set; }
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
    public int Versiyon { get; set; } = 1;
    public TemaRenkTaslagi Renkler { get; set; } = new();
    public TemaTipografiTaslagi Tipografi { get; set; } = new();
    public TemaBoslukTaslagi Bosluklar { get; set; } = new();
    public TemaGeometriTaslagi Geometri { get; set; } = new();
    public TemaGolgeTaslagi Golgeler { get; set; } = new();
    public TemaGlassmorphismTaslagi Glassmorphism { get; set; } = new();
    public TemaAnimasyonTaslagi Animasyon { get; set; } = new();
    public TemaLayoutTaslagi Layout { get; set; } = new();
    public string IkonSeti { get; set; } = "Material Icons Outlined";
    public bool GlassmorphismAktif { get; set; }
    public List<string> Etiketler { get; set; } = [];
}

public sealed class TemaRenkTaslagi
{
    public string Birincil { get; set; } = "#121212";
    public string Ikincil { get; set; } = "#252525";
    public string Vurgu { get; set; } = "#D4AF37";
    public string VurguAcik { get; set; } = "#F3E5AB";
    public string VurguKoyu { get; set; } = "#AA8C2C";
    public string ArkaPlan { get; set; } = "#121212";
    public string ArkaPlan2 { get; set; } = "#1a1a1a";
    public string Yuzey { get; set; } = "#1e1e1e";
    public string YuzeyHover { get; set; } = "#282828";
    public string Cizgi { get; set; } = "#333333";
    public string Metin { get; set; } = "#E0E0E0";
    public string MetinIkincil { get; set; } = "#A0A0A0";
    public string MetinSoluk { get; set; } = "#666666";
    public string MetinTers { get; set; } = "#FFFFFF";
    public string Basari { get; set; } = "#4a7c59";
    public string Uyari { get; set; } = "#c9a449";
    public string Hata { get; set; } = "#9b3d3d";
    public string Bilgi { get; set; } = "#4a6c8c";
}

public sealed class TemaTipografiTaslagi
{
    public string BaslikAilesi { get; set; } = "Noto Serif";
    public string BaslikFallback { get; set; } = "Georgia, serif";
    public int BaslikAgirlik { get; set; } = 700;
    public string BaslikHarfAraligi { get; set; } = "0";
    public string GovdeAilesi { get; set; } = "Manrope";
    public string GovdeFallback { get; set; } = "system-ui, sans-serif";
    public int GovdeAgirlik { get; set; } = 400;
    public string VurguAilesi { get; set; } = "Cormorant Garamond";
    public string MonoAilesi { get; set; } = "JetBrains Mono";
    public decimal BoyutSkalaRatio { get; set; } = 1.25m;
    public string BaslikBuyuklukClamp { get; set; } = "clamp(2.5rem, 6vw, 5rem)";
}

public sealed class TemaBoslukTaslagi
{
    public string Xs { get; set; } = "0.25rem";
    public string Sm { get; set; } = "0.5rem";
    public string Md { get; set; } = "1rem";
    public string Lg { get; set; } = "1.5rem";
    public string Xl { get; set; } = "2.5rem";
    public string IkiXl { get; set; } = "4rem";
    public string UcXl { get; set; } = "6rem";
}

public sealed class TemaGeometriTaslagi
{
    public int KoseSm { get; set; } = 2;
    public int KoseMd { get; set; } = 4;
    public int KoseLg { get; set; } = 8;
    public int KoseXl { get; set; } = 16;
    public int KoseTam { get; set; } = 9999;
    public int BorderKalinlik { get; set; } = 1;
    public string BorderStil { get; set; } = "solid";
}

public sealed class TemaGolgeTaslagi
{
    public string Sm { get; set; } = "0 2px 8px rgba(0,0,0,0.20)";
    public string Md { get; set; } = "0 4px 20px rgba(0,0,0,0.30)";
    public string Lg { get; set; } = "0 10px 40px rgba(0,0,0,0.40)";
    public string Xl { get; set; } = "0 20px 60px rgba(0,0,0,0.50)";
    public string Vurgu { get; set; } = "0 0 15px rgba(212,175,55,0.30)";
    public string GlowStil { get; set; } = "altin";
}

public sealed class TemaGlassmorphismTaslagi
{
    public bool Aktif { get; set; }
    public string Blur { get; set; } = "20px";
    public decimal BlurSaturate { get; set; } = 1.8m;
    public decimal BgOpacity { get; set; } = 0.06m;
    public decimal BorderOpacity { get; set; } = 0.10m;
    public bool YariSaydam { get; set; }
}

public sealed class TemaAnimasyonTaslagi
{
    public string Hizi { get; set; } = "normal";
    public string GecisHizli { get; set; } = "0.15s ease";
    public string GecisNormal { get; set; } = "0.3s cubic-bezier(0.4, 0, 0.2, 1)";
    public string GecisYavas { get; set; } = "0.6s cubic-bezier(0.22, 1, 0.36, 1)";
    public string CubicBezier { get; set; } = "cubic-bezier(0.22, 1, 0.36, 1)";
    public int HoverYukseklik { get; set; } = 4;
    public decimal HoverOlcek { get; set; } = 1.03m;
    public bool ScrollReveal { get; set; } = true;
    public bool MagneticCursor { get; set; }
    public bool PariltiEfekti { get; set; } = true;
    public bool ShimmerEfekti { get; set; } = true;
    public string Tip { get; set; } = "lux-yumusak";
}

public sealed class TemaLayoutTaslagi
{
    public string Header { get; set; } = "solid-with-border";
    public string Footer { get; set; } = "minimal";
    public string HeroTipi { get; set; } = "slider";
    public string KartStili { get; set; } = "solid-elevation";
    public int IcerikGenislik { get; set; } = 1440;
    public int KenarBosluk { get; set; } = 80;
    public int SutunSayisi { get; set; } = 4;
    public string BolumAyirici { get; set; } = "cizgi";
}
