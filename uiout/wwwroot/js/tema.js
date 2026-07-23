/**
 * VizitLink3D Tema Motoru
 * localStorage + CSS degiskenleri ile admin panel temasini canli degistirir.
 */
window.vizitlink3dTema = {
    siteBaslat: async function (varsayilanTema) {
        var varsayilanFirma = 'orpay';

        if (window.location.pathname.indexOf('/admin') === 0) {
            this.adminTemaIzoleEt();
            return;
        }

        var aktifFirma = localStorage.getItem('aktif_firma');
        if (!aktifFirma) {
            aktifFirma = varsayilanFirma;
            localStorage.setItem('aktif_firma', aktifFirma);
        }

        var temaSlug = localStorage.getItem('vizitlink3d_site_tema') || varsayilanTema || 'gold';
        var apiTemelUrl = window.vizitlink3dApiBaseUrl || window.location.origin;

        try {
            var cevap = await fetch(apiTemelUrl + '/api/firma-tema', {
                headers: { 'X-Firma': aktifFirma }
            });

            if (cevap.ok) {
                var json = await cevap.json();
                var apiTema = json && json.veri && json.veri.siteTema;
                if (apiTema) {
                    temaSlug = apiTema;
                }
            }
        } catch (e) { }

        localStorage.setItem('vizitlink3d_site_tema', temaSlug);
        this.siteUygula(temaSlug);
    },

    uygula: function (birincil, vurgu, arkaPlan, yuzey, koyuTemaMi, adminTemaAdi) {
        var root = document.documentElement;
        root.setAttribute('data-admin-tema', adminTemaAdi || '');
        this.adminTemaIzoleEt();
        root.style.setProperty('--admin-bg', arkaPlan);
        root.style.setProperty('--admin-surface', yuzey);
        root.style.setProperty('--admin-surface-hover', koyuTemaMi ? '#1a1a1a' : '#f0f0f0');
        root.style.setProperty('--admin-surface-raised', koyuTemaMi ? '#161616' : '#fafafa');
        root.style.setProperty('--admin-primary', birincil);
        root.style.setProperty('--admin-primary-light', koyuTemaMi ? '#151515' : '#f5f5f5');
        root.style.setProperty('--admin-accent', vurgu);
        root.style.setProperty('--admin-accent-light', vurgu + 'CC');
        root.style.setProperty('--admin-text', koyuTemaMi ? '#ffffff' : '#1a1a1a');
        root.style.setProperty('--admin-text-secondary', koyuTemaMi ? 'rgba(255,255,255,0.70)' : 'rgba(0,0,0,0.65)');
        root.style.setProperty('--admin-text-muted', koyuTemaMi ? 'rgba(255,255,255,0.40)' : 'rgba(0,0,0,0.40)');
        root.style.setProperty('--admin-text-dim', koyuTemaMi ? 'rgba(255,255,255,0.20)' : 'rgba(0,0,0,0.15)');
        root.style.setProperty('--admin-border', koyuTemaMi ? 'rgba(255,255,255,0.06)' : 'rgba(0,0,0,0.08)');
        root.style.setProperty('--admin-border-light', koyuTemaMi ? 'rgba(255,255,255,0.04)' : 'rgba(0,0,0,0.04)');
        root.style.setProperty('--admin-border-focus', 'rgba(' + this._hexToRgb(vurgu) + ',0.25)');
        root.style.setProperty('--admin-shadow', koyuTemaMi ? '0 4px 24px rgba(0,0,0,0.3)' : '0 4px 24px rgba(0,0,0,0.06)');
        root.style.setProperty('--admin-shadow-hover', koyuTemaMi ? '0 8px 32px rgba(0,0,0,0.5)' : '0 8px 32px rgba(0,0,0,0.1)');
        root.style.setProperty('--admin-shadow-glow', '0 0 20px rgba(' + this._hexToRgb(vurgu) + ',0.08)');

    },

    /**
     * Site temasını uygular — data-tema-id attribute'unu set eder,
     * CSS dosyalarını lazy load eder, sayfa yenilenmez.
     */
    siteUygula: function (temaSlug) {
        if (!temaSlug) return;
        if (window.location.pathname.indexOf('/admin') === 0) {
            this.adminTemaIzoleEt();
            return;
        }

        var root = document.documentElement;
        var apiTemelUrl = window.vizitlink3dApiBaseUrl || window.location.origin;
        root.setAttribute('data-tema-id', temaSlug);
        root.setAttribute('data-site-tema', temaSlug); // geriye uyumluluk

        // localStorage'dan kayıtlı modu oku, yoksa koyu varsay
        var mevcutMod = localStorage.getItem('temaMod') || 'koyu';
        root.setAttribute('data-tema-mod', mevcutMod);

        var mevcutTemaLinkleri = document.querySelectorAll('link[data-tema-parca]');
        mevcutTemaLinkleri.forEach(function (link) {
            var parcaDegeri = link.dataset.temaParca || '';
            if (!parcaDegeri.startsWith(temaSlug + '-')) {
                link.parentNode && link.parentNode.removeChild(link);
            }
        });

        var dosyalar = ['tokens', 'bilesenler', 'animasyonlar'];
        dosyalar.forEach(function (dosya) {
            var parcaDegeri = temaSlug + '-' + dosya;
            if (!document.querySelector('style[data-tema-parca="' + parcaDegeri + '"]')) {
                fetch(apiTemelUrl + '/api/tema/css/' + encodeURIComponent(temaSlug) + '/' + encodeURIComponent(dosya), {
                    cache: 'no-store'
                })
                    .then(function (r) { return r.text(); })
                    .then(function (css) {
                        var style = document.createElement('style');
                        style.textContent = css;
                        style.dataset.temaParca = parcaDegeri;
                        document.head.appendChild(style);
                    })
                    .catch(function (e) { console.error('CSS yüklenemedi: ' + dosya, e); });
            }
        });

    },

    adminTemaIzoleEt: function () {
        var root = document.documentElement;
        // Admin paneli her zaman koyu modda calisir — site temasindan bagimsiz
        root.removeAttribute('data-tema-id');
        root.removeAttribute('data-site-tema');
        root.setAttribute('data-tema-mod', 'koyu');
        root.style.colorScheme = 'dark';
        localStorage.setItem('temaMod', 'koyu');

        var temaLinkleri = document.querySelectorAll('link[data-tema-parca]');
        temaLinkleri.forEach(function (link) {
            link.parentNode && link.parentNode.removeChild(link);
        });
    },

    /**
     * Tema modunu (acik/koyu) değiştirir.
     * localStorage'a kaydeder ve data-tema-mod attribute'unu günceller.
     */
    modUygula: function (mod) {
        var root = document.documentElement;
        root.setAttribute('data-tema-mod', mod);
        localStorage.setItem('temaMod', mod);
        root.style.colorScheme = mod === 'acik' ? 'light' : 'dark';
    },

    adminModUygula: function (_mod) {
        // Admin paneli her zaman koyu modda kalir
        this.uygula(
            '#C5A059',
            '#d4a574',
            '#0a0a0a',
            '#111111',
            true,
            'orpay-admin'
        );

        document.documentElement.setAttribute('data-tema-mod', 'koyu');
        document.documentElement.style.colorScheme = 'dark';
        localStorage.setItem('temaMod', 'koyu');
    },

    _hexToRgb: function (hex) {
        var h = hex.replace('#', '');
        if (h.length === 3) h = h[0] + h[0] + h[1] + h[1] + h[2] + h[2];
        var r = parseInt(h.substring(0, 2), 16);
        var g = parseInt(h.substring(2, 4), 16);
        var b = parseInt(h.substring(4, 6), 16);
        return r + ',' + g + ',' + b;
    }
};

window.vizitlink3dDil = {
    tercihDiliniGetir: function () {
        var diller = navigator.languages && navigator.languages.length
            ? navigator.languages
            : [navigator.language || navigator.userLanguage || 'tr'];

        return diller[0] || 'tr';
    },

    htmlDiliniAyarla: function (dil) {
        document.documentElement.setAttribute('lang', dil || 'tr');
    }
};
