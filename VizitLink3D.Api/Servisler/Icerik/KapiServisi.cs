using VizitLink3D.Api.Modeller;
using VizitLink3D.Ortak.Modeller;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Servisler.Icerik;

public class KapiServisi(VizitLink3D.Api.VeriTabani.VizitLink3DDbContext db)
{
    public async Task<List<KapakModeli>> AktifModelleriGetirAsync()
    {
        return await db.KapakModelleri
            .AsNoTracking()
            .Include(k => k.GaleriResimleri)
            .Where(k => !k.SilindiMi)
            .OrderBy(k => k.SiraNo)
            .ToListAsync();
    }

    public async Task<KapakModeli?> ModeleGoreGetirAsync(int id)
    {
        return await db.KapakModelleri
            .AsNoTracking()
            .Include(k => k.GaleriResimleri)
            .FirstOrDefaultAsync(k => k.Id == id && !k.SilindiMi);
    }

    public async Task<List<KapakModeli>> KategoriyeGoreGetirAsync(int kategoriId)
    {
        return await db.KapakModelleri
            .AsNoTracking()
            .Include(k => k.GaleriResimleri)
            .Where(k => k.KategoriId == kategoriId && !k.SilindiMi)
            .OrderBy(k => k.SiraNo)
            .ToListAsync();
    }

    public async Task EkleAsync(KapakModeli model)
    {
        model.OlusturulmaTarihi = DateTime.UtcNow;
        db.KapakModelleri.Add(model);
        await db.SaveChangesAsync();
    }

    public async Task GuncelleAsync(KapakModeli model)
    {
        model.GuncellenmeTarihi = DateTime.UtcNow;
        db.KapakModelleri.Update(model);
        await db.SaveChangesAsync();
    }

    public async Task SilAsync(int id)
    {
        var model = await db.KapakModelleri.FindAsync(id);
        if (model != null)
        {
            model.SilindiMi = true;
            model.SilinmeTarihi = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
    }
}
