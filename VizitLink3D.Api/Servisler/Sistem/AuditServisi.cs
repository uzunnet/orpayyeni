using VizitLink3D.Ortak.Modeller;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Servisler.Sistem;

public class AuditServisi(VizitLink3D.Api.VeriTabani.VizitLink3DDbContext db, IHttpContextAccessor httpErisimi)
{
    public async Task KaydetAsync(string eylem, string? eskiDeger = null, string? yeniDeger = null, string? kullaniciId = null, string? firmaId = null)
    {
        var ip = httpErisimi.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        var tarayici = httpErisimi.HttpContext?.Request?.Headers?.UserAgent.ToString();

        var kayit = new AuditLog
        {
            ZamanDamgasi = DateTime.UtcNow,
            Eylem = eylem,
            EskiDeger = eskiDeger,
            YeniDeger = yeniDeger,
            KullaniciId = kullaniciId,
            FirmaId = firmaId,
            IPAdresi = ip,
            Tarayici = tarayici,
            CorrelationId = httpErisimi.HttpContext?.TraceIdentifier
        };

        db.AuditLoglar.Add(kayit);
        await db.SaveChangesAsync();
    }

    public async Task<List<AuditLog>> SonKayitlariGetirAsync(int adet = 50, string? firmaId = null)
    {
        var sorgu = db.AuditLoglar.AsQueryable();

        if (!string.IsNullOrEmpty(firmaId))
            sorgu = sorgu.Where(a => a.FirmaId == firmaId);

        return await sorgu
            .OrderByDescending(a => a.ZamanDamgasi)
            .Take(adet)
            .ToListAsync();
    }
}
