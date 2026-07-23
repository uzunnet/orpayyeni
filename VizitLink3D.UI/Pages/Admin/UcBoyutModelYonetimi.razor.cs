using System.Net.Http.Headers;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages.Admin;

public partial class UcBoyutModelYonetimi : ComponentBase
{
    private const long MAKSIMUM_MODEL_DOSYA_BOYUTU = 30 * 1024 * 1024;

    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogServisi { get; set; } = default!;

    private List<UrunUcBoyutModeli> _liste = [];
    private List<UrunUcBoyutModeli> _filtreliListe = [];
    private List<Urun> _urunler = [];
    private bool _yukleniyor = true, _formAcik, _duzenlemeModu, _kaydediliyor;
    private UrunUcBoyutModeli _form = new();
    private int? _duzenlenenId;
    private string _arama = string.Empty;
    private IBrowserFile? _secilenDosya;
    private int? _ozetUrunId;

    protected override async Task OnInitializedAsync() => await Yukle();

    private async Task Yukle()
    {
        _yukleniyor = true;
        StateHasChanged();
        _liste = await Api.GetAsync<List<UrunUcBoyutModeli>>("api/uc-boyut/modeller") ?? [];
        _urunler = await Api.GetAsync<List<Urun>>("api/urunler") ?? [];
        AramaUygula();
        _yukleniyor = false;
    }

    private void AramaYap() => AramaUygula();

    private void AramaUygula()
    {
        var a = _arama?.ToLowerInvariant() ?? string.Empty;
        _filtreliListe = string.IsNullOrWhiteSpace(a) ? _liste :
            _liste.Where(x => x.UrunId.ToString().Contains(a) ||
                              (x.ModelAdi.ToLowerInvariant().Contains(a)) ||
                              (x.ModelYolu?.ToLowerInvariant().Contains(a) ?? false) ||
                              (x.ModelTipi?.ToLowerInvariant().Contains(a) ?? false) ||
                              (UrunAdiGetir(x.UrunId).ToLowerInvariant().Contains(a))).ToList();
    }

    private string UrunAdiGetir(int urunId)
    {
        var urun = _urunler.FirstOrDefault(u => u.Id == urunId);
        return urun != null ? $"{urun.Ad} ({urun.Kod})" : $"{DilServisi.T("admin.ortak.urun", "Ürün")} #{urunId}";
    }

    private string UrunResimUrlGetir(int urunId)
    {
        var urun = _urunler.FirstOrDefault(u => u.Id == urunId);
        if (urun == null) return "/medya/vizitlink3d_default.png";
        if (urun.AnaGorselMedyaId is long medyaId and > 0)
            return $"{Api.ApiBaseUrl}/api/medya/dosya/{medyaId}";
        var numStr = new string(urun.Kod.Where(char.IsDigit).ToArray());
        return string.IsNullOrWhiteSpace(numStr)
            ? "/medya/vizitlink3d_default.png"
            : "";
    }

    private string ModelOnizlemeUrlGetir(UrunUcBoyutModeli model)
    {
        if (model.OnizlemeMedyaId is long onizlemeId and > 0)
            return $"{Api.ApiBaseUrl}/api/medya/dosya/{onizlemeId}";
        if (!string.IsNullOrWhiteSpace(model.ModelYolu))
            return $"{Api.ApiBaseUrl}/{model.ModelYolu.TrimStart('/')}";
        return string.Empty;
    }

    private Urun? UrunGetir(int urunId) => _urunler.FirstOrDefault(u => u.Id == urunId);

    private void OzetGoster(int urunId)
    {
        _ozetUrunId = _ozetUrunId == urunId ? null : urunId;
    }

    private void YeniAc()
    {
        _form = new UrunUcBoyutModeli { ModelTipi = "Glb", Versiyon = 1, AktifMi = true, VarsayilanMi = true };
        _duzenlenenId = null;
        _duzenlemeModu = false;
        _secilenDosya = null;
        _formAcik = true;
    }

    private void Duzenle(UrunUcBoyutModeli x)
    {
        _form = new UrunUcBoyutModeli
        {
            Id = x.Id,
            UrunId = x.UrunId,
            MedyaId = x.MedyaId,
            ModelDosyaYolu = x.ModelDosyaYolu,
            ModelYolu = x.ModelYolu,
            OnizlemeMedyaId = x.OnizlemeMedyaId,
            ModelTipi = x.ModelTipi,
            DosyaBoyutuByte = x.DosyaBoyutuByte,
            Versiyon = x.Versiyon,
            VarsayilanMi = x.VarsayilanMi,
            KameraAyarJson = x.KameraAyarJson,
            IsikAyarJson = x.IsikAyarJson,
            CevreAyarJson = x.CevreAyarJson,
            ModelAnalizJson = x.ModelAnalizJson,
            AktifMi = x.AktifMi,
            OlusturulmaTarihi = x.OlusturulmaTarihi
        };
        _duzenlenenId = x.Id;
        _duzenlemeModu = true;
        _formAcik = true;
    }

