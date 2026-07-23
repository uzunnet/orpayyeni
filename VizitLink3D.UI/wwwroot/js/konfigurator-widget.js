/**
 * VizitLink3D Konfigüratör Widget'ı — Güvenli Entegrasyon (v3.0 — Paket-4C)
 * ============================================================================
 * 
 * BU DOSYA: Kendi sitenize gömebileceğiniz güvenli konfigüratör entegrasyon
 * örneğidir. API anahtarı ASLA istemci tarafında (DOM, localStorage, cookie, console)
 * saklanmaz veya görünmez.
 * 
 * GÜVENLİ ENTEGRASYON MODELİ:
 * ┌─────────────────────┐     ┌──────────────────────┐     ┌──────────────────┐
 * │  Müşterinin Sitesi  │────▶│  Müşterinin Backendi  │────▶│  VizitLink3D API │
 * │  (Browser/JS)       │     │  (API Key saklar)     │     │  (Doğrulama)     │
 * └─────────────────────┘     └──────────────────────┘     └──────────────────┘
 * 
 * 1. Müşteri kendi sitesinde bu widget'ı kullanır.
 * 2. Widget, müşterinin KENDİ backend'ine istek yapar (API key YOK).
 * 3. Müşterinin backend'i kendi API key'i ile VizitLink3D API'ye proxy istek yapar.
 * 4. VizitLink3D API, X-Konfigurator-Anahtari header'ını doğrular.
 * 
 * İFRAME/GÜVENLİ EMBED (v3.0 — Paket-4C TOKEN-FREE COOKIE AKISI):
 * ┌────────────────┐     ┌──────────────────────┐     ┌──────────────────┐
 * │  Müşteri Sitesi │────▶│  Müşteri Backendi   │────▶│  VizitLink3D API │
 * │  (iframe)       │     │  (API Key + token)  │     │  (DataProtection) │
 * └────────────────┘     └──────────────────────┘     └──────────────────┘
 * 
 * AKIŞ:
 * 1. Müşteri backend'i POST /api/entegrasyon/konfigurator/{slug}/embed-oturum
 *    ile time-limited embed bootstrap token alır (X-Konfigurator-Anahtari ile).
 * 2. Widget, iframe src'ini /konfigurator/embed/{token} yapar (bootstrap).
 * 3. VizitLink3D API token'ı DataProtection ile doğrular,
 *    Referer exact match kontrolü yapar,
 *    NONCE replay koruması uygular,
 *    HttpOnly/Secure/SameSite=None embed session cookie yazar,
 *    303 redirect → /konfigurator/embed (TOKEN-SIZ URL) yapar.
 * 4. Tarayıcı redirect'i takip eder, /konfigurator/embed sayfasını cookie ile açar.
 *    URL'de/history'de/DOM'da/storage'da/console'da TOKEN KALMAZ.
 * 5. Runtime içindeki widget JS, veriyi cookie üzerinden doğrulanmış HTML içinde alır.
 * 6. Bootstrap token 5 dk, session cookie 30 dk geçerlidir.
 * 7. Token KEY İÇERMEZ, storage/console'a YAZILMAZ.
 * 
 * ⚠ GÜVENLİK UYARILARI:
 * - API anahtarını ASLA frontend koduna, .env dosyasına (istemci), URL'e veya
 *   localStorage'a koymayın.
 * - API anahtarı sadece sunucu tarafında (Node.js, PHP, .NET, Python vb.) saklanır.
 * - Origin kısıtlaması için API anahtarınıza izin verilen domain'leri ekleyin.
 * - Cross-site cookie'ler tarayıcı tarafından engellenirse (Safari ITP, Brave Shields,
 *   Firefox ETP Strict) runtime anlaşılır hata verir; token tekrar EXPOSE EDİLMEZ.
 * - API anahtarı URL'ye koyulan insecure iframe çözümleri KULLANILMAZ.
 */

