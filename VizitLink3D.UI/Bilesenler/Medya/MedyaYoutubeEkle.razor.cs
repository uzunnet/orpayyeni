using System.Text.RegularExpressions;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Medya;
using VizitLink3D.UI.Servisler;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace VizitLink3D.UI.Bilesenler.Medya;

public partial class MedyaYoutubeEkle : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    [Parameter] public int? KlasorId { get; set; }

    private string _url = string.Empty;
    private string _videoId = string.Empty;
    private string _videoBaslik = string.Empty;
    private string _miniaturUrl = string.Empty;
    private string _hata = string.Empty;
    private bool _gecerli;
    private bool _onizlemeYukleniyor;

    private static readonly Regex YoutubeRegex = new(
        @"(?:https?:\/\/)?(?:www\.)?(?:youtube\.com\/(?:watch\?v=|embed\/|v\/|.*[?&]v=)|youtu\.be\/)([a-zA-Z0-9_-]{11})",
        RegexOptions.Compiled);

    private async Task UrlDogrula()
    {
        _hata = string.Empty;
        _miniaturUrl = string.Empty;
        _videoBaslik = string.Empty;
        _gecerli = false;

        if (string.IsNullOrWhiteSpace(_url)) return;

        var eslesme = YoutubeRegex.Match(_url);
        if (!eslesme.Success)
        {
            _hata = "Geçersiz YouTube URL. Lütfen geçerli bir YouTube bağlantısı girin.";
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
            if (cevap != null)
            {
                _videoBaslik = cevap.Baslik ?? string.Empty;
                _miniaturUrl = cevap.MiniaturUrl ?? $"https://img.youtube.com/vi/{_videoId}/hqdefault.jpg";
            }
            else
            {
                _miniaturUrl = $"https://img.youtube.com/vi/{_videoId}/hqdefault.jpg";
            }
        }
        catch
        {
            _miniaturUrl = $"https://img.youtube.com/vi/{_videoId}/hqdefault.jpg";
        }

        _onizlemeYukleniyor = false;
    }

    private async Task EkleAsync()
    {
        if (!_gecerli || MudDialog == null) return;

        var govde = new { videoId = _videoId, kaynakUrl = _url, klasorId = KlasorId };
        var cevap = await Api.PostAsync<Ortak.Modeller.Medya.Medya>("api/medya/youtube-ekle", govde);

        if (cevap?.BasariliMi == true)
        {
            Snackbar.Add("YouTube videosu medya havuzuna eklendi", Severity.Success);
            MudDialog.Close(DialogResult.Ok(cevap.Veri));
        }
        else
        {
            _hata = cevap?.Mesaj ?? "Video eklenemedi.";
        }
    }

    private void Iptal()
    {
        MudDialog?.Cancel();
    }

    private class YoutubeOnizlemeDto
    {
        public string? Baslik { get; set; }
        public string? MiniaturUrl { get; set; }
    }
}
