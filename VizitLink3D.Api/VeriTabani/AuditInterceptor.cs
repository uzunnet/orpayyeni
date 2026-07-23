using System.Text.Json;
using VizitLink3D.Ortak.Modeller;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace VizitLink3D.Api.VeriTabani;

public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor? _httpErisimi;

    public AuditInterceptor(IHttpContextAccessor? httpErisimi = null)
    {
        _httpErisimi = httpErisimi;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData olayVerisi, InterceptionResult<int> sonuc)
    {
        AuditKayitlariniEkle(olayVerisi.Context!);
        return base.SavingChanges(olayVerisi, sonuc);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData olayVerisi, InterceptionResult<int> sonuc, CancellationToken iptal = default)
    {
        AuditKayitlariniEkle(olayVerisi.Context!);
        return await base.SavingChangesAsync(olayVerisi, sonuc, iptal);
    }

    private void AuditKayitlariniEkle(DbContext baglam)
    {
        if (baglam is not VizitLink3DDbContext vt)
            return;

        // Uygulama açılışındaki tohumlama ve bakım işlemleri bir kullanıcı isteği değildir.
        // Bunları denetlemek hem anlamsız kayıt üretir hem de SQLite üzerinde açılışı kilitleyebilir.
        var httpBaglami = _httpErisimi?.HttpContext;
        if (httpBaglami is null)
            return;

        var degisiklikler = baglam.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            // AuditLog entity'sinin kendisi icin AuditLog yazma (sonsuz dongu / recursion onleyici).
            // Tohum verisi ve toplu islemlerde yuzlerce gereksiz AuditLog satirini de engeller.
            .Where(e => e.Entity is not AuditLog)
            .ToList();

        if (!degisiklikler.Any())
            return;

        var ip = httpBaglami.Connection.RemoteIpAddress?.ToString();
        var tarayici = httpBaglami.Request.Headers.UserAgent.ToString();
        var correlationId = httpBaglami.TraceIdentifier;

        foreach (var giris in degisiklikler)
        {
            var entiteAdi = giris.Entity.GetType().Name;
            var eylem = giris.State switch
            {
                EntityState.Added => $"{entiteAdi}.Eklendi",
                EntityState.Modified => $"{entiteAdi}.Guncellendi",
                EntityState.Deleted => $"{entiteAdi}.Silindi",
                _ => $"{entiteAdi}.Bilinmeyen"
            };

            string? eskiDeger = null;
            string? yeniDeger = null;

            if (giris.State == EntityState.Modified)
            {
                var eski = new Dictionary<string, object?>();
                var yeni = new Dictionary<string, object?>();

                foreach (var ozellik in giris.Properties.Where(p => p.IsModified))
                {
                    eski[ozellik.Metadata.Name] = ozellik.OriginalValue;
                    yeni[ozellik.Metadata.Name] = ozellik.CurrentValue;
                }

                eskiDeger = JsonSerializer.Serialize(eski);
                yeniDeger = JsonSerializer.Serialize(yeni);
            }
            else if (giris.State == EntityState.Added)
            {
                var deger = new Dictionary<string, object?>();
                foreach (var ozellik in giris.Properties)
                    deger[ozellik.Metadata.Name] = ozellik.CurrentValue;

                yeniDeger = JsonSerializer.Serialize(deger);
            }
            else if (giris.State == EntityState.Deleted)
            {
                var deger = new Dictionary<string, object?>();
                foreach (var ozellik in giris.Properties)
                    deger[ozellik.Metadata.Name] = ozellik.OriginalValue;

                eskiDeger = JsonSerializer.Serialize(deger);
            }

            vt.AuditLoglar.Add(new AuditLog
            {
                ZamanDamgasi = DateTime.UtcNow,
                Eylem = eylem,
                EskiDeger = eskiDeger,
                YeniDeger = yeniDeger,
                IPAdresi = ip,
                Tarayici = tarayici,
                CorrelationId = correlationId
            });
        }
    }
}