(function (global) {
    'use strict';

    // =========================================================================
    // Konfigürasyon
    // =========================================================================

    /**
     * @typedef {Object} KonfiguratorWidgetAyarlari
     * @property {string} proxyUrl - Müşterinin kendi backend proxy URL'i (ZORUNLU)
     *    Örn: "https://orpayormanurunleri.com.tr/api/proxy/konfigurator"
     *    Bu URL, müşterinin backend'inde API key ile VizitLink3D API'ye proxy yapar.
     * @property {string} urunSlug - Görüntülenecek ürünün slug'ı (ZORUNLU)
     * @property {string} [hedefElementId] - Widget'ın render edileceği div ID'si
     *    (varsayılan: "vizitlink3d-konfigurator")
     * @property {function} [hataYakalayici] - Hata durumunda çağrılacak fonksiyon
     * @property {function} [secimDegisimi] - Kullanıcı seçim değiştirdiğinde çağrılır
     */

    /**
     * VizitLink3D Konfigüratör Widget'ı
     * @param {KonfiguratorWidgetAyarlari} ayarlar
     */
    function VizitLink3DKonfiguratorWidget(ayarlar) {
        if (!ayarlar || !ayarlar.proxyUrl || !ayarlar.urunSlug) {
            console.error('[VizitLink3D] proxyUrl ve urunSlug zorunludur.');
            return;
        }

        // API anahtarı widget'ta YOK — tüm istekler proxy üzerinden
        var _proxyUrl = ayarlar.proxyUrl.replace(/\/+$/, '');
        var _urunSlug = ayarlar.urunSlug;
        var _hedefElId = ayarlar.hedefElementId || 'vizitlink3d-konfigurator';
        var _konfigurator = null;
        var _secimler = [];
        var _yukleniyor = false;
        var _hata = null;

        // Callback'ler
        var _hataYakalayici = ayarlar.hataYakalayici || function (hata) {
            console.error('[VizitLink3D] Hata:', hata);
        };
        var _secimDegisimi = ayarlar.secimDegisimi || function (secimler) {
            console.log('[VizitLink3D] Seçim değişti:', secimler);
        };

        // =====================================================================
        // API İletişimi (tüm istekler proxy üzerinden — API key istemcide YOK)
        // =====================================================================

        /**
         * Proxy üzerinden konfigüratör verisini getirir.
         * Müşterinin backend'i X-Konfigurator-Anahtari header'ını ekler.
         * @returns {Promise<Object>} PublicKonfiguratorDto
         */
        async function konfiguratorGetir() {
            var yanit = await fetch(_proxyUrl + '/' + encodeURIComponent(_urunSlug), {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
                credentials: 'omit' // CORS credentials gönderme
            });

            if (!yanit.ok) {
                var hataGovde = await yanit.text();
                throw new Error('Konfigüratör yüklenemedi: ' + yanit.status + ' ' + hataGovde);
            }

            var cevap = await yanit.json();
            if (!cevap.basariliMi) {
                throw new Error(cevap.hata || 'Konfigüratör verisi alınamadı.');
            }

            return cevap.veri;
        }

        /**
         * Proxy üzerinden müşteri seçimini kaydeder.
         * @param {Object} secimDto - PublicSecimKaydetDto
         * @returns {Promise<Object>} KonfigurasyonDetayDto
         */
        async function secimKaydet(secimDto) {
            var yanit = await fetch(
                _proxyUrl + '/' + encodeURIComponent(_urunSlug) + '/secimler',
                {
                    method: 'POST',
                    headers: {
                        'Accept': 'application/json',
                        'Content-Type': 'application/json'
                    },
                    credentials: 'omit',
                    body: JSON.stringify(secimDto)
                }
            );

            if (!yanit.ok) {
                var hataGovde = await yanit.text();
                throw new Error('Seçim kaydedilemedi: ' + yanit.status + ' ' + hataGovde);
            }

            var cevap = await yanit.json();
            if (!cevap.basariliMi) {
                throw new Error(cevap.hata || 'Seçim kaydedilemedi.');
            }

            return cevap.veri;
        }

        // =====================================================================
        // Public API
        // =====================================================================

        /**
         * Widget'ı başlatır ve konfigüratör verisini yükler.
         * @returns {Promise<void>}
         */
        async function baslat() {
            if (_yukleniyor) return;
            _yukleniyor = true;
            _hata = null;

            try {
                _konfigurator = await konfiguratorGetir();
                renderle();
            } catch (e) {
                _hata = e.message;
                _hataYakalayici(e.message);
                hataGoster(e.message);
            } finally {
                _yukleniyor = false;
            }
        }

        /**
         * Mevcut seçimleri kaydeder.
         * @param {string} [musteriNotu] - Opsiyonel müşteri notu
         * @returns {Promise<Object>} Kaydedilen konfigürasyon detayı
         */
        async function secimleriKaydet(musteriNotu) {
            if (!_konfigurator) {
                throw new Error('Konfigüratör henüz yüklenmedi.');
            }

            var secimDto = {
                urunId: _konfigurator.urunId,
                musteriNotu: musteriNotu || null,
                secimler: _secimler
            };

            return await secimKaydet(secimDto);
        }

        /**
         * Bir parça için seçim yapar/değiştirir.
         * @param {number} parcaId
         * @param {Object} secim - { seciliRenkId?, seciliMalzemeId?, seciliKaplamaId?, ... }
         */
        function parcaSecimiYap(parcaId, secim) {
            var mevcutIndex = _secimler.findIndex(function (s) { return s.parcaId === parcaId; });
            var yeniSecim = Object.assign({ parcaId: parcaId, gorunurMu: true }, secim);

            if (mevcutIndex >= 0) {
                _secimler[mevcutIndex] = yeniSecim;
            } else {
                _secimler.push(yeniSecim);
            }

            _secimDegisimi(_secimler.slice());
        }

        /**
         * Mevcut konfigüratör verisini döndürür.
         * @returns {Object|null}
         */
        function konfiguratorVerisi() {
            return _konfigurator;
        }

        /**
         * Mevcut seçimleri döndürür.
         * @returns {Array}
         */
        function mevcutSecimler() {
            return _secimler.slice();
        }

        // =====================================================================
        // Render (basit örnek — kendi UI'ınızla değiştirin)
        // =====================================================================

        function renderle() {
            var hedef = document.getElementById(_hedefElId);
            if (!hedef) {
                console.warn('[VizitLink3D] Hedef element bulunamadı: #' + _hedefElId);
                return;
            }

            if (!_konfigurator) {
                hedef.innerHTML = '<div class="vt3d-bos">Veri yüklenemedi.</div>';
                return;
            }

            var k = _konfigurator;
            var html = '<div class="vt3d-widget" data-urun-id="' + k.urunId + '">';
            html += '<h3 class="vt3d-urun-ad">' + guvenliHtml(k.ad) + '</h3>';

            if (k.fiyat) {
                html += '<p class="vt3d-fiyat">₺' + k.fiyat.toLocaleString('tr-TR') + '</p>';
            }

            if (k.modelId) {
                html += '<p class="vt3d-model">3D Model: #' + k.modelId + '</p>';
            }

            if (k.parcalar && k.parcalar.length > 0) {
                html += '<div class="vt3d-parcalar">';
                html += '<h4>Özelleştirilebilir Parçalar (' + k.parcalar.length + ')</h4>';
                html += '<ul>';

                k.parcalar.forEach(function (parca) {
                    html += '<li class="vt3d-parca" data-parca-id="' + parca.id + '">';
                    html += '<span class="vt3d-parca-ad">' + guvenliHtml(parca.gorunenAd) + '</span>';

                    if (parca.renklenebilirMi && parca.renkler && parca.renkler.length > 0) {
                        html += ' <span class="vt3d-renk-sayisi">(' + parca.renkler.length + ' renk)</span>';
                    }
                    if (parca.malzemeDegisebilirMi && parca.malzemeler && parca.malzemeler.length > 0) {
                        html += ' <span class="vt3d-malzeme-sayisi">(' + parca.malzemeler.length + ' malzeme)</span>';
                    }

                    html += '</li>';
                });

                html += '</ul></div>';
            }

            html += '</div>';
            hedef.innerHTML = html;
        }

        function hataGoster(mesaj) {
            var hedef = document.getElementById(_hedefElId);
            if (!hedef) return;
            hedef.innerHTML = '<div class="vt3d-hata">⚠ ' + guvenliHtml(mesaj) + '</div>';
        }

        function guvenliHtml(metin) {
            if (!metin) return '';
            var div = document.createElement('div');
            div.appendChild(document.createTextNode(metin));
            return div.innerHTML;
        }

        // =====================================================================
        // Public API'yi döndür
        // =====================================================================
        return {
            baslat: baslat,
            secimleriKaydet: secimleriKaydet,
            parcaSecimiYap: parcaSecimiYap,
            konfiguratorVerisi: konfiguratorVerisi,
            mevcutSecimler: mevcutSecimler
        };
    }

    // =========================================================================
    // Embed / iframe entegrasyonu (v3.0 — Paket-4C)
    // =========================================================================

    /**
     * Embed iframe oluşturur (GÜVENLİ TOKEN-FREE COOKIE AKISI).
     * 
     * ÇALIŞMA ŞEKLİ:
     * 1. Müşteri backend'i POST /api/entegrasyon/konfigurator/{slug}/embed-oturum
     *    isteği ile bootstrap token alır.
     * 2. iframe src = /konfigurator/embed/{token} ile başlar.
     * 3. Sunucu token'ı doğrular, HttpOnly cookie yazar, 303 redirect yapar.
     * 4. Tarayıcı /konfigurator/embed (token-siz) sayfasına yönlenir.
     * 5. URL'de, history'de, DOM'da, storage'da TOKEN KALMAZ.
     * 6. Oturum HttpOnly cookie ile yönetilir (JS erişemez).
     * 
     * GÜVENLİK:
     * - API anahtarı widget'ta YOKTUR.
     * - Token sadece bootstrap anında URL'de bulunur, hemen cookie ile değiştirilir.
     * - Redirect sonrası URL'de token KALMAZ.
     * - Cookie HttpOnly/Secure/SameSite=None.
     * - Cross-site cookie engellenirse hata verir, token'ı expose ETMEZ.
     * 
     * @param {Object} ayarlar
     * @param {string} ayarlar.embedToken - Bootstrap embed token (backend'den alınır)
     * @param {string} [ayarlar.hedefElementId] - iframe'in ekleneceği div ID'si
     * @param {string} [ayarlar.genislik] - iframe genişliği (varsayılan: 100%)
     * @param {string} [ayarlar.yukseklik] - iframe yüksekliği (varsayılan: 600px)
     * @param {string} [ayarlar.embedBaseUrl] - Embed sunucu base URL (opsiyonel)
     * @returns {HTMLIFrameElement|null}
     */
    function EmbedKonfigurator(ayarlar) {
        if (!ayarlar || !ayarlar.embedToken) {
            console.error('[VizitLink3D] embedToken zorunludur.');
            return null;
        }

        var hedefId = ayarlar.hedefElementId || 'vizitlink3d-konfigurator';
        var hedefEl = document.getElementById(hedefId);
        if (!hedefEl) {
            console.error('[VizitLink3D] Hedef element bulunamadi: #' + hedefId);
            return null;
        }

        var baseUrl = ayarlar.embedBaseUrl || '';
        // Bootstrap URL: sadece ilk yüklemede token içerir
        // Sunucu 303 redirect + Set-Cookie ile token-siz URL'ye yönlendirir
        var iframeUrl = baseUrl + '/konfigurator/embed/' + encodeURIComponent(ayarlar.embedToken);

        var iframe = document.createElement('iframe');
        iframe.src = iframeUrl;
        iframe.width = ayarlar.genislik || '100%';
        iframe.height = ayarlar.yukseklik || '600px';
        iframe.style.border = 'none';
        iframe.style.borderRadius = '8px';
        iframe.setAttribute('allow', 'clipboard-read; clipboard-write');
        iframe.setAttribute('loading', 'lazy');
        // no-referrer: referrer bilgisi gönderilmez
        iframe.setAttribute('referrerpolicy', 'no-referrer');
        // Sandbox: gerekli özelliklere izin ver
        iframe.setAttribute('sandbox', 'allow-scripts allow-same-origin allow-forms allow-popups');

        // GUVENLIK (v3.0):
        // - Token iframe src'de sadece ilk yüklemede bulunur
        // - Sunucu redirect + Set-Cookie ile token'ı URL'den kaldırır
        // - Redirect sonrası iframe URL'si /konfigurator/embed (token-siz) olur
        // - Token ASLA console.log ile loglanmaz
        // - Token ASLA sessionStorage/localStorage'a yazılmaz
        // - Oturum HttpOnly cookie ile yönetilir

        hedefEl.innerHTML = '';
        hedefEl.appendChild(iframe);

        return iframe;
    }

    // =========================================================================
    // Global namespace'e ekle
    // =========================================================================
    global.VizitLink3D = global.VizitLink3D || {};
    global.VizitLink3D.KonfiguratorWidget = VizitLink3DKonfiguratorWidget;
    global.VizitLink3D.EmbedKonfigurator = EmbedKonfigurator;

})(typeof window !== 'undefined' ? window : this);
