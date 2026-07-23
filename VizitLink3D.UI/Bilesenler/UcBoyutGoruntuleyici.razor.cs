using VizitLink3D.UI.Servisler;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;

namespace VizitLink3D.UI.Bilesenler;

public partial class UcBoyutGoruntuleyici : IAsyncDisposable
{
    [Parameter] public int? ModelId { get; set; }

    /// <summary>
    /// Yuklenecek GLB/GLTF model dosya yolu. Bos birakilirsa
    /// parametrik kapak geometrisi gosterilir.
    /// </summary>
    [Parameter] public string? ModelYolu { get; set; }

    /// <summary>
    /// Baslangicta uygulanacak renk (hex kodu).
    /// Varsayilan: kirik beyaz (#E8E4DF) — VizitLink3D renk paleti ilk rengi.
    /// </summary>
    [Parameter] public string BaslangicRenk { get; set; } = "#E8E4DF";

    /// <summary>
    /// Disaridan renk degistirilmek istendiginde kullanilir.
    /// Ust bilesen yeni renk degerini bu parametre ile iletir.
    /// </summary>
    [Parameter] public string SecilenRenk { get; set; } = "#E8E4DF";

    /// <summary>
    /// SecilenRenk parametresi degistiginde Blazor tarafindan tetiklenir.
    /// </summary>
    [Parameter] public EventCallback<string> SecilenRenkChanged { get; set; }

    /// <summary>
    /// 3D model üzerinde bir mesh tıklandığında tetiklenir.
    /// Tıklanan mesh adını üst bileşene iletir.
    /// </summary>
    [Parameter] public EventCallback<string> OnMeshSecildi { get; set; }

    /// <summary>
    /// Modelin gercek fotograf yolu (istege bagli). 
    /// Eger verilirse Gorsel/3D gecis tabi gosterilir.
    /// </summary>
    [Parameter] public string? GorselYolu { get; set; }
    [Parameter] public string? KameraAyarJson { get; set; }
    [Parameter] public string? IsikAyarJson { get; set; }
    [Parameter] public string? CevreAyarJson { get; set; }

    [Inject] public UcBoyutServisi UcBoyutSrv { get; set; } = default!;
    [Inject] public ApiIstemcisi Api { get; set; } = default!;

    // Kanvas ID'si — her bilesen ornegi benzersiz ID alir (coklu viewer destegi)
    private string _kanvasId = $"gb-viewer-{Guid.NewGuid():N}";
    private bool _yukleniyor = true;
    private bool _sahneHazir;
    private bool _otomatikDondurmeMi;
    private bool _izgaraGorunurMu;
    private bool _patlatilmisMi;
    private string _oncekiRenk = "";
    private string? _uygulananKameraJson;
    private string? _uygulananIsikJson;
    private string? _uygulananCevreJson;
    private string _aktifTab = "3d"; // "gorsel" veya "3d"
    private HubConnection? _sahneBaglantisi;
    private DotNetObjectReference<UcBoyutGoruntuleyici>? _dotNetRef;

    protected override void OnInitialized()
    {
        if (string.IsNullOrEmpty(ModelYolu))
        {
            _aktifTab = "gorsel";
        }
    }

