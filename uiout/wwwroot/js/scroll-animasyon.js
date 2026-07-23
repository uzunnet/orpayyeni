/**
 * scroll-animasyon.js — Aurelian Onyx
 * IntersectionObserver ile data-stitch-reveal atributü olan elementleri
 * görünür olduğunda tetikler.
 * IIFE, global scope kirletmez.
 */
(function () {
    'use strict';

    var gozlemci = null;

    function gozlemciOlustur() {
        if (gozlemci) return;

        gozlemci = new IntersectionObserver(function (girisler) {
            for (var i = 0; i < girisler.length; i++) {
                var giris = girisler[i];
                if (!giris.isIntersecting) continue;

                var el = giris.target;
                var tip = el.getAttribute('data-stitch-reveal') || 'up';

                if (tip === 'down') {
                    el.classList.add('stitch-reveal-down');
                } else if (tip === 'zoom') {
                    el.classList.add('stitch-reveal-zoom');
                } else if (tip === 'fade') {
                    el.classList.add('stitch-soft-fade');
                } else {
                    el.classList.add('stitch-reveal-up');
                }

                // Animasyonu tetiklemek için görünür class'ı ekle
                requestAnimationFrame(function () {
                    el.classList.add('gorunur');
                });

                // Bir kez tetiklendikten sonra gözlemlemeyi bırak
                gozlemci.unobserve(el);
            }
        }, {
            threshold: 0.15,
            rootMargin: '0px 0px -40px 0px'
        });
    }

    function elemanlariGozlemle() {
        gozlemciOlustur();
        var elemanlar = document.querySelectorAll('[data-stitch-reveal]:not(.gorunur)');
        for (var i = 0; i < elemanlar.length; i++) {
            gozlemci.observe(elemanlar[i]);
        }
    }

    // DOM hazırsa başlat
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', elemanlariGozlemle);
    } else {
        elemanlariGozlemle();
    }

    // Blazor enhanced navigation
    window.addEventListener('enhancedload', elemanlariGozlemle);

    // Global erişim
    window.scrollAnimasyon = {
        gozlemle: elemanlariGozlemle,
        sifirla: function () {
            if (gozlemci) { gozlemci.disconnect(); gozlemci = null; }
        }
    };

    // Blazor render'i JS'den once tamamlanmayabilir; birkac kez deneyerek
    // hedef id olustuktan sonra yumusak sekilde oraya kaydirir.
    window.vizitlink3dKancayaKaydir = function (id, denemeSayisi) {
        denemeSayisi = denemeSayisi || 0;
        var eleman = document.getElementById(id);
        if (eleman) {
            eleman.scrollIntoView({ behavior: 'smooth', block: 'start' });
        } else if (denemeSayisi < 20) {
            setTimeout(function () { window.vizitlink3dKancayaKaydir(id, denemeSayisi + 1); }, 100);
        }
    };
})();
