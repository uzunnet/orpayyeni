using VizitLink3D.Api.Modeller;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Servisler;

public class KapakGocServisi(VizitLink3DDbContext vt)
{
    public async Task<Cevap<GocSonucu>> GogAsync(CancellationToken iptal = default)
    {
        var sonuc = new GocSonucu();

        var kapakModelleri = await vt.KapakModelleri
            .AsNoTracking()
            .Where(k => !k.SilindiMi)
            .ToListAsync(iptal);

        if (!kapakModelleri.Any())
            return Cevap<GocSonucu>.Basarili(sonuc, "Goc edilecek kapak modeli bulunamadi.");

        foreach (var km in kapakModelleri)
        {
            var mevcutUrun = await vt.Urunler
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Kod == km.ModelKodu || u.Slug == km.Slug, iptal);

            if (mevcutUrun != null)
            {
                bool guncellendi = false;
                if (string.IsNullOrWhiteSpace(mevcutUrun.KisaAciklama) && !string.IsNullOrWhiteSpace(km.OnYazi))
                {
                    mevcutUrun.KisaAciklama = km.OnYazi;
                    guncellendi = true;
                }
                if (string.IsNullOrWhiteSpace(mevcutUrun.Aciklama) && !string.IsNullOrWhiteSpace(km.Aciklama))
                {
                    mevcutUrun.Aciklama = km.Aciklama;
                    guncellendi = true;
                }
                if (guncellendi)
                {
                    vt.Urunler.Update(mevcutUrun);
                    await vt.SaveChangesAsync(iptal);
                    sonuc.Gocen++;
                }
                else
                {
                    sonuc.Atlandi++;
                }
                continue;
            }

            var aileId = km.ModelTuru == "Kapi" ? 2 : 1;

            var urun = new Urun
            {
                Slug = string.IsNullOrWhiteSpace(km.Slug) ? SlugOlustur(km.ModelAdi) : km.Slug,
                Kod = km.ModelKodu,
                Ad = km.ModelAdi,
                KisaAciklama = km.OnYazi,
                Aciklama = km.Aciklama,
                UrunAilesiId = aileId,
                AktifMi = true,
                OneCikanMi = km.OneCikanMi,
                YeniMi = km.YeniMi,
                Fiyat = km.Fiyat,
                SiraNo = km.SiraNo,
                SeoBaslik = km.SeoBaslik,
                SeoAciklama = km.SeoAciklama,
                OlusturulmaTarihi = km.OlusturulmaTarihi
            };

            vt.Urunler.Add(urun);
            await vt.SaveChangesAsync(iptal);

            if (!string.IsNullOrWhiteSpace(km.ModelDosyaYolu))
            {
                var ucBoyutModeli = new UrunUcBoyutModeli
                {
                    UrunId = urun.Id,
                    ModelAdi = km.ModelAdi,
                    ModelYolu = km.ModelDosyaYolu,
                    ModelDosyaYolu = km.ModelDosyaYolu,
                    VarsayilanMi = true,
                    AktifMi = true,
                    OlusturulmaTarihi = DateTime.UtcNow
                };
                vt.UrunUcBoyutModelleri.Add(ucBoyutModeli);
                await vt.SaveChangesAsync(iptal);

                // Urun'un varsayilan 3D modelini bagla (aksi halde 3D bagli gorunmez).
                urun.VarsayilanUcBoyutModeliId = ucBoyutModeli.Id;
                vt.Urunler.Update(urun);
                await vt.SaveChangesAsync(iptal);
            }

            if (!string.IsNullOrWhiteSpace(km.AnaGorselUrl))
            {
                vt.UrunMedyalari.Add(new UrunMedya
                {
                    UrunId = urun.Id,
                    MedyaUrl = km.AnaGorselUrl,
                    MedyaTuru = "Resim",
                    SiraNo = 1,
                    AnaGosterim = true
                });

                // ANA GORSEL: gercek Medya kaydi olustur. /api/medya/dosya/{id} bu
                // tablodan okudugu icin AnaGorselMedyaId mutlaka Medya.Id olmali
                // (onceden UrunMedya.Id atandigi icin endpoint 404 donuyordu).
                var anaMedya = new VizitLink3D.Ortak.Modeller.Medya.Medya
                {
                    Ad = System.IO.Path.GetFileNameWithoutExtension(km.AnaGorselUrl),
                    OrijinalAd = System.IO.Path.GetFileName(km.AnaGorselUrl),
                    DosyaYolu = km.AnaGorselUrl.TrimStart('/'),
                    Tip = VizitLink3D.Ortak.Modeller.Medya.MedyaTipi.Resim,
                    Kaynak = VizitLink3D.Ortak.Modeller.Medya.MedyaKaynagi.Yerel,
                    OlusturulmaTarihi = DateTime.UtcNow
                };
                vt.Medyalar.Add(anaMedya);
                await vt.SaveChangesAsync(iptal);

                urun.AnaGorselMedyaId = anaMedya.Id;
                vt.Urunler.Update(urun);
                await vt.SaveChangesAsync(iptal);
            }

            sonuc.Gocen++;
        }

        sonuc.Toplam = kapakModelleri.Count;
        return Cevap<GocSonucu>.Basarili(sonuc, $"Goc tamamlandi: {sonuc.Gocen} goc etti, {sonuc.Atlandi} atlandi.");
    }

    private static string SlugOlustur(string ad)
    {
        return ad
            .ToLowerInvariant()
            .Replace(' ', '-')
            .Replace('ı', 'i').Replace('ğ', 'g').Replace('ü', 'u')
            .Replace('ş', 's').Replace('ö', 'o').Replace('ç', 'c')
            .Replace('İ', 'i').Replace('Ğ', 'g').Replace('Ü', 'u')
            .Replace('Ş', 's').Replace('Ö', 'o').Replace('Ç', 'c');
    }
}

public class GocSonucu
{
    public int Toplam { get; set; }
    public int Gocen { get; set; }
    public int Atlandi { get; set; }
}
