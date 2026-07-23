using Microsoft.AspNetCore.Components;
using MudBlazor;
using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.UI.Pages.Admin;

public partial class ApiEntegrasyonlari : ComponentBase
{
    private string _gaId = string.Empty;
    private string _pixelId = string.Empty;
    private string _gtmId = string.Empty;
    private string _geminiKey = string.Empty;
    private string _openAiKey = string.Empty;
    private bool _kaydediliyor = false;

    [Inject] ISnackbar Snackbar { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        try 
        {
            var ayarlar = await api.GetAsync<List<SistemAyari>>("api/ayarlar/api-entegrasyon");
            if (ayarlar != null)
            {
                _gaId = AyarDegeri(ayarlar, "api.GoogleAnalyticsId");
                _pixelId = AyarDegeri(ayarlar, "api.FacebookPixelId");
                _gtmId = AyarDegeri(ayarlar, "api.GoogleTagManagerId");
                _geminiKey = AyarDegeri(ayarlar, "ai.GeminiApiKey");
                _openAiKey = AyarDegeri(ayarlar, "ai.OpenAiApiKey");
            }
        }
        catch 
        {
            // Fallback to defaults
        }
        await base.OnInitializedAsync();
    }

    private async Task KaydetAnalytics()
    {
        _kaydediliyor = true;
        
        var analitikAyarlar = new {
            GoogleAnalyticsId = _gaId,
            FacebookPixelId = _pixelId,
            GoogleTagManagerId = _gtmId
        };
        
        var sonuc = await api.PostAsync<object>("api/ayarlar/analytics", analitikAyarlar);
        
        _kaydediliyor = false;
        
        if (sonuc != null && sonuc.BasariliMi)
            Snackbar.Add(dil.T("admin.api.analyticsKaydedildi", "Analytics ayarlari basariyla kaydedildi."), Severity.Success);
        else
            Snackbar.Add(dil.T("admin.api.analyticsHata", "Analytics ayarlari kaydedilirken hata olustu."), Severity.Error);
    }

    private async Task KaydetAi()
    {
        _kaydediliyor = true;
        
        var aiAyarlar = new {
            GeminiApiKey = _geminiKey,
            OpenAiApiKey = _openAiKey
        };
        
        var sonuc = await api.PostAsync<object>("api/ayarlar/ai", aiAyarlar);
        
        _kaydediliyor = false;
        
        if (sonuc != null && sonuc.BasariliMi)
            Snackbar.Add(dil.T("admin.api.aiKaydedildi", "Yapay Zeka ayarlari basariyla kaydedildi."), Severity.Success);
        else
            Snackbar.Add(dil.T("admin.api.aiHata", "Yapay Zeka ayarlari kaydedilirken hata olustu."), Severity.Error);
    }

    private async Task TestAi()
    {
        var sonuc = await api.GetAsync<bool>("api/ayarlar/ai/test");
        if (sonuc)
            Snackbar.Add(dil.T("admin.api.aiTestBasarili", "Baglanti basarili. Modeller erisilebilir durumda."), Severity.Info);
        else
            Snackbar.Add(dil.T("admin.api.aiTestBasarisiz", "Yapay zeka baglanti testi basarisiz oldu."), Severity.Error);
    }

    private static string AyarDegeri(List<SistemAyari> ayarlar, string anahtar)
        => ayarlar.FirstOrDefault(a => a.Anahtar == anahtar)?.Deger ?? string.Empty;
}

