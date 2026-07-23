using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Moduller.IsTakip.Kontrolcu;

[ApiController]
[Route("api/is-takip")]
[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,SuperAdmin")]
public class IsTakipKontrolcu(VizitLink3DDbContext vt) : ControllerBase
{
    [HttpGet]
    public async Task<Cevap<List<IsTakipKaydi>>> Liste(
        [FromQuery] string? durum = null,
        [FromQuery] string? kategori = null)
    {
        var sorgu = vt.IsTakipKayitlari.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(durum) && durum != "Tumu")
            sorgu = sorgu.Where(i => i.Durum == durum);

        if (!string.IsNullOrEmpty(kategori) && kategori != "Tumu")
            sorgu = sorgu.Where(i => i.Kategori == kategori);

        var liste = await sorgu
            .OrderByDescending(i => i.Oncelik == "Kritik")
            .ThenByDescending(i => i.Oncelik == "Yuksek")
            .ThenBy(i => i.SiraNo)
            .ToListAsync();

        return Cevap<List<IsTakipKaydi>>.Basarili(liste);
    }

    [HttpGet("istatistik")]
    public async Task<Cevap<IsTakipIstatistik>> Istatistik()
    {
        var tumu = await vt.IsTakipKayitlari.CountAsync();
        var bekleyen = await vt.IsTakipKayitlari.CountAsync(i => i.Durum == "Bekliyor");
        var yapiliyor = await vt.IsTakipKayitlari.CountAsync(i => i.Durum == "Yapiliyor");
        var tamamlanan = await vt.IsTakipKayitlari.CountAsync(i => i.Durum == "Tamamlandi");
        var kritik = await vt.IsTakipKayitlari.CountAsync(i => i.Oncelik == "Kritik" && i.Durum != "Tamamlandi");

        return Cevap<IsTakipIstatistik>.Basarili(new IsTakipIstatistik
        {
            Toplam = tumu,
            Bekleyen = bekleyen,
            Yapiliyor = yapiliyor,
            Tamamlanan = tamamlanan,
            Kritik = kritik
        });
    }

    [HttpPost]
    public async Task<Cevap<IsTakipKaydi>> Ekle([FromBody] IsTakipKaydi kayit)
    {
        kayit.OlusturulmaTarihi = DateTime.UtcNow;
        vt.IsTakipKayitlari.Add(kayit);
        await vt.SaveChangesAsync();
        return Cevap<IsTakipKaydi>.Basarili(kayit, "İş kaydı eklendi.");
    }

    [HttpPut("{id:int}")]
    public async Task<Cevap<IsTakipKaydi>> Guncelle(int id, [FromBody] IsTakipKaydi kayit)
    {
        var mevcut = await vt.IsTakipKayitlari.FindAsync(id);
        if (mevcut is null) return Cevap<IsTakipKaydi>.Hata("Kayıt bulunamadı.");

        mevcut.Baslik = kayit.Baslik;
        mevcut.Aciklama = kayit.Aciklama;
        mevcut.Durum = kayit.Durum;
        mevcut.Oncelik = kayit.Oncelik;
        mevcut.Kategori = kayit.Kategori;
        mevcut.SiraNo = kayit.SiraNo;
        mevcut.GuncellenmeTarihi = DateTime.UtcNow;

        if (kayit.Durum == "Tamamlandi" && mevcut.TamamlanmaTarihi == null)
            mevcut.TamamlanmaTarihi = DateTime.UtcNow;

        await vt.SaveChangesAsync();
        return Cevap<IsTakipKaydi>.Basarili(mevcut, "İş kaydı güncellendi.");
    }

    [HttpDelete("{id:int}")]
    public async Task<Cevap<bool>> Sil(int id)
    {
        var mevcut = await vt.IsTakipKayitlari.FindAsync(id);
        if (mevcut is null) return Cevap<bool>.Hata("Kayıt bulunamadı.");

        mevcut.SilindiMi = true;
        mevcut.SilinmeTarihi = DateTime.UtcNow;
        await vt.SaveChangesAsync();
        return Cevap<bool>.Basarili(true, "İş kaydı silindi.");
    }
}
