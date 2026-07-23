using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Medya;
using VizitLink3D.Ortak.Modeller.Urunler;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace VizitLink3D.Api.Servisler;

public class MedyaGocServisi(VizitLink3DDbContext vt, IWebHostEnvironment env, IConfiguration yapilandirma)
{
    public async Task<Cevap<AktarimSonucu>> GocEtAsync(CancellationToken iptalToken = default)
    {
        var sonuc = new AktarimSonucu();
        var kaynakDizin = yapilandirma["MedyaGoc:KaynakDizin"] ?? Path.Combine(env.ContentRootPath, "medya-goc-kaynak");

        if (!Directory.Exists(kaynakDizin))
        {
            sonuc.Hatalar.Add($"Kaynak dizin bulunamadı: {kaynakDizin}");
            return Cevap<AktarimSonucu>.Hata($"Kaynak dizin bulunamadı. Lütfen {kaynakDizin} dizininin varlığından emin olun.", sonuc.Hatalar);
        }

        var dosyalar = Directory.EnumerateFiles(kaynakDizin, "*.*", SearchOption.AllDirectories)
            .Where(f => {
                var ext = Path.GetExtension(f).ToLowerInvariant();
                return ext == ".pdf" || ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".svg";
            })
            .ToList();

        sonuc.Toplam = dosyalar.Count;

        var medyaGocKlasor = Path.Combine(env.WebRootPath, "medya", "goc");
        if (!Directory.Exists(medyaGocKlasor)) Directory.CreateDirectory(medyaGocKlasor);

        var katalogKlasor = Path.Combine(env.WebRootPath, "medya", "kataloglar");
        if (!Directory.Exists(katalogKlasor)) Directory.CreateDirectory(katalogKlasor);

        foreach (var dosyaYolu in dosyalar)
        {
            try
            {
                var dosyaAdi = Path.GetFileName(dosyaYolu);
                var uzanti = Path.GetExtension(dosyaYolu).ToLowerInvariant();

                string hash;
                using (var stream = File.OpenRead(dosyaYolu))
                {
                    hash = Convert.ToHexStringLower(SHA256.HashData(stream));
                }

                var mevcutMedya = await vt.Medyalar
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(m => m.Hash == hash && !m.SilindiMi, iptalToken);

                if (mevcutMedya != null)
                {
                    if (uzanti == ".pdf")
                    {
                        await PdfKaynakVeKatalogOlusturAsync(mevcutMedya, dosyaAdi);
                        sonuc.AktarilanPdf++;
                    }
                    else
                    {
                        sonuc.AktarilanResim++;
                    }
                    continue;
                }

                string hedefDosyaYolu;
                string bagilYol;
                MedyaTipi tip;
                string mime;

                if (uzanti == ".pdf")
                {
                    hedefDosyaYolu = Path.Combine(katalogKlasor, dosyaAdi);
                    bagilYol = $"medya/kataloglar/{dosyaAdi}";
                    tip = MedyaTipi.Pdf;
                    mime = "application/pdf";
                }
                else
                {
                    hedefDosyaYolu = Path.Combine(medyaGocKlasor, dosyaAdi);
                    bagilYol = $"medya/goc/{dosyaAdi}";
                    tip = MedyaTipi.Resim;
                    mime = uzanti switch
                    {
                        ".png" => "image/png",
                        ".svg" => "image/svg+xml",
                        _ => "image/jpeg"
                    };
                }

                File.Copy(dosyaYolu, hedefDosyaYolu, overwrite: true);

                var yeniMedya = new Medya
                {
                    Ad = Path.GetFileNameWithoutExtension(dosyaAdi),
                    OrijinalAd = dosyaAdi,
                    DosyaYolu = bagilYol,
                    Tip = tip,
                    Kaynak = MedyaKaynagi.Yerel,
                    BoyutByte = new FileInfo(hedefDosyaYolu).Length,
                    MimeTipi = mime,
                    Hash = hash,
                    OlusturulmaTarihi = DateTime.UtcNow
                };

                vt.Medyalar.Add(yeniMedya);
                await vt.SaveChangesAsync(iptalToken);

                if (uzanti == ".pdf")
                {
                    await PdfKaynakVeKatalogOlusturAsync(yeniMedya, dosyaAdi);
                    sonuc.AktarilanPdf++;
                }
                else
                {
                    sonuc.AktarilanResim++;
                }
            }
            catch (Exception ex)
            {
                sonuc.Hatalar.Add($"{Path.GetFileName(dosyaYolu)} aktarılırken hata oluştu: {ex.Message}");
            }
        }

        return Cevap<AktarimSonucu>.Basarili(sonuc, $"Dosya göçü tamamlandı. {sonuc.AktarilanResim} görsel, {sonuc.AktarilanPdf} PDF katalog sisteme aktarıldı.");
    }

    private async Task PdfKaynakVeKatalogOlusturAsync(Medya medya, string dosyaAdi)
    {
        var ad = Path.GetFileNameWithoutExtension(dosyaAdi);

        var mevcutKaynak = await vt.UrunPdfKaynaklari
            .FirstOrDefaultAsync(k => k.MedyaId == medya.Id || k.Ad == ad);

        if (mevcutKaynak == null)
        {
            vt.UrunPdfKaynaklari.Add(new UrunPdfKaynagi
            {
                Ad = ad,
                MedyaId = medya.Id,
                CozumlemeDurumu = "Bekliyor",
                OlusturulmaTarihi = DateTime.UtcNow
            });
            await vt.SaveChangesAsync();
        }

        var mevcutKatalog = await vt.Kataloglar
            .FirstOrDefaultAsync(k => k.PdfDosyaYolu == medya.DosyaYolu);

        if (mevcutKatalog == null)
        {
            var sira = await vt.Kataloglar.CountAsync() + 1;
            vt.Kataloglar.Add(new Katalog
            {
                Baslik = ad,
                Aciklama = $"Otomatik aktarılan katalog: {ad}",
                PdfDosyaYolu = medya.DosyaYolu ?? string.Empty,
                Yil = DateTime.UtcNow.Year,
                SiraNo = sira,
                AktifMi = true,
                OlusturulmaTarihi = DateTime.UtcNow
            });
            await vt.SaveChangesAsync();
        }
    }
}

public class AktarimSonucu
{
    public int Toplam { get; set; }
    public int AktarilanResim { get; set; }
    public int AktarilanPdf { get; set; }
    public List<string> Hatalar { get; set; } = new();
}
