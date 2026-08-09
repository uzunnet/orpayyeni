using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using VizitLink3D.Ortak.Modeller.Renkler;
using VizitLink3D.Ortak.Modeller.Urunler;
using VizitLink3D.Ortak.Yardimcilar;
using VizitLink3D.UI.Models;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages;

public partial class UrunDetay : ComponentBase, IDisposable
{
    [Parameter] public string Slug { get; set; } = string.Empty;

    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private FirmaBilgisiServisi FirmaBilgisi { get; set; } = default!;

    protected override void OnInitialized()
    {
        dil.DilDegisti += DilDegistiginde;
    }

    private void DilDegistiginde() => InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        dil.DilDegisti -= DilDegistiginde;
    }

    private Urun? _urun;
    private List<Urun> BenzerUrunler { get; set; } = [];
    private List<Urun> EnCokGezilenler { get; set; } = [];
    private List<string> GaleriGorselleri { get; set; } = [];
    private int _aktifGorselIndex;
    private List<UrunUcBoyutModeli> Modeller { get; set; } = [];
    private List<RalRengi> Renkler { get; set; } = [];
    private List<UrunMedya> Medyalar { get; set; } = [];
    private string KoleksiyonAdi { get; set; } = string.Empty;
    private string KoleksiyonAciklama { get; set; } = string.Empty;
    private string KategoriAdi { get; set; } = string.Empty;
    private string HeroGorselUrl { get; set; } = "/medya/vizitlink3d_default.png";
    private string? HataMesaji { get; set; }
    private bool _yukleniyor = true;
    private OrpayKatalogUrunu? _katalogVerisi;

    private string NormalizeUrl(string url, string firmaSlug)
    {
        if (firmaSlug == "localhost" || firmaSlug == "127.0.0.1" || firmaSlug == "platform") firmaSlug = "orpay";
        if (string.IsNullOrWhiteSpace(url)) return url;
        string tmp = url;
        if (tmp.StartsWith(Api.ApiBaseUrl, StringComparison.OrdinalIgnoreCase))
            tmp = tmp.Substring(Api.ApiBaseUrl.Length);
            
        if (tmp.StartsWith("/medya/", StringComparison.OrdinalIgnoreCase))
            return $"{Api.ApiBaseUrl}/firmalar/{firmaSlug}{tmp}";
            
        if (tmp.StartsWith("medya/", StringComparison.OrdinalIgnoreCase))
            return $"{Api.ApiBaseUrl}/firmalar/{firmaSlug}/{tmp}";
            
        return url;
    }

    private List<string> KatalogOzellikleri => _katalogVerisi?.Ozellikler.ToList() ?? [];
    private List<OrpayKatalogOlcusu> KatalogOlculer => _katalogVerisi?.Olculer.ToList() ?? [];

    // ─── LIGHTBOX (galeri buyutme) ─────────────────────────────────────
    private bool _lightboxAcik;
    private int _lightboxIndex;

    private void LightboxAc(int index)
    {
        if (index < 0 || index >= GaleriGorselleri.Count) return;
        _lightboxIndex = index;
        _lightboxAcik = true;
    }

    private void GorselSec(int index)
    {
        if (index < 0 || index >= GaleriGorselleri.Count) return;
        _aktifGorselIndex = index;
        HeroGorselUrl = GaleriGorselleri[index];
    }

    private void LightboxKapat() => _lightboxAcik = false;

    private void LightboxOnceki()
    {
        if (GaleriGorselleri.Count == 0) return;
        _lightboxIndex = (_lightboxIndex - 1 + GaleriGorselleri.Count) % GaleriGorselleri.Count;
    }

    private void LightboxSonraki()
    {
        if (GaleriGorselleri.Count == 0) return;
        _lightboxIndex = (_lightboxIndex + 1) % GaleriGorselleri.Count;
    }

    private List<(string Ad, string Hex)> RenkSwatchListesi =>
        _katalogVerisi is not null && _katalogVerisi.Renkler.Length > 0
            ? _katalogVerisi.Renkler.Select(r => (Ad: r, Hex: orpayKatalogRenkPaleti.HexBul(r))).ToList()
            : Renkler.Select(r => (Ad: r.Ad, Hex: r.HexKod ?? "#B0A99A")).ToList();

    /// <summary>
    /// Admin panelinden urune ozel yuklenen teknik cizim varsa onu kullan;
    /// yoksa (eski) statik katalog gorseline dus.
    /// NOT: Medya havuzu yollari relative (<c>/api/medya/dosya/96</c>) gelir,
    ///      Blazor WASM tarayicida API portuna gidebilmesi icin
    ///      <c>Api.ApiBaseUrl</c> ile absolute URL'e cevrilir.
    /// </summary>
    private string? TeknikCizimUrl
    {
        get
        {
            var teknikCizim = Medyalar.FirstOrDefault(m =>
                m.MedyaTuru.Equals("TeknikCizim", StringComparison.OrdinalIgnoreCase));

            if (teknikCizim?.MedyaUrl is { } url && !string.IsNullOrWhiteSpace(url))
            {
                // Medya havuzu yoluysa (/api/medya/dosya/...) absolute URL'e cevir
                if (MedyaHavuzuYolu.HavuzDosyaYoluMu(url))
                    return MedyaHavuzuYolu.TamUrl(url, Api.ApiBaseUrl);

                // Dogrudan dosya yoluysa (/medya/orpay-katalog/...) direkt don
                return url;
            }

            // Fallback: eski statik katalog gorseli (bos / null ise gosterme)
            var fallback = _katalogVerisi?.TeknikGorselUrl;
            return string.IsNullOrWhiteSpace(fallback) ? null : fallback;
        }
    }

    private static readonly Dictionary<string, string> OzellikIkonEslesmesi = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Soft Kapak"] = "/medya/brand/ozellik-ikonlar/soft-kapak.svg",
        ["MDF Ahşap"] = "/medya/brand/ozellik-ikonlar/mdf.svg",
        ["Dokunmatik Ledli Ayna"] = "/medya/brand/ozellik-ikonlar/dokunmatik.svg",
        ["Kolay Montaj"] = "/medya/brand/ozellik-ikonlar/kolay-montaj.svg",
        ["Kolay Temizlenir"] = "/medya/brand/ozellik-ikonlar/kolay-temizlik.svg",
        ["Renk Seçenekleri"] = "/medya/brand/ozellik-ikonlar/renk.svg",
        ["Stone Lavabo"] = "/medya/brand/ozellik-ikonlar/stone-lavabo.svg",
        ["Cam Lavabo"] = "/medya/brand/ozellik-ikonlar/stone-lavabo.svg",
    };

    private static string? OzellikIkonuBul(string ozellik) =>
        OzellikIkonEslesmesi.TryGetValue(ozellik, out var ikon) ? ikon : null;

    protected override async Task OnParametersSetAsync()
    {
        try
        {
            _yukleniyor = true;
            HataMesaji = null;
            _urun = null;
            _katalogVerisi = null;
            BenzerUrunler = [];
            EnCokGezilenler = [];
            GaleriGorselleri = [];
            _aktifGorselIndex = 0;
            Modeller = [];
            Renkler = [];
            Medyalar = [];
            KoleksiyonAdi = string.Empty;
            KoleksiyonAciklama = string.Empty;
            KategoriAdi = string.Empty;
            HeroGorselUrl = "/medya/vizitlink3d_default.png";

            if (string.IsNullOrWhiteSpace(Slug))
                return;

            var firmaSlug = await FirmaBilgisi.GetSlugAsync();

            var urun = await Api.GetAsync<Urun>($"api/urunler/slug/{Uri.EscapeDataString(Slug)}?dil=tr");
            if (urun is null)
                return;

            _urun = urun;
            _katalogVerisi = UrunGorunumYardimcisi.KatalogVerisiBul(urun);

            var aileler = (await Api.GetAsync<List<UrunAilesi>>("api/urun-ailesi") ?? [])
                .Where(x => x.AktifMi && !x.SilindiMi)
                .ToDictionary(x => x.Id);
            var kategoriler = (await Api.GetAsync<List<UrunKategori>>("api/urun-kategorileri") ?? [])
                .Where(x => x.AktifMi && !x.SilindiMi)
                .ToDictionary(x => x.Id);

            Modeller = (await Api.GetAsync<List<UrunUcBoyutModeli>>($"api/urunler/{urun.Id}/uc-boyut-modelleri") ?? [])
                .Where(x => x.AktifMi && !x.SilindiMi)
                .ToList();
            Renkler = (await Api.GetAsync<List<RalRengi>>($"api/urunler/{urun.Id}/renkler") ?? [])
                .Where(x => x.AktifMi)
                .ToList();
            Medyalar = (await Api.GetAsync<List<UrunMedya>>($"api/urunler/{urun.Id}/medyalar") ?? [])
                .OrderBy(x => x.SiraNo)
                .ToList();
            BenzerUrunler = (await Api.GetAsync<List<Urun>>($"api/urunler/{urun.Id}/benzer?adet=6&dil=tr") ?? [])
                .Where(x => x.AktifMi && !x.SilindiMi && x.Id != urun.Id)
                .Take(3)
                .ToList();
            EnCokGezilenler = (await Api.GetAsync<List<Urun>>("api/urunler/en-cok-gezilen?adet=6&dil=tr") ?? [])
                .Where(x => x.AktifMi && !x.SilindiMi && x.Id != urun.Id)
                .Take(3)
                .ToList();

            KoleksiyonAdi = UrunGorunumYardimcisi.KoleksiyonAdiBul(urun, aileler);
            KategoriAdi = UrunGorunumYardimcisi.KategoriAdiBul(urun, kategoriler);
            KoleksiyonAciklama = aileler.TryGetValue(urun.UrunAilesiId, out var aile) && !string.IsNullOrWhiteSpace(aile.Aciklama)
                ? aile.Aciklama!
                : kategoriler.TryGetValue(urun.UrunKategoriId ?? -1, out var kategori) && !string.IsNullOrWhiteSpace(kategori.Aciklama)
                    ? kategori.Aciklama!
                    : UrunGorunumYardimcisi.OzetMetniBul(urun);

            HeroGorselUrl = NormalizeUrl(UrunGorunumYardimcisi.AnaGorselUrl(urun, Api.ApiBaseUrl), firmaSlug);

            // Yönetilen ürün görsellerinin tek kaynağı medya havuzudur. Eski statik
            // katalog ve harici WordPress yolları galeriyi artık geçersiz kılamaz.
            var hamGaleri = MedyaHavuzuYolu.UrunGalerisiOlustur(urun, Medyalar, Api.ApiBaseUrl);
            GaleriGorselleri = hamGaleri.Select(x => NormalizeUrl(x, firmaSlug)).ToList();

            if (GaleriGorselleri.Count == 0)
                GaleriGorselleri = [HeroGorselUrl];
            else
                HeroGorselUrl = GaleriGorselleri[0];
        }
        catch (Exception ex)
        {
            HataMesaji = ex.Message;
            Console.Error.WriteLine($"[UrunDetay] {ex}");
        }
        finally
        {
            _yukleniyor = false;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Icerik API'den async geldigi icin ilk render'da .reveal-element / .scale-in
        // elemanlari henuz DOM'da olmuyor — her render'da yeni eklenen
        // (henuz .visible olmayan) elemanlari gozlemciye kaydediyoruz.
        try
        {
            await JS.InvokeVoidAsync("eval", @"
                (function () {
                    if (!window.__revealObserver) {
                        window.__revealObserver = new IntersectionObserver((entries) => {
                            entries.forEach(entry => {
                                if (entry.isIntersecting) {
                                    entry.target.classList.add('visible');
                                    window.__revealObserver.unobserve(entry.target);
                                }
                            });
                        }, { threshold: 0.1, rootMargin: '0px 0px -40px 0px' });
                    }
                    document.querySelectorAll('.reveal-element:not(.visible), .scale-in:not(.visible)').forEach(el => window.__revealObserver.observe(el));
                })();
            ");
        }
        catch { }
    }

    private string AnaOzeti =>
        _urun is null
            ? string.Empty
            : UrunGorunumYardimcisi.OzetMetniBul(_urun);

    private string ModelOzetMetni =>
        Modeller.Count > 0
            ? $"{Modeller.Count} 3D model"
            : "3D model bilgisi yok";

    private string RenkOzetMetni =>
        Renkler.Count > 0
            ? $"{Renkler.Count} renk seçeneği"
            : "Renk bilgisi yok";

    private string MedyaOzetMetni =>
        Medyalar.Count > 0
            ? $"{Medyalar.Count} medya"
            : "Medya bilgisi yok";

    private string ModelBasligi(UrunUcBoyutModeli model) =>
        string.IsNullOrWhiteSpace(model.ModelAdi) ? "Model" : model.ModelAdi;
}
