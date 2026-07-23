using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Kontrolculer.Pazarlama;

/// <summary>
/// Dinamik sayfa goruntuleme API'si.
/// Public sayfalari admin paneldeki verilerle olusturur.
/// </summary>
[ApiController]
[Route("api/sayfa-gorunumu")]
public class SayfaGorunumuKontrolcu(VizitLink3DDbContext vt) : ControllerBase
{
    private string ApiMedyaDosyaUrl(long medyaId)
        => $"{Request.Scheme}://{Request.Host}/api/medya/dosya/{medyaId}";

    [HttpGet("{slug}")]
    public async Task<Cevap<SayfaGorunumDto>> SayfaGorunumuGetir(string slug, [FromQuery] string dil = "tr")
    {
        var apiTemeli = $"{Request.Scheme}://{Request.Host}".TrimEnd('/');

        // Sayfa icerigini bolum bazli getir
        var icerikler = await vt.SayfaIcerikleri
            .Where(s => s.Bolum == slug && s.Dil == dil && !s.SilindiMi)
            .ToListAsync();

        if (!icerikler.Any() && slug == "anasayfa")
            return Cevap<SayfaGorunumDto>.Basarili(VarsayilanAnasayfa());

        // Hero slider
        var slaytlar = await vt.Slaytlar
            .Where(s => s.AktifMi && !s.SilindiMi && s.Dil == dil && s.SayfaKodu == slug)
            .OrderBy(s => s.SiraNo)
            .ToListAsync();

        // Urunler
        var urunEntityler = await vt.Urunler
            .Where(u => u.AktifMi && !u.SilindiMi && u.OneCikanMi)
            .OrderBy(u => u.SiraNo)
            .ToListAsync();

        var urunler = urunEntityler
            .Take(8)
            .Select(u => new BlokDto
            {
                BlokTipi = "Urun",
                Icerik = u.Ad,
                GorselUrl = u.AnaGorselMedyaId.HasValue ? $"{apiTemeli}/api/medya/dosya/{u.AnaGorselMedyaId.Value}" : null,
                Link = "/urun/" + u.Slug
            })
            .ToList();

        var sayfa = new SayfaGorunumDto
        {
            Slug = slug,
            Baslik = slug == "anasayfa" ? "Ana Sayfa" : icerikler.FirstOrDefault()?.Bolum ?? slug,
            SayfaTipi = slug == "anasayfa" ? "Anasayfa" : "Dinamik"
        };

        // Hero bolumu
        if (slaytlar.Any())
        {
            sayfa.Bolumler.Add(new SayfaBolumuDto
            {
                BolumKodu = "hero",
                BolumTipi = "HeroSlider",
                Sira = 1,
                AnimasyonTipi = "fade",
                Bloklar = slaytlar.Select(s => new BlokDto
                {
                    BlokTipi = "Slayt",
                    Icerik = s.Baslik,
                    GorselUrl = s.ArkaplanResim,
                    Link = s.ButonLink1,
                    Ikon = s.AltBaslik
                }).ToList()
            });
        }

        // One cikan urunler
        if (urunler.Any())
        {
            sayfa.Bolumler.Add(new SayfaBolumuDto
            {
                BolumKodu = "urunler",
                BolumTipi = "KartGrid",
                Baslik = "One Cikan Urunler",
                Sira = 2,
                Bloklar = urunler
            });
        }

        // Icerik bolumleri
        var bolumGruplari = icerikler.GroupBy(i => i.Anahtar.Split('_')[0]);
        int sira = 3;
        foreach (var grup in bolumGruplari)
        {
            var bolum = new SayfaBolumuDto
            {
                BolumKodu = grup.Key,
                BolumTipi = "MetinGorsel",
                Sira = sira++
            };
            foreach (var icerik in grup)
            {
                if (icerik.Anahtar.EndsWith("Baslik")) bolum.Baslik = icerik.Deger;
                else if (icerik.Anahtar.EndsWith("Aciklama")) bolum.Aciklama = icerik.Deger;
                else if (icerik.Anahtar.EndsWith("Gorsel")) bolum.GorselUrl = icerik.Deger;
                else if (icerik.Anahtar.EndsWith("ButonYazi")) bolum.ButonMetni = icerik.Deger;
                else if (icerik.Anahtar.EndsWith("ButonLink")) bolum.ButonLink = icerik.Deger;
            }
            if (!string.IsNullOrEmpty(bolum.Baslik) || !string.IsNullOrEmpty(bolum.Aciklama))
                sayfa.Bolumler.Add(bolum);
        }

        return Cevap<SayfaGorunumDto>.Basarili(sayfa);
    }

    private static SayfaGorunumDto VarsayilanAnasayfa() => new()
    {
        Slug = "anasayfa",
        Baslik = "Ana Sayfa",
        SayfaTipi = "Anasayfa",
        Bolumler = new()
        {
            new() { BolumKodu = "hero", BolumTipi = "HeroSlider", Baslik = "Her Mekana Her Yasama", Aciklama = "1992'den beri kalite ve estetik.", GorselUrl = "https://images.unsplash.com/photo-1600585152220-90363fe7e115?w=1200", Sira = 1 },
            new() { BolumKodu = "kategori", BolumTipi = "KartGrid", Baslik = "Urun Kategorileri", Sira = 2, Bloklar = new() { new() { BlokTipi = "Metin", Icerik = "Yukleniyor..." } } }
        }
    };
}



