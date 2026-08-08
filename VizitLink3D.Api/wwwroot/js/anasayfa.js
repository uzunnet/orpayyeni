/**
 * Orpay Ana Sayfa / Global JS
 * Tema yonetimi icin vizitlink3dTema motorunu kullanir.
 * Otomatik tema baslatma yapmaz — her sayfa/layout kendi OnAfterRenderAsync'inda baslatir.
 */
function anasayfaBaslat() {
    var reveals = document.querySelectorAll('.reveal');
    var observer = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
            if (entry.isIntersecting) {
                entry.target.classList.add('active');
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.1, rootMargin: '0px 0px -50px 0px' });
    reveals.forEach(function (el) { observer.observe(el); });

    var slides = document.querySelectorAll('.hero-slider-img');
    if (slides.length > 0) {
        var cur = 0;
        setInterval(function () {
            slides[cur].classList.remove('active');
            cur = (cur + 1) % slides.length;
            slides[cur].classList.add('active');
        }, 5000);
    }

    document.querySelectorAll('.accordion-button').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var c = btn.nextElementSibling;
            if (c && c.style.maxHeight) {
                c.style.maxHeight = null;
            } else if (c) {
                c.style.maxHeight = c.scrollHeight + 'px';
            }
        });
    });
}

// Geriye uyumlu: aktif tema slug'ini dondur
window.orpayAktifTemaSlug = function () {
    var root = document.documentElement;
    return root.getAttribute('data-tema-id') ||
           localStorage.getItem('orpay_site_tema') ||
           localStorage.getItem('vizitlink3d_site_tema') ||
           'luxe-industrial-dark';
};

// Geriye uyumlu: tema CSS yukle
window.orpayTemaCssYukle = function (temaOverride) {
    var tema = temaOverride || window.orpayAktifTemaSlug();
    if (typeof vizitlink3dTema !== 'undefined') {
        vizitlink3dTema.temaYukle(tema);
    }
};

// Geriye uyumlu: tema degistir
window.orpayTemaDegistir = function () {
    if (typeof vizitlink3dTema !== 'undefined') {
        vizitlink3dTema.orpayTemaDegistir();
    }
};

// Geriye uyumlu: tema class korumasi (bos - yeni motor yapiyor)
window.orpayTemaClassKoru = function () {
    /* Yeni tema motoru MutationObserver ile koruyor */
};
