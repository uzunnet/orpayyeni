using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using VizitLink3D.UI.Servisler;
using VizitLink3D.UI.Models;

namespace VizitLink3D.UI.Pages.Admin;

public partial class SayfaDuzenle : ComponentBase
{
    [Parameter] public string? Slug { get; set; }
    
    [Inject] private NavigationManager NavManager { get; set; } = default!;
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogServisi { get; set; } = default!;

    private MudForm _form = default!;
    private SayfaYonetimi.SayfaTanim _sayfa = new();
    
    private string _sayfaBasligi = string.Empty;
    private string _sayfaIcerigi = string.Empty;
    
    // Medya & Video Alanları
    private string _videoUrl = string.Empty;
    private string _youtubeUrl = string.Empty;
    private string _videoSure = string.Empty;
    private string _videoHiz = "1.0";
    private bool _videoMute = true;

    // Döküman & Katalog
    private string _pdfKatalogUrl = string.Empty;

    // Ürün Adet Ayarı
    private string _urunAdet = "12";

    private bool _kaydediliyor;
    private bool _yeniKayit;

    protected override async Task OnInitializedAsync()
    {
        _yeniKayit = string.IsNullOrEmpty(Slug);

        if (!_yeniKayit)
        {
            _sayfa.Bolum = Slug!;
            await IcerikYukleAsync();
        }
    }

    private async Task IcerikYukleAsync()
    {
        var yanit = await Api.GetAsync<Dictionary<string, string>>($"api/sayfa-icerigi/{_sayfa.Bolum}");
        
        if (yanit != null)
        {
            _sayfaBasligi = yanit.GetValueOrDefault("SayfaBasligi", "");
            _sayfaIcerigi = yanit.GetValueOrDefault("SayfaIcerigi", "");
            
            _videoUrl = yanit.GetValueOrDefault("VideoUrl", "");
            _youtubeUrl = yanit.GetValueOrDefault("YoutubeUrl", "");
            _videoSure = yanit.GetValueOrDefault("VideoSure", "");
            _videoHiz = yanit.GetValueOrDefault("VideoHiz", "1.0");
            _videoMute = yanit.GetValueOrDefault("VideoMute", "true") == "true";
            
            _pdfKatalogUrl = yanit.GetValueOrDefault("PdfKatalogUrl", "");
            _urunAdet = yanit.GetValueOrDefault("UrunAdet", "12");
            
            // Eğer _sayfaBasligi boşsa fallback olarak Slug göster
            if (string.IsNullOrEmpty(_sayfaBasligi))
            {
                _sayfaBasligi = Slug!;
            }
        }
    }

    private void IptalEt() => NavManager.NavigateTo("admin/sayfa-yonetimi");

    private async Task DosyaYukleAsync(IBrowserFile dosya)
    {
        if (dosya != null)
        {
            using var icerik = new MultipartFormDataContent();
            var dosyaIcerigi = new StreamContent(dosya.OpenReadStream(20 * 1024 * 1024)); // 20MB limit
            icerik.Add(dosyaIcerigi, "dosya", dosya.Name);

            var sonuc = await Api.PostMultipartAsync<string>("api/sayfa-icerigi/yukle/katalog", icerik);
            if (sonuc?.BasariliMi == true)
            {
                _pdfKatalogUrl = sonuc.Veri ?? string.Empty;
                Snackbar.Add("Katalog başarıyla yüklendi.", Severity.Success);
            }
            else
            {
                Snackbar.Add("Katalog yüklenirken hata oluştu.", Severity.Error);
            }
        }
    }

    private async Task KaydetAsync()
    {
        await _form.ValidateAsync();
        if (!_form.IsValid) return;

        _kaydediliyor = true;

        try
        {
            // Veri sözlüğü hazırlayalım
            var icerikler = new Dictionary<string, string>
            {
                { "SayfaBasligi", _sayfaBasligi },
                { "SayfaIcerigi", _sayfaIcerigi },
                { "VideoUrl", _videoUrl },
                { "YoutubeUrl", _youtubeUrl },
                { "VideoSure", _videoSure },
                { "VideoHiz", _videoHiz },
                { "VideoMute", _videoMute ? "true" : "false" },
                { "PdfKatalogUrl", _pdfKatalogUrl },
                { "UrunAdet", _urunAdet }
            };

            foreach (var kvp in icerikler)
            {
                await Api.PutAsync<object>("api/sayfa-icerigi", new 
                { 
                    Bolum = _sayfa.Bolum, 
                    Anahtar = kvp.Key, 
                    Deger = kvp.Value, 
                    Dil = "tr" 
                });
            }

            Snackbar.Add("Sayfa başarıyla kaydedildi.", Severity.Success);
            NavManager.NavigateTo("admin/sayfa-yonetimi");
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Kayıt hatası: {ex.Message}", Severity.Error);
        }
        finally
        {
            _kaydediliyor = false;
        }
    }

    private async Task AICeviriDialogAc()
    {
        var parameters = new DialogParameters
        {
            { "KayitId", 0 },
            { "TabloOnEki", $"SayfaIcerigi_{_sayfa.Bolum}" },
            { "CevrilecekAlanlar", new Dictionary<string, string>
                {
                    { "SayfaBasligi", _sayfaBasligi ?? "" },
                    { "SayfaIcerigi", _sayfaIcerigi ?? "" }
                }
            }
        };

        var dialog = await DialogServisi.ShowAsync<VizitLink3D.UI.Bilesenler.Admin.AICeviriDialog>("🌍 Yapay Zeka Çevirisi", parameters);
        await dialog.Result;
    }
}

