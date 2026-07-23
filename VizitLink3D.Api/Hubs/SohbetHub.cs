using Microsoft.AspNetCore.SignalR;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Api.Modeller;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Hubs;

public class SohbetHub : Hub
{
    private readonly VizitLink3DDbContext _vt;

    public SohbetHub(VizitLink3DDbContext vt)
    {
        _vt = vt;
    }

    // Müşterinin mesaj göndermesi
    public async Task MusteriMesajGonder(string oturumId, string gonderenAd, string mesajMetni)
    {
        var yeniMesaj = new CanliSohbetMesaji
        {
            OturumId = oturumId,
            GonderenAd = gonderenAd,
            MesajMetni = mesajMetni,
            YoneticiMi = false,
            Tarih = DateTime.UtcNow,
            OkunduMu = false
        };

        _vt.CanliSohbetMesajlari.Add(yeniMesaj);
        await _vt.SaveChangesAsync();

        // Müşterinin kendine mesajın ulaştığını bildirmesi
        await Clients.Caller.SendAsync("MesajIletildi", yeniMesaj);
        
        // Tüm adminlere "YeniMesaj" eventini yolla
        await Clients.Group("Yonetici").SendAsync("YeniMesajGeldi", yeniMesaj);
    }

    // Yöneticinin cevap göndermesi
    public async Task YoneticiMesajGonder(string oturumId, string mesajMetni)
    {
        var yeniMesaj = new CanliSohbetMesaji
        {
            OturumId = oturumId,
            GonderenAd = "Yönetici",
            MesajMetni = mesajMetni,
            YoneticiMi = true,
            Tarih = DateTime.UtcNow,
            OkunduMu = false
        };

        _vt.CanliSohbetMesajlari.Add(yeniMesaj);
        await _vt.SaveChangesAsync();

        // İlgili müşteriye (oturumId ile grubuna) yolla
        await Clients.Group(oturumId).SendAsync("YeniMesajGeldi", yeniMesaj);
        
        // Adminlere de gönder ki ekran güncellensin
        await Clients.Group("Yonetici").SendAsync("MesajIletildi", yeniMesaj);
    }

    // Admin olarak SignalR grubuna katılma
    public async Task YoneticiOlarakBaglan()
    {
        // Admin JWT token içeriyorsa bu kısım otomatik yapılabilir, şimdilik manuel ekliyoruz
        await Groups.AddToGroupAsync(Context.ConnectionId, "Yonetici");
    }

    // Müşteri olarak gruba katılma
    public async Task MusteriOlarakBaglan(string oturumId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, oturumId);
    }
}
