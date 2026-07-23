using VizitLink3D.Ortak.Modeller;
using VizitLink3D.UI.Servisler;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace VizitLink3D.UI.Pages.Admin;

public partial class SertifikaYonetimi : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogServisi { get; set; } = default!;
    [Inject] private DilServisi DilServisi { get; set; } = default!;

    private const long MaksimumDosyaBoyutu = 50_000_000;

    private List<Sertifika> _liste = [];
    private List<Sertifika> _filtreliListe = [];
    private bool _yukleniyor = true;
    private bool _formAcik;
    private bool _duzenlemeModu;
    private bool _kaydediliyor;
    private bool _dosyaYukleniyor;
    private Sertifika _form = new();
    private int? _duzenlenenId;
    private string _arama = string.Empty;

    protected override async Task OnInitializedAsync() => await Yukle();

    private async Task Yukle()
    {
        _yukleniyor = true;
        StateHasChanged();
        _liste = await Api.GetAsync<List<Sertifika>>("api/sertifikalar/yonetim") ?? [];
        AramaUygula();
        _yukleniyor = false;
    }

    private void AramaMetniDegisti(string deger)
    {
        _arama = deger;
        AramaUygula();
    }

    private void AramaUygula()
    {
        var aranan = _arama?.ToLowerInvariant() ?? string.Empty;
        _filtreliListe = string.IsNullOrWhiteSpace(aranan)
            ? _liste
            : _liste.Where(x =>
                x.Ad.ToLowerInvariant().Contains(aranan) ||
                (x.Aciklama?.ToLowerInvariant().Contains(aranan) ?? false) ||
                (x.VerenKurum?.ToLowerInvariant().Contains(aranan) ?? false)).ToList();
    }

    private void YeniAc()
    {
        _form = new Sertifika { AktifMi = true, SiraNo = _liste.Count + 1 };
        _duzenlenenId = null;
        _duzenlemeModu = false;
        _formAcik = true;
    }

    private void Duzenle(Sertifika sertifika)
    {
        _form = new Sertifika
        {
            Id = sertifika.Id,
            Ad = sertifika.Ad,
            Aciklama = sertifika.Aciklama,
            Resim = sertifika.Resim,
            PdfDosya = sertifika.PdfDosya,
            VerilmeTarihi = sertifika.VerilmeTarihi,
            GecerlilikTarihi = sertifika.GecerlilikTarihi,
            VerenKurum = sertifika.VerenKurum,
            SiraNo = sertifika.SiraNo,
            AktifMi = sertifika.AktifMi
        };
        _duzenlenenId = sertifika.Id;
        _duzenlemeModu = true;
        _formAcik = true;
    }

    private async Task DosyaYukle(IBrowserFile dosya)
    {
        if (dosya is null)
        {
            return;
        }

        _dosyaYukleniyor = true;
        using var icerik = new MultipartFormDataContent();
        using var dosyaAkisi = dosya.OpenReadStream(MaksimumDosyaBoyutu);
        using var dosyaIcerigi = new StreamContent(dosyaAkisi);
        icerik.Add(dosyaIcerigi, "dosya", dosya.Name);

        var cevap = await Api.PostMultipartAsync<SertifikaDosyaYuklemeSonucu>("api/sertifikalar/dosya-yukle", icerik);
        if (cevap?.BasariliMi == true && cevap.Veri is not null)
        {
            if (cevap.Veri.DosyaTuru == "Pdf")
            {
                _form.PdfDosya = cevap.Veri.Yol;
                _form.Resim = cevap.Veri.OnizlemeYolu;
            }
            else
            {
                _form.Resim = cevap.Veri.Yol;
            }

            Snackbar.Add(DilServisi.T("admin.sertifika.dosyaYuklendi", "Dosya yuklendi. Kaydet butonuna basarak sertifikayi yayinlayabilirsiniz."), Severity.Success);
        }
        else
        {
            Snackbar.Add(cevap?.Mesaj ?? DilServisi.T("admin.sertifika.dosyaYuklemeHatasi", "Dosya yuklenemedi."), Severity.Error);
        }

        _dosyaYukleniyor = false;
    }

    private async Task SilOnay(Sertifika sertifika)
    {
        var onay = await DialogServisi.ShowMessageBoxAsync(
            DilServisi.T("ortak.silmeOnayi", "Silme Onayi"),
            string.Format(DilServisi.T("admin.sertifika.silmeMesaji", "'{0}' kaydi pasife alinacaktir. Emin misiniz?"), sertifika.Ad),
            yesText: DilServisi.T("ortak.evetSil", "Evet, Sil"),
            cancelText: DilServisi.T("ortak.iptal", "İptal"));
        if (onay == true)
        {
            await Sil(sertifika);
        }
    }

    private async Task Sil(Sertifika sertifika)
    {
        var cevap = await Api.DeleteAsync($"api/sertifikalar/{sertifika.Id}");
        if (cevap?.BasariliMi == true)
        {
            Snackbar.Add(cevap.Mesaj, Severity.Success);
            await Yukle();
        }
        else
        {
            Snackbar.Add(cevap?.Mesaj ?? DilServisi.T("admin.sertifika.silinemedi", "Sertifika silinemedi."), Severity.Error);
        }
    }

    private async Task Kaydet()
    {
        if (string.IsNullOrWhiteSpace(_form.Ad))
        {
            Snackbar.Add(DilServisi.T("admin.sertifika.adZorunlu", "Sertifika adi zorunludur."), Severity.Warning);
            return;
        }

        _kaydediliyor = true;
        Cevap<Sertifika>? cevap;
        if (_duzenlenenId.HasValue)
        {
            cevap = await Api.PutAsync<Sertifika>($"api/sertifikalar/{_duzenlenenId.Value}", _form);
        }
        else
        {
            cevap = await Api.PostAsync<Sertifika>("api/sertifikalar", _form);
        }

        if (cevap?.BasariliMi == true)
        {
            _formAcik = false;
            Snackbar.Add(cevap.Mesaj, Severity.Success);
            await Yukle();
        }
        else
        {
            Snackbar.Add(cevap?.Mesaj ?? DilServisi.T("admin.sertifika.kaydedilemedi", "Sertifika kaydedilemedi."), Severity.Error);
        }

        _kaydediliyor = false;
    }

    private void FormIptal()
    {
        _formAcik = false;
    }

    private string TamUrl(string? yol)
    {
        if (string.IsNullOrWhiteSpace(yol))
        {
            return string.Empty;
        }

        if (yol.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return yol;
        }

        return $"{Api.ApiBaseUrl}{(yol.StartsWith('/') ? yol : "/" + yol)}";
    }

    private static string? BelgeYolu(Sertifika sertifika)
    {
        if (!string.IsNullOrWhiteSpace(sertifika.PdfDosya))
        {
            return sertifika.PdfDosya;
        }

        return sertifika.Resim;
    }

    private static string DosyaTipi(Sertifika sertifika)
    {
        var yol = BelgeYolu(sertifika);
        if (string.IsNullOrWhiteSpace(yol))
        {
            return "-";
        }

        var uzanti = Path.GetExtension(yol).TrimStart('.').ToUpperInvariant();
        return string.IsNullOrWhiteSpace(uzanti) ? "URL" : uzanti;
    }

    private static string PdfGostericiUrl(Sertifika sertifika)
    {
        var belgeYolu = BelgeYolu(sertifika);
        if (string.IsNullOrWhiteSpace(belgeYolu))
        {
            return "/admin/sertifika-yonetimi";
        }

        return $"/pdf-gosterici?dosya={Uri.EscapeDataString(belgeYolu)}&baslik={Uri.EscapeDataString(sertifika.Ad)}&donus={Uri.EscapeDataString("/admin/sertifika-yonetimi")}";
    }

    private sealed record SertifikaDosyaYuklemeSonucu(string Yol, string DosyaTuru, string? OnizlemeYolu, long BoyutByte);
}

