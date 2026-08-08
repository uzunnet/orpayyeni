using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages;

public partial class AnaSayfa : IDisposable
{
    [Inject] private IJSRuntime JS { get; set; } = null!;
    [Inject] private DilServisi DilServisi { get; set; } = default!;
    [Inject] private ApiIstemcisi Api { get; set; } = null!;


    private List<UrunKategori> _koleksiyonlar = new();
    private List<Urun> _oneCikanUrunler = new();
    private List<Slayt> _slaytlar = new();
    private List<HizmetAdimi> _hizmetAdimlari = new();
    private List<MusteriYorumu> _yorumlar = new();
    private Dictionary<string, string> _sayfaIcerikleri = new();

    private string _firmaSlug = "varsayilan";
    private bool _yukleniyor = true;

    protected override async Task OnInitializedAsync()
    {
        DilServisi.DilDegisti += DilDegistiginde;
        
        try {
            var slug = await JS.InvokeAsync<string>("localStorage.getItem", "aktif_firma");
            _firmaSlug = !string.IsNullOrEmpty(slug) ? slug : "varsayilan";
        } catch { }

        await VerileriYukleAsync();
    }

    private void DilDegistiginde() => InvokeAsync(StateHasChanged);

    private async Task VerileriYukleAsync()
    {
        _yukleniyor = true;
        try
        {
            var taskKoleksiyonlar = Api.GetAsync<List<UrunKategori>>("api/urun-kategorileri");
            var taskOneCikanlar = Api.GetAsync<List<Urun>>("api/urunler?oneCikan=true");
            var taskSlaytlar = Api.GetAsync<List<Slayt>>($"api/slaytlar?sayfaKodu=anasayfa&dil={DilServisi.AktifDil}");
            var taskHizmetAdimlari = Api.GetAsync<List<HizmetAdimi>>("api/hizmet-adimlari");
            var taskYorumlar = Api.GetAsync<List<MusteriYorumu>>("api/musteri-yorumlari");
            var taskSayfaIcerigi = Api.GetAsync<Dictionary<string, string>>($"api/sayfa-icerigi/anasayfa?dil={DilServisi.AktifDil}");

            await Task.WhenAll(taskKoleksiyonlar, taskOneCikanlar, taskSlaytlar, taskHizmetAdimlari, taskYorumlar, taskSayfaIcerigi);

            _koleksiyonlar = taskKoleksiyonlar.Result ?? new();
            _oneCikanUrunler = taskOneCikanlar.Result ?? new();
            _slaytlar = taskSlaytlar.Result ?? new();
            _hizmetAdimlari = taskHizmetAdimlari.Result ?? new();
            _yorumlar = taskYorumlar.Result ?? new();
            _sayfaIcerikleri = taskSayfaIcerigi.Result ?? new();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Anasayfa yükleme hatası: {ex.Message}");
        }
        finally
        {
            _yukleniyor = false;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("eval", @"
                setTimeout(() => {
                    // Reveal Animation with Intersection Observer
                    const reveals = document.querySelectorAll('.reveal');
                    const observer = new IntersectionObserver((entries, obs) => {
                        entries.forEach((entry, index) => {
                            if (entry.isIntersecting) {
                                if(!entry.target.classList.contains('delay-100') && !entry.target.classList.contains('delay-200') && !entry.target.classList.contains('delay-300')) {
                                    setTimeout(() => entry.target.classList.add('active'), (index % 3) * 150);
                                } else {
                                    entry.target.classList.add('active');
                                }
                                obs.unobserve(entry.target);
                            }
                        });
                    }, { threshold: 0.1, rootMargin: '0px 0px -50px 0px' });
                    reveals.forEach(r => observer.observe(r));

                    // Hero Slider
                    const slides = document.querySelectorAll('.slider-img');
                    const contents = document.querySelectorAll('.slider-content');
                    if (slides.length > 0) {
                        let currentSlide = 0;
                        setInterval(() => {
                            slides[currentSlide].classList.remove('active');
                            slides[currentSlide].classList.add('hidden');
                            if(contents.length > 0) {
                                contents[currentSlide].classList.remove('active');
                                contents[currentSlide].classList.add('hidden');
                            }
                            
                            currentSlide = (currentSlide + 1) % slides.length;
                            
                            slides[currentSlide].classList.remove('hidden');
                            slides[currentSlide].classList.add('active');
                            if(contents.length > 0) {
                                contents[currentSlide].classList.remove('hidden');
                                contents[currentSlide].classList.add('active');
                            }
                        }, 5000);
                    }

                    // Testimonials
                    window.currentTestimonial = 0;
                    window.showTestimonial = function(index) {
                        const testimonials = document.querySelectorAll('.testimonial-slide');
                        const dots = document.querySelectorAll('.dot-btn');
                        if(!testimonials.length) return;
                        
                        testimonials.forEach(t => { t.classList.remove('opacity-100', 'z-10'); t.classList.add('opacity-0', 'z-0', 'hidden'); });
                        dots.forEach(d => { d.classList.remove('bg-primary'); d.classList.add('bg-outline-variant', 'dark:bg-glass-stroke'); });
                        
                        window.currentTestimonial = index;
                        if(testimonials[index]) {
                            testimonials[index].classList.remove('opacity-0', 'z-0', 'hidden');
                            testimonials[index].classList.add('opacity-100', 'z-10');
                        }
                        if(dots[index]) {
                            dots[index].classList.remove('bg-outline-variant', 'dark:bg-glass-stroke');
                            dots[index].classList.add('bg-primary');
                        }
                    };
                    if(document.querySelectorAll('.testimonial-slide').length > 0) {
                        setInterval(() => {
                            const t = document.querySelectorAll('.testimonial-slide');
                            if (t.length > 0) window.showTestimonial((window.currentTestimonial + 1) % t.length);
                        }, 6000);
                    }

                    // Particle Generator
                    const container = document.getElementById('particles-container');
                    if(container) {
                        for(let i=0; i<30; i++) {
                            const p = document.createElement('div');
                            p.className = 'particle';
                            const size = Math.random() * 8 + 2;
                            p.style.width = size + 'px';
                            p.style.height = size + 'px';
                            p.style.left = Math.random() * 100 + 'vw';
                            p.style.top = Math.random() * 100 + 'vh';
                            p.style.animationDuration = (Math.random() * 10 + 10) + 's';
                            p.style.animationDelay = (Math.random() * 10) + 's';
                            container.appendChild(p);
                        }
                    }
                }, 300);
            ");
        }
    }

    private string GorselUrlGetir(Urun urun)
    {
        var ilkMedyaUrl = urun.Medyalar?
            .Where(m => !m.SilindiMi)
            .OrderBy(m => m.SiraNo)
            .FirstOrDefault(m => m.AnaGosterim)?.MedyaUrl
            ?? urun.Medyalar?
                .Where(m => !m.SilindiMi)
                .OrderBy(m => m.SiraNo)
                .FirstOrDefault()?.MedyaUrl;

        if (!string.IsNullOrEmpty(ilkMedyaUrl))
        {
            var apiBase = Config["ApiTemelUrl"] ?? "http://localhost:5015";
            return apiBase + ilkMedyaUrl;
        }

        return "/medya/urunler/bilesenler/Kasa/Kasa_18-18_MDF_Kasa_A.jpg";
    }

    public void Dispose()
    {
        DilServisi.DilDegisti -= DilDegistiginde;
    }
}
