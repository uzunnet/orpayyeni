using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.SuperAdmin.VeriTabani;

namespace VizitLink3D.SuperAdmin.Components.Pages;

[Authorize]
public partial class ModulYonetimi : ComponentBase
{
    [Inject] private SuperAdminDbContext Vt { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private IDialogService DialogServisi { get; set; } = null!;

    private bool _yukleniyor = true;
    private bool _atamaYapiliyor;
    private List<Modul> _tumModuller = new();
    private List<Firma> _firmalar = new();
    private Firma? _seciliFirma;
    private Dictionary<int, bool> _firmaModulDurumu = new();
    private Dictionary<int, int> _modulFirmaSayisi = new();
    private int _toplamModulAtamasi;

    // Arama ve filtreleme
    private string _aramaMetni = "";
    private string _kategoriFiltresi = "";
    private List<Modul> _filtreliModuller = new();

    protected override async Task OnInitializedAsync()
    {
        _tumModuller = await Vt.Moduller.OrderBy(m => m.Kategori).ThenBy(m => m.Ad).ToListAsync();
        _firmalar = await Vt.Firmalar.Where(f => f.AktifMi).OrderBy(f => f.Ad).ToListAsync();

        // Her modül için firma sayısını hesapla
        var tumAtamalar = await Vt.FirmaModulAtamalari.AsNoTracking().ToListAsync();
        _modulFirmaSayisi = _tumModuller.ToDictionary(
            m => m.Id,
            m => tumAtamalar.Count(a => a.ModulId == m.Id));
        _toplamModulAtamasi = tumAtamalar.Count;

        Filtrele();
        _yukleniyor = false;
    }

    private void Filtrele()
    {
        var sonuc = _tumModuller.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(_aramaMetni))
        {
            var arama = _aramaMetni.ToLower();
            sonuc = sonuc.Where(m => m.Ad.Contains(arama, StringComparison.OrdinalIgnoreCase) || m.Kod.Contains(arama, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(_kategoriFiltresi))
        {
            sonuc = sonuc.Where(m => m.Kategori == _kategoriFiltresi);
        }

        _filtreliModuller = sonuc.ToList();
    }

    private void AramaDegisti()
    {
        Filtrele();
    }

    private async Task FirmaSecildi()
    {
        if (_seciliFirma is null) { _firmaModulDurumu.Clear(); return; }
        _atamaYapiliyor = true;

        var mevcutAtamalar = await Vt.FirmaModulAtamalari
            .Where(fa => fa.FirmaId == _seciliFirma.Id)
            .Select(fa => fa.ModulId)
            .ToListAsync();

        _firmaModulDurumu = _tumModuller.ToDictionary(m => m.Id, m => mevcutAtamalar.Contains(m.Id));

        _atamaYapiliyor = false;
        StateHasChanged();
    }

    private async Task ModulDurumunuDegistir(int modulId, bool yeniDurum)
    {
        if (_seciliFirma is null) return;
        _atamaYapiliyor = true;

        try
        {
            if (yeniDurum)
            {
                var mevcut = await Vt.FirmaModulAtamalari.AnyAsync(fa => fa.FirmaId == _seciliFirma.Id && fa.ModulId == modulId);
                if (!mevcut)
                {
                    Vt.FirmaModulAtamalari.Add(new FirmaModulAtama
                    {
                        FirmaId = _seciliFirma.Id,
                        ModulId = modulId,
                        AtanmaTarihi = DateTimeOffset.UtcNow
                    });
                    await Vt.SaveChangesAsync();
                    _modulFirmaSayisi[modulId] = (_modulFirmaSayisi.ContainsKey(modulId) ? _modulFirmaSayisi[modulId] : 0) + 1;
                    _toplamModulAtamasi++;
                }
            }
            else
            {
                var atama = await Vt.FirmaModulAtamalari.FirstOrDefaultAsync(fa => fa.FirmaId == _seciliFirma.Id && fa.ModulId == modulId);
                if (atama is not null)
                {
                    Vt.FirmaModulAtamalari.Remove(atama);
                    await Vt.SaveChangesAsync();
                    if (_modulFirmaSayisi.ContainsKey(modulId)) _modulFirmaSayisi[modulId]--;
                    _toplamModulAtamasi--;
                }
            }

            _firmaModulDurumu[modulId] = yeniDurum;
            var modulAd = _tumModuller.FirstOrDefault(m => m.Id == modulId)?.Ad ?? "?";
            Snackbar.Add(yeniDurum ? $"\"{modulAd}\" → {_seciliFirma.Ad}" : $"\"{modulAd}\" ← {_seciliFirma.Ad}", Severity.Success);
        }
        catch (Exception ex) { Snackbar.Add($"Hata: {ex.Message}", Severity.Error); }
        finally { _atamaYapiliyor = false; }
    }

    private async Task ModullerinTamaminiAc()
    {
        if (_seciliFirma is null) return;
        var sonuc = await DialogServisi.ShowMessageBoxAsync("Onay", "Tüm modülleri aktif etmek istediğinize emin misiniz?", yesText: "Evet", cancelText: "İptal");
        if (sonuc != true) return;

        foreach (var modul in _tumModuller)
        {
            if (!_firmaModulDurumu.ContainsKey(modul.Id) || !_firmaModulDurumu[modul.Id])
            {
                await ModulDurumunuDegistir(modul.Id, true);
            }
        }
    }

    private async Task ModullerinTamaminiKapat()
    {
        if (_seciliFirma is null) return;
        var sonuc = await DialogServisi.ShowMessageBoxAsync("Onay", "Tüm modülleri kapatmak istediğinize emin misiniz?", yesText: "Evet", cancelText: "İptal");
        if (sonuc != true) return;

        foreach (var modul in _tumModuller)
        {
            if (_firmaModulDurumu.ContainsKey(modul.Id) && _firmaModulDurumu[modul.Id])
            {
                await ModulDurumunuDegistir(modul.Id, false);
            }
        }
    }
}
