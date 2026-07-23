using VizitLink3D.Ortak.Modeller.AI;
using VizitLink3D.UI.Servisler;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace VizitLink3D.UI.Pages.Admin;

public partial class AIAyarlariSayfasi : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;

    private List<AISaglayicisi> _saglayicilar = new();
    private List<AICagrisiKaydi> _cagrilar = new();
    private MaliyetOzeti? _maliyet;
    private bool _yukleniyor = true;

    private bool _yeniFormAcik;
    private AISaglayicisi _yeniSaglayici = new();
    private string _yeniApiKey = string.Empty;

    private bool _duzenleDialogAcik;
    private AISaglayicisi? _duzenlenen;
    private string _duzenleApiKey = string.Empty;

    private decimal _toplamLimit;

    private class MaliyetOzeti
    {
        public int ToplamCagri { get; set; }
        public decimal ToplamMaliyet { get; set; }
    }

    protected override async Task OnInitializedAsync()
    {
        await Yukle();
    }

    private async Task Yukle()
    {
        _yukleniyor = true;
        StateHasChanged();

        _saglayicilar = await Api.GetAsync<List<AISaglayicisi>>("api/ai/saglayicilar") ?? new();
        _maliyet = await Api.GetAsync<MaliyetOzeti>("api/ai/maliyet");
        _cagrilar = await Api.GetAsync<List<AICagrisiKaydi>>("api/ai/cagrilar") ?? new();

        _toplamLimit = _saglayicilar.Sum(s => s.AylikLimitUsd);

        _yukleniyor = false;
    }

    private static string ApiKeyMasked(AISaglayicisi s)
    {
        if (!string.IsNullOrEmpty(s.ApiKeyEncrypted))
            return "sk-" + new string('\u2022', 8);
        return "---";
    }

    private void YeniSaglayiciEkle()
    {
        _yeniSaglayici = new AISaglayicisi();
        _yeniApiKey = string.Empty;
        _yeniFormAcik = true;
    }

    private void YeniFormIptal()
    {
        _yeniFormAcik = false;
    }

    private async Task YeniKaydet()
    {
        var govde = new
        {
            Ad = _yeniSaglayici.Ad,
            Tip = _yeniSaglayici.Tip,
            Model = _yeniSaglayici.Model,
            ApiKeyEncrypted = _yeniApiKey,
            AylikLimitUsd = _yeniSaglayici.AylikLimitUsd,
            AktifMi = _yeniSaglayici.AktifMi
        };

        var sonuc = await Api.PostAsync<AISaglayicisi>("api/ai/saglayici", govde);
        if (sonuc?.BasariliMi == true)
        {
            _yeniFormAcik = false;
            snackbar.Add(dil.T("bildirim.kaydedildi", "Kaydedildi"), Severity.Success);
            await Yukle();
        }
        else
        {
            snackbar.Add(dil.T("bildirim.hata", "Hata oluştu"), Severity.Error);
        }
    }

    private void DuzenleAc(AISaglayicisi s)
    {
        _duzenlenen = s;
        _duzenleApiKey = string.Empty;
        _duzenleDialogAcik = true;
    }

    private void DuzenleIptal()
    {
        _duzenleDialogAcik = false;
        _duzenlenen = null;
    }

    private async Task DuzenleKaydet()
    {
        if (_duzenlenen == null) return;

        var govde = new
        {
            Ad = _duzenlenen.Ad,
            Tip = _duzenlenen.Tip,
            Model = _duzenlenen.Model,
            ApiKeyEncrypted = string.IsNullOrEmpty(_duzenleApiKey) ? null : _duzenleApiKey,
            AylikLimitUsd = _duzenlenen.AylikLimitUsd,
            AktifMi = _duzenlenen.AktifMi
        };

        var sonuc = await Api.PutAsync<AISaglayicisi>($"api/ai/saglayici/{_duzenlenen.Id}", govde);
        if (sonuc?.BasariliMi == true)
        {
            _duzenleDialogAcik = false;
            _duzenlenen = null;
            snackbar.Add(dil.T("bildirim.guncellendi", "Güncellendi"), Severity.Success);
            await Yukle();
        }
        else
        {
            snackbar.Add(dil.T("bildirim.hata", "Hata oluştu"), Severity.Error);
        }
    }

    private async Task TestEt(int id)
    {
        var sonuc = await Api.PostAsync<object>($"api/ai/saglayici/{id}/test", null!);
        snackbar.Add(dil.T("ai.testTamam", "Test tamamlandı"), Severity.Info);
    }

    private string SaglayiciGirisEtiketi(AISaglayiciTipi tip, bool duzenleme)
    {
        return tip switch
        {
            AISaglayiciTipi.LlamaLocal => duzenleme
                ? dil.T("ai.baseUrlDuzenle", "Base URL (boş bırakılırsa değişmez)")
                : dil.T("ai.baseUrl", "Base URL"),
            AISaglayiciTipi.DeepSeek => duzenleme
                ? dil.T("ai.deepSeekApiDuzenle", "DeepSeek API Key (boş bırakılırsa değişmez)")
                : dil.T("ai.deepSeekApi", "DeepSeek API Key"),
            _ => duzenleme
                ? dil.T("ai.apiKeyDuzenle", "API Key (boş bırakılırsa değişmez)")
                : dil.T("ai.apiKey", "API Key")
        };
    }

    private string SaglayiciYardimMetni(AISaglayiciTipi tip, bool duzenleme)
    {
        return tip switch
        {
            AISaglayiciTipi.LlamaLocal => dil.T(
                "ai.llamaYerelYardim",
                "OpenAI uyumlu servislerin base URL bilgisini girin. Örnek: http://127.0.0.1:11434 veya sağlayıcınızın /v1/chat/completions sunucusu."
            ),
            AISaglayiciTipi.DeepSeek => dil.T(
                "ai.deepSeekYardim",
                "DeepSeek kod üretimi için kullanılır. Anahtarı buraya yapıştırmak yerine mümkünse güvenli ortam değişkeni tercih edin."
            ),
            _ => duzenleme
                ? dil.T("ai.apiKeyYardimDuzenle", "Bu alanı boş bırakırsanız mevcut anahtar korunur.")
                : dil.T("ai.apiKeyYardim", "Sağlayıcının API anahtarını girin.")
        };
    }

    private static string DurumMetni(AICagriDurumu d) => d switch
    {
        AICagriDurumu.Basarili => "Başarılı",
        AICagriDurumu.Hata => "Hata",
        AICagriDurumu.LimitAsildi => "Limit Aşıldı",
        _ => d.ToString()
    };

    private static Color DurumRengi(AICagriDurumu d) => d switch
    {
        AICagriDurumu.Basarili => Color.Success,
        AICagriDurumu.Hata => Color.Error,
        AICagriDurumu.LimitAsildi => Color.Warning,
        _ => Color.Default
    };
}
