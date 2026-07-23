using VizitLink3D.UI.Servisler;
using Microsoft.AspNetCore.Components;
using System.Text.Json;
using MudBlazor;

namespace VizitLink3D.UI.Pages.Test;

public partial class PiedraAnaliz : ComponentBase, IAsyncDisposable
{
    [Inject] private UcBoyutServisi UcBoyut { get; set; } = default!;

    private bool _analizYapiliyor;
    private string? _analizSonucu;
    private List<ParcaBilgisi> _parcalar = new();
    private const string KanvasId = "piedra-analiz-viewer";
    private bool _yuklendi;

    public class ParcaBilgisi
    {
        public int Sira { get; set; }
        public string Isim { get; set; } = "";
        public string Tip { get; set; } = "";
        public string Detay { get; set; } = "";
    }

    protected override async Task OnAfterRenderAsync(bool ilkRender)
    {
        if (ilkRender)
        {
            await UcBoyut.Baslat(KanvasId, "/medya/3d-modeller/piedra.glb", "#E8E4DF");
            _yuklendi = true;
            StateHasChanged();
        }
    }

    private async Task AnalizEt()
    {
        if (!_yuklendi) return;
        _analizYapiliyor = true;
        StateHasChanged();

        // Modelin yuklenmesi icin biraz bekle
        await Task.Delay(2000);

        var json = await UcBoyut.ModelAnalizEt(KanvasId);
        _analizSonucu = json;

        var parcalar = JsonSerializer.Deserialize<List<JsonElement>>(json);
        if (parcalar != null)
        {
            _parcalar = parcalar.Select((p, i) =>
            {
                var isim = p.GetProperty("isim").GetString() ?? "?";
                var tip = p.GetProperty("tip").GetString() ?? "?";
                string detay = tip == "Mesh"
                    ? $"🔺 {p.GetProperty("ucgenSayisi").GetInt32():N0} üçgen"
                    : $"📁 {p.GetProperty("cocukSayisi").GetInt32()} alt öğe";

                return new ParcaBilgisi { Sira = i + 1, Isim = isim, Tip = tip, Detay = detay };
            }).ToList();
        }

        _analizYapiliyor = false;
        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        await UcBoyut.Temizle(KanvasId);
    }
}
