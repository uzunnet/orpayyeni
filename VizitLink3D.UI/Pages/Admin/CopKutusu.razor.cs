using System.Text.Json;
using VizitLink3D.UI.Servisler;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace VizitLink3D.UI.Pages.Admin;

public partial class CopKutusu : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private List<SilinmisUrun> _silinmisUrunler = [];
    private List<SilinmisMenu> _silinmisMenuler = [];
    private List<SilinmisModel> _silinmisModeller = [];
    private bool _yukleniyor = true;

    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(UrunleriYukle(), MenuleriYukle(), ModelleriYukle());
        _yukleniyor = false;
    }

    private async Task UrunleriYukle()
    {
        try
        {
            var liste = await Api.GetAsync<List<SilinmisUrun>>("api/cop-kutusu/urunler");
            if (liste != null) _silinmisUrunler = liste;
        }
        catch { }
    }

    private async Task MenuleriYukle()
    {
        try
        {
            var liste = await Api.GetAsync<List<SilinmisMenu>>("api/cop-kutusu/menu-ogeleri");
            if (liste != null) _silinmisMenuler = liste;
        }
        catch { }
    }

    private async Task ModelleriYukle()
    {
        try
        {
            var liste = await Api.GetAsync<List<SilinmisModel>>("api/cop-kutusu/modeller");
            if (liste != null) _silinmisModeller = liste;
        }
        catch { }
    }

    private async Task UrunGeriAl(SilinmisUrun u)
    {
        var cevap = await Api.PostAsync<object>($"api/cop-kutusu/urun/{u.Id}/geri-al", new { });
        Snackbar.Add(cevap?.BasariliMi == true ? "Urun geri alindi." : "Geri alma basarisiz.", cevap?.BasariliMi == true ? Severity.Success : Severity.Error);
        if (cevap?.BasariliMi == true) await UrunleriYukle();
    }

    private async Task MenuGeriAl(SilinmisMenu m)
    {
        var cevap = await Api.PostAsync<object>($"api/cop-kutusu/menu/{m.Id}/geri-al", new { });
        Snackbar.Add(cevap?.BasariliMi == true ? "Menu geri alindi." : "Geri alma basarisiz.", cevap?.BasariliMi == true ? Severity.Success : Severity.Error);
        if (cevap?.BasariliMi == true) await MenuleriYukle();
    }

    private async Task ModelGeriAl(SilinmisModel m)
    {
        var cevap = await Api.PostAsync<object>($"api/cop-kutusu/model/{m.Id}/geri-al", new { });
        Snackbar.Add(cevap?.BasariliMi == true ? "Model geri alindi." : "Geri alma basarisiz.", cevap?.BasariliMi == true ? Severity.Success : Severity.Error);
        if (cevap?.BasariliMi == true) await ModelleriYukle();
    }

    public class SilinmisUrun
    {
        public int Id { get; set; }
        public string Ad { get; set; } = "";
        public string Kod { get; set; } = "";
        public string Slug { get; set; } = "";
        public DateTime? SilinmeTarihi { get; set; }
    }

    public class SilinmisMenu
    {
        public int Id { get; set; }
        public string Baslik { get; set; } = "";
        public string Url { get; set; } = "";
        public string Konum { get; set; } = "";
        public DateTime? SilinmeTarihi { get; set; }
    }

    public class SilinmisModel
    {
        public int Id { get; set; }
        public string ModelAdi { get; set; } = "";
        public string ModelYolu { get; set; } = "";
        public DateTime? SilinmeTarihi { get; set; }
    }
}
