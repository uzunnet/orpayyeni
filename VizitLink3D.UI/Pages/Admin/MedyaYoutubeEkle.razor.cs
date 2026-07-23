using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using VizitLink3D.Ortak.Modeller.Medya;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages.Admin;

public partial class MedyaYoutubeEkle : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private DilServisi DilServisi { get; set; } = default!;

    private static readonly Regex YoutubeRegex = new(
        @"(?:https?:\/\/)?(?:www\.)?(?:youtube\.com\/(?:watch\?v=|embed\/|v\/|.*[?&]v=)|youtu\.be\/)([a-zA-Z0-9_-]{11})",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private string _url = string.Empty;
    private string? _baslik;
    private string _videoId = string.Empty;
    private string _videoBaslik = string.Empty;
    private string _miniaturUrl = string.Empty;
    private string _hata = string.Empty;
    private bool _gecerli;
    private bool _kaydediliyor;
    private bool _onizlemeYukleniyor;

    private async Task UrlDogrula()
    {
        _hata = string.Empty;
        _miniaturUrl = string.Empty;
        _videoBaslik = string.Empty;
        _gecerli = false;

        if (string.IsNullOrWhiteSpace(_url))
            return;

        var eslesme = YoutubeRegex.Match(_url);
        if (!eslesme.Success)
        {
            _hata = DilServisi.T("medya.youtubeGecersizUrl", "Geçersiz YouTube URL. Lütfen geçerli bir YouTube bağlantısı girin.");
            return;
        }

        _videoId = eslesme.Groups[1].Value;
        _gecerli = true;
        await OnizlemeYukleAsync();
    }

    private async Task OnizlemeYukleAsync()
    {
        _onizlemeYukleniyor = true;

        try
        {
            var cevap = await Api.GetAsync<YoutubeOnizlemeDto>($"api/medya/youtube-onizleme?videoId={_videoId}");
            _videoBaslik = cevap?.Baslik ?? string.Empty;
            _miniaturUrl = cevap?.MiniaturUrl ?? $"https://img.youtube.com/vi/{_videoId}/hqdefault.jpg";
        }
        catch
        {
            _miniaturUrl = $"https://img.youtube.com/vi/{_videoId}/hqdefault.jpg";
        }
        finally
        {
            _onizlemeYukleniyor = false;
        }
    }

    private async Task EkleAsync()
    {
        if (!_gecerli || _kaydediliyor)
            return;

        _kaydediliyor = true;

        try
        {
            var cevap = await Api.PostAsync<Medya>("api/medya/youtube-ekle", new
            {
                videoId = _videoId,
                kaynakUrl = _url,
                baslik = _baslik
            });

            if (cevap?.BasariliMi == true)
            {
                Snackbar.Add(DilServisi.T("medya.youtubeEklendi", "YouTube videosu medya havuzuna eklendi."), Severity.Success);
                FormuTemizle();
                return;
            }

            _hata = cevap?.Mesaj ?? DilServisi.T("medya.youtubeEklenemedi", "Video eklenemedi.");
            Snackbar.Add(_hata, Severity.Error);
        }
        catch
        {
            _hata = DilServisi.T("medya.youtubeEklenemedi", "Video eklenemedi.");
            Snackbar.Add(_hata, Severity.Error);
        }
        finally
        {
            _kaydediliyor = false;
        }
    }

    private void FormuTemizle()
    {
        _url = string.Empty;
        _baslik = null;
        _videoId = string.Empty;
        _videoBaslik = string.Empty;
        _miniaturUrl = string.Empty;
        _hata = string.Empty;
        _gecerli = false;
    }

    private sealed class YoutubeOnizlemeDto
    {
        public string? Baslik { get; set; }
        public string? MiniaturUrl { get; set; }
    }
}
