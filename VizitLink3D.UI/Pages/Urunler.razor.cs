using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using VizitLink3D.Ortak.Modeller.Urunler;
using VizitLink3D.UI.Models;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages;

public partial class Urunler : ComponentBase, IDisposable
{
    private sealed record KoleksiyonGrubu(string Ad, string Anchor, string Aciklama, List<Urun> Urunler);

    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private List<Urun> _urunler = [];
    private Dictionary<int, UrunAilesi> _aileler = [];
    private Dictionary<int, UrunKategori> _kategoriler = [];
    private bool _yukleniyor = true;
    private string? HataMesaji;
    private string? _seciliKoleksiyonSlug;
    private string? _kancaHedefi;

    protected override async Task OnInitializedAsync()
    {
        dil.DilDegisti += DilDegistiginde;

        try
        {
            _yukleniyor = true;
            HataMesaji = null;
            await Task.WhenAll(UrunleriYukleAsync(), AileleriYukleAsync(), KategorileriYukleAsync());

            var parca = new Uri(Nav.Uri).Fragment;
            if (!string.IsNullOrWhiteSpace(parca))
            {
                var hedefSlug = parca.TrimStart('#').Trim().ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(hedefSlug) && KoleksiyonAdlari.Any(ad => SlugaCevir(ad) == hedefSlug))
                {
                    _seciliKoleksiyonSlug = hedefSlug;
                    _kancaHedefi = hedefSlug;
                }
            }
        }
        catch (Exception ex)
        {
            HataMesaji = ex.Message;
            Console.Error.WriteLine($"[Urunler] {ex}");
        }
        finally
        {
            _yukleniyor = false;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Not: firstRender=true, OnInitializedAsync henuz tamamlanmadan (yukleniyor
        // ekrani icin) da tetiklenebiliyor; bu yuzden kanca kontrolu firstRender'a
        // degil, sadece _kancaHedefi'nin dolu olmasina bagli (bir kez tetiklenip null'lanir).
        if (_kancaHedefi is not null)
        {
            var hedef = _kancaHedefi;
            _kancaHedefi = null;
            await JS.InvokeVoidAsync("vizitlink3dKancayaKaydir", hedef);
        }
    }

    private void DilDegistiginde() => InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        dil.DilDegisti -= DilDegistiginde;
    }

    private async Task UrunleriYukleAsync()
    {
        _urunler = await Api.GetAsync<List<Urun>>("api/urunler?dil=tr") ?? [];
    }

    private async Task AileleriYukleAsync()
    {
        var aileler = await Api.GetAsync<List<UrunAilesi>>("api/urun-ailesi") ?? [];
        _aileler = aileler
            .Where(x => x.AktifMi && !x.SilindiMi)
            .OrderBy(x => x.SiraNo)
            .ThenBy(x => x.Ad)
            .ToDictionary(x => x.Id);
    }

    private async Task KategorileriYukleAsync()
    {
        var kategoriler = await Api.GetAsync<List<UrunKategori>>("api/urun-kategorileri") ?? [];
        _kategoriler = kategoriler
            .Where(x => x.AktifMi && !x.SilindiMi)
            .OrderBy(x => x.SiraNo)
            .ThenBy(x => x.Ad)
            .ToDictionary(x => x.Id);
    }

    private IReadOnlyList<Urun> TumUrunler => _urunler
        .Where(x => x.AktifMi && !x.SilindiMi)
        .OrderByDescending(x => x.OneCikanMi)
        .ThenBy(x => x.SiraNo)
        .ThenBy(x => x.Ad)
        .ToList();

    private IReadOnlyList<Urun> VitrinUrunleri =>
        TumUrunler.Where(x => x.OneCikanMi).Take(3).Concat(TumUrunler.Take(3)).DistinctBy(x => x.Id).Take(3).ToList();

    private int ToplamUrun => TumUrunler.Count;
    private int OneCikanAdedi => TumUrunler.Count(x => x.OneCikanMi);

    /// <summary>
    /// Görüntüleme koleksiyon anahtarı: statik ORPAY katalogunda varsa
    /// gerçek koleksiyon grubu (Exclusive/Premium/Trend/Standart), yoksa ürün ailesi adı.
    /// </summary>
    private string KoleksiyonAnahtari(Urun urun) =>
        UrunGorunumYardimcisi.KatalogVerisiBul(urun)?.KoleksiyonGrubu
            ?? (_aileler.TryGetValue(urun.UrunAilesiId, out var aile) ? aile.Ad : "Diğer");

    private static string SlugaCevir(string metin) =>
        metin.Trim().ToLowerInvariant()
            .Replace(" ", "-").Replace("ı", "i").Replace("ş", "s")
            .Replace("ğ", "g").Replace("ü", "u").Replace("ö", "o").Replace("ç", "c");

    private IEnumerable<string> KoleksiyonAdlari =>
        TumUrunler.Select(KoleksiyonAnahtari).Distinct()
            .OrderBy(OrpayKatalogUrunleri.KoleksiyonSirasiBul)
            .ThenBy(ad => ad);

    private IEnumerable<KoleksiyonGrubu> Gruplar =>
        TumUrunler
            .Where(x => string.IsNullOrWhiteSpace(_seciliKoleksiyonSlug) || SlugaCevir(KoleksiyonAnahtari(x)) == _seciliKoleksiyonSlug)
            .GroupBy(KoleksiyonAnahtari)
            .OrderBy(g => OrpayKatalogUrunleri.KoleksiyonSirasiBul(g.Key))
            .ThenBy(g => g.Key)
            .Select(g =>
            {
                var aciklama = OrpayKatalogUrunleri.KoleksiyonAciklamasiBul(g.Key);
                if (string.IsNullOrWhiteSpace(aciklama))
                    aciklama = "Admin panelinden eklenen ürünler bu grupta listelenir.";

                return new KoleksiyonGrubu(
                    g.Key,
                    SlugaCevir(g.Key),
                    aciklama,
                    g.OrderBy(x => x.SiraNo).ThenBy(x => x.Ad).ToList());
            });

    private void KoleksiyonSec(string? koleksiyonSlug)
    {
        _seciliKoleksiyonSlug = _seciliKoleksiyonSlug == koleksiyonSlug ? null : koleksiyonSlug;
    }

    private string KoleksiyonSinifi(string? koleksiyonSlug) =>
        _seciliKoleksiyonSlug == koleksiyonSlug ? "gb-chip gb-chip--active" : "gb-chip";

    private string KoleksiyonAdi(Urun urun) =>
        UrunGorunumYardimcisi.KoleksiyonAdiBul(urun, _aileler);

    private string KategoriAdi(Urun urun) =>
        UrunGorunumYardimcisi.KategoriAdiBul(urun, _kategoriler);

    private string GorselUrl(Urun urun) =>
        UrunGorunumYardimcisi.AnaGorselUrl(urun, Api.ApiBaseUrl);

    private static string OzetMetni(Urun urun) =>
        UrunGorunumYardimcisi.OzetMetniBul(urun);
}
