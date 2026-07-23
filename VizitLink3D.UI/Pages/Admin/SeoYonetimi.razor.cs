using Microsoft.AspNetCore.Components;
using MudBlazor;
using VizitLink3D.UI.Models;
using VizitLink3D.UI.Servisler;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace VizitLink3D.UI.Pages.Admin;

public partial class SeoYonetimi : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private string? _seciliBolum;
    private string _title = string.Empty;
    private string _desc = string.Empty;
    private string _keywords = string.Empty;

    private bool _yukleniyor = true;
    private bool _kaydediliyor = false;
    private List<string> _sayfaListesi = new();

    public class SayfaIcerigiDto
    {
        public string Bolum { get; set; } = string.Empty;
        public string Anahtar { get; set; } = string.Empty;
        public string Deger { get; set; } = string.Empty;
    }

    protected override async Task OnInitializedAsync()
    {
        await SayfalariYukleAsync();
    }

    private async Task SayfalariYukleAsync()
    {
        _yukleniyor = true;
        try
        {
            var icerikler = await Api.GetAsync<List<SayfaIcerigiDto>>("api/sayfa-icerigi");
            if (icerikler != null && icerikler.Any())
            {
                _sayfaListesi = icerikler.Select(x => x.Bolum).Distinct().ToList();
                if (!_sayfaListesi.Contains("anasayfa")) _sayfaListesi.Insert(0, "anasayfa");
                
                // Select the first one by default
                _seciliBolum = _sayfaListesi.FirstOrDefault();
                if (!string.IsNullOrEmpty(_seciliBolum))
                {
                    await SeciliSayfaDegisti(_seciliBolum);
                }
            }
            else
            {
                // Fallback basic list
                _sayfaListesi = new List<string> { "anasayfa", "vizyon-misyon", "hakkimizda", "iletisim" };
                _seciliBolum = "anasayfa";
            }
        }
        catch
        {
            _sayfaListesi = new List<string> { "anasayfa", "vizyon-misyon", "hakkimizda", "iletisim" };
            _seciliBolum = "anasayfa";
        }
        finally
        {
            _yukleniyor = false;
        }
    }

    private async Task SeciliSayfaDegisti(string bolum)
    {
        _seciliBolum = bolum;
        _yukleniyor = true;
        try
        {
            var yanit = await Api.GetAsync<Dictionary<string, string>>($"api/sayfa-icerigi/{bolum}");
            if (yanit != null)
            {
                _title = yanit.GetValueOrDefault("SeoTitle", yanit.GetValueOrDefault("SayfaBasligi", ""));
                _desc = yanit.GetValueOrDefault("SeoDescription", "");
                _keywords = yanit.GetValueOrDefault("SeoKeywords", "");
            }
            else
            {
                _title = ""; _desc = ""; _keywords = "";
            }
        }
        catch
        {
            _title = ""; _desc = ""; _keywords = "";
        }
        finally
        {
            _yukleniyor = false;
        }
    }

    private async Task KaydetAsync()
    {
        if (string.IsNullOrEmpty(_seciliBolum)) return;
        _kaydediliyor = true;

        try
        {
            var keys = new Dictionary<string, string>
            {
                { "SeoTitle", _title },
                { "SeoDescription", _desc },
                { "SeoKeywords", _keywords }
            };

            foreach (var kvp in keys)
            {
                await Api.PutAsync<object>("api/sayfa-icerigi", new 
                { 
                    Bolum = _seciliBolum, 
                    Anahtar = kvp.Key, 
                    Deger = kvp.Value, 
                    Dil = "tr" 
                });
            }

            Snackbar.Add("SEO ayarları başarıyla kaydedildi.", Severity.Success);
        }
        catch
        {
            Snackbar.Add("SEO ayarları kaydedilirken hata oluştu.", Severity.Error);
        }
        finally
        {
            _kaydediliyor = false;
        }
    }
}

