using VizitLink3D.Ortak.Modeller.Urunler;
using VizitLink3D.UI.Servisler;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace VizitLink3D.UI.Pages.Admin;

public partial class PdfUygulamaEsleme : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private List<UygulamaAlanSonucu> _liste = [];
    private List<UrunOzetDto> _urunler = [];
    private Dictionary<int, KartSecim> _secimler = [];
    private bool _yukleniyor = true;
    private bool _isleniyor;
    private int? _kaydedilenId;

    protected override async Task OnInitializedAsync()
    {
        _urunler = await Api.GetAsync<List<UrunOzetDto>>("api/urunler") ?? [];
        await ListeyiYukle();
    }

    private async Task ListeyiYukle()
    {
        _yukleniyor = true;
        _liste = await Api.GetAsync<List<UygulamaAlanSonucu>>("api/pdf-uygulama/liste") ?? [];
        _secimler = _liste.ToDictionary(
            g => g.MedyaId,
            g => new KartSecim
            {
                SeciliUrun = _urunler.FirstOrDefault(u => u.Id == g.EslenenUrunId),
                SiraNo = g.EslenenSiraNo > 0 ? g.EslenenSiraNo : 1
            });
        _yukleniyor = false;
    }

    private async Task TumunuIsle()
    {
        _isleniyor = true;
        try
        {
            var cevap = await Api.PostAsync<List<UygulamaAlanSonucu>>("api/pdf-uygulama/isle", new { });
            if (cevap?.BasariliMi == true)
            {
                Snackbar.Add(cevap.Mesaj, Severity.Success);
                await ListeyiYukle();
            }
            else
                Snackbar.Add(cevap?.Mesaj ?? "İşlem başarısız.", Severity.Error);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Hata: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isleniyor = false;
        }
    }

    private async Task Kaydet(UygulamaAlanSonucu gorsel)
    {
        var secim = _secimler[gorsel.MedyaId];
        if (secim.SeciliUrun == null) return;

        _kaydedilenId = gorsel.MedyaId;
        try
        {
            var cevap = await Api.PostAsync<string>("api/pdf-uygulama/urun-esle", new UrunEslemeIstegi
            {
                MedyaId = gorsel.MedyaId,
                UrunId = secim.SeciliUrun.Id,
                SiraNo = secim.SiraNo
            });

            if (cevap?.BasariliMi == true)
            {
                Snackbar.Add($"'{secim.SeciliUrun.Ad}' ürününe {secim.SiraNo}. görsel olarak eklendi.", Severity.Success);
                gorsel.EslenenUrunId = secim.SeciliUrun.Id;
                gorsel.EslenenUrunAdi = secim.SeciliUrun.Ad;
            }
            else
                Snackbar.Add("Eşleme kaydedilemedi.", Severity.Error);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Hata: {ex.Message}", Severity.Error);
        }
        finally
        {
            _kaydedilenId = null;
        }
    }

    private async Task EslemeKaldir(UygulamaAlanSonucu gorsel)
    {
        if (!gorsel.EslenenUrunId.HasValue) return;
        try
        {
            await Api.DeleteAsync($"api/pdf-uygulama/{gorsel.MedyaId}/urun/{gorsel.EslenenUrunId.Value}");
            Snackbar.Add("Eşleme kaldırıldı.", Severity.Success);
            gorsel.EslenenUrunId = null;
            gorsel.EslenenUrunAdi = null;
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Hata: {ex.Message}", Severity.Error);
        }
    }

    private Task<IEnumerable<UrunOzetDto>> UrunAra(string metin, CancellationToken iptal)
    {
        if (string.IsNullOrWhiteSpace(metin))
            return Task.FromResult(_urunler.Take(20));

        var alt = metin.ToLower();
        var filtreli = _urunler
            .Where(u => (u.Ad?.ToLower().Contains(alt) ?? false) || (u.Slug?.ToLower().Contains(alt) ?? false))
            .Take(20);
        return Task.FromResult(filtreli);
    }

    private sealed class KartSecim
    {
        public UrunOzetDto? SeciliUrun { get; set; }
        public int SiraNo { get; set; } = 1;
    }
}
