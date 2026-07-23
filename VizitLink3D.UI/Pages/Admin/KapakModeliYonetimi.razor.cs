using VizitLink3D.Ortak.Modeller;
using VizitLink3D.UI.Models;
using VizitLink3D.UI.Servisler;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace VizitLink3D.UI.Pages.Admin;

public partial class KapakModeliYonetimi : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogServisi { get; set; } = default!;
    [Inject] private DilServisi DilServisi { get; set; } = default!;

    private List<KapakModeliDto> _liste = [];
    private List<KapakModeliDto> _filtreliListe = [];
    private KapakModeliKayitModeli _form = new();
    private bool _yukleniyor = true;
    private bool _formAcik;
    private bool _duzenlemeModu;
    private bool _kaydediliyor;
    private int? _duzenlenenId;
    private string _arama = string.Empty;
    private string _modelTuruFiltre = "Tümü";

    protected override async Task OnInitializedAsync() => await Yukle();

    private async Task Yukle()
    {
        _yukleniyor = true;
        _liste = await Api.GetAsync<List<KapakModeliDto>>("api/kapak-modelleri") ?? [];
        AramaUygula();
        _yukleniyor = false;
    }

    private void AramaMetniDegisti(string deger)
    {
        _arama = deger;
        AramaUygula();
    }

    private void ModelTuruFiltresiDegisti(string deger)
    {
        _modelTuruFiltre = deger;
        AramaUygula();
    }

    private void AramaUygula()
    {
        var aranan = _arama?.ToLowerInvariant() ?? string.Empty;
        _filtreliListe = _liste
            .Where(x => _modelTuruFiltre == "Tümü" || x.ModelTuru == _modelTuruFiltre)
            .Where(x => string.IsNullOrWhiteSpace(aranan)
                || x.ModelAdi.ToLowerInvariant().Contains(aranan)
                || x.ModelKodu.ToLowerInvariant().Contains(aranan)
                || x.Kategori.ToLowerInvariant().Contains(aranan))
            .OrderBy(x => x.ModelTuru)
            .ThenBy(x => x.Kategori)
            .ThenBy(x => x.ModelAdi)
            .ToList();
    }

    private void YeniAc()
    {
        _form = new KapakModeliKayitModeli
        {
            ModelTuru = "Kapi",
            Kategori = "Lake Kapilar / Duz Modeller",
            SiraNo = _liste.Count + 1,
            RenkSecenekleriJson = VarsayilanRenkJson(),
            NiteliklerJson = "[\"Kapi Modeli\"]",
            UygulamaGorselleriJson = "[]"
        };
        _duzenlenenId = null;
        _duzenlemeModu = false;
        _formAcik = true;
    }

    private void Duzenle(KapakModeliDto model)
    {
        _form = new KapakModeliKayitModeli
        {
            Id = model.Id,
            ModelAdi = model.ModelAdi,
            ModelKodu = model.ModelKodu,
            Slug = model.Slug,
            Kategori = model.Kategori,
            ModelTuru = string.IsNullOrWhiteSpace(model.ModelTuru) ? "Kapi" : model.ModelTuru,
            AnaGorselUrl = GorselYolu(model),
            Aciklama = model.Aciklama,
            OnYazi = model.OnYazi,
            OneCikanMi = model.OneCikanMi,
            YeniMi = model.YeniMi,
            Fiyat = model.Fiyat,
            ModelDosyaYolu = model.ModelDosyaYolu,
            MinYukseklik = model.MinYukseklik,
            MaxYukseklik = model.MaxYukseklik,
            MinGenislik = model.MinGenislik,
            MaxGenislik = model.MaxGenislik,
            SiraNo = model.SiraNo,
            RenkSecenekleriJson = VarsayilanRenkJson(),
            NiteliklerJson = System.Text.Json.JsonSerializer.Serialize(model.Nitelikler),
            UygulamaGorselleriJson = System.Text.Json.JsonSerializer.Serialize(model.UygulamaGorselleri)
        };
        _duzenlenenId = model.Id;
        _duzenlemeModu = true;
        _formAcik = true;
    }

    private async Task Kaydet()
    {
        if (string.IsNullOrWhiteSpace(_form.ModelAdi) || string.IsNullOrWhiteSpace(_form.ModelKodu))
        {
            Snackbar.Add(DilServisi.T("admin.kapakModeli.zorunluAlan", "Model adı ve model kodu zorunludur."), Severity.Warning);
            return;
        }

        _kaydediliyor = true;
        Cevap<KapakModeliDto>? cevap = _duzenlenenId.HasValue
            ? await Api.PutAsync<KapakModeliDto>($"api/kapak-modelleri/{_duzenlenenId.Value}", _form)
            : await Api.PostAsync<KapakModeliDto>("api/kapak-modelleri", _form);

        if (cevap?.BasariliMi == true)
        {
            _formAcik = false;
            Snackbar.Add(cevap.Mesaj, Severity.Success);
            await Yukle();
        }
        else
        {
            Snackbar.Add(cevap?.Mesaj ?? DilServisi.T("admin.kapakModeli.kaydedilemedi", "Model kaydedilemedi."), Severity.Error);
        }

        _kaydediliyor = false;
    }

    private async Task SilOnay(KapakModeliDto model)
    {
        var onay = await DialogServisi.ShowMessageBoxAsync(
            DilServisi.T("ortak.silmeOnayi", "Silme Onayı"),
            string.Format(DilServisi.T("admin.kapakModeli.silmeMesaji", "'{0}' kaydı pasife alınacaktır. Emin misiniz?"), model.ModelAdi),
            yesText: DilServisi.T("ortak.evetSil", "Evet, Sil"),
            cancelText: DilServisi.T("ortak.iptal", "İptal"));

        if (onay == true)
        {
            var cevap = await Api.DeleteAsync($"api/kapak-modelleri/{model.Id}");
            if (cevap?.BasariliMi == true)
            {
                Snackbar.Add(cevap.Mesaj, Severity.Success);
                await Yukle();
            }
            else
            {
                Snackbar.Add(cevap?.Mesaj ?? DilServisi.T("admin.kapakModeli.silinemedi", "Model silinemedi."), Severity.Error);
            }
        }
    }

    private void FormIptal() => _formAcik = false;

    private string DetayUrl(KapakModeliDto model)
    {
        var kok = model.ModelTuru == "Kapi" ? "kapi" : "kapak";
        return $"/{kok}/{model.Id}/{Uri.EscapeDataString(model.ModelKodu)}";
    }

    private string TamUrl(string? yol)
    {
        if (string.IsNullOrWhiteSpace(yol))
        {
            return "/medya/vizitlink3d_default.png";
        }

        if (yol.StartsWith("http", StringComparison.OrdinalIgnoreCase) || yol.StartsWith("images/", StringComparison.OrdinalIgnoreCase))
        {
            return yol;
        }

        return $"{Api.ApiBaseUrl}{(yol.StartsWith('/') ? yol : "/" + yol)}";
    }

    private static string GorselYolu(KapakModeliDto model)
    {
        var numStr = new string(model.ModelKodu.Where(char.IsDigit).ToArray());
        if (!string.IsNullOrWhiteSpace(numStr))
            return "";
        if (!string.IsNullOrWhiteSpace(model.AnaGorselUrl))
            return model.AnaGorselUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? model.AnaGorselUrl
                : (model.AnaGorselUrl.StartsWith('/') ? model.AnaGorselUrl : $"/{model.AnaGorselUrl}");
        return "/medya/vizitlink3d_default.png";
    }

    private static string VarsayilanRenkJson() =>
        "[{\"ad\":\"RAL 9016\",\"hexKod\":\"#F6F5F0\",\"grup\":\"Beyazlar\",\"ureticiKodu\":\"RAL 9016\"},{\"ad\":\"RAL 7016\",\"hexKod\":\"#383E42\",\"grup\":\"Griler\",\"ureticiKodu\":\"RAL 7016\"},{\"ad\":\"RAL 9005\",\"hexKod\":\"#0E0E10\",\"grup\":\"Siyahlar\",\"ureticiKodu\":\"RAL 9005\"}]";

    private sealed class KapakModeliKayitModeli
    {
        public int Id { get; set; }
        public string ModelAdi { get; set; } = string.Empty;
        public string ModelKodu { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Kategori { get; set; } = string.Empty;
        public string ModelTuru { get; set; } = "Kapi";
        public string? AnaGorselUrl { get; set; }
        public string? Aciklama { get; set; }
        public string? OnYazi { get; set; }
        public bool OneCikanMi { get; set; }
        public bool YeniMi { get; set; }
        public decimal? Fiyat { get; set; }
        public string? ModelDosyaYolu { get; set; }
        public int? MinYukseklik { get; set; } = 1800;
        public int? MaxYukseklik { get; set; } = 2600;
        public int? MinGenislik { get; set; } = 600;
        public int? MaxGenislik { get; set; } = 1100;
        public string RenkSecenekleriJson { get; set; } = "[]";
        public string NiteliklerJson { get; set; } = "[]";
        public string UygulamaGorselleriJson { get; set; } = "[]";
        public int SiraNo { get; set; }
    }
}
