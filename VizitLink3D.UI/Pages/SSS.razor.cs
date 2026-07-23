using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.UI.Pages;

public partial class SSS
{
    private List<SikSorulanSoru> _sss = [];
    private bool _yukleniyor = true;

    protected override async Task OnInitializedAsync()
    {
        _sss = await api.GetAsync<List<SikSorulanSoru>>("api/sss") ?? [];
        _yukleniyor = false;
    }
}
