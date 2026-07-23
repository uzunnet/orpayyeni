using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Api.Modeller;
using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.Api.Kontrolculer.Iletisim;

/// <summary>
/// Canl� sohbet mesajlar�n� ve oturumlar�n� y�neten API kontrolc�s�.
/// </summary>
[ApiController]
[Route("api/sohbet")]
public class SohbetKontrolcu : ControllerBase
{
    private readonly VizitLink3DDbContext _vt;

    public SohbetKontrolcu(VizitLink3DDbContext vt)
    {
        _vt = vt;
    }

    /// <summary>
    /// T�m aktif sohbet oturumlar�n� gruplayarak getirir.
    /// </summary>
    [HttpGet("oturumlar")]
    public async Task<ActionResult<Cevap<List<object>>>> OturumlariGetir()
    {
        // Mesajlar� OturumId'ye g�re gruplay�p her oturumun son mesaj�n� al�yoruz
        var oturumlar = await _vt.CanliSohbetMesajlari
            .OrderByDescending(m => m.Tarih)
            .GroupBy(m => m.OturumId)
            .Select(g => new
            {
                OturumId = g.Key,
                Ad = g.First().GonderenAd,
                SonMesaj = g.First().MesajMetni,
                Tarih = g.First().Tarih,
                OkunmayanSayisi = g.Count(m => !m.OkunduMu && !m.YoneticiMi)
            })
            .ToListAsync();

        return Ok(new Cevap<List<object>>
        {
            Veri = oturumlar.Cast<object>().ToList(),
            BasariliMi = true,
            Mesaj = "Oturumlar ba�ar�yla getirildi"
        });
    }

    /// <summary>
    /// Belirli bir oturuma ait t�m mesaj ge�mi�ini getirir.
    /// </summary>
    [HttpGet("gecmis/{oturumId}")]
    public async Task<ActionResult<Cevap<List<CanliSohbetMesaji>>>> GecmisiGetir(string oturumId)
    {
        var mesajlar = await _vt.CanliSohbetMesajlari
            .Where(m => m.OturumId == oturumId)
            .OrderBy(m => m.Tarih)
            .ToListAsync();

        // Admin ge�mi�i okudu�u i�in okunmam�� mesajlar� i�aretliyoruz
        var okunmamislar = mesajlar.Where(m => !m.OkunduMu && !m.YoneticiMi).ToList();
        if (okunmamislar.Any())
        {
            okunmamislar.ForEach(m => m.OkunduMu = true);
            await _vt.SaveChangesAsync();
        }

        return Ok(new Cevap<List<CanliSohbetMesaji>>
        {
            Veri = mesajlar,
            BasariliMi = true,
            Mesaj = "Sohbet ge�mi�i ba�ar�yla getirildi"
        });
    }
}


