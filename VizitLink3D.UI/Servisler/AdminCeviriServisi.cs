using System.Security.Cryptography;
using System.Text;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Ceviriler;
using VizitLink3D.Ortak.Modeller.Istekler;

namespace VizitLink3D.UI.Servisler;

public class AdminCeviriServisi(ApiIstemcisi api)
{
    public async Task<AdminCeviriAnalizSonucu> AnalizEtAsync(string modulAdi, IEnumerable<AdminCeviriKaydi> kayitlar)
    {
        var hedefDiller = await HedefDilleriGetirAsync();
        var ceviriler = await CevirileriGetirAsync();
        var indeks = CeviriIndeksiOlustur(ceviriler);
        var durumlar = new Dictionary<int, AdminCeviriKayitDurumu>();
        int guncel = 0, eksik = 0, kaynakDegisti = 0, hata = 0;

        foreach (var kayit in kayitlar)
        {
            var detaylar = new List<AdminCeviriAlanDurumu>();
            var kayitDurumu = AdminCeviriDurumu.Guncel;

            foreach (var hedefDil in hedefDiller)
            {
                foreach (var alan in kayit.Alanlar.Where(x => !string.IsNullOrWhiteSpace(x.Deger)))
                {
                    var alanDurumu = AlanDurumuGetir(modulAdi, kayit.Id, hedefDil.Kod, alan, indeks);
                    if (alanDurumu == AdminCeviriDurumu.Guncel)
                        continue;

                    detaylar.Add(new AdminCeviriAlanDurumu(hedefDil.Kod, alan.Ad, alanDurumu));
                    kayitDurumu = EnYuksekOncelikliDurum(kayitDurumu, alanDurumu);
                }
            }

            durumlar[kayit.Id] = new AdminCeviriKayitDurumu(kayitDurumu, detaylar);

            switch (kayitDurumu)
            {
                case AdminCeviriDurumu.Guncel: guncel++; break;
                case AdminCeviriDurumu.Eksik: eksik++; break;
                case AdminCeviriDurumu.KaynakDegisti: kaynakDegisti++; break;
                case AdminCeviriDurumu.Hata: hata++; break;
            }
        }

        return new AdminCeviriAnalizSonucu(
            durumlar,
            new AdminCeviriOzet(guncel, eksik, kaynakDegisti, hata),
            hedefDiller);
    }

    public async Task<AdminCeviriIslemSonucu> TumunuCevirAsync(
        string modulAdi,
        IEnumerable<AdminCeviriKaydi> kayitlar,
        Func<int, int, Task>? ilerlemeBildir = null)
    {
        var hedefDiller = await HedefDilleriGetirAsync();
        var ceviriler = await CevirileriGetirAsync();
        var indeks = CeviriIndeksiOlustur(ceviriler);
        var liste = kayitlar.ToList();
        int cevrilenKayitSayisi = 0;
        int cevrilenAlanSayisi = 0;
        int islenen = 0;

        foreach (var kayit in liste)
        {
            var sonuc = await KaydiCevirDahiliAsync(modulAdi, kayit, hedefDiller, indeks);
            if (sonuc.CevrilenAlanSayisi > 0)
            {
                cevrilenKayitSayisi++;
                cevrilenAlanSayisi += sonuc.CevrilenAlanSayisi;
            }

            islenen++;
            if (ilerlemeBildir is not null)
                await ilerlemeBildir(islenen, liste.Count);
        }

        return new AdminCeviriIslemSonucu(cevrilenKayitSayisi, cevrilenAlanSayisi);
    }

    public async Task<AdminCeviriIslemSonucu> KaydiCevirAsync(string modulAdi, AdminCeviriKaydi kayit)
    {
        var hedefDiller = await HedefDilleriGetirAsync();
        var ceviriler = await CevirileriGetirAsync();
        var indeks = CeviriIndeksiOlustur(ceviriler);
        return await KaydiCevirDahiliAsync(modulAdi, kayit, hedefDiller, indeks);
    }

    private async Task<AdminCeviriIslemSonucu> KaydiCevirDahiliAsync(
        string modulAdi,
        AdminCeviriKaydi kayit,
        IReadOnlyList<Dil> hedefDiller,
        IReadOnlyDictionary<string, Ceviri> indeks)
    {
        int cevrilenAlanSayisi = 0;

        foreach (var hedefDil in hedefDiller)
        {
            foreach (var alan in kayit.Alanlar.Where(x => !string.IsNullOrWhiteSpace(x.Deger)))
            {
                var durum = AlanDurumuGetir(modulAdi, kayit.Id, hedefDil.Kod, alan, indeks);
                if (durum == AdminCeviriDurumu.Guncel)
                    continue;

                var ceviriYanit = await api.PostAsync<string>("api/yonetim/ceviri/cevir", new OtomatikCeviriIstegi
                {
                    Metin = alan.Deger,
                    KaynakDil = "tr",
                    HedefDil = hedefDil.Kod
                });

                if (ceviriYanit?.BasariliMi != true || string.IsNullOrWhiteSpace(ceviriYanit.Veri))
                    continue;

                if (!CeviriKaliteKontrolu(alan.Deger, ceviriYanit.Veri))
                    continue;

                var ceviriKayitYanit = await api.PostAsync<string>("api/yonetim/ceviri/kaydet", new CeviriKayitIstegi
                {
                    Anahtar = CeviriAnahtari(modulAdi, kayit.Id, alan.Ad),
                    Dil = hedefDil.Kod,
                    Deger = ceviriYanit.Veri
                });

                if (ceviriKayitYanit?.BasariliMi != true)
                    continue;

                var hashYanit = await api.PutAsync<Ceviri>("api/dil/admin/ceviri", new Ceviri
                {
                    Anahtar = KaynakHashAnahtari(modulAdi, kayit.Id, alan.Ad, hedefDil.Kod),
                    Dil = "meta",
                    Deger = KaynakHashUret(alan.Deger),
                    Bolum = modulAdi
                });

                if (hashYanit?.BasariliMi == true)
                    cevrilenAlanSayisi++;
            }
        }

        return new AdminCeviriIslemSonucu(cevrilenAlanSayisi > 0 ? 1 : 0, cevrilenAlanSayisi);
    }

