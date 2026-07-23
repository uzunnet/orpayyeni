using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;
using VizitLink3D.Ortak.Modeller.Medya;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using VizitLink3D.UI.Servisler;
using VizitLink3D.UI.Models;

namespace VizitLink3D.UI.Pages.Admin;

public partial class UrunYonetimi : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogServisi { get; set; } = default!;

    private List<Urun> _liste = [];
    private List<Urun> _filtreliListe = [];
    private List<UrunAilesi> _aileler = [];
    private List<UrunKategori> _kategoriler = [];
    private List<UrunUcBoyutModeli> _ucBoyutModeller = [];
    private bool _yukleniyor = true;
    private bool _formAcik;
    private bool _duzenlemeModu;
    private bool _kaydediliyor;
    private Urun _form = new();
    private int? _duzenlenenId;
    private string _arama = string.Empty;
    private int? _secilenKategoriId;
    private int _sutunAdet = 4;
    private int _satirAdet = 3;
    private int _sayfaBasinaAdet = 12;
    private bool _sayfalamaAktif = true;
    private int _aktifSayfa = 1;
    protected override async Task OnInitializedAsync() => await Yukle();

    private async Task Yukle()
    {
        _yukleniyor = true;
        StateHasChanged();
        await Task.WhenAll(UrunleriYukleAsync(), ReferansVerileriYukleAsync());
        _aktifSayfa = 1;
        _yukleniyor = false;
    }

    private async Task UrunleriYukleAsync()
    {
        var sorgu = new List<string>();
        if (_secilenKategoriId is int kategoriId && kategoriId > 0)
        {
            sorgu.Add($"kategoriId={kategoriId}");
        }

        if (!string.IsNullOrWhiteSpace(_arama))
        {
            sorgu.Add($"arama={Uri.EscapeDataString(_arama)}");
        }

        var url = sorgu.Count > 0
            ? $"api/urunler?{string.Join("&", sorgu)}"
            : "api/urunler";

        _liste = await Api.GetAsync<List<Urun>>(url) ?? [];
        _filtreliListe = _liste;
    }

    private async Task ReferansVerileriYukleAsync()
    {
        var aileler = await Api.GetAsync<List<UrunAilesi>>("api/urun-ailesi");
        if (aileler != null) _aileler = aileler;
        var kategoriler = await Api.GetAsync<List<UrunKategori>>("api/urun-kategorileri");
        if (kategoriler != null) _kategoriler = kategoriler;
        var modeller = await Api.GetAsync<List<UrunUcBoyutModeli>>("api/uc-boyut/modeller");
        if (modeller != null) _ucBoyutModeller = modeller;
        var duzen = await Api.GetAsync<SayfaDuzenAyariDto>("api/sayfa-duzen-ayarlari/urun-havuzu");
        if (duzen != null)
        {
            _sutunAdet = duzen.SutunAdet > 0 ? duzen.SutunAdet : 4;
            _satirAdet = duzen.SatirAdet > 0 ? duzen.SatirAdet : 3;
            _sayfaBasinaAdet = duzen.SayfaBasinaAdet > 0 ? duzen.SayfaBasinaAdet : _sutunAdet * _satirAdet;
            _sayfalamaAktif = duzen.SayfalamaAktif;
        }
    }

    private string UcBoyutTooltip(Urun urun)
    {
        if (urun.VarsayilanUcBoyutModeliId is not int modelId) return "";
        var model = _ucBoyutModeller.FirstOrDefault(m => m.Id == modelId);
        if (model == null) return $"Model #{modelId}";
        var boyut = model.DosyaBoyutuByte > 0 ? $" · {model.DosyaBoyutuByte / 1_048_576.0:F1} MB" : "";
        return $"{model.ModelAdi} ({model.ModelTipi}){boyut}";
    }

    private string GorselUrl(Urun urun)
    {
        if (urun.AnaGorselMedyaId is long medyaId and > 0)
            return $"{Api.ApiBaseUrl}/api/medya/dosya/{medyaId}";
        return "/medya/vizitlink3d_default.png";
    }

    // Urune varsayilan 3D model atanmis mi.
    private static bool UcBoyutModeliVarMi(Urun urun) => urun.VarsayilanUcBoyutModeliId is not null;

    private async Task AramaYap()
    {
        await UrunleriYukleAsync();
        StateHasChanged();
    }

    private async Task KategoriDegistir(int? kategoriId)
    {
        _secilenKategoriId = kategoriId;
        _aktifSayfa = 1;
        await UrunleriYukleAsync();
        StateHasChanged();
    }

    private async Task FiltreleriTemizle()
    {
        _arama = string.Empty;
        _secilenKategoriId = null;
        _aktifSayfa = 1;
        await UrunleriYukleAsync();
        StateHasChanged();
    }

    private void YeniAc()
    {
        _form = new Urun { AktifMi = true, UrunKategoriId = _secilenKategoriId };
        _duzenlenenId = null;
        _duzenlemeModu = false;
        _formAcik = true;
    }

    private void Duzenle(Urun u)
    {
        _form = new Urun
        {
            Id = u.Id,
            Slug = u.Slug,
            Kod = u.Kod,
            Ad = u.Ad,
            KisaAciklama = u.KisaAciklama,
            Aciklama = u.Aciklama,
            UrunAilesiId = u.UrunAilesiId,
            UrunKategoriId = u.UrunKategoriId,
            AnaGorselMedyaId = u.AnaGorselMedyaId,
            VarsayilanUcBoyutModeliId = u.VarsayilanUcBoyutModeliId,
            Fiyat = u.Fiyat,
            Birim = u.Birim,
            OneCikanMi = u.OneCikanMi,
            YeniMi = u.YeniMi,
            SiraNo = u.SiraNo,
            SeoBaslik = u.SeoBaslik,
            SeoAciklama = u.SeoAciklama,
            AktifMi = u.AktifMi
        };
        _duzenlenenId = u.Id;
        _duzenlemeModu = true;
        _formAcik = true;
    }

    private async Task SilOnay(Urun u)
    {
        var onay = await DialogServisi.ShowMessageBoxAsync(
            dil.T("ortak.silmeOnayi", "Silme Onayı"),
            dil.T("admin.urun.silOnay", "Bu ürün pasife alınacak ve yayından kaldırılacak. Emin misiniz?"),
            yesText: dil.T("ortak.sil", "Sil"),
            cancelText: dil.T("ortak.iptal", "İptal"));
        if (onay == true) await Sil(u);
    }

    private async Task Sil(Urun u)
    {
        var cevap = await Api.DeleteAsync($"api/urunler/{u.Id}");
        Snackbar.Add(
            cevap?.BasariliMi == true
                ? dil.T("ortak.kayitSilindi", "Kayıt silindi.")
                : cevap?.Mesaj ?? dil.T("ortak.islemBasarisiz", "İşlem tamamlanamadı."),
            cevap?.BasariliMi == true ? Severity.Success : Severity.Error);
        await Yukle();
    }

    private async Task Kaydet()
    {
        if (string.IsNullOrWhiteSpace(_form.Ad) || string.IsNullOrWhiteSpace(_form.Kod))
        {
            Snackbar.Add(dil.T("admin.urun.adKodZorunlu", "Ürün adı ve kodu zorunludur."), Severity.Warning);
            return;
        }

        _kaydediliyor = true;
        Cevap<Urun>? cevap;

        if (_duzenlenenId.HasValue)
            cevap = await Api.PutAsync<Urun>($"api/urunler/{_duzenlenenId.Value}", _form);
        else
            cevap = await Api.PostAsync<Urun>("api/urunler", _form);

        _kaydediliyor = false;

        if (cevap?.BasariliMi != true)
        {
            Snackbar.Add(cevap?.Mesaj ?? dil.T("ortak.kayitBasarisiz", "Kayıt işlemi tamamlanamadı."), Severity.Error);
            return;
        }

        _formAcik = false;
        Snackbar.Add(_duzenlenenId.HasValue ? dil.T("ortak.kayitGuncellendi", "Kayıt güncellendi.") : dil.T("ortak.kayitEklendi", "Yeni kayıt eklendi."), Severity.Success);
        await Yukle();
    }

    private void FormIptal() { _formAcik = false; }

    private async Task MedyaHavuzundanSec()
    {
        var p = new DialogParameters { { "CokluSecim", false }, { "BaslangicTipi", MedyaTipi.Resim } };
        var opts = new DialogOptions { MaxWidth = MaxWidth.ExtraLarge, FullWidth = true, CloseButton = true };
        var d = await DialogServisi.ShowAsync<VizitLink3D.UI.Bilesenler.Medya.MedyaSecici>(dil.T("admin.medya.sec", "Medya Seç"), p, opts);
        var r = await d.Result;
        if (r is { Canceled: false, Data: List<long> ids } && ids.Any())
        {
            _form.AnaGorselMedyaId = ids[0];
            StateHasChanged();
            Snackbar.Add(dil.T("admin.urun.anaGorselSecildi", "Ana görsel seçildi."), Severity.Success);
        }
    }

    private string KategoriAdi(int? kategoriId)
    {
        if (kategoriId is null || kategoriId <= 0)
            return dil.T("ortak.bos", "Yok");

        return _kategoriler.FirstOrDefault(k => k.Id == kategoriId)?.Ad
               ?? string.Format(dil.T("admin.urun.kategoriId", "Kategori #{0}"), kategoriId);
    }

    private string AileAdi(int aileId)
    {
        return _aileler.FirstOrDefault(a => a.Id == aileId)?.Ad
               ?? string.Format(dil.T("admin.urun.aileId", "Aile #{0}"), aileId);
    }

    private IEnumerable<Urun> GosterilecekUrunler()
    {
        if (_sayfalamaAktif)
        {
            return _filtreliListe
                .Skip((_aktifSayfa - 1) * _sayfaBasinaAdet)
                .Take(_sayfaBasinaAdet);
        }

        return _filtreliListe;
    }

    private int ToplamSayfa => Math.Max(1, (int)Math.Ceiling((double)_filtreliListe.Count / Math.Max(1, _sayfaBasinaAdet)));

    private int GridMd => _sutunAdet switch
    {
        <= 2 => 6,
        3 => 4,
        4 => 3,
        5 => 3,
        _ => 2
    };

    private int GridLg => _sutunAdet switch
    {
        <= 2 => 6,
        3 => 4,
        4 => 3,
        5 => 2,
        _ => 2
    };

    private void SayfaDegistir(int sayfa)
    {
        _aktifSayfa = Math.Clamp(sayfa, 1, ToplamSayfa);
    }
}
