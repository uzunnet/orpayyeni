using Microsoft.JSInterop;

namespace VizitLink3D.UI.Servisler;

/// <summary>
/// GSAP ve ScrollTrigger harici animasyon kutuphanelerini sarmalayan Turkce servistir.
/// Anayasa §11 wrapper kurali geregi dogrudan JSRuntime erisimini gizler.
/// </summary>
public class AnimasyonMotoruServisi : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;

    public AnimasyonMotoruServisi(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task ScrollAnimasyonlariniBaslatAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("vizitlink3dAnimasyon.baslatScrollAnimasyonlari");
            await _jsRuntime.InvokeVoidAsync("vizitlink3dAnimasyon.smoothScroll");
        }
        catch { /* animasyon başlatılamazsa sayfa işlevselliği etkilenmez */ }
    }

    public async Task SayfaGirisAsync()
    {
        try { await _jsRuntime.InvokeVoidAsync("vizitlink3dAnimasyon.sayfaGirisAnimasyonu"); }
        catch { /* animasyon başlatılamazsa sayfa işlevselliği etkilenmez */ }
    }

    public async Task HeroAnimasyonunuOynatAsync()
    {
        try { await _jsRuntime.InvokeVoidAsync("vizitlink3dAnimasyon.heroAnimasyonunuOynat"); }
        catch { /* animasyon başlatılamazsa sayfa işlevselliği etkilenmez */ }
    }

    public async Task AnimasyonlariYenileAsync()
    {
        try { await _jsRuntime.InvokeVoidAsync("vizitlink3dAnimasyon.yenile"); }
        catch { /* animasyon başlatılamazsa sayfa işlevselliği etkilenmez */ }
    }

    public async ValueTask DisposeAsync() => await Task.CompletedTask;
}
