using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Kontrolculer;

[ApiController]
[Route("api/sayfa-duzen-ayarlari")]
public class SayfaDuzenAyariKontrolcu(VizitLink3DDbContext vt) : ControllerBase
{
    [HttpGet]
    public async Task<Cevap<List<SayfaDuzenAyari>>> Listele()
    {
        var ayarlar = await vt.SayfaDuzenAyarlari
            .AsNoTracking()
            .Where(a => !a.SilindiMi)
            .OrderBy(a => a.SayfaKodu)
            .ToListAsync();
        return Cevap<List<SayfaDuzenAyari>>.Basarili(ayarlar);
    }

    [HttpGet("{sayfaKodu}")]
    public async Task<Cevap<SayfaDuzenAyari>> Getir(string sayfaKodu)
    {
        var ayar = await vt.SayfaDuzenAyarlari
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.SayfaKodu == sayfaKodu && !a.SilindiMi);
        if (ayar == null)
            return Cevap<SayfaDuzenAyari>.Basarili(new SayfaDuzenAyari { SayfaKodu = sayfaKodu, SayfaAdi = sayfaKodu });
        return Cevap<SayfaDuzenAyari>.Basarili(ayar);
    }

    [HttpPut("{sayfaKodu}")]
    public async Task<Cevap<SayfaDuzenAyari>> Guncelle(string sayfaKodu, [FromBody] SayfaDuzenAyari guncelleme)
    {
        var ayar = await vt.SayfaDuzenAyarlari
            .FirstOrDefaultAsync(a => a.SayfaKodu == sayfaKodu && !a.SilindiMi);

        if (ayar == null)
        {
            guncelleme.SayfaKodu = sayfaKodu;
            guncelleme.OlusturulmaTarihi = DateTime.UtcNow;
            vt.SayfaDuzenAyarlari.Add(guncelleme);
        }
        else
        {
            ayar.SayfaAdi = guncelleme.SayfaAdi;
            ayar.SutunAdet = guncelleme.SutunAdet;
            ayar.SatirAdet = guncelleme.SatirAdet;
            ayar.SayfaBasinaAdet = guncelleme.SayfaBasinaAdet;
            ayar.SayfalamaAktif = guncelleme.SayfalamaAktif;
            ayar.AktifMi = guncelleme.AktifMi;
            ayar.GuncellenmeTarihi = DateTime.UtcNow;
        }

        await vt.SaveChangesAsync();
        return Cevap<SayfaDuzenAyari>.Basarili(ayar ?? guncelleme);
    }

    [HttpPut]
    public async Task<Cevap<bool>> TopluGuncelle([FromBody] List<SayfaDuzenAyari> ayarlar)
    {
        foreach (var guncelleme in ayarlar)
        {
            var ayar = await vt.SayfaDuzenAyarlari
                .FirstOrDefaultAsync(a => a.SayfaKodu == guncelleme.SayfaKodu && !a.SilindiMi);

            if (ayar == null)
            {
                guncelleme.OlusturulmaTarihi = DateTime.UtcNow;
                vt.SayfaDuzenAyarlari.Add(guncelleme);
            }
            else
            {
                ayar.SayfaAdi = guncelleme.SayfaAdi;
                ayar.SutunAdet = guncelleme.SutunAdet;
                ayar.SatirAdet = guncelleme.SatirAdet;
                ayar.SayfaBasinaAdet = guncelleme.SayfaBasinaAdet;
                ayar.SayfalamaAktif = guncelleme.SayfalamaAktif;
                ayar.AktifMi = guncelleme.AktifMi;
                ayar.GuncellenmeTarihi = DateTime.UtcNow;
            }
        }

        await vt.SaveChangesAsync();
        return Cevap<bool>.Basarili(true);
    }

    [HttpPost]
    public async Task<Cevap<SayfaDuzenAyari>> Ekle([FromBody] SayfaDuzenAyari yeni)
    {
        var mevcut = await vt.SayfaDuzenAyarlari
            .AnyAsync(a => a.SayfaKodu == yeni.SayfaKodu && !a.SilindiMi);
        if (mevcut)
            return Cevap<SayfaDuzenAyari>.Hata("Bu sayfa kodu zaten mevcut.");

        yeni.OlusturulmaTarihi = DateTime.UtcNow;
        vt.SayfaDuzenAyarlari.Add(yeni);
        await vt.SaveChangesAsync();
        return Cevap<SayfaDuzenAyari>.Basarili(yeni);
    }
}



