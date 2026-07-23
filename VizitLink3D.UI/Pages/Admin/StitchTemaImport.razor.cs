using Microsoft.AspNetCore.Components;
using MudBlazor;
using VizitLink3D.Ortak.Modeller.Tema;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages.Admin;

public partial class StitchTemaImport : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private readonly List<FirmaSecimDto> _firmalar = [];

    private string _stitchProjeId = string.Empty;
    private string _temaAdi = string.Empty;
    private string _temaSlug = string.Empty;
    private string _seciliFirmaId = "varsayilan";
    private string _designMdYolu = string.Empty;
    private string _designMdIcerik = string.Empty;

    private bool _aktifEt = true;
    private bool _yukleniyor;

    private StitchTemaTaslakSonucu? _taslakSonucu;
    private StitchTemaOnaySonucu? _onaySonucu;
    private string? _hamDesignMd;

    protected override async Task OnInitializedAsync()
    {
        var firmalar = await Api.GetAsync<List<FirmaSecimDto>>("api/firma-tema/firmalar") ?? [];
        _firmalar.Clear();
        _firmalar.AddRange(firmalar);
    }

    private async Task TaslakOlustur()
    {
        _yukleniyor = true;
        _onaySonucu = null;

        var cevap = await Api.PostAsync<StitchTemaTaslakSonucu>("api/tema/stitch/taslak", new StitchTemaTaslakIstek
        {
            Ad = _temaAdi,
            Slug = _temaSlug,
            FirmaId = _seciliFirmaId,
            AktifEt = _aktifEt,
            Notlar = $"Stitch proje: {_stitchProjeId}",
            DesignMdIcerik = null,
            DesignMdYolu = null
        });

        if (cevap?.BasariliMi == true && cevap.Veri is not null)
        {
            _taslakSonucu = cevap.Veri;
            _taslakSonucu.Taslak.StitchProjeId = _stitchProjeId;
            _hamDesignMd = null;
            Snackbar.Add("Tema taslağı hazırlandı.", Severity.Success);
        }
        else
        {
            Snackbar.Add(cevap?.Mesaj ?? "Tema taslağı oluşturulamadı.", Severity.Error);
        }

        _yukleniyor = false;
    }

    private async Task DesignMdIleOlustur()
    {
        _yukleniyor = true;
        _onaySonucu = null;

        var cevap = await Api.PostAsync<StitchTemaTaslakSonucu>("api/tema/stitch/taslak", new StitchTemaTaslakIstek
        {
            Ad = _temaAdi,
            Slug = _temaSlug,
            FirmaId = _seciliFirmaId,
            AktifEt = _aktifEt,
            DesignMdYolu = BosIseNull(_designMdYolu),
            DesignMdIcerik = BosIseNull(_designMdIcerik)
        });

        if (cevap?.BasariliMi == true && cevap.Veri is not null)
        {
            _taslakSonucu = cevap.Veri;
            _hamDesignMd = BosIseNull(_designMdIcerik);
            Snackbar.Add("DESIGN.md taslağı hazırlandı.", Severity.Success);
        }
        else
        {
            Snackbar.Add(cevap?.Mesaj ?? "DESIGN.md taslağı oluşturulamadı.", Severity.Error);
        }

        _yukleniyor = false;
    }

    private async Task Onayla()
    {
        if (_taslakSonucu?.GecerliMi != true)
        {
            Snackbar.Add("Geçerli bir tema taslağı olmadan onay verilemez.", Severity.Warning);
            return;
        }

        _yukleniyor = true;

        var cevap = await Api.PostAsync<StitchTemaOnaySonucu>("api/tema/stitch/onay", new StitchTemaOnayIstek
        {
            Taslak = _taslakSonucu.Taslak,
            FirmaId = _seciliFirmaId,
            AktifEt = _aktifEt,
            HamDesignMd = _hamDesignMd
        });

        if (cevap?.BasariliMi == true && cevap.Veri is not null)
        {
            _onaySonucu = cevap.Veri;
            Snackbar.Add("Tema başarıyla sisteme eklendi.", Severity.Success);
        }
        else
        {
            Snackbar.Add(cevap?.Mesaj ?? "Tema onayı başarısız oldu.", Severity.Error);
        }

        _yukleniyor = false;
    }

    private void TaslakIptal()
    {
        _taslakSonucu = null;
        _hamDesignMd = null;
    }

    private void YeniTaslak()
    {
        _taslakSonucu = null;
        _onaySonucu = null;
        _hamDesignMd = null;
    }

    private void Kapat()
    {
        _onaySonucu = null;
    }

    private static string? BosIseNull(string? deger)
        => string.IsNullOrWhiteSpace(deger) ? null : deger;

    private sealed class FirmaSecimDto
    {
        public int FirmaId { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string Ad { get; set; } = string.Empty;
    }
}
