using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller.AI;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Moduller.AI.Servisler;

public interface IAIMaliyetTakipServisi
{
    Task<bool> LimitKontrolAsync(int saglayiciId, int? kullaniciId);
    Task KaydetAsync(int saglayiciId, int? kullaniciId, string amac, string promptOzet, int istekToken, int cevapToken, decimal maliyet, bool basarili, string? hata);
    Task<decimal> AylikKullanimAsync(int saglayiciId);
}

public class AIMaliyetTakipServisi : IAIMaliyetTakipServisi
{
    private readonly VizitLink3DDbContext _db;
    private const decimal AYLIK_LIMIT_USD = 100m;
    private const int GUNLUK_KULLANICI_LIMIT = 50;

    public AIMaliyetTakipServisi(VizitLink3DDbContext db) => _db = db;

    public async Task<bool> LimitKontrolAsync(int saglayiciId, int? kullaniciId)
    {
        var suankiAy = DateTime.UtcNow.Month;

        var aylik = await _db.AICagrisiKayitlari
            .Where(k => k.SaglayiciId == saglayiciId && k.OlusturulmaTarihi.Month == suankiAy)
            .SumAsync(k => k.ToplamMaliyetUsd);

        if (aylik >= AYLIK_LIMIT_USD) return false;

        if (kullaniciId.HasValue)
        {
            var suankiTarih = DateTime.UtcNow.Date;
            var kullaniciIdStr = kullaniciId.Value.ToString();

            var gunluk = await _db.AICagrisiKayitlari
                .CountAsync(k => k.KullaniciId == kullaniciIdStr && k.OlusturulmaTarihi.Date == suankiTarih);
            if (gunluk >= GUNLUK_KULLANICI_LIMIT) return false;
        }

        return true;
    }

    public async Task KaydetAsync(int saglayiciId, int? kullaniciId, string amac, string promptOzet, int istekToken, int cevapToken, decimal maliyet, bool basarili, string? hata)
    {
        var kayit = new AICagrisiKaydi
        {
            SaglayiciId = saglayiciId,
            KullaniciId = kullaniciId?.ToString(),
            KullanimAmaci = amac,
            IstekTokenSayisi = istekToken,
            CevapTokenSayisi = cevapToken,
            ToplamMaliyetUsd = maliyet,
            Prompt = promptOzet.Length > 500 ? promptOzet[..500] : promptOzet,
            Durum = basarili ? AICagriDurumu.Basarili : AICagriDurumu.Hata,
            HataMesaji = hata,
            SureMs = 0,
            OlusturulmaTarihi = DateTime.UtcNow
        };
        _db.AICagrisiKayitlari.Add(kayit);
        await _db.SaveChangesAsync();
    }

    public async Task<decimal> AylikKullanimAsync(int saglayiciId)
    {
        var suankiAy = DateTime.UtcNow.Month;
        return await _db.AICagrisiKayitlari
            .Where(k => k.SaglayiciId == saglayiciId && k.OlusturulmaTarihi.Month == suankiAy)
            .SumAsync(k => k.ToplamMaliyetUsd);
    }
}