    private async Task SilOnay(UrunUcBoyutModeli x)
    {
        var onay = await DialogServisi.ShowMessageBoxAsync(
            DilServisi.T("ortak.silmeOnayi", "Silme Onayı"),
            DilServisi.T("admin.3d.modelSilOnay", "Bu 3D model pasife alınacak ve yayından kaldırılacak. Emin misiniz?"),
            yesText: DilServisi.T("ortak.sil", "Sil"),
            cancelText: DilServisi.T("ortak.iptal", "İptal"));
        if (onay == true) await Sil(x);
    }

    private async Task Sil(UrunUcBoyutModeli x)
    {
        var cevap = await Api.DeleteAsync($"api/uc-boyut/modeller/{x.Id}");
        Snackbar.Add(
            cevap?.BasariliMi == true
                ? DilServisi.T("ortak.kayitSilindi", "Kayıt silindi.")
                : cevap?.Mesaj ?? DilServisi.T("ortak.islemBasarisiz", "İşlem tamamlanamadı."),
            cevap?.BasariliMi == true ? Severity.Success : Severity.Error);
        await Yukle();
    }

    private async Task Kaydet()
    {
        if (_form.UrunId <= 0)
        {
            Snackbar.Add(DilServisi.T("admin.3d.urunZorunlu", "Ürün seçimi zorunludur."), Severity.Warning);
            return;
        }

        _kaydediliyor = true;
        Cevap<UrunUcBoyutModeli>? cevap;

        if (_duzenlenenId.HasValue)
            cevap = await Api.PutAsync<UrunUcBoyutModeli>($"api/uc-boyut/modeller/{_duzenlenenId.Value}", _form);
        else if (_secilenDosya != null)
            cevap = await ModelDosyasiYukleAsync();
        else
            cevap = await Api.PostAsync<UrunUcBoyutModeli>("api/uc-boyut/modeller", _form);

        _kaydediliyor = false;

        if (cevap?.BasariliMi != true)
        {
            Snackbar.Add(cevap?.Mesaj ?? DilServisi.T("ortak.kayitBasarisiz", "Kayıt işlemi tamamlanamadı."), Severity.Error);
            return;
        }

        _formAcik = false;
        _secilenDosya = null;
        Snackbar.Add(_duzenlenenId.HasValue ? DilServisi.T("ortak.kayitGuncellendi", "Kayıt güncellendi.") : DilServisi.T("ortak.kayitEklendi", "Yeni kayıt eklendi."), Severity.Success);
        await Yukle();
    }

    private async Task<Cevap<UrunUcBoyutModeli>?> ModelDosyasiYukleAsync()
    {
        if (_secilenDosya is null)
            return null;

        using var icerik = new MultipartFormDataContent();
        var dosyaIcerigi = new StreamContent(_secilenDosya.OpenReadStream(MAKSIMUM_MODEL_DOSYA_BOYUTU));
        dosyaIcerigi.Headers.ContentType = new MediaTypeHeaderValue(_secilenDosya.ContentType);
        icerik.Add(dosyaIcerigi, "dosya", _secilenDosya.Name);
        icerik.Add(new StringContent(_form.UrunId.ToString()), "urunId");
        icerik.Add(new StringContent(_form.ModelAdi ?? _secilenDosya.Name), "modelAdi");
        icerik.Add(new StringContent(_form.ModelTipi ?? "Glb"), "modelTipi");
        return await Api.PostMultipartAsync<UrunUcBoyutModeli>("api/uc-boyut/modeller/yukle", icerik);
    }

    private void FormIptal()
    {
        _formAcik = false;
        _secilenDosya = null;
    }

    private void DosyaSecildi(IBrowserFile dosya)
    {
        _secilenDosya = dosya;
        _form.ModelAdi = string.IsNullOrWhiteSpace(_form.ModelAdi) ? dosya.Name : _form.ModelAdi;
        _form.ModelDosyaYolu = string.Empty;
        _form.ModelYolu = string.Empty;
        _form.DosyaBoyutuByte = dosya.Size;
        _form.ModelTipi = "Glb";
    }
}
