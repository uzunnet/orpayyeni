using Microsoft.AspNetCore.SignalR;

namespace VizitLink3D.Api.Hubs;

public class SahneAyarHub : Hub
{
    public async Task SahneAyarGuncellendi(int modelId, string ayarTipi, string ayarJson)
    {
        await Clients.Group($"sahne_{modelId}").SendAsync("SahneAyarGuncellendi", modelId, ayarTipi, ayarJson);
    }

    public async Task SahneGrubunaKatil(int modelId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"sahne_{modelId}");
    }

    public async Task UrunUcBoyutGrubunaKatil(int urunId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"urun_3d_{urunId}");
    }

    public async Task SahneGrubundanAyril(int modelId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"sahne_{modelId}");
    }

    public async Task UrunUcBoyutGrubundanAyril(int urunId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"urun_3d_{urunId}");
    }
}
