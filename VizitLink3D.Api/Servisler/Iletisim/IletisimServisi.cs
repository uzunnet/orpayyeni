using VizitLink3D.Ortak.Modeller;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Servisler.Iletisim;

public class IletisimServisi(VizitLink3D.Api.VeriTabani.VizitLink3DDbContext db)
{
    public async Task<List<IletisimMesaji>> BekleyenMesajlariGetirAsync()
    {
        return await db.IletisimMesajlari
            .Where(m => !m.CevaplandiMi)
            .OrderByDescending(m => m.Tarih)
            .ToListAsync();
    }

    public async Task<IletisimMesaji?> MesajGetirAsync(int id)
    {
        return await db.IletisimMesajlari.FindAsync(id);
    }

    public async Task OkunduIsaretleAsync(int id)
    {
        var mesaj = await db.IletisimMesajlari.FindAsync(id);
        if (mesaj != null && !mesaj.OkunduMu)
        {
            mesaj.OkunduMu = true;
            mesaj.OkunmaTarihi = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
    }

    public async Task CevaplaAsync(int id, string cevapMetni)
    {
        var mesaj = await db.IletisimMesajlari.FindAsync(id);
        if (mesaj != null)
        {
            mesaj.CevaplandiMi = true;
            mesaj.CevapTarihi = DateTime.UtcNow;
            mesaj.CevapMetni = cevapMetni;
            await db.SaveChangesAsync();
        }
    }
}
