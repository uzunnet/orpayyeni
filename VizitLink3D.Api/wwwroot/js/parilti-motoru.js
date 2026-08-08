/**
 * parilti-motoru.js — Aurelian Onyx
 * Shimmer, gold-underline, active-dot efektlerini yönetir.
 * IIFE ile global scope kirletmez. Tema.js'den sonra yüklenir.
 */
(function () {
    'use strict';

    var gozlemciAktif = false;

    /** data-stitch-shimmer atributü olan elementlere shimmer pseudo uygular */
    function shimmerBaslat() {
        var elemanlar = document.querySelectorAll('[data-stitch-shimmer]');
        for (var i = 0; i < elemanlar.length; i++) {
            elemanlar[i].classList.add('anim-shimmer');
        }
    }

    /** data-stitch-gold-underline atributü olan elementlere hover underline uygular */
    function goldUnderlineBaslat() {
        var elemanlar = document.querySelectorAll('[data-stitch-gold-underline]');
        for (var i = 0; i < elemanlar.length; i++) {
            elemanlar[i].classList.add('stitch-gold-underline');
        }
    }

    /** Aktif nav linkine dot ekler */
    function activeDotBaslat() {
        var aktifLink = document.querySelector('.vizit-nav-link[href="' + window.location.pathname + '"], ' +
            '.vizit-nav-link.aktif');
        if (aktifLink) {
            aktifLink.classList.add('stitch-active-dot');
        }
    }

    /** Tüm efektleri başlat */
    function tumunuBaslat() {
        shimmerBaslat();
        goldUnderlineBaslat();
        activeDotBaslat();
        gozlemciAktif = true;
    }

    // DOM hazırsa hemen başlat, değilse DOMContentLoaded bekle
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', tumunuBaslat);
    } else {
        tumunuBaslat();
    }

    // Blazor sayfa geçişlerinde yeniden çalıştır
    window.addEventListener('enhancedload', tumunuBaslat);

    // Global erişim (tema.js veya Blazor için)
    window.pariltiMotoru = {
        baslat: tumunuBaslat,
        shimmerBaslat: shimmerBaslat,
        goldUnderlineBaslat: goldUnderlineBaslat,
        activeDotBaslat: activeDotBaslat
    };
})();
