using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages.Admin;

public partial class UcBoyutParcaEsleme : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogServisi { get; set; } = default!;

    private List<UrunUcBoyutModeli> _modeller = [];
    private List<UrunUcBoyutParcasi> _parcalar = [];
    private List<UrunUcBoyutParcasi> _filtreliListe = [];
    private bool _yukleniyor = true, _formAcik, _duzenlemeModu, _kaydediliyor;
    private UrunUcBoyutParcasi _form = new();
    private int? _duzenlenenId;
    private int? _seciliModelId;

    protected override async Task OnInitializedAsync() => await ModelleriYukle();

    private async Task ModelleriYukle()
    {
        _yukleniyor = true;
        StateHasChanged();
        _modeller = await Api.GetAsync<List<UrunUcBoyutModeli>>("api/uc-boyut/modeller") ?? [];
        _yukleniyor = false;
    }

    private async Task OnModelSecildi(int? modelId)
    {
        _seciliModelId = modelId;
        _formAcik = false;
        if (modelId.HasValue)
            await ParcalariYukle();
        else
        {
            _parcalar = [];
            _filtreliListe = [];
        }
    }

    private async Task ParcalariYukle()
    {
        if (!_seciliModelId.HasValue) return;
        _yukleniyor = true;
        StateHasChanged();
        _parcalar = await Api.GetAsync<List<UrunUcBoyutParcasi>>($"api/uc-boyut/modeller/{_seciliModelId.Value}/parcalar") ?? [];
        _filtreliListe = _parcalar;
        _yukleniyor = false;
    }

    private void YeniAc()
    {
        _form = new UrunUcBoyutParcasi
        {
            UrunUcBoyutModeliId = _seciliModelId ?? 0,
            HareketTipi = "Yok",
            SecilebilirMi = true,
            RenklenebilirMi = true,
            MalzemeDegisebilirMi = true,
            AktifMi = true
        };
        _duzenlenenId = null;
        _duzenlemeModu = false;
        _formAcik = true;
    }

    private void Duzenle(UrunUcBoyutParcasi x)
    {
        _form = new UrunUcBoyutParcasi
        {
            Id = x.Id,
            UrunUcBoyutModeliId = x.UrunUcBoyutModeliId,
            MeshAdi = x.MeshAdi,
            GorunenAd = x.GorunenAd,
            ParcaGrubuId = x.ParcaGrubuId,
            SecilebilirMi = x.SecilebilirMi,
            RenklenebilirMi = x.RenklenebilirMi,
            MalzemeDegisebilirMi = x.MalzemeDegisebilirMi,
            GizlenebilirMi = x.GizlenebilirMi,
            HareketliMi = x.HareketliMi,
            HareketTipi = x.HareketTipi,
            MinDeger = x.MinDeger,
            MaxDeger = x.MaxDeger,
            VarsayilanDeger = x.VarsayilanDeger,
            VarsayilanRenkId = x.VarsayilanRenkId,
            VarsayilanMalzemeId = x.VarsayilanMalzemeId,
            SiraNo = x.SiraNo,
            AktifMi = x.AktifMi,
            AdminOnayliMi = x.AdminOnayliMi,
            ParcaTipi = x.ParcaTipi,
            MalzemeTipiKisiti = x.MalzemeTipiKisiti,
            OlusturulmaTarihi = x.OlusturulmaTarihi
        };
        _duzenlenenId = x.Id;
        _duzenlemeModu = true;
        _formAcik = true;
    }

    private async Task SilOnay(UrunUcBoyutParcasi x)
    {
        var onay = await DialogServisi.ShowMessageBoxAsync(
            DilServisi.T("ortak.silmeOnayi", "Silme Onayı"),
            DilServisi.T("admin.3d.parcaSilOnay", "Bu parça pasife alınacak ve kullanıcı arayüzünden kaldırılacak. Emin misiniz?"),
            yesText: DilServisi.T("ortak.sil", "Sil"),
            cancelText: DilServisi.T("ortak.iptal", "İptal"));
        if (onay == true) await Sil(x);
    }

    private async Task Sil(UrunUcBoyutParcasi x)
    {
        var cevap = await Api.DeleteAsync($"api/uc-boyut/modeller/parcalar/{x.Id}");
        Snackbar.Add(
            cevap?.BasariliMi == true
                ? DilServisi.T("ortak.kayitSilindi", "Kayıt silindi.")
                : cevap?.Mesaj ?? DilServisi.T("ortak.islemBasarisiz", "İşlem tamamlanamadı."),
            cevap?.BasariliMi == true ? Severity.Success : Severity.Error);
        await ParcalariYukle();
    }

    private async Task Kaydet()
    {
        if (!_seciliModelId.HasValue)
        {
            Snackbar.Add(DilServisi.T("admin.3d.modelZorunlu", "Önce model seçiniz."), Severity.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(_form.GorunenAd))
        {
            Snackbar.Add(DilServisi.T("admin.3d.parcaAdZorunlu", "Parça adı zorunludur."), Severity.Warning);
            return;
        }

        _kaydediliyor = true;
        Cevap<UrunUcBoyutParcasi>? cevap;

        if (_duzenlenenId.HasValue)
            cevap = await Api.PutAsync<UrunUcBoyutParcasi>($"api/uc-boyut/modeller/parcalar/{_duzenlenenId.Value}", _form);
        else
            cevap = await Api.PostAsync<UrunUcBoyutParcasi>($"api/uc-boyut/modeller/{_seciliModelId}/parcalar", _form);

        _kaydediliyor = false;

        if (cevap?.BasariliMi != true)
        {
            Snackbar.Add(cevap?.Mesaj ?? DilServisi.T("ortak.kayitBasarisiz", "Kayıt işlemi tamamlanamadı."), Severity.Error);
            return;
        }

        _formAcik = false;
        Snackbar.Add(_duzenlenenId.HasValue ? DilServisi.T("ortak.kayitGuncellendi", "Kayıt güncellendi.") : DilServisi.T("ortak.kayitEklendi", "Yeni kayıt eklendi."), Severity.Success);
        await ParcalariYukle();
    }

    private void FormIptal() { _formAcik = false; }
}
