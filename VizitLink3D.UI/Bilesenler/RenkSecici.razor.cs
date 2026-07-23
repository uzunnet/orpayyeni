using VizitLink3D.Ortak.Modeller.Urunler;
using Microsoft.AspNetCore.Components;

namespace VizitLink3D.UI.Bilesenler;

public partial class RenkSecici : ComponentBase
{
    [Parameter] public List<RalRengi> Renkler { get; set; } = [];
    [Parameter] public RalRengi? SeciliRenk { get; set; }
    [Parameter] public EventCallback<RalRengi> SeciliRenkDegisti { get; set; }
    [Parameter] public bool CokluSecim { get; set; } = false;
    [Parameter] public List<RalRengi> SecilenRenkler { get; set; } = [];
    [Parameter] public EventCallback<List<RalRengi>> SecilenRenklerDegisti { get; set; }

    private RalRengi? _secilenRenk;
    private string? _aktifGrup;
    private string _aramaMetni = "";

    private IEnumerable<string> RenkGruplari =>
        Renkler.Where(r => !string.IsNullOrEmpty(r.Grup))
               .Select(r => r.Grup!)
               .Distinct()
               .OrderBy(g => g);

    protected override void OnParametersSet()
    {
        if (SeciliRenk != null)
        {
            _secilenRenk = SeciliRenk;
        }
        else if (Renkler.Any() && _secilenRenk == null)
        {
            _secilenRenk = Renkler.First();
        }

        if (string.IsNullOrEmpty(_aktifGrup) && RenkGruplari.Any())
        {
            _aktifGrup = RenkGruplari.First();
        }
    }

    private async Task RenkSec(RalRengi renk)
    {
        if (CokluSecim)
        {
            if (SecilenRenkler.Any(r => r.Id == renk.Id))
            {
                SecilenRenkler.RemoveAll(r => r.Id == renk.Id);
            }
            else
            {
                SecilenRenkler.Add(renk);
            }
            await SecilenRenklerDegisti.InvokeAsync(SecilenRenkler);
        }
        else
        {
            _secilenRenk = renk;
            await SeciliRenkDegisti.InvokeAsync(renk);
        }
    }

    private async Task AktifGrubuSecVeyaKaldir()
    {
        var filtrelenmisRenkler = FiltreleRenkler().ToList();
        if (!filtrelenmisRenkler.Any()) return;

        bool hepsiSeciliMi = filtrelenmisRenkler.All(gr => SecilenRenkler.Any(sr => sr.Id == gr.Id));

        if (hepsiSeciliMi)
        {
            foreach (var r in filtrelenmisRenkler)
            {
                var silinecek = SecilenRenkler.FirstOrDefault(sr => sr.Id == r.Id);
                if (silinecek != null) SecilenRenkler.Remove(silinecek);
            }
        }
        else
        {
            foreach (var r in filtrelenmisRenkler)
            {
                if (!SecilenRenkler.Any(sr => sr.Id == r.Id))
                {
                    SecilenRenkler.Add(r);
                }
            }
        }

        await SecilenRenklerDegisti.InvokeAsync(SecilenRenkler);
    }

    private async Task TumKataloguSecVeyaKaldir()
    {
        bool hepsiSeciliMi = Renkler.All(r => SecilenRenkler.Any(sr => sr.Id == r.Id));

        if (hepsiSeciliMi)
        {
            SecilenRenkler.Clear();
        }
        else
        {
            foreach (var r in Renkler)
            {
                if (!SecilenRenkler.Any(sr => sr.Id == r.Id))
                {
                    SecilenRenkler.Add(r);
                }
            }
        }

        await SecilenRenklerDegisti.InvokeAsync(SecilenRenkler);
    }

    private bool RenkSeciliMi(RalRengi renk)
    {
        if (CokluSecim)
        {
            return SecilenRenkler.Any(r => r.Id == renk.Id);
        }
        return _secilenRenk?.Id == renk.Id;
    }

    private void GrupSec(string? grup)
    {
        _aktifGrup = grup;
    }

    private IEnumerable<RalRengi> FiltreleRenkler()
    {
        var sonuclar = Renkler.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(_aramaMetni))
        {
            sonuclar = sonuclar.Where(r =>
                (r.Ad != null && r.Ad.Contains(_aramaMetni, StringComparison.InvariantCultureIgnoreCase)) ||
                (r.Kod != null && r.Kod.Contains(_aramaMetni, StringComparison.InvariantCultureIgnoreCase)));
        }
        else if (!string.IsNullOrEmpty(_aktifGrup))
        {
            sonuclar = sonuclar.Where(r => r.Grup == _aktifGrup);
        }

        return sonuclar;
    }
}
