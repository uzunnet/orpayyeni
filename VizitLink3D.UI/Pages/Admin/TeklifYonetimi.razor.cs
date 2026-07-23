using VizitLink3D.Ortak.Modeller.Urunler;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages.Admin;

public partial class TeklifYonetimi : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private List<TeklifIstegi> _liste = [];
    private List<TeklifIstegi> _filtreliListe = [];
    private bool _yukleniyor = true;
    private string _arama = "";
    private TeklifIstegi? _seciliTeklif;

    private int _bekleyenSayisi;
    private int _incelemedeSayisi;
    private int _teklifHazirSayisi;
    private int _tamamlandiSayisi;

    private static readonly string[] DurumSirasi = { "Bekliyor", "Incelemede", "TeklifHazirlandi", "Tamamlandi" };

    protected override async Task OnInitializedAsync() => await Yukle();

    async Task Yukle()
    {
        _yukleniyor = true;
        StateHasChanged();
        _liste = await Api.GetAsync<List<TeklifIstegi>>("api/teklifler") ?? [];
        SayilariGuncelle();
        AramaUygula();
        _yukleniyor = false;
    }

    void SayilariGuncelle()
    {
        _bekleyenSayisi = _liste.Count(x => x.Durum == "Bekliyor");
        _incelemedeSayisi = _liste.Count(x => x.Durum == "Incelemede");
        _teklifHazirSayisi = _liste.Count(x => x.Durum == "TeklifHazirlandi");
        _tamamlandiSayisi = _liste.Count(x => x.Durum == "Tamamlandi");
    }

    void AramaYap(KeyboardEventArgs e) => AramaUygula();

    void AramaUygula()
    {
        var a = _arama?.ToLower() ?? "";
        _filtreliListe = string.IsNullOrWhiteSpace(a)
            ? _liste
            : _liste.Where(x =>
                (x.MusteriAdSoyad?.ToLower().Contains(a) ?? false) ||
                (x.Eposta?.ToLower().Contains(a) ?? false) ||
                (x.Telefon?.ToLower().Contains(a) ?? false)).ToList();
    }

    void DetayAcKapa(TeklifIstegi t)
    {
        _seciliTeklif = _seciliTeklif?.Id == t.Id ? null : t;
    }

    async Task IptalEt(TeklifIstegi t) => await DurumDegistir(t, "İptal");

    async Task DurumDegistir(TeklifIstegi t, string yeniDurum)
    {
        t.Durum = yeniDurum;
        await Api.PutAsync<TeklifIstegi>($"api/teklifler/{t.Id}", t);
        Snackbar.Add($"Teklif durumu '{DurumMetni(yeniDurum)}' olarak güncellendi.", Severity.Success);
        await Yukle();
    }

    List<string> GecerliDurumlar(string mevcutDurum)
    {
        var index = Array.IndexOf(DurumSirasi, mevcutDurum);
        if (index < 0) return [];
        return DurumSirasi.Skip(index + 1).ToList();
    }

    string DurumMetni(string d) => d switch
    {
        "Bekliyor" => "Bekliyor",
        "Incelemede" => "İnceleniyor",
        "TeklifHazirlandi" => "Teklif Hazır",
        "Tamamlandi" => "Tamamlandı",
        "İptal" => "İptal",
        _ => d
    };

    string DurumRengi(string d) => d switch
    {
        "Bekliyor" => "var(--mud-palette-warning)",
        "Incelemede" => "var(--mud-palette-info)",
        "TeklifHazirlandi" => "var(--mud-palette-primary)",
        "Tamamlandi" => "var(--mud-palette-success)",
        "İptal" => "var(--mud-palette-error)",
        _ => "var(--mud-palette-default)"
    };
}
