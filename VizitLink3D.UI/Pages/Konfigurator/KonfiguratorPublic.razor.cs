using System.Text.Json;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages.Konfigurator;

public partial class KonfiguratorPublic : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    [Parameter] public string? Slug { get; set; }

    // --- State ---
    private bool _yukleniyor = true;
    private bool _hataVar;
    private string _hataMesaji = "";

    private PublicKonfiguratorDto? _konfigurator;
    private PublicParcaDto? _secilenParca;

    // Seçim state'i
    private readonly Dictionary<int, int?> _parcaRenkSecimleri = [];   // ParcaId → RenkId
    private readonly Dictionary<int, int?> _parcaMalzemeSecimleri = []; // ParcaId → MalzemeId
    private readonly Dictionary<int, int?> _parcaKaplamaSecimleri = []; // ParcaId → KaplamaId
    private readonly Dictionary<int, bool> _parcaGorunurluk = [];       // ParcaId → GorunurMu
    private readonly Dictionary<int, double> _parcaHareketDegeri = [];  // ParcaId → deger
    private string? _musteriNotu;
    private bool _kaydediliyor;

    // --- Lifecycle ---
    protected override async Task OnParametersSetAsync()
    {
        if (string.IsNullOrWhiteSpace(Slug))
        {
            _hataVar = true;
            _hataMesaji = "Ürün belirtilmedi.";
            _yukleniyor = false;
            return;
        }

        if (_konfigurator == null)
            await KonfiguratorYukle();
    }

    private async Task KonfiguratorYukle()
    {
        _yukleniyor = true;
        _hataVar = false;

        try
        {
            // GetAsync unwrap yapar: Cevap<T>.Veri → T?
            _konfigurator = await Api.GetAsync<PublicKonfiguratorDto>(
                $"api/konfigurasyon/public/{Slug}");

            if (_konfigurator != null)
            {
                // Varsayılan seçimleri başlat
                foreach (var p in _konfigurator.Parcalar)
                {
                    _parcaGorunurluk[p.Id] = true;

                    if (p.VarsayilanRenkId.HasValue)
                        _parcaRenkSecimleri[p.Id] = p.VarsayilanRenkId.Value;
                    else if (p.Renkler.Count > 0)
                        _parcaRenkSecimleri[p.Id] = p.Renkler[0].RenkId;

                    if (p.VarsayilanMalzemeId.HasValue)
                        _parcaMalzemeSecimleri[p.Id] = p.VarsayilanMalzemeId.Value;
                    else if (p.Malzemeler.Count > 0)
                        _parcaMalzemeSecimleri[p.Id] = p.Malzemeler[0].MalzemeId;

                    if (p.VarsayilanDeger.HasValue)
                        _parcaHareketDegeri[p.Id] = p.VarsayilanDeger.Value;
                }
            }
            else
            {
                _hataVar = true;
                _hataMesaji = "Ürün bulunamadı veya konfigüratör hazır değil.";
            }
        }
        catch (Exception ex)
        {
            _hataVar = true;
            _hataMesaji = "Konfigüratör yüklenirken bir hata oluştu.";
        }
        finally
        {
            _yukleniyor = false;
        }
    }

    // --- Parça seçimi ---
    private void ParcaSec(int parcaId)
    {
        _secilenParca = _konfigurator?.Parcalar.FirstOrDefault(p => p.Id == parcaId);
    }

    // --- Renk değiştir ---
    private void RenkDegistir(int parcaId, int renkId)
    {
        _parcaRenkSecimleri[parcaId] = renkId;
    }

    // --- Malzeme değiştir ---
    private void MalzemeDegistir(int parcaId, int malzemeId)
    {
        _parcaMalzemeSecimleri[parcaId] = malzemeId;
    }

    // --- Kaplama/Doku değiştir ---
    private void KaplamaDegistir(int parcaId, int kaplamaId)
    {
        _parcaKaplamaSecimleri[parcaId] = kaplamaId;
    }

    // --- Görünürlük toggle ---
    private void GorunurlukToggle(int parcaId)
    {
        var mevcut = _parcaGorunurluk.TryGetValue(parcaId, out var g) && g;
        _parcaGorunurluk[parcaId] = !mevcut;
    }

    // --- Hareket değeri değiştir ---
    private void HareketDegeriDegistir(int parcaId, double deger)
    {
        _parcaHareketDegeri[parcaId] = deger;
    }

    // --- Kaydet ---
    private async Task SecimiKaydet()
    {
        if (_konfigurator == null) return;
        _kaydediliyor = true;

        try
        {
            var dto = new PublicSecimKaydetDto
            {
                UrunId = _konfigurator.UrunId,
                MusteriNotu = _musteriNotu,
                Secimler = _konfigurator.Parcalar.Select(p =>
                {
                    _parcaRenkSecimleri.TryGetValue(p.Id, out var r);
                    _parcaMalzemeSecimleri.TryGetValue(p.Id, out var m);
                    _parcaKaplamaSecimleri.TryGetValue(p.Id, out var k);
                    _parcaHareketDegeri.TryGetValue(p.Id, out var hd);
                    var gorunur = _parcaGorunurluk.TryGetValue(p.Id, out var g) && g;

                    return new PublicParcaSecimiDto
                    {
                        ParcaId = p.Id,
                        SeciliRenkId = r,
                        SeciliMalzemeId = m,
                        SeciliKaplamaId = k,
                        HareketDegeri = hd,
                        GorunurMu = gorunur
                    };
                }).ToList()
            };

            var cevap = await Api.PostAsync<JsonElement>(
                "api/konfigurasyon/public/secim-kaydet", dto);

            if (cevap is { BasariliMi: true })
            {
                Snackbar.Add("Konfigürasyon kaydedildi.", Severity.Success);
            }
            else
            {
                Snackbar.Add(cevap?.Mesaj ?? "Kaydedilemedi.", Severity.Error);
            }
        }
        catch
        {
            Snackbar.Add("Kayıt sırasında bir hata oluştu.", Severity.Error);
        }
        finally
        {
            _kaydediliyor = false;
        }
    }

    // --- Yardımcı metotlar ---
    private string SeciliRenkHex(int parcaId)
    {
        if (_secilenParca == null || _secilenParca.Id != parcaId) return "";
        if (!_parcaRenkSecimleri.TryGetValue(parcaId, out var renkId) || renkId == null) return "";
        var renk = _secilenParca.Renkler.FirstOrDefault(r => r.RenkId == renkId.Value);
        return renk?.HexKodu ?? "";
    }

    private string SeciliMalzemeAdi(int parcaId)
    {
        if (_secilenParca == null || _secilenParca.Id != parcaId) return "";
        if (!_parcaMalzemeSecimleri.TryGetValue(parcaId, out var mId) || mId == null) return "";
        var m = _secilenParca.Malzemeler.FirstOrDefault(x => x.MalzemeId == mId.Value);
        return m?.MalzemeAdi ?? "";
    }

    private static string HareketTipiLabel(string hareketTipi)
    {
        return hareketTipi switch
        {
            "Menteseli" => "Açı (derece)",
            "Surgulu" => "Kaydırma",
            "Cekmece" => "Açıklık",
            "YukariAcilir" => "Açı (derece)",
            "Pivot" => "Dönüş",
            "Recliner" => "Yatış",
            _ => "Değer"
        };
    }
}
