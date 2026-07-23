using VizitLink3D.Ortak.Modeller.Medya;
using VizitLink3D.UI.Servisler;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;

namespace VizitLink3D.UI.Pages.Admin;

public class DosyaTaramaItem
{
    public string Ad { get; set; } = "";
    public string Yol { get; set; } = "";
    public long BoyutByte { get; set; }
    public string Tip { get; set; } = "";
    public DateTime OlusturmaTarihi { get; set; }
    public string Klasor { get; set; } = "";
}

public partial class MedyaHavuzu : ComponentBase
{
    private const long MAKS_BOYUT = 100_000_000;

    // ─── Sekme ───────────────────────────────────────────────────────────────
    private int _aktifSekme = 0; // 0=Tüm Dosyalar, 1=DB Medya

    // ─── Tüm Dosyalar (wwwroot fiziksel tarama) ──────────────────────────────
    private List<DosyaTaramaItem> _tumDosyalar = [];
    private List<string> _dizinler = [];
    private string _secilenDizin = ".";
    private string _dosyaArama = "";
    private string _tipFiltre = "Tumu";
    private bool _dosyaYukleniyor = true;
    private DosyaTaramaItem? _onizlemeDosya;
    private readonly HashSet<string> _seciliYollar = [];

    // ─── DB Medya (mevcut sistem) ─────────────────────────────────────────────
    private List<Medya> _medyaListesi = [];
    private List<Medya> _seciliMedyalar = [];
    private List<MedyaKlasoru> _klasorler = [];
    private string _arama = "";
    private int? _seciliKlasorId;
    private bool _yukleniyor = true;

