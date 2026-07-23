using Microsoft.AspNetCore.Components;
using MudBlazor;
using VizitLink3D.UI.Models;

namespace VizitLink3D.UI.Pages.Admin;

public partial class DilVeCeviri : ComponentBase
{
    private List<DilBilgisi> diller = new()
    {
        new() { Kod = "TR", Ad = "Türkçe", Aktif = true, Oran = 100 },
        new() { Kod = "EN", Ad = "English", Aktif = true, Oran = 85 },
        new() { Kod = "DE", Ad = "Deutsch", Aktif = false, Oran = 30 },
        new() { Kod = "AR", Ad = "العربية", Aktif = false, Oran = 0 }
    };

    private bool _yukleniyor;

    class DilBilgisi
    {
        public string Kod { get; set; } = "";
        public string Ad { get; set; } = "";
        public bool Aktif { get; set; }
        public int Oran { get; set; }
    }

    protected override async Task OnInitializedAsync()
    {
        try 
        {
            var sonuc = await api.GetAsync<List<DilBilgisi>>("api/ayarlar/diller");
            if (sonuc != null && sonuc.Count > 0) 
            {
                diller = sonuc;
            }
        }
        catch 
        {
            // Fallback to defaults already defined
        }
        await base.OnInitializedAsync();
    }

    private async Task AiCeviriYap(DilBilgisi dil)
    {
        _yukleniyor = true;
        StateHasChanged();
        snackbar.Add($"{dil.Ad} için yapay zeka çevirisi başlatıldı...", Severity.Info);
        
        var sonuc = await api.PostAsync<object>($"api/ayarlar/diller/{dil.Kod}/ai-ceviri", new { });
        
        if (sonuc != null && sonuc.BasariliMi)
        {
            dil.Oran = 100;
            snackbar.Add($"{dil.Ad} çevirisi tamamlandı.", Severity.Success);
        }
        else
        {
            snackbar.Add($"{dil.Ad} çevirisi başarısız oldu.", Severity.Error);
        }
        
        _yukleniyor = false;
        StateHasChanged();
    }

    private async Task TumunuCevir()
    {
        _yukleniyor = true;
        StateHasChanged();
        snackbar.Add($"Tüm diller için çeviri işlemi başlatılıyor...", Severity.Info);
        
        var aktifDiller = diller.Where(d => d.Aktif).Select(d => d.Kod).ToList();
        var sonuc = await api.PostAsync<object>("api/ayarlar/diller/toplu-ai-ceviri", new { Diller = aktifDiller });
        
        if (sonuc != null && sonuc.BasariliMi)
        {
            foreach(var dil in diller)
            {
                if(!dil.Aktif) continue;
                dil.Oran = 100;
            }
            snackbar.Add($"Tüm aktif diller başarıyla çevrildi.", Severity.Success);
        }
        else
        {
            snackbar.Add($"Toplu çeviri işlemi başarısız oldu.", Severity.Error);
        }
        
        _yukleniyor = false;
        StateHasChanged();
    }
}

