using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace VizitLink3D.UI.Pages.Admin;

public partial class LisansYonetimi : ComponentBase
{
    private LisansDurumDto? _durum;
    private readonly LisansKaydetDto _form = new();
    private DateTime? _baslangicTarihi = DateTime.UtcNow.Date;
    private DateTime? _bitisTarihi;
    private bool _yukleniyor = true;
    private bool _kaydediliyor;

    private Color DurumRengi
        => _durum?.GecerliMi == true ? Color.Success : Color.Error;

    private string DurumMetni
        => _durum?.GecerliMi == true
            ? dil.T("admin.lisans.gecerli", "Geçerli")
            : dil.T("admin.lisans.gecersiz", "Geçersiz");

    private string KalanSureMetni
    {
        get
        {
            if (_durum?.SuresizMi == true)
            {
                return dil.T("admin.lisans.suresiz", "Süresiz");
            }

            if (_durum?.KalanGun is int kalanGun)
            {
                return $"{kalanGun} {dil.T("admin.lisans.gun", "gün")}";
            }

            return "-";
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await DurumYukleAsync();
    }

    private async Task DurumYukleAsync()
    {
        _yukleniyor = true;
        _durum = await api.GetAsync<LisansDurumDto>("api/lisans/durum");

        if (_durum is not null)
        {
            _form.LisansTipi = string.IsNullOrWhiteSpace(_durum.LisansTipi) ? "Suresiz" : _durum.LisansTipi;
            _form.BirincilDomain = _durum.BirincilDomain;
            _form.YedekDomain = _durum.YedekDomain;
            _form.AktifMi = _durum.AktifMi || !_durum.LisansVarMi;
            _baslangicTarihi = _durum.BaslangicTarihi?.Date ?? DateTime.UtcNow.Date;
            _bitisTarihi = _durum.SuresizMi ? null : _durum.BitisTarihi?.Date;
        }

        _yukleniyor = false;
    }

    private async Task KaydetAsync()
    {
        _kaydediliyor = true;
        _form.BaslangicTarihi = _baslangicTarihi;
        _form.BitisTarihi = _form.LisansTipi == "Suresiz" ? null : _bitisTarihi;

        var sonuc = await api.PutAsync<LisansDurumDto>("api/lisans/aktif-firma", _form);
        if (sonuc?.BasariliMi == true)
        {
            _durum = sonuc.Veri;
            snackbar.Add(dil.T("admin.lisans.kaydedildi", "Lisans kaydedildi."), Severity.Success);
        }
        else
        {
            snackbar.Add(dil.T("admin.lisans.kaydedilemedi", "Lisans kaydedilemedi."), Severity.Error);
        }

        _kaydediliyor = false;
    }

    private string LisansTipiMetni(string tip)
        => tip switch
        {
            "Demo" => dil.T("admin.lisans.demo", "Demo"),
            "Yillik" => dil.T("admin.lisans.yillik", "1 Yıllık"),
            "IkiYillik" => dil.T("admin.lisans.ikiYillik", "2 Yıllık"),
            "UcYillik" => dil.T("admin.lisans.ucYillik", "3 Yıllık"),
            "BesYillik" => dil.T("admin.lisans.besYillik", "5 Yıllık"),
            "Suresiz" => dil.T("admin.lisans.suresiz", "Süresiz"),
            _ => tip
        };

    private sealed class LisansDurumDto
    {
        public string FirmaAdi { get; set; } = string.Empty;
        public bool LisansVarMi { get; set; }
        public bool GecerliMi { get; set; }
        public bool AktifMi { get; set; }
        public string LisansTipi { get; set; } = string.Empty;
        public bool SuresizMi { get; set; }
        public string BirincilDomain { get; set; } = string.Empty;
        public string? YedekDomain { get; set; }
        public DateTime? BaslangicTarihi { get; set; }
        public DateTime? BitisTarihi { get; set; }
        public int? KalanGun { get; set; }
    }

    private sealed class LisansKaydetDto
    {
        public string LisansTipi { get; set; } = "Suresiz";
        public string? BirincilDomain { get; set; }
        public string? YedekDomain { get; set; }
        public DateTime? BaslangicTarihi { get; set; }
        public DateTime? BitisTarihi { get; set; }
        public bool AktifMi { get; set; } = true;
    }
}