    private async Task<List<Dil>> HedefDilleriGetirAsync()
    {
        return (await api.GetAsync<List<Dil>>("api/dil/desteklenen") ?? [])
            .Where(x => x.AktifMi && !x.Kod.Equals("tr", StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.SiraNo)
            .ToList();
    }

    private async Task<List<Ceviri>> CevirileriGetirAsync()
    {
        return await api.GetAsync<List<Ceviri>>("api/dil/admin/tum-ceviriler") ?? [];
    }

    private static AdminCeviriDurumu AlanDurumuGetir(
        string modulAdi,
        int kayitId,
        string hedefDil,
        AdminCeviriAlani alan,
        IReadOnlyDictionary<string, Ceviri> indeks)
    {
        var ceviri = indeks.GetValueOrDefault(IndeksAnahtari(CeviriAnahtari(modulAdi, kayitId, alan.Ad), hedefDil));

        if (ceviri is null || string.IsNullOrWhiteSpace(ceviri.Deger))
            return AdminCeviriDurumu.Eksik;

        if (!CeviriKaliteKontrolu(alan.Deger, ceviri.Deger))
            return AdminCeviriDurumu.Hata;

        var hashKaydi = indeks.GetValueOrDefault(IndeksAnahtari(KaynakHashAnahtari(modulAdi, kayitId, alan.Ad, hedefDil), "meta"));

        var anlikHash = KaynakHashUret(alan.Deger);
        if (hashKaydi is null || !string.Equals(hashKaydi.Deger, anlikHash, StringComparison.Ordinal))
            return AdminCeviriDurumu.KaynakDegisti;

        return AdminCeviriDurumu.Guncel;
    }

    // Ceviri listesini (Anahtar + Dil) -> Ceviri seklinde indeksler; lineer FirstOrDefault yerine O(1) arama.
    // Ayni anahtar+dil icin son kayit gecerli sayilir.
    private static Dictionary<string, Ceviri> CeviriIndeksiOlustur(IReadOnlyList<Ceviri> ceviriler)
    {
        var indeks = new Dictionary<string, Ceviri>(ceviriler.Count, StringComparer.Ordinal);
        foreach (var ceviri in ceviriler)
            indeks[IndeksAnahtari(ceviri.Anahtar, ceviri.Dil)] = ceviri;
        return indeks;
    }

    private static string IndeksAnahtari(string anahtar, string dil) => $"{anahtar}{dil.ToLowerInvariant()}";

    private static string CeviriAnahtari(string modulAdi, int kayitId, string alanAdi) => $"{modulAdi}_{kayitId}_{alanAdi}";
    private static string KaynakHashAnahtari(string modulAdi, int kayitId, string alanAdi, string dilKodu) => $"{modulAdi}_{kayitId}_{alanAdi}_{dilKodu}_KaynakHash";

    private static string KaynakHashUret(string metin)
    {
        var normalize = metin.Trim().Replace("\r\n", "\n");
        var bytes = Encoding.UTF8.GetBytes(normalize);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    private static bool CeviriKaliteKontrolu(string kaynakMetin, string ceviriMetni)
    {
        if (string.IsNullOrWhiteSpace(ceviriMetni))
            return false;

        var kaynak = kaynakMetin.Trim();
        var ceviri = ceviriMetni.Trim();

        if (string.Equals(kaynak, ceviri, StringComparison.OrdinalIgnoreCase))
            return false;

        if (kaynak.Length >= 20 && ceviri.Length < Math.Max(8, kaynak.Length / 6))
            return false;

        return true;
    }

    private static AdminCeviriDurumu EnYuksekOncelikliDurum(AdminCeviriDurumu mevcut, AdminCeviriDurumu yeni)
    {
        int Oncelik(AdminCeviriDurumu durum) => durum switch
        {
            AdminCeviriDurumu.Hata => 4,
            AdminCeviriDurumu.KaynakDegisti => 3,
            AdminCeviriDurumu.Eksik => 2,
            _ => 1
        };

        return Oncelik(yeni) > Oncelik(mevcut) ? yeni : mevcut;
    }
}

public sealed record AdminCeviriAlani(string Ad, string Deger);
public sealed record AdminCeviriKaydi(int Id, IReadOnlyList<AdminCeviriAlani> Alanlar);
public sealed record AdminCeviriAlanDurumu(string DilKodu, string AlanAdi, AdminCeviriDurumu Durum);
public sealed record AdminCeviriKayitDurumu(AdminCeviriDurumu Durum, IReadOnlyList<AdminCeviriAlanDurumu> Detaylar);
public sealed record AdminCeviriOzet(int Guncel, int Eksik, int KaynakDegisti, int Hata);
public sealed record AdminCeviriAnalizSonucu(
    IReadOnlyDictionary<int, AdminCeviriKayitDurumu> Durumlar,
    AdminCeviriOzet Ozet,
    IReadOnlyList<Dil> HedefDiller);
public sealed record AdminCeviriIslemSonucu(int CevrilenKayitSayisi, int CevrilenAlanSayisi);

public enum AdminCeviriDurumu
{
    Guncel = 0,
    Eksik = 1,
    KaynakDegisti = 2,
    Hata = 3
}
