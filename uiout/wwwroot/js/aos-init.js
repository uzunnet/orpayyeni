// Basit ve performanslı Animate On Scroll (AOS) alternatifi
// Elementlerin ekrana girdiğinde animasyon sınıflarını (gb-anim-*) tetiklemesi için kullanılır.

window.BasitAOS = {
    baslat: function () {
        const elements = document.querySelectorAll('.gb-observe');
        
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    // Element ekrana girdiğinde
                    const el = entry.target;
                    const animClass = el.getAttribute('data-anim') || 'gb-anim-fade-up';
                    
                    // Gecikme varsa uygula
                    const delay = el.getAttribute('data-delay');
                    if(delay) {
                        el.style.animationDelay = delay + 's';
                    }

                    // Sınıfı ekle ve opaklığı düzelt
                    el.classList.add(animClass);
                    el.classList.remove('gb-observe'); // Sadece bir kere çalışsın
                    observer.unobserve(el);
                }
            });
        }, {
            threshold: 0.15, // Elementin %15'i göründüğünde tetikle
            rootMargin: '0px 0px -50px 0px' // Biraz daha aşağı inince tetikle
        });

        elements.forEach(el => {
            // Başlangıçta görünmez yap (Animasyon sınıfları zaten opacity: 0 yapar ama emin olmak için)
            el.style.opacity = '0';
            observer.observe(el);
        });
    }
};

// Sayfa yüklendiğinde başlat
document.addEventListener('DOMContentLoaded', () => {
    // Blazor SPA olduğu için DOMContentLoaded yeterli olmayabilir.
    // İlk render için gecikmeli de başlat.
    setTimeout(() => {
        if(window.BasitAOS) window.BasitAOS.baslat();
    }, 500);
});

// Blazor yönlendirmelerinden sonra (OnAfterRender) çağrılabilmesi için global fonskiyon
window.AOSTetikle = function() {
    if(window.BasitAOS) window.BasitAOS.baslat();
}
