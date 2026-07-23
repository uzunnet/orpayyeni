using VizitLink3D.UI.Servisler;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System.Text.Json;

namespace VizitLink3D.UI.Pages.Vitrin;

public partial class PiedraKonfigurator : ComponentBase, IAsyncDisposable
{
    [Inject] private UcBoyutServisi UcBoyut { get; set; } = default!;

    private const string KanvasId = "piedra-konfig-viewer";
    private List<ParcaBilgisi> _parcalar = new();
    private bool _yuklendi;

    private static readonly (string Ad, string Hex)[] _renkler = new[]
    {
        ("Beyaz", "#FFFFFF"), ("Krem", "#F5F0E8"), ("Açık Gri", "#D4D4D4"),
        ("Antrasit", "#3D3D3D"), ("Mat Siyah", "#1A1A1A"), ("Krom", "#C0C0C0"),
        ("Altın", "#C5A059"), ("Lacivert", "#1C2B4A"), ("Yeşil", "#5A6B4A"),
    };

    private static readonly (string Isim, string Tahmin)[] _parcaTahminleri = new[]
    {
        ("Box581", "Ana Gövde (Dolap)"),
        ("Torus001", "Lavabo Teknesi"),
        ("Cylinder058", "Ayak (Büyük)"),
        ("Cylinder059", "Ayak (Küçük)"),
        ("Shape001", "Musluk"),
        ("Rectangle042", "Kapak (Sol)"),
        ("Rectangle043", "Kapak (Sağ)"),
        ("Object083", "Detay Parça"),
        ("Object086", "Vida / Pim"),
    };

    public class ParcaBilgisi
    {
        public string Isim { get; set; } = "";
        public string Tahmin { get; set; } = "";
        public int Ucgen { get; set; }
        public bool Gorunur { get; set; } = true;
        public int Derece { get; set; } = 0;
        public string Malzeme { get; set; } = "";
    }

    private double _isikSeviyesi = 1.0;
    private string _aktifMalzeme = "";
    private string _secilenParca = "";

    [JSInvokable]
    public async Task ParcaSecildi(string parcaIsmi)
    {
        _secilenParca = parcaIsmi;
        StateHasChanged();
    }

    private void SecimiTemizle()
    {
        _secilenParca = "";
    }

    private static readonly (string Ad, string Kod, string Onizleme)[] _malzemeler = new[]
    {
        ("Metal",  "metal",  "linear-gradient(135deg,#C0C0C0,#808080)"),
        ("Cam",    "cam",    "linear-gradient(135deg,#ADD8E6,rgba(255,255,255,0.4))"),
        ("Ayna",   "ayna",   "linear-gradient(135deg,#E8E8E8,#C0C0C0)"),
        ("Ahşap",  "ahsap",  "linear-gradient(135deg,#8B5A2B,#D2B48C)"),
        ("Plastik","plastik","linear-gradient(135deg,#F5F5F5,#D4D4D4)"),
        ("Krom",   "krom",   "linear-gradient(135deg,#E8E8E8,#707070)"),
        ("Porselen","porselen","linear-gradient(135deg,#FFFFF0,#F5F5DC)"),
    };

    protected override async Task OnAfterRenderAsync(bool ilkRender)
    {
        if (ilkRender)
        {
            await UcBoyut.Baslat(KanvasId, "/medya/3d-modeller/piedra.glb", "#E8E4DF");
            await Task.Delay(3000);
            await UcBoyut.IsikKaydet(KanvasId);
            await UcBoyut.ParcaSecCallbackKaydet(KanvasId, DotNetObjectReference.Create(this));
            await AnalizEt();
            _yuklendi = true;
            StateHasChanged();
        }
    }

    private async Task AnalizEt()
    {
        var json = await UcBoyut.ModelAnalizEt(KanvasId);
        var ham = JsonSerializer.Deserialize<List<JsonElement>>(json);
        if (ham == null) return;

        _parcalar = ham
            .Where(p => p.GetProperty("tip").GetString() == "Mesh" && p.GetProperty("isim").GetString() != "(isimsiz)")
            .Select(p =>
            {
                var isim = p.GetProperty("isim").GetString()!;
                var tahmin = _parcaTahminleri.FirstOrDefault(t => t.Isim == isim).Tahmin ?? isim;
                return new ParcaBilgisi
                {
                    Isim = isim,
                    Tahmin = tahmin,
                    Ucgen = p.GetProperty("ucgenSayisi").GetInt32(),
                    Gorunur = true
                };
            }).ToList();
    }

    private async Task ParcaToggle(ParcaBilgisi p)
    {
        p.Gorunur = !p.Gorunur;
        await UcBoyut.ParcaGorunurluk(KanvasId, p.Isim, p.Gorunur);
    }

    private async Task RenkDegistir(string hex)
    {
        if (!string.IsNullOrEmpty(_secilenParca))
        {
            await UcBoyut.ParcaRenk(KanvasId, _secilenParca, hex);
        }
        else
        {
            await UcBoyut.ParcaRenk(KanvasId, "Box581", hex);
        }
    }

    private async Task TumunuGoster()
    {
        foreach (var p in _parcalar) { p.Gorunur = true; await UcBoyut.ParcaGorunurluk(KanvasId, p.Isim, true); }
    }

    private async Task SadeceDolapGoster()
    {
        foreach (var p in _parcalar)
        {
            var goster = p.Isim == "Box581";
            p.Gorunur = goster;
            await UcBoyut.ParcaGorunurluk(KanvasId, p.Isim, goster);
        }
    }

    private async Task KapakAcKapa(ParcaBilgisi p)
    {
        // Her tıklamada 0 → 60 → 90 → 0 döngüsü
        p.Derece = p.Derece switch { 0 => 50, 50 => 90, 90 => 0, _ => 0 };
        await UcBoyut.KapakDerece(KanvasId, p.Isim, p.Derece);
    }

    private async Task MalzemeDegistir(string malzemeKodu)
    {
        _aktifMalzeme = _aktifMalzeme == malzemeKodu ? "" : malzemeKodu;

        if (!string.IsNullOrEmpty(_secilenParca) && !string.IsNullOrEmpty(_aktifMalzeme))
        {
            await UcBoyut.ParcaMalzeme(KanvasId, _secilenParca, _aktifMalzeme);
            // Seçili parçanın malzemesini kaydet
            var parca = _parcalar.FirstOrDefault(x => x.Isim == _secilenParca);
            if (parca != null) parca.Malzeme = _aktifMalzeme;
            var m = _malzemeler.First(x => x.Kod == malzemeKodu);
            snackbar.Add($"{_secilenParca} → {m.Ad}", Severity.Info);
        }
        else if (!string.IsNullOrEmpty(_aktifMalzeme))
        {
            foreach (var p in _parcalar.Where(x => x.Isim.StartsWith("Rectangle")))
            {
                await UcBoyut.ParcaMalzeme(KanvasId, p.Isim, _aktifMalzeme);
                p.Malzeme = _aktifMalzeme;
            }
            snackbar.Add($"Tüm kapaklar → {_malzemeler.First(x => x.Kod == malzemeKodu).Ad}", Severity.Info);
        }
    }

    private async Task IsikDegisti(double seviye)
    {
        _isikSeviyesi = seviye;
        await UcBoyut.IsikAyar(KanvasId, seviye);
    }

    public async ValueTask DisposeAsync()
    {
        if (_yuklendi) await UcBoyut.Temizle(KanvasId);
    }
}
