using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using VizitLink3D.Api.Hubs;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Tema;

namespace VizitLink3D.Api.Moduller.Tema.Servisler;

/// <summary>
/// Çoklu tema servisi. Temaları DB'den okur, manifest.json dosyalarından CSS üretir.
/// Tema değiştiğinde SignalR üzerinden tüm açık tarayıcılara broadcast yapar.
/// </summary>
public class CokluTemaServisi(
    IWebHostEnvironment ortam,
    IHubContext<TemaHub> temaHub,
    ILogger<CokluTemaServisi> log,
    VizitLink3DDbContext db)
{
    public const string VARSAYILAN_TEMA = "gold";
    public const string VARSAYILAN_ADMIN_TEMA = "endustri-karanlik";
    public const string AKTIF_TEMA_AYAR = "site.aktifTema";
    private static readonly JsonSerializerOptions JsonAyar = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    /// <summary>Tüm aktif temaları DB'den çeker. Kapsam filtresi ile isteğe bağlı daraltma yapar.</summary>
    public async Task<IReadOnlyList<TemaOzetDto>> KatalogGetirAsync(TemaKapsam? kapsam = null)
    {
        await EksikTemaKayitlariniIceriAktarAsync(kapsam);

        IQueryable<TemaSablonu> sorgu = db.TemaSablonlari
            .Where(t => t.AktifMi && !t.SilindiMi);

        if (kapsam.HasValue)
        {
            var k = kapsam.Value;
            if (k == TemaKapsam.Sadece_Site)
                sorgu = sorgu.Where(t => t.Kapsam == TemaKapsam.Sadece_Site || t.Kapsam == TemaKapsam.Her_ikisi);
            else if (k == TemaKapsam.Sadece_Admin)
                sorgu = sorgu.Where(t => t.Kapsam == TemaKapsam.Sadece_Admin || t.Kapsam == TemaKapsam.Her_ikisi);
        }

        var dbTemalari = await sorgu.OrderBy(t => t.Ad).ToListAsync();
        var dosyaTemalari = await DosyadakiTemaOzetleriniGetirAsync(kapsam);

        return dbTemalari
            .Select(t => new TemaOzetDto(t.Slug, t.Ad, t.Aciklama, t.GlassmorphismAktif, t.Premium, t.Etiketler, t.ThumbnailUrl))
            .Concat(dosyaTemalari)
            .GroupBy(t => t.Slug, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(t => t.Ad)
            .ToList();
    }

    /// <summary>Sadece site kapsamlı (frontend) temaları getirir.</summary>
    public async Task<IReadOnlyList<TemaOzetDto>> SiteTemalariGetirAsync()
    {
        return await KatalogGetirAsync(TemaKapsam.Sadece_Site);
    }

    /// <summary>Slug'dan tema bulur (DB'den).</summary>
    public async Task<TemaSablonu?> TemaBulAsync(string slug)
    {
        var tema = await db.TemaSablonlari
            .FirstOrDefaultAsync(t => t.Slug == slug && !t.SilindiMi);

        if (tema != null)
            return tema;

        await EksikTemaKayitlariniIceriAktarAsync();

        return await db.TemaSablonlari
            .FirstOrDefaultAsync(t => t.Slug == slug && !t.SilindiMi);
    }

    public async Task<bool> TemaMevcutMuAsync(string slug, TemaKapsam? kapsam = null)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return false;

        var temizSlug = slug.Trim().ToLowerInvariant();
        await EksikTemaKayitlariniIceriAktarAsync(kapsam);

        var sorgu = db.TemaSablonlari.Where(t => t.Slug == temizSlug && t.AktifMi && !t.SilindiMi);
        if (kapsam.HasValue)
        {
            var hedefKapsam = kapsam.Value;
            if (hedefKapsam == TemaKapsam.Sadece_Site)
                sorgu = sorgu.Where(t => t.Kapsam == TemaKapsam.Sadece_Site || t.Kapsam == TemaKapsam.Her_ikisi);
            else if (hedefKapsam == TemaKapsam.Sadece_Admin)
                sorgu = sorgu.Where(t => t.Kapsam == TemaKapsam.Sadece_Admin || t.Kapsam == TemaKapsam.Her_ikisi);
        }

        return await sorgu.AnyAsync();
    }

    /// <summary>
    /// Site (frontend) temasını yükler ve uygular.
    /// - Kapsam kontrolü yapar (Sadece_Admin kapsamlı tema hata döner)
    /// - Tema dosyalarını UI wwwroot'una kopyalar (Blazor WASM'in görmesi için)
    /// - FirmaTemaAtamalari tablosuna Tur="site" ile kayıt açar
    /// - SignalR üzerinden SiteTemaDegisti broadcast yapar
    /// </summary>
    public async Task<bool> TemaYukleVeUygulaAsync(string slug, string mod = "koyu", string firmaId = "varsayilan")
    {
        var tema = await db.TemaSablonlari
            .FirstOrDefaultAsync(t => t.Slug == slug && !t.SilindiMi);

        if (tema == null)
        {
            log.LogWarning("Tema bulunamadı: {Slug}", slug);
            return false;
        }

        if (tema.Kapsam == TemaKapsam.Sadece_Admin)
        {
            log.LogWarning("Bu tema admin tarafında kullanılamaz (Kapsam=Sadece_Admin): {Slug}", slug);
            return false;
        }

        // Tema dosyalarının kaynağını bul ve iki tarafa da eşitle.
        var apiTemaKlasoru = Path.Combine(ortam.ContentRootPath, "wwwroot", "css", "temalar", slug);
        var uiKoku = Path.GetFullPath(Path.Combine(ortam.ContentRootPath, "..", "VizitLink3D.UI", "wwwroot"));
        var uiTemaKlasoru = Path.Combine(uiKoku, "css", "temalar", slug);
        var uiLiveTemaKlasoru = Path.GetFullPath(Path.Combine(ortam.ContentRootPath, "..", "ui-live", "wwwroot", "css", "temalar", slug));
        var kaynakTemaKlasoru = TemaKaynakKlasoruBul(apiTemaKlasoru, uiTemaKlasoru, uiLiveTemaKlasoru);

        if (kaynakTemaKlasoru == null)
        {
            log.LogWarning("Tema klasörü bulunamadı: {Slug}", slug);
            return false;
        }

        try
        {
            KlasorKopyala(kaynakTemaKlasoru, apiTemaKlasoru);
            KlasorKopyala(kaynakTemaKlasoru, uiTemaKlasoru);
            log.LogInformation("Tema dosyaları eşitlendi: {Slug} kaynak:{Kaynak}", slug, kaynakTemaKlasoru);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Tema dosyaları eşitlenemedi: {Slug}", slug);
            return false;
        }

        // FirmaTemaAtamalari'na kayıt aç (site teması)
        if (int.TryParse(firmaId, out var fId))
        {
            var mevcutAtama = await db.FirmaTemaAtamalari
                .FirstOrDefaultAsync(a => a.FirmaId == fId && a.Tur == "site");
            if (mevcutAtama == null)
            {
                db.FirmaTemaAtamalari.Add(new FirmaTemaAtama
                {
                    FirmaId = fId,
                    TemaSablonuId = tema.Id,
                    Tur = "site",
                    AtamaTarihi = DateTime.UtcNow
                });
            }
            else
            {
                mevcutAtama.TemaSablonuId = tema.Id;
                mevcutAtama.AtamaTarihi = DateTime.UtcNow;
            }
            await db.SaveChangesAsync();
        }

        await temaHub.Clients.Group(firmaId).SendAsync("SiteTemaDegisti", new { tema = slug, firmaId });
        return true;
    }

    /// <summary>
    /// Admin panel temasını uygular. Sadece DB'de Firma.AdminTema alanını günceller.
    /// Frontend CSS'ine ve SignalR'a DOKUNMAZ (admin tarafı bağımsızdır).
    /// </summary>
    public async Task<bool> TemaYukleVeUygulaAdminAsync(string slug, string firmaId = "varsayilan")
    {
        var tema = await db.TemaSablonlari
            .FirstOrDefaultAsync(t => t.Slug == slug && !t.SilindiMi);

        if (tema == null)
        {
            log.LogWarning("Admin tema bulunamadı: {Slug}", slug);
            return false;
        }

        // Admin endpoint sadece Sadece_Admin veya Her_ikisi kapsamlı temaları kabul etsin
        if (tema.Kapsam == TemaKapsam.Sadece_Site)
        {
            log.LogWarning("Bu tema admin tarafında kullanılamaz (Kapsam=Sadece_Site): {Slug}", slug);
            return false;
        }

        if (!int.TryParse(firmaId, out var fId))
        {
            log.LogWarning("Geçersiz firma ID (TemaYukleVeUygulaAdminAsync): {FirmaId}", firmaId);
            return false;
        }

        var firma = await db.Firmalar.FirstOrDefaultAsync(f => f.Id == fId && f.AktifMi);
        if (firma == null)
        {
            log.LogWarning("Firma bulunamadı: {FirmaId}", firmaId);
            return false;
        }

        firma.AdminTema = slug;
        firma.GuncellenmeTarihi = DateTime.UtcNow;
        await db.SaveChangesAsync();

        log.LogInformation("Admin tema uygulandı: {Slug}, firma:{FirmaId}", slug, firmaId);
        return true;
    }

    /// <summary>Klasörü ve içindeki tüm dosyaları özyinelemeli kopyalar.</summary>
    private static void KlasorKopyala(string kaynakKlasor, string hedefKlasor)
    {
        Directory.CreateDirectory(hedefKlasor);

        foreach (var dosya in Directory.GetFiles(kaynakKlasor))
        {
            var hedefDosya = Path.Combine(hedefKlasor, Path.GetFileName(dosya));
            try
            {
                File.Copy(dosya, hedefDosya, true);
            }
            catch (IOException)
            {
                // Dosya başka bir process (antivirüs/indexer) tarafından kilitliyse
                // uygulamayı çökertme — bu dosyayı atla, sonraki senkronda güncellenir.
            }
        }

        foreach (var altKlasor in Directory.GetDirectories(kaynakKlasor))
        {
            var hedefAlt = Path.Combine(hedefKlasor, Path.GetFileName(altKlasor));
            KlasorKopyala(altKlasor, hedefAlt);
        }
    }

    private static string? TemaKaynakKlasoruBul(params string[] adayKlasorler)
    {
        foreach (var klasor in adayKlasorler)
        {
            if (Directory.Exists(klasor) && File.Exists(Path.Combine(klasor, "manifest.json")))
            {
                return klasor;
            }
        }

        return null;
    }

    private async Task EksikTemaKayitlariniIceriAktarAsync(TemaKapsam? kapsam = null)
    {
        var diskTemalari = await DosyadakiTemaOzetleriniGetirAsync(kapsam);
        if (diskTemalari.Count == 0)
            return;

        var dbSluglari = await db.TemaSablonlari
            .Where(t => !t.SilindiMi)
            .Select(t => t.Slug)
            .ToListAsync();

        var eksikSluglar = diskTemalari
            .Select(t => t.Slug)
            .Where(slug => !dbSluglari.Contains(slug, StringComparer.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (eksikSluglar.Count == 0)
            return;

        await TopluTemaOlusturAsync(eksikSluglar);
    }

    private async Task<List<TemaOzetDto>> DosyadakiTemaOzetleriniGetirAsync(TemaKapsam? kapsam = null)
    {
        var sonuc = new List<TemaOzetDto>();
        var kokler = TemaKokleriniGetir();
        var gorulenSluglar = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var kok in kokler.Where(Directory.Exists))
        {
            foreach (var klasor in Directory.GetDirectories(kok))
            {
                var slug = Path.GetFileName(klasor);
                if (string.IsNullOrWhiteSpace(slug) || slug.StartsWith("_") || slug.Equals("eskitema", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!gorulenSluglar.Add(slug))
                    continue;

                var manifestYolu = Path.Combine(klasor, "manifest.json");
                if (!File.Exists(manifestYolu))
                    continue;

                try
                {
                    var manifestJson = await File.ReadAllTextAsync(manifestYolu, Encoding.UTF8);
                    using var doc = JsonDocument.Parse(manifestJson);
                    var root = doc.RootElement;

                    var temaKapsami = root.TryGetProperty("kapsam", out var kp) &&
                                      Enum.TryParse<TemaKapsam>(kp.GetString(), out var kaps)
                        ? kaps
                        : TemaKapsam.Sadece_Site;

                    if (kapsam.HasValue)
                    {
                        if (kapsam == TemaKapsam.Sadece_Site && temaKapsami == TemaKapsam.Sadece_Admin)
                            continue;
                        if (kapsam == TemaKapsam.Sadece_Admin && temaKapsami == TemaKapsam.Sadece_Site)
                            continue;
                    }

                    var aktifMi = !root.TryGetProperty("aktif", out var ak) || ak.GetBoolean();
                    if (!aktifMi)
                        continue;

                    var ad = root.TryGetProperty("ad", out var adDeger) ? adDeger.GetString() ?? slug : slug;
                    var aciklama = root.TryGetProperty("aciklama", out var aciklamaDeger) ? aciklamaDeger.GetString() ?? "" : "";
                    var premium = root.TryGetProperty("premium", out var premiumDeger) && premiumDeger.GetBoolean();
                    var glass = root.TryGetProperty("glassmorphismAktif", out var glassDeger) && glassDeger.GetBoolean();
                    var etiketler = root.TryGetProperty("etiketler", out var etiketDeger)
                        ? string.Join(",", etiketDeger.EnumerateArray().Select(x => x.GetString()).Where(x => !string.IsNullOrWhiteSpace(x)))
                        : "";
                    var thumb = root.TryGetProperty("thumbnailUrl", out var thumbDeger) ? thumbDeger.GetString() : $"/css/temalar/{slug}/ekran-goruntusu.jpg";

                    sonuc.Add(new TemaOzetDto(slug, ad, aciklama, glass, premium, etiketler, thumb));
                }
                catch (Exception ex)
                {
                    log.LogWarning(ex, "Tema manifest okunamadı: {Slug}", slug);
                }
            }
        }

        return sonuc;
    }

    private IEnumerable<string> TemaKokleriniGetir()
    {
        yield return Path.Combine(ortam.ContentRootPath, "wwwroot", "css", "temalar");
        yield return Path.GetFullPath(Path.Combine(ortam.ContentRootPath, "..", "VizitLink3D.UI", "wwwroot", "css", "temalar"));
        yield return Path.GetFullPath(Path.Combine(ortam.ContentRootPath, "..", "ui-live", "wwwroot", "css", "temalar"));
    }

    /// <summary>Stitch taslağını doğrular, dosyaları üretir, DB kayıtlarını günceller ve hub yayını yapar.</summary>
    public async Task<Cevap<StitchTemaOnaySonucu>> StitchTaslagiOnaylaAsync(
        StitchTemaOnayIstek istek,
        CancellationToken iptal = default)
    {
        var hatalar = TemaTaslagiDogrula(istek.Taslak);
        if (hatalar.Count > 0)
            return Cevap<StitchTemaOnaySonucu>.Hata("Tema taslağı doğrulanamadı.", hatalar);

        var manifestJson = ManifestJsonUret(istek.Taslak);
        var tokensCss = ManifestToTokensCss(manifestJson, istek.Taslak.Slug);
        var bilesenlerCss = BilesenlerCssUret(istek.Taslak);
        var animasyonlarCss = AnimasyonlarCssUret(istek.Taslak);

        var temaKlasoru = await TemaDosyalariniAtomikYazAsync(
            istek.Taslak.Slug,
            manifestJson,
            tokensCss,
            bilesenlerCss,
            animasyonlarCss,
            iptal);

        // Aynı slug ile mevcut kayıt varsa UPDATE yap, yoksa INSERT.
        // Bu sayede aynı slug'dan iki kayıt oluşmaz (unique index ile de garanti altında).
        var mevcut = await db.TemaSablonlari
            .FirstOrDefaultAsync(t => t.Slug == istek.Taslak.Slug && !t.SilindiMi, iptal);

        var yeniMi = mevcut is null;
        var tema = mevcut ?? new TemaSablonu
        {
            Slug = istek.Taslak.Slug,
            OlusturulmaTarihi = DateTime.UtcNow
        };

        TemaSablonunuTaslaklaGuncelle(tema, istek.Taslak, yeniMi);
        if (yeniMi)
            db.TemaSablonlari.Add(tema);

        await db.SaveChangesAsync(iptal);

        var revizyon = new TemaRevizyonu
        {
            TemaSablonuId = tema.Id,
            Versiyon = tema.Versiyon,
            KaynakTipi = "stitch",
            HamDesignMd = istek.HamDesignMd,
            UretilenManifestJson = manifestJson,
            Notlar = istek.Notlar,
            OlusturulmaTarihi = DateTime.UtcNow
        };

        db.TemaRevizyonlari.Add(revizyon);
        await db.SaveChangesAsync(iptal);

        var aktifEdildi = false;
        if (istek.AktifEt)
            aktifEdildi = await TemaYukleVeUygulaAsync(tema.Slug, firmaId: istek.FirmaId);
        else
            await temaHub.Clients.Group(istek.FirmaId).SendAsync("TemaSablonuGuncellendi", new { tema = tema.Slug, firmaId = istek.FirmaId }, iptal);

        log.LogInformation("Stitch tema onaylandı: {Slug}, v{Versiyon}", tema.Slug, tema.Versiyon);

        return Cevap<StitchTemaOnaySonucu>.Basarili(new StitchTemaOnaySonucu
        {
            TemaSablonuId = tema.Id,
            RevizyonId = revizyon.Id,
            Slug = tema.Slug,
            Versiyon = tema.Versiyon,
            TemaKlasoru = temaKlasoru,
            AktifEdildiMi = aktifEdildi
        }, "Stitch teması onaylandı.");
    }

    public List<string> TemaTaslagiDogrula(TemaManifestTaslagi taslak)
    {
        var hatalar = new List<string>();

        if (string.IsNullOrWhiteSpace(taslak.Ad))
            hatalar.Add("Tema adı zorunludur.");

        if (string.IsNullOrWhiteSpace(taslak.Slug) || !Regex.IsMatch(taslak.Slug, "^[a-z0-9][a-z0-9-]{1,78}[a-z0-9]$"))
            hatalar.Add("Tema slug değeri küçük harf, rakam ve tire içermelidir.");

        if (string.IsNullOrWhiteSpace(taslak.Kod) || !Regex.IsMatch(taslak.Kod, "^[A-Z0-9_]{2,80}$"))
            hatalar.Add("Tema kodu büyük harf, rakam ve alt çizgi içermelidir.");

        var renkler = new[]
        {
            taslak.Renkler.Birincil, taslak.Renkler.Ikincil, taslak.Renkler.Vurgu,
            taslak.Renkler.ArkaPlan, taslak.Renkler.Yuzey, taslak.Renkler.Metin
        };

        if (renkler.Any(r => string.IsNullOrWhiteSpace(r) || !GecerliCssRenkMi(r)))
            hatalar.Add("Zorunlu renk tokenları geçerli CSS renk değeri olmalıdır.");

        if (string.IsNullOrWhiteSpace(taslak.Tipografi.BaslikAilesi) || string.IsNullOrWhiteSpace(taslak.Tipografi.GovdeAilesi))
            hatalar.Add("Başlık ve gövde font ailesi zorunludur.");

        if (taslak.Layout.SutunSayisi is < 1 or > 12)
            hatalar.Add("Sütun sayısı 1 ile 12 arasında olmalıdır.");

        return hatalar;
    }

    public string ManifestJsonUret(TemaManifestTaslagi taslak)
    {
        return JsonSerializer.Serialize(taslak, JsonAyar);
    }

    public string BilesenlerCssUret(TemaManifestTaslagi taslak)
    {
        var slug = taslak.Slug;
        var css = new StringBuilder();
        css.AppendLine("/*");
        css.AppendLine($" * {taslak.Ad} — bilesenler.css");
        css.AppendLine(" * CokluTemaServisi tarafından manifest taslağından otomatik üretilir.");
        css.AppendLine(" */");
        css.AppendLine();
        css.AppendLine($":root[data-tema-id=\"{slug}\"] {{");
        css.AppendLine("    color: var(--tema-metin);");
        css.AppendLine("    background: var(--tema-arkaplan);");
        css.AppendLine("}");
        css.AppendLine();
        css.AppendLine($":root[data-tema-id=\"{slug}\"] h1,");
        css.AppendLine($":root[data-tema-id=\"{slug}\"] h2,");
        css.AppendLine($":root[data-tema-id=\"{slug}\"] h3 {{");
        css.AppendLine("    font-family: var(--tema-font-baslik);");
        css.AppendLine("    font-weight: var(--tema-baslik-agirlik);");
        css.AppendLine("    letter-spacing: var(--tema-baslik-harf-araligi);");
        css.AppendLine("}");
        css.AppendLine();
        css.AppendLine($":root[data-tema-id=\"{slug}\"] .navbar {{");
        css.AppendLine("    background: var(--tema-yuzey);");
        css.AppendLine("    border-bottom: var(--tema-border-kalinlik) var(--tema-border-stil) var(--tema-cizgi);");
        css.AppendLine("    transition: var(--tema-gecis-normal);");
        css.AppendLine("}");
        css.AppendLine();
        css.AppendLine($":root[data-tema-id=\"{slug}\"] .urun-kart {{");
        css.AppendLine(taslak.Glassmorphism.Aktif
            ? "    background: var(--tema-cam-bg);"
            : "    background: var(--tema-yuzey);");
        if (taslak.Glassmorphism.Aktif)
        {
            css.AppendLine("    backdrop-filter: var(--tema-cam-blur);");
            css.AppendLine("    -webkit-backdrop-filter: var(--tema-cam-blur);");
        }
        css.AppendLine("    border: var(--tema-border-kalinlik) var(--tema-border-stil) var(--tema-cizgi);");
        css.AppendLine("    border-radius: var(--tema-kose-md);");
        css.AppendLine("    box-shadow: var(--tema-golge-md);");
        css.AppendLine("    transition: transform var(--tema-gecis-yavas), box-shadow var(--tema-gecis-yavas), border-color var(--tema-gecis-normal);");
        css.AppendLine("}");
        css.AppendLine();
        css.AppendLine($":root[data-tema-id=\"{slug}\"] .urun-kart:hover {{");
        css.AppendLine($"    transform: translateY(-{taslak.Animasyon.HoverYukseklik}px);");
        css.AppendLine("    border-color: var(--tema-vurgu);");
        css.AppendLine("    box-shadow: var(--tema-golge-vurgu), var(--tema-golge-lg);");
        css.AppendLine("}");
        css.AppendLine();
        css.AppendLine($":root[data-tema-id=\"{slug}\"] .btn-birincil,");
        css.AppendLine($":root[data-tema-id=\"{slug}\"] .mud-button-filled {{");
        css.AppendLine("    background: var(--tema-vurgu);");
        css.AppendLine("    color: var(--tema-metin-ters);");
        css.AppendLine("    border-radius: var(--tema-kose-md);");
        css.AppendLine("    transition: var(--tema-gecis-normal);");
        css.AppendLine("}");
        css.AppendLine();
        css.AppendLine($":root[data-tema-id=\"{slug}\"] .footer {{");
        css.AppendLine("    background: var(--tema-birincil);");
        css.AppendLine("    color: var(--tema-metin);");
        css.AppendLine("    border-top: var(--tema-border-kalinlik) var(--tema-border-stil) var(--tema-cizgi);");
        css.AppendLine("}");

        return css.ToString();
    }

    public string AnimasyonlarCssUret(TemaManifestTaslagi taslak)
    {
        var slug = taslak.Slug;
        var animasyonAdi = $"tema-marka-akis-{slug}";
        var css = new StringBuilder();
        css.AppendLine("/*");
        css.AppendLine($" * {taslak.Ad} — animasyonlar.css");
        css.AppendLine(" * Tema-özgü motion presetleri CokluTemaServisi tarafından üretilir.");
        css.AppendLine(" */");
        css.AppendLine();
        css.AppendLine($"@keyframes {animasyonAdi} {{");
        css.AppendLine("    0%, 100% { background-position: 0% 50%; }");
        css.AppendLine("    50% { background-position: 100% 50%; }");
        css.AppendLine("}");
        css.AppendLine();
        css.AppendLine($":root[data-tema-id=\"{slug}\"] .marka-parilti {{");
        css.AppendLine("    background: linear-gradient(135deg, var(--tema-vurgu), var(--tema-vurgu-acik), var(--tema-vurgu));");
        css.AppendLine("    background-size: 200% 200%;");
        css.AppendLine($"    animation: {animasyonAdi} 3s ease infinite;");
        css.AppendLine("}");
        css.AppendLine();
        css.AppendLine($":root[data-tema-id=\"{slug}\"] .gecis-tum {{");
        css.AppendLine("    transition: all var(--tema-gecis-normal);");
        css.AppendLine("}");

        return css.ToString();
    }

    private async Task<string> TemaDosyalariniAtomikYazAsync(
        string slug,
        string manifestJson,
        string tokensCss,
        string bilesenlerCss,
        string animasyonlarCss,
        CancellationToken iptal)
    {
        var apiTemalarKoku = Path.Combine(ortam.ContentRootPath, "wwwroot", "css", "temalar");
        var uiTemalarKoku = Path.GetFullPath(Path.Combine(ortam.ContentRootPath, "..", "VizitLink3D.UI", "wwwroot", "css", "temalar"));

        await TemaDosyalariniTekKokeYazAsync(apiTemalarKoku, slug, manifestJson, tokensCss, bilesenlerCss, animasyonlarCss, iptal);
        await TemaDosyalariniTekKokeYazAsync(uiTemalarKoku, slug, manifestJson, tokensCss, bilesenlerCss, animasyonlarCss, iptal);

        return Path.Combine(apiTemalarKoku, slug);
    }

    private static async Task TemaDosyalariniTekKokeYazAsync(
        string temalarKoku,
        string slug,
        string manifestJson,
        string tokensCss,
        string bilesenlerCss,
        string animasyonlarCss,
        CancellationToken iptal)
    {
        var hedefKlasor = Path.Combine(temalarKoku, slug);
        var geciciKlasor = Path.Combine(temalarKoku, $".tmp-{slug}-{Guid.NewGuid():N}");

        Directory.CreateDirectory(geciciKlasor);
        await File.WriteAllTextAsync(Path.Combine(geciciKlasor, "manifest.json"), manifestJson, new UTF8Encoding(false), iptal);
        await File.WriteAllTextAsync(Path.Combine(geciciKlasor, "tokens.css"), tokensCss, new UTF8Encoding(true), iptal);
        await File.WriteAllTextAsync(Path.Combine(geciciKlasor, "bilesenler.css"), bilesenlerCss, new UTF8Encoding(true), iptal);
        await File.WriteAllTextAsync(Path.Combine(geciciKlasor, "animasyonlar.css"), animasyonlarCss, new UTF8Encoding(true), iptal);

        Directory.CreateDirectory(hedefKlasor);
        foreach (var dosyaAdi in new[] { "manifest.json", "tokens.css", "bilesenler.css", "animasyonlar.css" })
        {
            var kaynak = Path.Combine(geciciKlasor, dosyaAdi);
            var hedef = Path.Combine(hedefKlasor, dosyaAdi);
            File.Move(kaynak, hedef, true);
        }

        Directory.Delete(geciciKlasor, true);
    }

    public async Task<int> TemaVarliklariniEsitleVeIceriAktarAsync(CancellationToken iptal = default)
    {
        var temaKokleri = new[]
        {
            Path.Combine(ortam.ContentRootPath, "wwwroot", "css", "temalar"),
            Path.GetFullPath(Path.Combine(ortam.ContentRootPath, "..", "VizitLink3D.UI", "wwwroot", "css", "temalar")),
            Path.GetFullPath(Path.Combine(ortam.ContentRootPath, "..", "ui-live", "wwwroot", "css", "temalar"))
        };

        var bulunanTemaSluglari = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var kok in temaKokleri.Where(Directory.Exists))
        {
            foreach (var klasor in Directory.GetDirectories(kok))
            {
                var slug = Path.GetFileName(klasor);
                if (string.IsNullOrWhiteSpace(slug) || slug.StartsWith("_") || slug.Equals("eskitema", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!File.Exists(Path.Combine(klasor, "manifest.json")))
                    continue;

                bulunanTemaSluglari.Add(slug);
            }
        }

        foreach (var slug in bulunanTemaSluglari)
        {
            iptal.ThrowIfCancellationRequested();

            var apiTemaKlasoru = Path.Combine(ortam.ContentRootPath, "wwwroot", "css", "temalar", slug);
            var uiTemaKlasoru = Path.GetFullPath(Path.Combine(ortam.ContentRootPath, "..", "VizitLink3D.UI", "wwwroot", "css", "temalar", slug));
            var uiLiveTemaKlasoru = Path.GetFullPath(Path.Combine(ortam.ContentRootPath, "..", "ui-live", "wwwroot", "css", "temalar", slug));
            var kaynak = TemaKaynakKlasoruBul(apiTemaKlasoru, uiTemaKlasoru, uiLiveTemaKlasoru);

            if (kaynak == null)
                continue;

            KlasorKopyala(kaynak, apiTemaKlasoru);
            KlasorKopyala(kaynak, uiTemaKlasoru);
        }

        var adet = await TopluTemaOlusturAsync(bulunanTemaSluglari.OrderBy(x => x).ToList(), iptal);
        log.LogInformation("Tema varlıkları eşitlendi, bulunan tema sayısı: {TemaAdedi}, yeni içe aktarılan: {IceriAktarilan}", bulunanTemaSluglari.Count, adet);
        return bulunanTemaSluglari.Count;
    }

    private static void TemaSablonunuTaslaklaGuncelle(TemaSablonu tema, TemaManifestTaslagi taslak, bool yeniMi)
    {
        tema.Kod = taslak.Kod;
        tema.Ad = taslak.Ad;
        tema.Slug = taslak.Slug;
        tema.Aciklama = taslak.Aciklama;
        tema.Kaynak = taslak.Kaynak;
        tema.StitchProjeId = taslak.StitchProjeId;
        tema.GlassmorphismAktif = taslak.Glassmorphism.Aktif || taslak.GlassmorphismAktif;
        tema.Premium = taslak.Premium;
        tema.Fiyat = taslak.Fiyat;
        tema.ParaBirimi = taslak.ParaBirimi;
        tema.ThumbnailUrl = taslak.ThumbnailUrl;
        tema.AktifMi = taslak.Aktif;
        tema.VarsayilanMi = taslak.VarsayilanMi;
        tema.Etiketler = string.Join(",", taslak.Etiketler);
        tema.RenklerJson = JsonSerializer.Serialize(taslak.Renkler, JsonAyar);
        tema.TipografiJson = JsonSerializer.Serialize(taslak.Tipografi, JsonAyar);
        tema.GeometriJson = JsonSerializer.Serialize(taslak.Geometri, JsonAyar);
        tema.GolgelerJson = JsonSerializer.Serialize(taslak.Golgeler, JsonAyar);
        tema.GlassmorphismJson = JsonSerializer.Serialize(taslak.Glassmorphism, JsonAyar);
        tema.AnimasyonJson = JsonSerializer.Serialize(taslak.Animasyon, JsonAyar);
        tema.LayoutJson = JsonSerializer.Serialize(taslak.Layout, JsonAyar);
        tema.IkonSeti = taslak.IkonSeti;
        tema.AdAnahtar = $"tema.{taslak.Slug}.ad";
        tema.AciklamaAnahtar = $"tema.{taslak.Slug}.aciklama";
        tema.AdVarsayilanTr = taslak.Ad;
        tema.AdVarsayilanEn = taslak.Ad;
        tema.AciklamaVarsayilanTr = taslak.Aciklama;
        tema.AciklamaVarsayilanEn = taslak.Aciklama;
        tema.GuncellenmeTarihi = DateTime.UtcNow;
        tema.Versiyon = yeniMi ? Math.Max(1, taslak.Versiyon) : tema.Versiyon + 1;
    }

    private static bool GecerliCssRenkMi(string deger)
    {
        if (Regex.IsMatch(deger, "^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$"))
            return true;

        return deger.StartsWith("rgb(", StringComparison.OrdinalIgnoreCase)
            || deger.StartsWith("rgba(", StringComparison.OrdinalIgnoreCase)
            || deger.StartsWith("var(", StringComparison.OrdinalIgnoreCase)
            || deger.StartsWith("color-mix(", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Firma için atanmış aktif tema slug'ını getirir.</summary>
    public async Task<string> FirmaIcinAktifTemplateGetirAsync(int firmaId)
    {
        var atama = await db.FirmaTemaAtamalari
            .Where(f => f.FirmaId == firmaId && f.AktifMi)
            .OrderByDescending(f => f.AtamaTarihi)
            .FirstOrDefaultAsync();

        if (atama != null)
        {
            var tema = await db.TemaSablonlari.FindAsync(atama.TemaSablonuId);
            if (tema != null && tema.AktifMi && !tema.SilindiMi)
                return tema.Slug;
        }

        // Varsayılan temaya düş
        var varsayilan = await db.TemaSablonlari
            .FirstOrDefaultAsync(t => t.VarsayilanMi && t.AktifMi && !t.SilindiMi);
        return varsayilan?.Slug ?? VARSAYILAN_TEMA;
    }

    /// <summary>Tema modunu (acik/koyu) değiştirir ve Hub'a bildirir.</summary>
    public async Task<bool> ModDegistirAsync(string slug, string mod, string firmaId = "varsayilan")
    {
        var tema = await db.TemaSablonlari
            .FirstOrDefaultAsync(t => t.Slug == slug && !t.SilindiMi);
        if (tema == null) return false;

        var manifestYolu = Path.Combine(ortam.ContentRootPath, "wwwroot", "css", "temalar", slug, "manifest.json");
        if (!File.Exists(manifestYolu)) return false;

        var manifestJson = await File.ReadAllTextAsync(manifestYolu, Encoding.UTF8);
        using var doc = JsonDocument.Parse(manifestJson);
        var modlar = doc.RootElement.TryGetProperty("modlar", out var m)
            ? m.EnumerateArray().Select(e => e.GetString()).Where(s => s != null).ToHashSet()
            : new HashSet<string> { "koyu" };

        if (!modlar.Contains(mod)) return false;

        await temaHub.Clients.Group(firmaId).SendAsync("TemaModDegisti", new { tema = slug, mod, firmaId });
        log.LogInformation("Tema modu değişti: {Slug} → {Mod}", slug, mod);
        return true;
    }

    /// <summary>Bir tema slug'ının varsayılan modunu manifest'ten okur.</summary>
    public async Task<string> AcikVeyaKoyuVarsayilanModGetirAsync(string slug)
    {
        var manifestYolu = Path.Combine(ortam.ContentRootPath, "wwwroot", "css", "temalar", slug, "manifest.json");
        if (!File.Exists(manifestYolu)) return "koyu";

        var manifestJson = await File.ReadAllTextAsync(manifestYolu, Encoding.UTF8);
        using var doc = JsonDocument.Parse(manifestJson);
        return doc.RootElement.TryGetProperty("varsayilanMod", out var vm)
            ? vm.GetString() ?? "koyu"
            : "koyu";
    }

    /// <summary>Tema slug + mod alarak yükleme yapar, data-tema-mod attribute'unu da set eder.</summary>
    public async Task<bool> TemaYukleVeUygulaModluAsync(string slug, string mod, string firmaId = "varsayilan")
    {
        var sonuc = await TemaYukleVeUygulaAsync(slug, mod, firmaId);
        if (sonuc)
            await temaHub.Clients.Group(firmaId).SendAsync("TemaModDegisti", new { tema = slug, mod, firmaId });
        return sonuc;
    }

    /// <summary>Toplu olarak tüm placeholder temalar için DB kaydı ve CSS dosyaları üretir.</summary>
    public async Task<int> TopluTemaOlusturAsync(IReadOnlyList<string> slugListesi, CancellationToken iptal = default)
    {
        var eklenen = 0;
        var temalarKoku = Path.Combine(ortam.ContentRootPath, "wwwroot", "css", "temalar");

        foreach (var slug in slugListesi)
        {
            iptal.ThrowIfCancellationRequested();

            var manifestYolu = Path.Combine(temalarKoku, slug, "manifest.json");
            if (!File.Exists(manifestYolu)) continue;

            var mevcut = await db.TemaSablonlari
                .FirstOrDefaultAsync(t => t.Slug == slug && !t.SilindiMi, iptal);
            if (mevcut != null) continue;

            var manifestJson = await File.ReadAllTextAsync(manifestYolu, Encoding.UTF8, iptal);
            using var doc = JsonDocument.Parse(manifestJson);
            var root = doc.RootElement;

            var tema = new TemaSablonu
            {
                Kod = root.TryGetProperty("kod", out var k) ? k.GetString() ?? slug.ToUpperInvariant().Replace("-", "_") : slug.ToUpperInvariant().Replace("-", "_"),
                Ad = root.TryGetProperty("ad", out var ad) ? ad.GetString() ?? slug : slug,
                Slug = slug,
                Aciklama = root.TryGetProperty("aciklamaAnahtar", out var ack) ? ack.GetString() ?? "" : "",
                Kaynak = root.TryGetProperty("kaynak", out var ky) ? ky.GetString() ?? "elle" : "elle",
                StitchProjeId = root.TryGetProperty("stitchProjeId", out var sp) ? sp.GetString() : null,
                GlassmorphismAktif = root.TryGetProperty("glassmorphismAktif", out var ga) && ga.GetBoolean(),
                Premium = root.TryGetProperty("premium", out var pr) && pr.GetBoolean(),
                Fiyat = 0,
                ParaBirimi = "TRY",
                ThumbnailUrl = $"/css/temalar/{slug}/ekran-goruntusu.jpg",
                Kapsam = root.TryGetProperty("kapsam", out var kp) && Enum.TryParse<TemaKapsam>(kp.GetString(), out var kapsam) ? kapsam : TemaKapsam.Sadece_Site,
                AktifMi = root.TryGetProperty("aktif", out var ak) && ak.GetBoolean(),
                VarsayilanMi = root.TryGetProperty("varsayilanMi", out var vr) && vr.GetBoolean(),
                Etiketler = root.TryGetProperty("etiketler", out var et)
                    ? string.Join(",", et.EnumerateArray().Select(e => e.GetString() ?? ""))
                    : "",
                RenklerJson = root.TryGetProperty("renkler", out var rj) ? rj.GetRawText() : "{}",
                TipografiJson = root.TryGetProperty("tipografi", out var tj) ? tj.GetRawText() : "{}",
                GeometriJson = root.TryGetProperty("geometri", out var gj) ? gj.GetRawText() : "{}",
                GolgelerJson = root.TryGetProperty("golgeler", out var glj) ? glj.GetRawText() : "{}",
                GlassmorphismJson = root.TryGetProperty("glassmorphism", out var gmj) ? gmj.GetRawText() : "{}",
                AnimasyonJson = root.TryGetProperty("animasyon", out var aj) ? aj.GetRawText() : "{}",
                LayoutJson = root.TryGetProperty("layout", out var lj) ? lj.GetRawText() : "{}",
                IkonSeti = root.TryGetProperty("ikonSeti", out var ik) ? ik.GetString() ?? "Material Icons" : "Material Icons",
                AdAnahtar = $"tema.{slug}.ad",
                AciklamaAnahtar = $"tema.{slug}.aciklama",
                AdVarsayilanTr = root.TryGetProperty("adVarsayilanTr", out var advt) ? advt.GetString() ?? slug : slug,
                AdVarsayilanEn = root.TryGetProperty("adVarsayilanEn", out var adve) ? adve.GetString() ?? slug : slug,
                AciklamaVarsayilanTr = root.TryGetProperty("aciklamaVarsayilanTr", out var acvt) ? acvt.GetString() ?? "" : "",
                AciklamaVarsayilanEn = root.TryGetProperty("aciklamaVarsayilanEn", out var acve) ? acve.GetString() ?? "" : "",
                OlusturulmaTarihi = DateTime.UtcNow,
                Versiyon = 1
            };

            // Bilesenler.css ve animasyonlar.css üret
            var taslak = new TemaManifestTaslagi
            {
                Ad = tema.Ad,
                Slug = slug,
                Kod = tema.Kod,
                Kaynak = tema.Kaynak,
                Glassmorphism = new TemaGlassmorphismTaslagi { Aktif = tema.GlassmorphismAktif },
                Animasyon = new TemaAnimasyonTaslagi { HoverYukseklik = 4 },
                Layout = new TemaLayoutTaslagi { SutunSayisi = 4 }
            };

            var bilesenlerCss = BilesenlerCssUret(taslak);
            var animasyonlarCss = AnimasyonlarCssUret(taslak);

            var hedefKlasor = Path.Combine(temalarKoku, slug);
            await File.WriteAllTextAsync(Path.Combine(hedefKlasor, "bilesenler.css"), bilesenlerCss, new UTF8Encoding(true), iptal);
            await File.WriteAllTextAsync(Path.Combine(hedefKlasor, "animasyonlar.css"), animasyonlarCss, new UTF8Encoding(true), iptal);

            db.TemaSablonlari.Add(tema);
            eklenen++;
        }

        if (eklenen > 0)
        {
            await db.SaveChangesAsync(iptal);
            log.LogInformation("Toplu tema oluşturma tamamlandı: {Adet} yeni tema", eklenen);
        }

        return eklenen;
    }

    /// <summary>Yeni tema ekler.</summary>
    public async Task<TemaSablonu?> TemaEkleAsync(TemaSablonu tema)
    {
        if (await db.TemaSablonlari.AnyAsync(t => t.Slug == tema.Slug && !t.SilindiMi))
        {
            log.LogWarning("Bu slug ile zaten bir tema var: {Slug}", tema.Slug);
            return null;
        }

        db.TemaSablonlari.Add(tema);
        await db.SaveChangesAsync();
        log.LogInformation("Yeni tema eklendi: {Slug}", tema.Slug);
        return tema;
    }

    /// <summary>
    /// Slug'a göre tema varsa günceller, yoksa ekler (upsert).
    /// StitchTaslagiOnaylaAsync'in sadeleştirilmiş facade'ıdır; CSS dosyalarını yazar, DB kaydını yapar, hub yayını yapmaz.
    /// </summary>
    public async Task<Cevap<TemaSablonu>> KaydetVeyaGuncelleAsync(
        TemaManifestTaslagi taslak,
        bool dosyaYaz = true,
        CancellationToken iptal = default)
    {
        if (string.IsNullOrWhiteSpace(taslak.Slug))
            return Cevap<TemaSablonu>.Hata("Slug zorunludur.");

        if (dosyaYaz)
        {
            var manifestJson = ManifestJsonUret(taslak);
            var tokensCss = ManifestToTokensCss(manifestJson, taslak.Slug);
            var bilesenlerCss = BilesenlerCssUret(taslak);
            var animasyonlarCss = AnimasyonlarCssUret(taslak);
            await TemaDosyalariniAtomikYazAsync(taslak.Slug, manifestJson, tokensCss, bilesenlerCss, animasyonlarCss, iptal);
        }

        var mevcut = await db.TemaSablonlari
            .FirstOrDefaultAsync(t => t.Slug == taslak.Slug && !t.SilindiMi, iptal);

        var yeniMi = mevcut is null;
        var tema = mevcut ?? new TemaSablonu
        {
            Slug = taslak.Slug,
            OlusturulmaTarihi = DateTime.UtcNow
        };

        TemaSablonunuTaslaklaGuncelle(tema, taslak, yeniMi);
        if (yeniMi)
            db.TemaSablonlari.Add(tema);

        await db.SaveChangesAsync(iptal);

        log.LogInformation("Tema kaydedildi/güncellendi: {Slug}, yeni:{YeniMi}, v{Versiyon}", tema.Slug, yeniMi, tema.Versiyon);
        return Cevap<TemaSablonu>.Basarili(tema, yeniMi ? "Tema oluşturuldu." : "Tema güncellendi.");
    }

    /// <summary>Temayı günceller.</summary>
    public async Task<bool> TemaGuncelleAsync(TemaSablonu tema)
    {
        var mevcut = await db.TemaSablonlari.FindAsync(tema.Id);
        if (mevcut == null) return false;

        mevcut.Ad = tema.Ad;
        mevcut.Aciklama = tema.Aciklama;
        mevcut.GlassmorphismAktif = tema.GlassmorphismAktif;
        mevcut.Premium = tema.Premium;
        mevcut.Fiyat = tema.Fiyat;
        mevcut.Etiketler = tema.Etiketler;
        mevcut.RenklerJson = tema.RenklerJson;
        mevcut.TipografiJson = tema.TipografiJson;
        mevcut.GeometriJson = tema.GeometriJson;
        mevcut.GolgelerJson = tema.GolgelerJson;
        mevcut.GlassmorphismJson = tema.GlassmorphismJson;
        mevcut.AnimasyonJson = tema.AnimasyonJson;
        mevcut.LayoutJson = tema.LayoutJson;
        mevcut.IkonSeti = tema.IkonSeti;
        mevcut.AktifMi = tema.AktifMi;
        mevcut.VarsayilanMi = tema.VarsayilanMi;
        mevcut.GuncellenmeTarihi = DateTime.UtcNow;
        mevcut.Versiyon++;

        await db.SaveChangesAsync();
        log.LogInformation("Tema güncellendi: {Slug}, v{Versiyon}", tema.Slug, mevcut.Versiyon);
        return true;
    }

    /// <summary>Temayı soft delete ile siler.</summary>
    public async Task<bool> TemaSilAsync(int id)
    {
        var tema = await db.TemaSablonlari.FindAsync(id);
        if (tema == null) return false;

        tema.SilindiMi = true;
        await db.SaveChangesAsync();
        log.LogInformation("Tema silindi: {Slug}", tema.Slug);
        return true;
    }

    /// <summary>manifest.json'dan tokens.css üretir.</summary>
    public string ManifestToTokensCss(string manifestJson, string slug)
    {
        var css = new StringBuilder();
        css.AppendLine("/*");
        css.AppendLine($" * {slug} — tokens.css");
        css.AppendLine(" * CokluTemaServisi tarafından manifest.json'dan otomatik üretilir.");
        css.AppendLine(" * Manuel düzenleme ÖNERİLMEZ.");
        css.AppendLine(" */");
        css.AppendLine();
        css.AppendLine($":root[data-tema-id=\"{slug}\"] {{");

        try
        {
            using var doc = JsonDocument.Parse(manifestJson);
            var root = doc.RootElement;

            if (root.TryGetProperty("renkler", out var renkler))
            {
                css.AppendLine("    /* === RENK === */");
                RenkCssEkle(css, renkler, "birincil", "--tema-birincil");
                RenkCssEkle(css, renkler, "ikincil", "--tema-ikincil");
                RenkCssEkle(css, renkler, "vurgu", "--tema-vurgu");
                RenkCssEkle(css, renkler, "vurguAcik", "--tema-vurgu-acik");
                RenkCssEkle(css, renkler, "vurguKoyu", "--tema-vurgu-koyu");
                RenkCssEkle(css, renkler, "arkaPlan", "--tema-arkaplan");
                RenkCssEkle(css, renkler, "arkaPlan2", "--tema-arkaplan-2");
                RenkCssEkle(css, renkler, "yuzey", "--tema-yuzey");
                RenkCssEkle(css, renkler, "yuzeyHover", "--tema-yuzey-hover");
                RenkCssEkle(css, renkler, "cizgi", "--tema-cizgi");
                RenkCssEkle(css, renkler, "metin", "--tema-metin");
                RenkCssEkle(css, renkler, "metinIkincil", "--tema-metin-ikincil");
                RenkCssEkle(css, renkler, "metinSoluk", "--tema-metin-soluk");
                RenkCssEkle(css, renkler, "metinTers", "--tema-metin-ters");
                RenkCssEkle(css, renkler, "basari", "--tema-basari");
                RenkCssEkle(css, renkler, "uyari", "--tema-uyari");
                RenkCssEkle(css, renkler, "hata", "--tema-hata");
                RenkCssEkle(css, renkler, "bilgi", "--tema-bilgi");
            }

            if (root.TryGetProperty("tipografi", out var tipografi))
            {
                css.AppendLine();
                css.AppendLine("    /* === TİPOGRAFİ === */");
                FontCssEkle(css, tipografi, "baslikAilesi", "baslikFallback", "--tema-font-baslik");
                FontCssEkle(css, tipografi, "govdeAilesi", "govdeFallback", "--tema-font-govde");
                FontCssEkle(css, tipografi, "vurguAilesi", null, "--tema-font-vurgu");
                FontCssEkle(css, tipografi, "monoAilesi", null, "--tema-font-mono");
                DegerCssEkle(css, tipografi, "baslikAgirlik", "--tema-baslik-agirlik");
                DegerCssEkle(css, tipografi, "baslikHarfAraligi", "--tema-baslik-harf-araligi");
            }

            if (root.TryGetProperty("bosluklar", out var bosluklar))
            {
                css.AppendLine();
                css.AppendLine("    /* === BOŞLUK === */");
                DegerCssEkle(css, bosluklar, "xs", "--tema-bosluk-xs");
                DegerCssEkle(css, bosluklar, "sm", "--tema-bosluk-sm");
                DegerCssEkle(css, bosluklar, "md", "--tema-bosluk-md");
                DegerCssEkle(css, bosluklar, "lg", "--tema-bosluk-lg");
                DegerCssEkle(css, bosluklar, "xl", "--tema-bosluk-xl");
                DegerCssEkle(css, bosluklar, "ikiXl", "--tema-bosluk-2xl");
                DegerCssEkle(css, bosluklar, "ucXl", "--tema-bosluk-3xl");
            }

            if (root.TryGetProperty("geometri", out var geometri))
            {
                css.AppendLine();
                css.AppendLine("    /* === GEOMETRİ === */");
                DegerCssEkle(css, geometri, "koseSm", "--tema-kose-sm", "px");
                DegerCssEkle(css, geometri, "koseMd", "--tema-kose-md", "px");
                DegerCssEkle(css, geometri, "koseLg", "--tema-kose-lg", "px");
                DegerCssEkle(css, geometri, "koseXl", "--tema-kose-xl", "px");
                DegerCssEkle(css, geometri, "koseTam", "--tema-kose-tam", "px");
                DegerCssEkle(css, geometri, "borderKalinlik", "--tema-border-kalinlik", "px");
                DegerCssEkle(css, geometri, "borderStil", "--tema-border-stil");
            }

            if (root.TryGetProperty("golgeler", out var golgeler))
            {
                css.AppendLine();
                css.AppendLine("    /* === GÖLGE === */");
                DegerCssEkle(css, golgeler, "sm", "--tema-golge-sm");
                DegerCssEkle(css, golgeler, "md", "--tema-golge-md");
                DegerCssEkle(css, golgeler, "lg", "--tema-golge-lg");
                DegerCssEkle(css, golgeler, "xl", "--tema-golge-xl");
                DegerCssEkle(css, golgeler, "vurgu", "--tema-golge-vurgu");
            }

            if (root.TryGetProperty("glassmorphism", out var cam) && cam.TryGetProperty("aktif", out var camAktif) && camAktif.GetBoolean())
            {
                css.AppendLine();
                css.AppendLine("    /* === GLASSMORPHISM === */");
                var bgOpacity = cam.TryGetProperty("bgOpacity", out var bgO) ? bgO.GetDouble() : 0.06;
                var borderOpacity = cam.TryGetProperty("borderOpacity", out var bO) ? bO.GetDouble() : 0.10;
                css.AppendLine($"    --tema-cam-bg: rgba(255, 255, 255, {bgOpacity});");
                css.AppendLine($"    --tema-cam-cizgi: rgba(255, 255, 255, {borderOpacity});");
                DegerCssEkle(css, cam, "blur", "--tema-cam-blur", "", "blur(");
                css.AppendLine($"    --tema-cam-glow: 0 0 15px rgba(212, 175, 55, 0.25);");
            }

            if (root.TryGetProperty("animasyon", out var anim))
            {
                css.AppendLine();
                css.AppendLine("    /* === ANİMASYON === */");
                DegerCssEkle(css, anim, "gecisHizli", "--tema-gecis-hizli");
                DegerCssEkle(css, anim, "gecisNormal", "--tema-gecis-normal");
                DegerCssEkle(css, anim, "gecisYavas", "--tema-gecis-yavas");
                DegerCssEkle(css, anim, "cubicBezier", "--tema-bezier");
            }

            if (root.TryGetProperty("layout", out var layout))
            {
                css.AppendLine();
                css.AppendLine("    /* === LAYOUT === */");
                DegerCssEkle(css, layout, "icerikGenislik", "--tema-icerik-genislik", "px");
                DegerCssEkle(css, layout, "kenarBosluk", "--tema-kenar-bosluk", "px");
                DegerCssEkle(css, layout, "sutunSayisi", "--tema-sutun-sayisi");
            }

            // Legacy alias'lar
            css.AppendLine();
            css.AppendLine("    /* === LEGACY UYUMLULUK === */");
            css.AppendLine("    --vizit-primary: var(--tema-birincil);");
            css.AppendLine("    --vizit-accent: var(--tema-vurgu);");
            css.AppendLine("    --vizit-bg-base: var(--tema-arkaplan);");
            css.AppendLine("    --vizit-text: var(--tema-metin);");
            css.AppendLine("    --vizit-text-inverse: var(--tema-metin-ters);");
            css.AppendLine("    --vizit-font-serif: var(--tema-font-baslik);");
            css.AppendLine("    --vizit-font-sans: var(--tema-font-govde);");
            css.AppendLine("    --vizit-radius-md: var(--tema-kose-md);");
            css.AppendLine("    --vizit-transition: var(--tema-gecis-normal);");
            css.AppendLine("    --vizit-shadow-md: var(--tema-golge-md);");
            css.AppendLine("    --vizit-shadow-lux: var(--tema-golge-vurgu);");
            css.AppendLine("    --vizit-border: var(--tema-cizgi);");
            css.AppendLine("    --aureli-glass-bg: var(--tema-cam-bg);");
            css.AppendLine("    --aureli-glass-border: var(--tema-cam-cizgi);");
            css.AppendLine("    --aureli-glow: var(--tema-cam-glow);");
            css.AppendLine("    --aureli-blur: 20px;");
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "Manifest parse hatası: {Slug}", slug);
        }

        css.AppendLine("}");
        return css.ToString();
    }

    private static void RenkCssEkle(StringBuilder css, JsonElement parent, string jsonKey, string cssVar)
    {
        if (parent.TryGetProperty(jsonKey, out var val))
            css.AppendLine($"    {cssVar}: {val.GetString()};");
    }

    private static void FontCssEkle(StringBuilder css, JsonElement parent, string aileKey, string? fallbackKey, string cssVar)
    {
        if (!parent.TryGetProperty(aileKey, out var aile)) return;
        var aileDeger = aile.GetString() ?? "sans-serif";
        var fallback = fallbackKey != null && parent.TryGetProperty(fallbackKey, out var fb) ? fb.GetString() : "system-ui, sans-serif";
        css.AppendLine($"    {cssVar}: '{aileDeger}', {fallback};");
    }

    private static void DegerCssEkle(StringBuilder css, JsonElement parent, string jsonKey, string cssVar, string birim = "", string sarmalayici = "")
    {
        if (!parent.TryGetProperty(jsonKey, out var val)) return;
        var str = val.ValueKind == JsonValueKind.Number ? val.GetRawText() : val.GetString() ?? "";
        if (!string.IsNullOrEmpty(sarmalayici))
            css.AppendLine($"    {cssVar}: {sarmalayici}{str});");
        else
            css.AppendLine($"    {cssVar}: {str}{birim};");
    }

    // Eski Design.md tabanlı dönüşüm (geriye uyumluluk için korunuyor)
    public string DesignMdToTokensCss(string icerik)
    {
        string tokensJson;
        if (icerik.TrimStart().StartsWith("{"))
            tokensJson = icerik;
        else
            tokensJson = FrontMatterCikar(icerik) ?? "";

        if (string.IsNullOrEmpty(tokensJson)) return VarsayilanCss();

        var css = new StringBuilder();
        css.AppendLine("/* CokluTemaServisi tarafından otomatik üretildi (legacy). */");
        css.AppendLine(":root {");

        try
        {
            using var doc = JsonDocument.Parse(tokensJson);
            if (doc.RootElement.TryGetProperty("tokens", out var tokens))
            {
                CssRenkUret(css, tokens);
                CssTipografiUret(css, tokens);
            }
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "Token parse hatası, varsayılan CSS kullanılıyor.");
            return VarsayilanCss();
        }

        css.AppendLine("}");
        return css.ToString();
    }

    private static void CssRenkUret(StringBuilder css, JsonElement tokens)
    {
        if (!tokens.TryGetProperty("color", out var c)) return;
        css.AppendLine("    /* Renk paleti */");
        RenkEkle(css, "--vizit-primary", c, "primary");
        RenkEkle(css, "--vizit-accent", c, "accent");
        RenkEkle(css, "--vizit-bg-base", c, "bg-base");
        RenkEkle(css, "--vizit-text", c, "text");
        RenkEkle(css, "--vizit-text-inverse", c, "text-inverse");
    }

    private static void CssTipografiUret(StringBuilder css, JsonElement tokens)
    {
        if (!tokens.TryGetProperty("typography", out var t)) return;
        css.AppendLine("    /* Tipografi */");
        FontEkle(css, "--vizit-font-serif", t, "heading");
        FontEkle(css, "--vizit-font-sans", t, "body");
    }

    private static void RenkEkle(StringBuilder css, string degisken, JsonElement parent, string key)
    {
        if (parent.TryGetProperty(key, out var t) && t.TryGetProperty("value", out var v))
            css.AppendLine($"    {degisken}: {v.GetString()};");
    }

    private static void FontEkle(StringBuilder css, string degisken, JsonElement parent, string key)
    {
        if (parent.TryGetProperty(key, out var t) && t.TryGetProperty("value", out var v))
            css.AppendLine($"    {degisken}: '{v.GetString()}', serif;");
    }

    private static string? FrontMatterCikar(string icerik)
    {
        var match = Regex.Match(icerik, @"^---\s*\n(.*?)\n---", RegexOptions.Singleline);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static string VarsayilanCss() =>
        ":root {\n    --vizit-primary: #0a0a0a;\n    --vizit-accent: #c19b76;\n    --vizit-bg-base: #ffffff;\n    --vizit-text: #1A1A1A;\n}";
}

/// <summary>Admin paneli için tema özet DTO.</summary>
public record TemaOzetDto(string Slug, string Ad, string Aciklama, bool GlassmorphismAktif, bool Premium, string Etiketler, string? ThumbnailUrl);
