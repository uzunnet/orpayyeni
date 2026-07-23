using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Components;

namespace VizitLink3D.UI.Bilesenler.Anasayfa;

public partial class SSSBolumu : ComponentBase
{
    private List<SikSorulanSoru> _sss = [];
    private readonly Dictionary<int, bool> _acikSorular = [];

    protected override async Task OnInitializedAsync()
    {
        try { var l = await api.GetAsync<List<SikSorulanSoru>>("api/sss"); if (l != null) _sss = l; } catch { /* API erişilemezse SSS bölümü boş kalır */ }
    }

    private void SoruAcKapa(int id)
    {
        if (_acikSorular.ContainsKey(id))
            _acikSorular[id] = !_acikSorular[id];
        else
            _acikSorular[id] = true;
    }
}
