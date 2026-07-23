using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Globalization;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages;

public partial class Iletisim : ComponentBase, IDisposable
{
    [Inject]
    private ApiIstemcisi ApiIstemcisi { get; set; } = default!;

    [Inject]
    private DilServisi DilServisi { get; set; } = default!;

    private string? _adres;
    private string? _showroomAdres;
    private string? _email;
    private List<string> _telefonlar = [];
    private string? _whatsapp;
    private string? _calismaSaatleri;
    private string? _enlem;
    private string? _boylam;
    private string? _haritaUrl;
    private string? _haritaBaglantiUrl;
    private string? _instagram;
    private string? _facebook;

    protected override async Task OnInitializedAsync()
    {
        DilServisi.DilDegisti += DilDegistiginde;
        await IcerigiYukleAsync();
    }

    private async void DilDegistiginde()
    {
        await IcerigiYukleAsync();
        StateHasChanged();
    }

    private async Task IcerigiYukleAsync()
    {
        try
        {
            var icerigi = await ApiIstemcisi.GetAsync<Dictionary<string, string>>($"api/sayfa-icerigi/ayarlar?dil={DilServisi.AktifDil}");

            if (icerigi != null)
            {
                if (icerigi.TryGetValue("Adres", out var adres))
                    _adres = adres;
                if (icerigi.TryGetValue("ShowroomAdres", out var showroomAdres))
                    _showroomAdres = showroomAdres;

                if (icerigi.TryGetValue("Eposta", out var email))
                    _email = email;

                _telefonlar = [];
                if (icerigi.TryGetValue("Telefon1", out var tel1))
                    _telefonlar.Add(tel1);
                if (icerigi.TryGetValue("Telefon2", out var tel2))
                    _telefonlar.Add(tel2);
                if (icerigi.TryGetValue("Telefon3", out var tel3))
                    _telefonlar.Add(tel3);
                if (icerigi.TryGetValue("Whatsapp", out var whatsapp))
                    _whatsapp = whatsapp;
                if (icerigi.TryGetValue("CalismaSaatleri", out var saatler))
                    _calismaSaatleri = saatler;
                if (icerigi.TryGetValue("Enlem", out var enlem))
                    _enlem = enlem;
                if (icerigi.TryGetValue("Boylam", out var boylam))
                    _boylam = boylam;
                if (icerigi.TryGetValue("Instagram", out var instagram))
                    _instagram = instagram;
                if (icerigi.TryGetValue("Facebook", out var facebook))
                    _facebook = facebook;

                _haritaUrl = HaritaUrlOlustur();
                _haritaBaglantiUrl = HaritaBaglantiUrlOlustur();
            }
        }
        catch
        {
            _adres = DilServisi.T("iletisim.adresYuklenemedi", "Adres bilgisi yüklenemedi");
            _email = DilServisi.T("iletisim.epostaYuklenemedi", "E-posta bilgisi yüklenemedi");
            _telefonlar = [DilServisi.T("iletisim.telefonYuklenemedi", "Telefon bilgisi yüklenemedi")];
        }
    }

    private string? HaritaUrlOlustur()
    {
        if (double.TryParse(_enlem, NumberStyles.Float, CultureInfo.InvariantCulture, out var enlem)
            && double.TryParse(_boylam, NumberStyles.Float, CultureInfo.InvariantCulture, out var boylam))
        {
            return $"https://www.google.com/maps?q={enlem.ToString(CultureInfo.InvariantCulture)},{boylam.ToString(CultureInfo.InvariantCulture)}&z=17&output=embed";
        }

        if (!string.IsNullOrWhiteSpace(_adres))
        {
            return $"https://www.google.com/maps?q={Uri.EscapeDataString(_adres)}&output=embed";
        }

        return null;
    }

    private string? HaritaBaglantiUrlOlustur()
    {
        if (double.TryParse(_enlem, NumberStyles.Float, CultureInfo.InvariantCulture, out var enlem)
            && double.TryParse(_boylam, NumberStyles.Float, CultureInfo.InvariantCulture, out var boylam))
        {
            return $"https://www.google.com/maps/search/?api=1&query={enlem.ToString(CultureInfo.InvariantCulture)},{boylam.ToString(CultureInfo.InvariantCulture)}";
        }

        return string.IsNullOrWhiteSpace(_adres)
            ? null
            : $"https://www.google.com/maps/search/?api=1&query={Uri.EscapeDataString(_adres)}";
    }

    private MudForm? _form;
    private IletisimFormu _formVerisi = new();
    private bool _gonderiliyor;
    private bool _formBasariliMi;
    private string? _formDurumMesaji;

    private async Task MesajGonderAsync()
    {
        if (_form is null)
            return;

        await _form.ValidateAsync();
        if (!_form.IsValid)
            return;

        _gonderiliyor = true;
        _formDurumMesaji = null;

        var cevap = await ApiIstemcisi.PostAsync<object>("api/iletisim", new
        {
            _formVerisi.AdSoyad,
            Email = _formVerisi.Eposta,
            _formVerisi.Telefon,
            _formVerisi.Konu,
            _formVerisi.Mesaj
        });

        _formBasariliMi = cevap?.BasariliMi == true;
        _formDurumMesaji = cevap?.Mesaj ?? DilServisi.T("iletisim.gonderimHata", "Mesaj gönderilemedi. Lütfen tekrar deneyin.");

        if (_formBasariliMi)
        {
            _formVerisi = new IletisimFormu();
            await _form.ResetAsync();
        }

        _gonderiliyor = false;
    }

    private sealed class IletisimFormu
    {
        public string AdSoyad { get; set; } = string.Empty;
        public string Eposta { get; set; } = string.Empty;
        public string Telefon { get; set; } = string.Empty;
        public string Konu { get; set; } = string.Empty;
        public string Mesaj { get; set; } = string.Empty;
    }

    public void Dispose()
    {
        DilServisi.DilDegisti -= DilDegistiginde;
    }
}
