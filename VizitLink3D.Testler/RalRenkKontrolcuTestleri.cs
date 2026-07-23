using VizitLink3D.Api.Moduller.Urunler.Kontrolcüler;
using VizitLink3D.Api.Servisler;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller.Renkler;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Testler;

public class RalRenkKontrolcuTestleri : IDisposable
{
    private readonly SqliteConnection _baglanti;
    private readonly VizitLink3DDbContext _vt;
    private readonly RalRenkKontrolcu _kontrolcu;

    public RalRenkKontrolcuTestleri()
    {
        _baglanti = new SqliteConnection("DataSource=:memory:");
        _baglanti.Open();

        var secenekler = new DbContextOptionsBuilder<VizitLink3DDbContext>()
            .UseSqlite(_baglanti)
            .Options;

        _vt = new VizitLink3DDbContext(secenekler, new KiraciServisi(null));
        _vt.Database.EnsureCreated();
        _kontrolcu = new RalRenkKontrolcu(_vt);
    }

    public void Dispose()
    {
        _vt.Dispose();
        _baglanti.Close();
        _baglanti.Dispose();
    }

    [Fact]
    public async Task Olustur_GecerliRenk_NormalizeEdipKaydetmeli()
    {
        var cevap = await _kontrolcu.Olustur(new RalRengi
        {
            Kod = " ral 9016 ",
            Ad = " Trafik Beyazi ",
            HexKod = "#f1f0ea",
            KatalogId = 1,
            SiraNo = 1
        });

        Assert.True(cevap.BasariliMi);
        Assert.NotNull(cevap.Veri);
        Assert.Equal("RAL 9016", cevap.Veri.Kod);
        Assert.Equal("#F1F0EA", cevap.Veri.HexKod);
        Assert.Equal("Mat", cevap.Veri.YuzeyTipi);
    }

    [Fact]
    public async Task Olustur_GecersizHexKod_HataDonmeli()
    {
        var cevap = await _kontrolcu.Olustur(new RalRengi
        {
            Kod = "RAL 9016",
            Ad = "Trafik Beyazi",
            HexKod = "F1F0EA"
        });

        Assert.False(cevap.BasariliMi);
        Assert.Contains("#RRGGBB", cevap.Mesaj);
        Assert.Empty(await _vt.RalRenkleri.ToListAsync());
    }

    [Fact]
    public async Task Olustur_AyniKodAktifKayitta_HataDonmeli()
    {
        await _kontrolcu.Olustur(new RalRengi { Kod = "RAL 9005", Ad = "Derin Siyah", HexKod = "#0E0E10" });

        var tekrar = await _kontrolcu.Olustur(new RalRengi { Kod = "RAL 9005", Ad = "Siyah", HexKod = "#111111" });

        Assert.False(tekrar.BasariliMi);
        Assert.Equal(1, await _vt.RalRenkleri.CountAsync());
    }

    [Fact]
    public async Task Olustur_SilinmisAyniKodVarsa_YenidenAktifEtmeli()
    {
        var silinmis = new RalRengi
        {
            Kod = "RAL 7035",
            Ad = "Eski",
            HexKod = "#C9C9C6",
            SilindiMi = true,
            SilinmeTarihi = DateTime.UtcNow
        };
        _vt.RalRenkleri.Add(silinmis);
        await _vt.SaveChangesAsync();

        var cevap = await _kontrolcu.Olustur(new RalRengi
        {
            Kod = "RAL 7035",
            Ad = "Acik Gri",
            HexKod = "#C9C9C6",
            YuzeyTipi = "Saten"
        });

        var kayit = await _vt.RalRenkleri.IgnoreQueryFilters().SingleAsync();
        Assert.True(cevap.BasariliMi);
        Assert.False(kayit.SilindiMi);
        Assert.Null(kayit.SilinmeTarihi);
        Assert.Equal("Acik Gri", kayit.Ad);
        Assert.Equal("Saten", kayit.YuzeyTipi);
    }

    [Fact]
    public async Task Sil_FizikselSilmedenSoftDeleteYapmali()
    {
        var olustur = await _kontrolcu.Olustur(new RalRengi { Kod = "RAL 7016", Ad = "Antrasit Grisi", HexKod = "#383E42" });
        Assert.NotNull(olustur.Veri);

        var cevap = await _kontrolcu.Sil(olustur.Veri.Id);

        var kayit = await _vt.RalRenkleri.IgnoreQueryFilters().SingleAsync();
        Assert.True(cevap.BasariliMi);
        Assert.True(kayit.SilindiMi);
        Assert.NotNull(kayit.SilinmeTarihi);
        Assert.NotNull(kayit.GuncellenmeTarihi);
    }
}