    /// <summary>
    /// Bilesen DOM'a eklendikten sonra Three.js sahnesini baslatir.
    /// Ilk render tamamlanmadan JS cagrisi yapilamaz — bu yuzden
    /// OnAfterRenderAsync kullanilir.
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool ilkRender)
    {
        if (ilkRender)
        {
            // 100ms bekle — DOM tamamen hazir olsun
            await Task.Delay(100);
            if (!string.IsNullOrEmpty(ModelYolu))
            {
                await UcBoyutSrv.Baslat(_kanvasId, ModelYolu, BaslangicRenk);
                await CanliSahneBaglantisiniBaslat();
                _sahneHazir = true;
                await SahneAyarlariUygula();

                // 3D mesh tıklama callback'ini kaydet
                if (OnMeshSecildi.HasDelegate)
                {
                    _dotNetRef = DotNetObjectReference.Create(this);
                    await UcBoyutSrv.ParcaSecCallbackKaydet(_kanvasId, _dotNetRef);
                }
            }
            _yukleniyor = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Parametre degisimlerini izler. SecilenRenk degistiginde
    /// sahneye yeni rengi uygular. Gereksiz cagrilari onlemek icin
    /// onceki renkle karsilastirma yapilir.
    /// </summary>
    protected override async Task OnParametersSetAsync()
    {
        if (!string.IsNullOrEmpty(SecilenRenk) && SecilenRenk != _oncekiRenk)
        {
            _oncekiRenk = SecilenRenk;
            // Ilk render oncesi JS cagrisi yapma
            if (!_yukleniyor)
            {
                await UcBoyutSrv.RenkUygula(_kanvasId, SecilenRenk);
            }
        }

        if (_sahneHazir)
        {
            await SahneAyarlariUygula();
        }
    }

    /// <summary>
    /// Gorunumu sifirlar — kamera baslangic pozisyonuna doner.
    /// </summary>
    private async Task KameraYeSifirla()
    {
        await UcBoyutSrv.KameraSifirla(_kanvasId);
    }

    /// <summary>
    /// Otomatik dondurmeyi acar/kapatir.
    /// </summary>
    private async Task DondurmeyiToggle()
    {
        _otomatikDondurmeMi = !_otomatikDondurmeMi;
        await UcBoyutSrv.OtomatikDondur(_kanvasId, _otomatikDondurmeMi);
    }

    private async Task IzgarayiToggle()
    {
        _izgaraGorunurMu = !_izgaraGorunurMu;
        await UcBoyutSrv.IzgaraGoster(_kanvasId, _izgaraGorunurMu);
    }

    private async Task PatlatilmisGorunumToggle()
    {
        _patlatilmisMi = !_patlatilmisMi;
        await UcBoyutSrv.PatlatilmisGorunum(_kanvasId, _patlatilmisMi);
    }

    /// <summary>
    /// Viewer'i tam ekrana alir.
    /// </summary>
    private async Task TamEkranaAl()
    {
        await UcBoyutSrv.TamEkran(_kanvasId);
    }

    private async Task CanliSahneBaglantisiniBaslat()
    {
        if (!ModelId.HasValue || _sahneBaglantisi is not null)
            return;

        _sahneBaglantisi = new HubConnectionBuilder()
            .WithUrl($"{Api.ApiBaseUrl}/hubs/sahne-ayar")
            .WithAutomaticReconnect()
            .Build();

        _sahneBaglantisi.On<int, string, string>("SahneAyarGuncellendi", async (modelId, ayarTipi, ayarJson) =>
        {
            if (modelId != ModelId.Value)
                return;

            await UcBoyutSrv.SahneAyarUygula(_kanvasId, ayarTipi, ayarJson);
        });

        await _sahneBaglantisi.StartAsync();
        await _sahneBaglantisi.InvokeAsync("SahneGrubunaKatil", ModelId.Value);
    }

    private async Task SahneAyarlariUygula()
    {
        if (!string.IsNullOrWhiteSpace(KameraAyarJson) && KameraAyarJson != _uygulananKameraJson)
        {
            await UcBoyutSrv.SahneAyarUygula(_kanvasId, "kamera", KameraAyarJson);
            _uygulananKameraJson = KameraAyarJson;
        }

        if (!string.IsNullOrWhiteSpace(IsikAyarJson) && IsikAyarJson != _uygulananIsikJson)
        {
            await UcBoyutSrv.SahneAyarUygula(_kanvasId, "isik", IsikAyarJson);
            _uygulananIsikJson = IsikAyarJson;
        }

        if (!string.IsNullOrWhiteSpace(CevreAyarJson) && CevreAyarJson != _uygulananCevreJson)
        {
            await UcBoyutSrv.SahneAyarUygula(_kanvasId, "cevre", CevreAyarJson);
            _uygulananCevreJson = CevreAyarJson;
        }
    }

    /// <summary>
    /// 3D sahnede bir mesh tıklandığında JS tarafından çağrılır.
    /// Tıklanan mesh adını OnMeshSecildi EventCallback'i ile üst bileşene iletir.
    /// </summary>
    [JSInvokable]
    public async Task ParcaSecildi(string parcaIsmi)
    {
        if (OnMeshSecildi.HasDelegate)
            await OnMeshSecildi.InvokeAsync(parcaIsmi);
    }

    /// <summary>
    /// Bilesen kaldirildiginda Three.js sahnesini temizler.
    /// Bellek sizintisini onlemek icin zorunludur.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_sahneBaglantisi is not null)
        {
            if (ModelId.HasValue)
                await _sahneBaglantisi.InvokeAsync("SahneGrubundanAyril", ModelId.Value);

            await _sahneBaglantisi.DisposeAsync();
        }

        _dotNetRef?.Dispose();
        await UcBoyutSrv.Temizle(_kanvasId);
    }
}
