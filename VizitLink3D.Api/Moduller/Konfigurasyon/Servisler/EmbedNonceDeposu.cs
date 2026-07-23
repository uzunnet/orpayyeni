using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller.Guvenlik;

namespace VizitLink3D.Api.Moduller.Konfigurasyon.Servisler;

/// <summary>
/// Embed nonce deposu soyutlamasi.
/// 
/// AMAC: Nonce one-time consume (atomik tuketim) islemini soyutlamak.
/// EmbedTokenServisi bu soyutlamaya baglidir; dogrudan IMemoryCache/IDistributedCache BILMEZ.
/// 
/// MEVCUT IMPLEMENTASYON: Veritabani tabanli (PostgreSQL/SQLite multi-instance).
///   - Atomiklik: NonceHash unique constraint ile DB seviyesinde saglanir.
///   - DbUpdateException yakalanarak eszamanli insert'te yalniz bir istek basarili olur.
///   - TTL: SonKullanmaTarihi gecmis kayitlar soft-delete (SilindiMi=true) ile temizlenir.
///   - Fiziksel DELETE YOKTUR.
/// 
/// GUvENLIK:
///   - Nonce veritabaninda plaintext degil, SHA256 hash olarak saklanir.
///   - NonceHash [JsonIgnore] ile API yanitlarinda gonderilmez.
///   - Multi-instance SaaS: database-per-tenant veya shared DB fark etmez,
///     unique constraint instance'lar arasi atomik koruma saglar.
/// </summary>
public interface IEmbedNonceDeposu
{
    /// <summary>
    /// Nonce'i atomik olarak dener ve kaydeder.
    /// Nonce daha once gorulmemisse kaydedip true doner (ilk tuketim basarili).
    /// Nonce daha once gorulmusse false doner (replay saldirisi).
    /// TTL sonunda nonce soft-delete ile temizlenir.
    /// </summary>
    /// <param name="nonce">Hex formatinda nonce degeri (min 16 karakter)</param>
    /// <param name="ttl">Nonce'in gecerli olacagi sure</param>
    /// <returns>true: ilk tuketim basarili, false: replay (daha once gorulmus)</returns>
    Task<bool> DeneVeKaydetAsync(string nonce, TimeSpan ttl);
}

/// <summary>
/// IEmbedNonceDeposu'nun veritabani tabanli implementasyonu.
/// 
/// GUvENLIK:
/// - NonceHash unique constraint ile DB seviyesinde atomik one-time consume.
///   Eszamanli bootstrap isteklerinde ayni nonce yalniz bir kez tuketilir.
/// - DbUpdateException yakalanir, diger hatalar yukari firlatilir (controller try-catch YOK).
/// - TTL asmis kayitlar soft-delete (SilindiMi=true) ile temizlenir,
///   fiziksel DELETE YOKTUR.
/// 
/// SINIRLAMA:
/// - Yok. Multi-instance SaaS ortaminda PostgreSQL/SQLite unique constraint
///   instance'lar arasi atomiklik saglar.
/// </summary>
public class EmbedNonceDeposu : IEmbedNonceDeposu
{
    private readonly VizitLink3DDbContext _dbContext;

    public EmbedNonceDeposu(VizitLink3DDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc/>
    public async Task<bool> DeneVeKaydetAsync(string nonce, TimeSpan ttl)
    {
        // Guvenlik: gecersiz/kisa nonce reddedilir
        if (string.IsNullOrWhiteSpace(nonce) || nonce.Length < 16)
            return false;

        // Nonce hash'ini hesapla — duz metin ASLA saklanmaz
        var nonceBytes = Encoding.UTF8.GetBytes(nonce);
        var hashBytes = SHA256.HashData(nonceBytes);
        var nonceHash = Convert.ToHexStringLower(hashBytes);

        // TTL asmis (son kullanma tarihi gecmis) kayitlari soft-delete ile temizle.
        // Fiziksel DELETE YOK — SilindiMi = true yapilir.
        var suresiGecmisKayitlar = await _dbContext
            .Set<EmbedOturumNonceKaydi>()
            .Where(k => !k.SilindiMi && k.SonKullanmaTarihi < DateTime.UtcNow)
            .ToListAsync();

        if (suresiGecmisKayitlar.Count > 0)
        {
            foreach (var kayit in suresiGecmisKayitlar)
            {
                kayit.SilindiMi = true;
                kayit.SilinmeTarihi = DateTime.UtcNow;
            }
        }

        // Yeni nonce kaydini olustur ve ekle
        var yeniKayit = new EmbedOturumNonceKaydi
        {
            NonceHash = nonceHash,
            SonKullanmaTarihi = DateTime.UtcNow + ttl,
            OlusturulmaTarihi = DateTime.UtcNow
        };

        try
        {
            _dbContext.Set<EmbedOturumNonceKaydi>().Add(yeniKayit);
            await _dbContext.SaveChangesAsync();
            return true; // Ilk tuketim basarili
        }
        catch (DbUpdateException)
        {
            // Unique constraint ihlali → ayni nonce zaten tuketilmis (replay saldirisi)
            return false;
        }
        // NOT: Diger exception'lar yakalanmaz, yukari firlar.
        // Controller'da try-catch YOKTUR — HataYonetimiMiddleware yakalar.
    }
}
