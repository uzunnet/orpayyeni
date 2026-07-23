using Microsoft.AspNetCore.SignalR;

namespace VizitLink3D.Api.Hubs;

public class TemaHub : Hub
{
    public async Task FirmayaKatil(string firmaId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, firmaId);
    }

    public async Task FirmadanAyril(string firmaId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, firmaId);
    }
}