    // ─── Dialog ───────────────────────────────────────────────────────────────
    private bool _yuklemeDialog;
    private bool _klasorDialog;
    private bool _duzenlemeDialog;
    private bool _kaydediliyor;
    private int _yuklemeYuzdesi;
    private string _yeniKlasorAdi = "";
    private Medya _duzenlenenMedya = new();

    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(TumDosyalariYukleAsync(), DbMedyaYukleAsync());
    }

    // ──────────────────────────────────────────────────────────────────────────
    // SEKME 1: TÜM DOSYALAR
    // ──────────────────────────────────────────────────────────────────────────

    private async Task TumDosyalariYukleAsync()
    {
        _dosyaYukleniyor = true;
        try
        {
            var dizinCevap = await api.GetAsync<List<string>>("api/medya/wwwroot-dizinler");
            _dizinler = dizinCevap ?? [];
            await DosyalariFiltreleAsync();
        }
        catch { }
        finally { _dosyaYukleniyor = false; }
    }

    private async Task DosyalariFiltreleAsync()
    {
        _dosyaYukleniyor = true;
        var d = _secilenDizin == "(Kök)" ? "." : _secilenDizin;
        var url = $"api/medya/wwwroot-tara?dizin={Uri.EscapeDataString(d)}&q={Uri.EscapeDataString(_dosyaArama)}&tip={Uri.EscapeDataString(_tipFiltre)}";
        var sonuclar = await api.GetAsync<List<DosyaTaramaItem>>(url);
        _tumDosyalar = sonuclar ?? [];
        _seciliYollar.Clear();
        _dosyaYukleniyor = false;
        StateHasChanged();
    }

    private async Task DizinSec(string dizin)
    {
        _secilenDizin = dizin;
        await DosyalariFiltreleAsync();
    }

    private async Task TipFiltreDegistir(string tip)
    {
        _tipFiltre = tip;
        await DosyalariFiltreleAsync();
    }

    private async Task DosyaAramaDegistir(string deger)
    {
        _dosyaArama = deger;
        await DosyalariFiltreleAsync();
    }

    private void DosyaTikla(DosyaTaramaItem dosya)
    {
        if (_seciliYollar.Contains(dosya.Yol)) _seciliYollar.Remove(dosya.Yol);
        else _seciliYollar.Add(dosya.Yol);
    }

    private bool DosyaSeciliMi(DosyaTaramaItem d) => _seciliYollar.Contains(d.Yol);

    private void OnizlemeAc(DosyaTaramaItem dosya) => _onizlemeDosya = dosya;
    private void OnizlemeKapat() => _onizlemeDosya = null;

    private async Task DosyaSil(DosyaTaramaItem dosya)
    {
        var onay = await dialogService.ShowMessageBoxAsync(
            "Dosya Sil",
            $"'{dosya.Ad}' fiziksel olarak silinecek. Bu işlem geri alınamaz. Emin misiniz?",
            yesText: "Sil", cancelText: "İptal");
        if (onay != true) return;

        var cevap = await api.DeleteAsync($"api/medya/wwwroot-sil?yol={Uri.EscapeDataString(dosya.Yol)}");
        if (cevap?.BasariliMi == true)
        {
            snackbar.Add("Dosya silindi.", Severity.Success);
            _onizlemeDosya = null;
            await DosyalariFiltreleAsync();
        }
        else snackbar.Add("Silme başarısız.", Severity.Error);
    }

    private async Task SeciliDosyalariSil()
    {
        if (_seciliYollar.Count == 0) return;
        var onay = await dialogService.ShowMessageBoxAsync(
            "Toplu Silme",
            $"{_seciliYollar.Count} dosya fiziksel olarak silinecek. Bu işlem geri alınamaz. Emin misiniz?",
            yesText: "Sil", cancelText: "İptal");
        if (onay != true) return;

        var hatalar = 0;
        foreach (var yol in _seciliYollar.ToList())
        {
            var c = await api.DeleteAsync($"api/medya/wwwroot-sil?yol={Uri.EscapeDataString(yol)}");
            if (c?.BasariliMi != true) hatalar++;
        }
        snackbar.Add(hatalar == 0 ? "Seçili dosyalar silindi." : $"{hatalar} dosya silinemedi.",
            hatalar == 0 ? Severity.Success : Severity.Warning);
        await DosyalariFiltreleAsync();
    }

    private async Task UrlKopyala(string yol)
    {
        var tamUrl = $"{api.ApiBaseUrl}{yol}";
        await js.InvokeVoidAsync("navigator.clipboard.writeText", tamUrl);
        snackbar.Add("URL kopyalandı.", Severity.Info);
    }

    private string DosyaOnizlemeSrc(DosyaTaramaItem d) => $"{api.ApiBaseUrl}{d.Yol}";

    private static string TipIkonu(string tip) => tip switch
    {
        "Resim" => Icons.Material.Filled.Image,
        "PDF"   => Icons.Material.Filled.PictureAsPdf,
        "3D"    => Icons.Material.Filled.ViewInAr,
        "Video" => Icons.Material.Filled.VideoFile,
        _       => Icons.Material.Filled.InsertDriveFile
    };

    private static string TipRengi(string tip) => tip switch
    {
        "Resim" => "#1976D2",
        "PDF"   => "#D32F2F",
        "3D"    => "#7B1FA2",
        "Video" => "#F57C00",
        _       => "#757575"
    };

    private static string BoyutFormati(long b) => b switch
    {
        >= 1_048_576 => $"{b / 1_048_576.0:F1} MB",
        >= 1024      => $"{b / 1024.0:F0} KB",
        _            => $"{b} B"
    };

    // ──────────────────────────────────────────────────────────────────────────
    // SEKME 2: DB MEDYA
    // ──────────────────────────────────────────────────────────────────────────

    private async Task DbMedyaYukleAsync()
    {
        _yukleniyor = true;
        var liste = await api.GetAsync<List<Medya>>($"api/medya?q={Uri.EscapeDataString(_arama)}&klasor={_seciliKlasorId}");
        _medyaListesi = liste ?? [];
        var agac = await api.GetAsync<List<MedyaKlasoru>>("api/medya/klasorler");
        _klasorler = agac ?? [];
        _seciliMedyalar = _seciliMedyalar.Where(s => _medyaListesi.Any(m => m.Id == s.Id)).ToList();
        _yukleniyor = false;
    }

    private async Task AramaDegisti(string deger)
    {
        _arama = deger;
        await DbMedyaYukleAsync();
    }

    private async Task TumunuSecAsync()
    {
        _seciliKlasorId = null;
        await DbMedyaYukleAsync();
    }

    private async Task KlasorSecAsync(MedyaKlasoru k)
    {
        _seciliKlasorId = k.Id;
        await DbMedyaYukleAsync();
    }

    private void MedyaTikla(Medya m)
    {
        if (_seciliMedyalar.Any(x => x.Id == m.Id)) _seciliMedyalar.RemoveAll(x => x.Id == m.Id);
        else _seciliMedyalar.Add(m);
    }

    private bool SeciliMi(Medya m) => _seciliMedyalar.Any(x => x.Id == m.Id);

    private void YeniYukleAc() => _yuklemeDialog = true;
    private void YeniKlasorAc() { _yeniKlasorAdi = ""; _klasorDialog = true; }

    private void DuzenleAc(Medya m)
    {
        _duzenlenenMedya = new Medya
        {
            Id = m.Id, Ad = m.Ad, AltMetin = m.AltMetin, Aciklama = m.Aciklama,
            EtiketlerJson = m.EtiketlerJson, KlasorId = m.KlasorId,
            DosyaYolu = m.DosyaYolu, MiniaturYolu = m.MiniaturYolu, Tip = m.Tip, BoyutByte = m.BoyutByte
        };
        _duzenlemeDialog = true;
    }

    private async Task KlasorKaydetAsync()
    {
        if (string.IsNullOrWhiteSpace(_yeniKlasorAdi)) { snackbar.Add("Klasör adı zorunludur.", Severity.Warning); return; }
        _kaydediliyor = true;
        var r = await api.PostAsync<MedyaKlasoru>("api/medya/klasor", new { Ad = _yeniKlasorAdi, UstKlasorId = _seciliKlasorId });
        _kaydediliyor = false;
        if (r?.BasariliMi == true) { _klasorDialog = false; snackbar.Add("Klasör oluşturuldu.", Severity.Success); await DbMedyaYukleAsync(); }
        else snackbar.Add(r?.Mesaj ?? "Hata oluştu.", Severity.Error);
    }

    private async Task MedyaKaydetAsync()
    {
        if (string.IsNullOrWhiteSpace(_duzenlenenMedya.Ad)) { snackbar.Add("Ad zorunludur.", Severity.Warning); return; }
        _kaydediliyor = true;
        var r = await api.PutAsync<Medya>($"api/medya/{_duzenlenenMedya.Id}", new
        {
            _duzenlenenMedya.Ad, _duzenlenenMedya.AltMetin, _duzenlenenMedya.Aciklama,
            _duzenlenenMedya.EtiketlerJson, _duzenlenenMedya.KlasorId
        });
        _kaydediliyor = false;
        if (r?.BasariliMi == true) { _duzenlemeDialog = false; snackbar.Add("Güncellendi.", Severity.Success); await DbMedyaYukleAsync(); }
        else snackbar.Add(r?.Mesaj ?? "Hata oluştu.", Severity.Error);
    }

    private async Task DosyalariYukle(IReadOnlyList<IBrowserFile> dosyalar)
    {
        if (dosyalar.Count == 0) return;
        for (var i = 0; i < dosyalar.Count; i++)
        {
            var d = dosyalar[i];
            using var ic = new MultipartFormDataContent();
            using var st = d.OpenReadStream(MAKS_BOYUT);
            ic.Add(new StreamContent(st), "dosya", d.Name);
            if (_seciliKlasorId.HasValue) ic.Add(new StringContent(_seciliKlasorId.Value.ToString()), "klasorId");
            _yuklemeYuzdesi = (i + 1) * 100 / dosyalar.Count;
            StateHasChanged();
            await api.PostMultipartAsync<Medya>("api/medya/yukle", ic);
        }
        _yuklemeYuzdesi = 0;
        _yuklemeDialog = false;
        snackbar.Add($"{dosyalar.Count} dosya yüklendi.", Severity.Success);
        await Task.WhenAll(DbMedyaYukleAsync(), DosyalariFiltreleAsync());
    }

    private async Task SeciliSil()
    {
        if (_seciliMedyalar.Count == 0) return;
        var onay = await dialogService.ShowMessageBoxAsync(
            "Silme Onayı",
            $"{_seciliMedyalar.Count} medya silinecek. Emin misiniz?",
            yesText: "Sil", cancelText: "İptal");
        if (onay != true) return;
        foreach (var m in _seciliMedyalar.ToList())
            await api.DeleteAsync($"api/medya/{m.Id}");
        _seciliMedyalar.Clear();
        snackbar.Add("Silindi.", Severity.Success);
        await DbMedyaYukleAsync();
    }

    private string MedyaGorselYolu(Medya m)
    {
        if (!string.IsNullOrWhiteSpace(m.MiniaturYolu)) return m.MiniaturYolu;
        if (!string.IsNullOrWhiteSpace(m.DosyaYolu))
            return m.DosyaYolu.StartsWith('/') ? m.DosyaYolu : $"/{m.DosyaYolu}";
        return "/medya/vizitlink3d_default.png";
    }

    private static string MedyaKartSinifi(Medya m) =>
        "medya-kart mud-elevation-0";
}
