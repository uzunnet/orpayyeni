using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages.Admin;

public partial class PdfKatalogYonetimi : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogServisi { get; set; } = default!;

    private List<UrunPdfKaynagi> _liste = [];
    private List<UrunPdfKaynagi> _filtreliListe = [];
    private List<PdfSayfaGorseli> _sayfalar = [];
    private bool _yukleniyor = true;
    private bool _formAcik;
    private bool _duzenlemeModu;
    private bool _kaydediliyor;
    private bool _sayfalarYukleniyor;
    private UrunPdfKaynagi _form = new();
    private int? _duzenlenenId;
    private int? _sayfaGosterilenId;
    private string _arama = string.Empty;
    private string _aciklama = string.Empty;
    private string _medyaIdMetni = "0";

    protected override async Task OnInitializedAsync() => await Yukle();

    private async Task Yukle()
    {
        _yukleniyor = true;
        StateHasChanged();
        _liste = await Api.GetAsync<List<UrunPdfKaynagi>>("api/pdf-katalog") ?? [];
        AramaUygula();
        _yukleniyor = false;
    }

    private void AramaYap(KeyboardEventArgs e) => AramaUygula();

    private void AramaUygula()
    {
        var a = _arama?.ToLower() ?? "";
        _filtreliListe = string.IsNullOrWhiteSpace(a)
            ? _liste
            : _liste.Where(x =>
                (x.Ad?.ToLower().Contains(a) ?? false) ||
                (x.CozumlemeDurumu?.ToLower().Contains(a) ?? false)).ToList();
    }

    private void YeniAc()
    {
        _form = new UrunPdfKaynagi();
        _aciklama = string.Empty;
        _medyaIdMetni = "0";
        _duzenlenenId = null;
        _duzenlemeModu = false;
        _formAcik = true;
    }

    private void Duzenle(UrunPdfKaynagi k)
    {
        _form = new UrunPdfKaynagi
        {
            Id = k.Id,
            Ad = k.Ad,
            MedyaId = k.MedyaId,
            SayfaSayisi = k.SayfaSayisi,
            CozumlemeDurumu = k.CozumlemeDurumu,
            HataMesaji = k.HataMesaji,
            OlusturulmaTarihi = k.OlusturulmaTarihi,
            CozumlemeTarihi = k.CozumlemeTarihi
        };
        _aciklama = string.Empty;
        _medyaIdMetni = k.MedyaId.ToString();
        _duzenlenenId = k.Id;
        _duzenlemeModu = true;
        _formAcik = true;
    }

    private async Task SilOnay(UrunPdfKaynagi k)
    {
        var onay = await DialogServisi.ShowMessageBoxAsync(
            "Silme Onayi",
            $"'{k.Ad}' kaydi kalici olarak silinecektir. Emin misiniz?",
            yesText: "Evet, Sil",
            cancelText: "İptal");
        if (onay == true) await Sil(k);
    }

    private async Task Sil(UrunPdfKaynagi k)
    {
        await Api.DeleteAsync($"api/pdf-katalog/{k.Id}");
        Snackbar.Add("Kayit basariyla silindi.", Severity.Success);
        await Yukle();
    }

    private async Task Kaydet()
    {
        _kaydediliyor = true;
        try
        {
            if (!long.TryParse(_medyaIdMetni, out var medyaId))
            {
                Snackbar.Add("Gecerli bir Medya ID giriniz.", Severity.Error);
                return;
            }
            _form.MedyaId = medyaId;

            var govde = new
            {
                _form.Ad,
                _form.MedyaId,
                Aciklama = _aciklama
            };

            if (_duzenlenenId.HasValue)
            {
                var putGovde = new
                {
                    _form.Ad,
                    _form.MedyaId,
                    Aciklama = _aciklama,
                    _form.SayfaSayisi,
                    _form.CozumlemeDurumu
                };
                await Api.PutAsync<UrunPdfKaynagi>($"api/pdf-katalog/{_duzenlenenId.Value}", putGovde);
            }
            else
            {
                await Api.PostAsync<UrunPdfKaynagi>("api/pdf-katalog/yukle", govde);
            }

            _formAcik = false;
            Snackbar.Add(_duzenlenenId.HasValue ? "Kayit guncellendi." : "Yeni kayit eklendi.", Severity.Success);
            await Yukle();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Hata: {ex.Message}", Severity.Error);
        }
        finally
        {
            _kaydediliyor = false;
        }
    }

    private void FormIptal()
    {
        _formAcik = false;
    }

    private async Task Cozumle(UrunPdfKaynagi k)
    {
        try
        {
            Snackbar.Add($"'{k.Ad}' icin cozumleme baslatiliyor...", Severity.Info);
            var cevap = await Api.PostAsync<object>($"api/pdf-katalog/{k.Id}/cozumle", new { });
            if (cevap?.BasariliMi == true)
                Snackbar.Add(cevap.Mesaj, Severity.Success);
            else
                Snackbar.Add(cevap?.Mesaj ?? "Cozumleme baslatilamadi.", Severity.Warning);
            await Yukle();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Cozumleme hatasi: {ex.Message}", Severity.Error);
        }
    }

    private async Task SayfalariGoster(UrunPdfKaynagi k)
    {
        if (_sayfaGosterilenId == k.Id)
        {
            SayfalariKapat();
            return;
        }

        _sayfaGosterilenId = k.Id;
        _sayfalarYukleniyor = true;
        StateHasChanged();

        _sayfalar = await Api.GetAsync<List<PdfSayfaGorseli>>($"api/pdf-katalog/{k.Id}/sayfalar") ?? [];
        _sayfalarYukleniyor = false;
    }

    private void SayfalariKapat()
    {
        _sayfaGosterilenId = null;
        _sayfalar = [];
    }

    private string SayfaMedyaUrl(long medyaId)
    {
        return $"{Api.ApiBaseUrl}/api/medya-havuzu/dosya/{medyaId}";
    }

    private static Color CozumlemeRengi(string? durum) => durum switch
    {
        "Isleniyor" => Color.Info,
        "Tamamlandi" => Color.Success,
        "Hata" => Color.Error,
        _ => Color.Default
    };
}
