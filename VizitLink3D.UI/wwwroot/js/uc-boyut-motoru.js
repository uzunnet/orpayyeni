/**
 * VIZITLINK3D — ÜÇ BOYUT MOTORU (THREE.JS TÜRKÇE SARMALAYICI)
 * =============================================================
 * Bu dosya, Three.js kütüphanesini doğrudan kullanmak yerine
 * tüm 3D görüntüleme işlemlerini Türkçe fonksiyon adlarıyla
 * sarmalar. Blazor tarafı yalnızca bu arayüzü bilir, Three.js'i değil.
 *
 * KURALLAR.md §2 uyarınca: Harici kütüphane doğrudan iş mantığında kullanılamaz.
 * Tüm çağrılar bu wrapper üzerinden geçer.
 */

window.UcBoyutMotoru = (function () {
    // =============================================
    // ÖZEL DEĞİŞKENLER (Private State)
    // Her kanvas için ayrı sahne tutulur (çoklu viewer desteği)
    // =============================================
    const _sahneler = {};

    /**
     * Belirtilen kanvas kimliğine ait sahne nesnesini döndürür.
     * Eğer sahne yoksa null döner.
     */
    function _sahneGetir(kanvasId) {
        return _sahneler[kanvasId] || null;
    }

    function _ralRengiOlustur(renkHex) {
        const renk = new THREE.Color();
        renk.setStyle(renkHex);
        return renk.convertSRGBToLinear();
    }

    function _ralMateryalUygula(materyal, renk) {
        if (!materyal || !materyal.color) {
            return;
        }

        if (materyal.emissive) {
            materyal.emissive.set(0x000000);
        }

        materyal.emissiveIntensity = 0;
        materyal.color.copy(renk);
        materyal.metalness = 0;
        materyal.roughness = Math.max(Number(materyal.roughness || 0.48), 0.48);
        materyal.envMapIntensity = 0.72;
        materyal.needsUpdate = true;
    }

    function _teknikKenarCizgisiEkle(model) {
        if (!model || typeof THREE.EdgesGeometry === 'undefined') {
            return;
        }

        model.traverse((nesne) => {
            if (!nesne.isMesh || nesne.userData.teknikKenarVar || nesne.userData.metal) {
                return;
            }

            const geometri = nesne.geometry;
            if (!geometri) {
                return;
            }

            const kenarGeo = new THREE.EdgesGeometry(geometri, 18);
            const kenarMat = new THREE.LineBasicMaterial({
                color: 0x2f2b24,
                transparent: true,
                opacity: 0.2,
                depthTest: true
            });
            const kenar = new THREE.LineSegments(kenarGeo, kenarMat);
            kenar.name = 'TeknikKenarCizgisi';
            kenar.userData.teknikKenar = true;
            kenar.renderOrder = 2;
            nesne.add(kenar);
            nesne.userData.teknikKenarVar = true;
        });
    }

    /**
     * Sahne için animasyon döngüsünü başlatır.
     * requestAnimationFrame ile sürekli render yapılır.
     */
    function _animasyonDongusu(kanvasId) {
        const veri = _sahneGetir(kanvasId);
        if (!veri || veri.durduruldu) return;

        veri.animKaresi = requestAnimationFrame(() => _animasyonDongusu(kanvasId));

        if (document.hidden) {
            return;
        }

        // Otomatik döndürme — sadece kullanıcı açıkça isterse çalışır
        if (veri.otomatikDondurmeMi && veri.model && !veri._surukleniyorMu) {
            veri.model.rotation.y += 0.0035;
        }

        // Kontrol güncellemesi (mouse/touch drag)
        if (veri.kontroller) {
            veri.kontroller.update();
        }

        veri.renderer.render(veri.sahne, veri.kamera);
    }

    /**
     * Boyutlandırma olayını işler — pencere yeniden boyutlandırıldığında
     * kamera ve renderer güncellenir, görüntü bozulmaz.
     */
    function _yenidenBoyutlandir(kanvasId) {
        const veri = _sahneGetir(kanvasId);
        if (!veri) return;

        const genislik = veri.konteyner.clientWidth;
        const yukseklik = veri.konteyner.clientHeight;

        veri.kamera.aspect = genislik / yukseklik;
        veri.kamera.updateProjectionMatrix();
        veri.renderer.setSize(genislik, yukseklik);
    }

    function _modeliKamerayaSigdir(kanvasId, durumKaydet) {
        const veri = _sahneGetir(kanvasId);
        if (!veri || !veri.model || !veri.kamera) {
            return;
        }

        const kutu = new THREE.Box3().setFromObject(veri.model);
        if (kutu.isEmpty()) {
            return;
        }

        const merkez = kutu.getCenter(new THREE.Vector3());
        const boyut = kutu.getSize(new THREE.Vector3());
        const oran = Math.max(veri.kamera.aspect || 1, 0.1);
        const dikeyAci = THREE.MathUtils.degToRad(veri.kamera.fov);
        const yatayAci = 2 * Math.atan(Math.tan(dikeyAci / 2) * oran);
        const guvenliPay = 1.28;

        const dikeyMesafe = (boyut.y / 2) / Math.tan(dikeyAci / 2);
        const yatayMesafe = (boyut.x / 2) / Math.tan(yatayAci / 2);
        const derinlikPayi = boyut.z * 0.65;
        const mesafe = Math.max(dikeyMesafe, yatayMesafe, derinlikPayi, 2.6) * guvenliPay;

        if (!veri.kameraOzellestirildi) {
            const yon = new THREE.Vector3(0.12, 0.18, 1).normalize();
            veri.kamera.position.copy(merkez).add(yon.multiplyScalar(mesafe));
            veri.kamera.near = Math.max(mesafe / 100, 0.01);
            veri.kamera.far = Math.max(mesafe * 8, 100);
            veri.kamera.lookAt(merkez);
        } else {
            veri.kamera.near = Math.max(mesafe / 100, 0.01);
            veri.kamera.far = Math.max(mesafe * 8, 100);
        }
        veri.kamera.updateProjectionMatrix();

        if (veri.kontroller) {
            if (!veri.kameraOzellestirildi) {
                veri.kontroller.target.copy(merkez);
            }
            veri.kontroller.minDistance = Math.max(mesafe * 0.42, 0.8);
            veri.kontroller.maxDistance = Math.max(mesafe * 2.2, veri.kontroller.minDistance + 1);
            veri.kontroller.update();

            if (durumKaydet && typeof veri.kontroller.saveState === 'function') {
                veri.kontroller.saveState();
            }
        }
    }

    function _endustriyelSahneKur(sahne) {
        const grup = new THREE.Group();
        grup.name = 'EndustriyelOlcuSahnesi';

        const zeminGeo = new THREE.PlaneGeometry(7.2, 7.2);
        const zeminMat = new THREE.ShadowMaterial({ opacity: 0.2 });
        const zemin = new THREE.Mesh(zeminGeo, zeminMat);
        zemin.rotation.x = -Math.PI / 2;
        zemin.position.y = -1.12;
        zemin.receiveShadow = true;
        grup.add(zemin);

        sahne.add(grup);
        return grup;
    }

    function _orijinalPozisyonlariKaydet(model) {
        if (!model) return;
        model.traverse((nesne) => {
            if (nesne.isMesh) {
                nesne.userData.orijinalPozisyon = nesne.position.clone();
            }
        });
    }

    function _patlatilmisGorunumUygula(model, aktifMi) {
        if (!model) return;

        const kutu = new THREE.Box3().setFromObject(model);
        const merkez = kutu.getCenter(new THREE.Vector3());

        model.traverse((nesne) => {
            if (!nesne.isMesh || !nesne.userData.orijinalPozisyon) return;

            if (!aktifMi) {
                nesne.position.copy(nesne.userData.orijinalPozisyon);
                return;
            }

            const dunyaMerkez = new THREE.Box3().setFromObject(nesne).getCenter(new THREE.Vector3());
            const yon = dunyaMerkez.sub(merkez);
            if (yon.lengthSq() < 0.0001) {
                yon.set(0, 0, 1);
            }

            yon.normalize();
            nesne.position.copy(nesne.userData.orijinalPozisyon).add(yon.multiplyScalar(0.08));
        });
    }

    // =============================================
    // GENEL (PUBLIC) API — Blazor bu fonksiyonları çağırır
    // =============================================
    return {

        /**
         * Belirtilen kanvas elementine 3D sahneyi başlatır.
         * Three.js Sahne, Kamera, Renderer ve OrbitControls oluşturulur.
         * Varsayılan model olarak parametrik bir kapak geometrisi çizilir.
         *
         * @param {string} kanvasId - HTML kanvas elementinin ID'si
         * @param {string|null} modelYolu - GLB/GLTF dosya yolu (opsiyonel)
         * @param {string} baslangicRenk - Başlangıç rengi (hex, örn: "#FFFFFF")
         */
        baslat: function (kanvasId, modelYolu, baslangicRenk, cevreJson) {
            // Zaten başlatılmışsa temizle
            if (_sahneler[kanvasId]) {
                this.temizle(kanvasId);
            }

            const konteyner = document.getElementById(kanvasId);
            if (!konteyner) {
                console.warn('[UcBoyutMotoru] Konteyner bulunamadı:', kanvasId);
                return;
            }

            // Three.js kontrolü
            if (typeof THREE === 'undefined') {
                console.error('[UcBoyutMotoru] Three.js yüklenmemiş!');
                return;
            }

            // --- SAHNE ---
            const sahne = new THREE.Scene();
            const varsayilanBg = 0x1a1a1a; // Koyu varsayılan
            sahne.background = new THREE.Color(varsayilanBg);

            // Hafif sis efekti (derinlik hissi)
            sahne.fog = new THREE.Fog(varsayilanBg, 9, 34);

            // --- KAMERA ---
            const genislik = konteyner.clientWidth || 600;
            const yukseklik = konteyner.clientHeight || 600;
            const kamera = new THREE.PerspectiveCamera(30, genislik / yukseklik, 0.1, 100);
            kamera.position.set(0.15, 0.42, 3.25);
            kamera.lookAt(0, 0.05, 0);

            // --- IŞIKLANDIRMA ---
            // Ortam ışığı (yumuşak genel aydınlatma)
            const ortamIsik = new THREE.AmbientLight(0xffffff, 0.38);
            ortamIsik.userData.orijinalSiddet = ortamIsik.intensity;
            sahne.add(ortamIsik);

            // Ana ışık
            const anaIsik = new THREE.DirectionalLight(0xfff5e4, 1.05);
            anaIsik.position.set(2.5, 4.5, 3.2);
            anaIsik.castShadow = true;
            anaIsik.shadow.mapSize.width = 2048;
            anaIsik.shadow.mapSize.height = 2048;
            anaIsik.userData.orijinalSiddet = anaIsik.intensity;
            sahne.add(anaIsik);

            // Dolgu ışığı (sağ taraftan yumuşak)
            const dolguIsik = new THREE.DirectionalLight(0xdde8ff, 0.26);
            dolguIsik.position.set(-3.5, 1.4, -2.2);
            dolguIsik.userData.orijinalSiddet = dolguIsik.intensity;
            sahne.add(dolguIsik);

            // Kenar ışığı modeli fondan ayırır ve eski showroom hissini geri verir
            const kenarIsik = new THREE.DirectionalLight(0xffdfaa, 0.62);
            kenarIsik.position.set(-1.8, 2.4, 3.8);
            kenarIsik.userData.orijinalSiddet = kenarIsik.intensity;
            sahne.add(kenarIsik);

            // Zemin yansıması ışığı
            const yansimaIsik = new THREE.HemisphereLight(0xffffff, 0xd7c4a4, 0.22);
            yansimaIsik.userData.orijinalSiddet = yansimaIsik.intensity;
            sahne.add(yansimaIsik);

            // --- RENDERER ---
            const renderer = new THREE.WebGLRenderer({
                antialias: true,
                alpha: false,
                powerPreference: 'high-performance'
            });
            renderer.setSize(genislik, yukseklik);
            renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
            renderer.shadowMap.enabled = true;
            renderer.shadowMap.type = THREE.PCFSoftShadowMap;
            if (THREE.SRGBColorSpace) {
                renderer.outputColorSpace = THREE.SRGBColorSpace;
            } else {
                renderer.outputEncoding = THREE.sRGBEncoding;
            }
            renderer.toneMapping = THREE.ACESFilmicToneMapping || THREE.LinearToneMapping;
            renderer.toneMappingExposure = 1.03;

            // Kanvas'ı konteyner'a ekle
            konteyner.innerHTML = '';
            konteyner.appendChild(renderer.domElement);

            // --- KONTROLLer (OrbitControls) ---
            let kontroller = null;
            let _surukleniyorMu = false;
            if (typeof THREE.OrbitControls !== 'undefined') {
                kontroller = new THREE.OrbitControls(kamera, renderer.domElement);
                kontroller.enableDamping = true;
                kontroller.dampingFactor = 0.08;
                kontroller.enableZoom = true;
                kontroller.minDistance = 1.45;
                kontroller.maxDistance = 6.2;
                kontroller.maxPolarAngle = Math.PI * 0.72;
                kontroller.minPolarAngle = Math.PI * 0.1;
                kontroller.autoRotate = false;
                kontroller.autoRotateSpeed = 0.7;
                kontroller.enablePan = false;

                // TİTREME FİX: Mouse sürüklenirken oto-döndürme dursun
                renderer.domElement.addEventListener('mousedown', () => {
                    _surukleniyorMu = true;
                    const sahneVeri = _sahneGetir(kanvasId);
                    if (sahneVeri) sahneVeri._surukleniyorMu = true;
                });
                renderer.domElement.addEventListener('touchstart', () => {
                    _surukleniyorMu = true;
                    const sahneVeri = _sahneGetir(kanvasId);
                    if (sahneVeri) sahneVeri._surukleniyorMu = true;
                }, { passive: true });
                renderer.domElement.addEventListener('mouseup', () => {
                    _surukleniyorMu = false;
                    const sahneVeri = _sahneGetir(kanvasId);
                    if (sahneVeri) sahneVeri._surukleniyorMu = false;
                });
                renderer.domElement.addEventListener('touchend', () => {
                    _surukleniyorMu = false;
                    const sahneVeri = _sahneGetir(kanvasId);
                    if (sahneVeri) sahneVeri._surukleniyorMu = false;
                });
            }

            // --- PARÇA SEÇME (Tıklama ile) ---
            const isaretci = new THREE.Vector2();
            const isinIzgarasi = new THREE.Raycaster();
            
            const parcaSecTiklayici = function (olay) {
                const mevcutModel = _sahneGetir(kanvasId)?.model;
                if (!mevcutModel) return;
                
                const rect = renderer.domElement.getBoundingClientRect();
                isaretci.x = ((olay.clientX - rect.left) / rect.width) * 2 - 1;
                isaretci.y = -((olay.clientY - rect.top) / rect.height) * 2 + 1;

                isinIzgarasi.setFromCamera(isaretci, kamera);
                const kesisme = isinIzgarasi.intersectObjects(mevcutModel.children, true);

                if (kesisme.length > 0) {
                    const tiklanan = kesisme[0].object;
                    const parcaIsmi = tiklanan.name || '(isimsiz)';
                    const veri = _sahneGetir(kanvasId);

                    if (veri) {
                        veri.seciliParca = tiklanan;
                    }

                    if (veri && veri.parcaSecildiCallback) {
                        veri.parcaSecildiCallback.invokeMethodAsync('ParcaSecildi', parcaIsmi);
                    }
                }
            };
            renderer.domElement.addEventListener('click', parcaSecTiklayici);

            // Başlangıç rengi
            const secilenRenk = baslangicRenk || '#E8E4DF';

            // --- ENDustriyel olcu zemini ---
            const izgaraGrubu = _endustriyelSahneKur(sahne);

            // --- DURUM KAYDET ---
            _sahneler[kanvasId] = {
                sahne, kamera, renderer, kontroller, model: null,
                konteyner,
                izgaraGrubu,
                otomatikDondurmeMi: false,
                patlatilmisMi: false,
                durduruldu: false,
                animKaresi: null,
                secilenRenk,
                seciliParca: null,
                seciliParcaOrijinalEmissive: null,
                parcaSecildiCallback: null,
                parcaSecTiklayici
            };

            // Pencere boyut olayı
            const boyutFonk = () => _yenidenBoyutlandir(kanvasId);
            window.addEventListener('resize', boyutFonk);
            _sahneler[kanvasId].boyutFonk = boyutFonk;

            if (typeof ResizeObserver !== 'undefined') {
                const boyutIzleyici = new ResizeObserver(() => _yenidenBoyutlandir(kanvasId));
                boyutIzleyici.observe(konteyner);
                _sahneler[kanvasId].boyutIzleyici = boyutIzleyici;
            }

            // Animasyon döngüsünü başlat
            _animasyonDongusu(kanvasId);

            // Gerçek model dosyası varsa yükle
            if (modelYolu && modelYolu.length > 0) {
                this.modeli_yukle(kanvasId, modelYolu, false);
            }

            console.log('[UcBoyutMotoru] Sahne başlatıldı:', kanvasId);

            // Başlatma sırasında CevreAyar uygula (arka plan rengi dahil)
            if (cevreJson) {
                try {
                    const cevre = typeof cevreJson === 'string' ? JSON.parse(cevreJson) : cevreJson;
                    const arkaPlan = cevre.arkaPlanRengi ?? cevre.ArkaPlanRengi;
                    if (arkaPlan) {
                        sahne.background = new THREE.Color(arkaPlan);
                        sahne.fog = new THREE.Fog(new THREE.Color(arkaPlan), 9, 34);
                    }
                    const zemin = cevre.zeminRengi ?? cevre.ZeminRengi;
                    if (zemin) {
                        const zeminNesnesi = sahne.children.find(c => c.userData?.tip === 'zemin');
                        if (zeminNesnesi && zeminNesnesi.material) {
                            zeminNesnesi.material.color = new THREE.Color(zemin);
                        }
                    }
                    console.log('[UcBoyutMotoru] CevreAyar uygulandı:', arkaPlan);
                } catch (e) {
                    console.warn('[UcBoyutMotoru] CevreAyar parse hatası:', e);
                }
            }
        },

        /**
         * Parametrik kapak geometrisi oluşturur.
         * Gerçek 3D model dosyası olmadığında gösterilecek
         * gerçekçi kapak formu.
         */
        _parametrikKapakOlustur: function (sahne, renk) {
            const grup = new THREE.Group();

            // Ana kapak gövdesi
            const govdeGeo = new THREE.BoxGeometry(1.4, 2.0, 0.06);
            const govdeMat = new THREE.MeshStandardMaterial({
                color: new THREE.Color(renk).convertSRGBToLinear(), // sRGB -> Linear dönüşümü
                roughness: 0.3, // Parlaklığı biraz azaltarak rengin patlamasını önle
                metalness: 0.0,
            });
            const govde = new THREE.Mesh(govdeGeo, govdeMat);
            govde.castShadow = true;
            govde.userData.anaRenk = true; // Renk değişimi bu mesh'e uygulanır
            grup.add(govde);

            // Çerçeve profili — üst
            const cerceveMat = new THREE.MeshStandardMaterial({
                color: new THREE.Color(renk).convertSRGBToLinear(),
                roughness: 0.3,
                metalness: 0.0
            });
            const ustCerceve = new THREE.Mesh(
                new THREE.BoxGeometry(1.4, 0.08, 0.085),
                cerceveMat.clone()
            );
            ustCerceve.position.set(0, 0.96, 0.01);
            ustCerceve.castShadow = true;
            ustCerceve.userData.cerceve = true;
            grup.add(ustCerceve);

            // Alt çerçeve
            const altCerceve = new THREE.Mesh(
                new THREE.BoxGeometry(1.4, 0.08, 0.085),
                cerceveMat.clone()
            );
            altCerceve.position.set(0, -0.96, 0.01);
            altCerceve.castShadow = true;
            altCerceve.userData.cerceve = true;
            grup.add(altCerceve);

            // Sol çerçeve
            const solCerceve = new THREE.Mesh(
                new THREE.BoxGeometry(0.08, 2.0, 0.085),
                cerceveMat.clone()
            );
            solCerceve.position.set(-0.66, 0, 0.01);
            solCerceve.castShadow = true;
            solCerceve.userData.cerceve = true;
            grup.add(solCerceve);

            // Sağ çerçeve
            const sagCerceve = new THREE.Mesh(
                new THREE.BoxGeometry(0.08, 2.0, 0.085),
                cerceveMat.clone()
            );
            sagCerceve.position.set(0.66, 0, 0.01);
            sagCerceve.castShadow = true;
            sagCerceve.userData.cerceve = true;
            grup.add(sagCerceve);

            // Endustriyel ic panel ve fitil detaylari
            const icPanelMat = new THREE.MeshStandardMaterial({
                color: new THREE.Color(renk).convertSRGBToLinear(),
                roughness: 0.42,
                metalness: 0.02
            });
            const icPanel = new THREE.Mesh(
                new THREE.BoxGeometry(1.08, 1.64, 0.028),
                icPanelMat
            );
            icPanel.position.set(0, 0, 0.052);
            icPanel.castShadow = true;
            icPanel.userData.anaRenk = true;
            grup.add(icPanel);

            const fitilMat = new THREE.MeshStandardMaterial({
                color: 0x1f1f1f,
                roughness: 0.82,
                metalness: 0.0
            });
            const fitilUst = new THREE.Mesh(new THREE.BoxGeometry(1.14, 0.018, 0.012), fitilMat);
            fitilUst.position.set(0, 0.83, 0.073);
            grup.add(fitilUst);

            const fitilAlt = fitilUst.clone();
            fitilAlt.position.y = -0.83;
            grup.add(fitilAlt);

            const fitilSol = new THREE.Mesh(new THREE.BoxGeometry(0.018, 1.66, 0.012), fitilMat.clone());
            fitilSol.position.set(-0.57, 0, 0.073);
            grup.add(fitilSol);

            const fitilSag = fitilSol.clone();
            fitilSag.position.x = 0.57;
            grup.add(fitilSag);

            // Menteşe ve kilit karsiliklari modeli teknik olarak okunur kilar
            const metalMat = new THREE.MeshStandardMaterial({
                color: 0x8f8b82,
                roughness: 0.18,
                metalness: 0.86
            });

            [-0.58, 0, 0.58].forEach((y) => {
                const mentese = new THREE.Mesh(new THREE.BoxGeometry(0.045, 0.22, 0.03), metalMat.clone());
                mentese.position.set(-0.72, y, 0.075);
                mentese.castShadow = true;
                mentese.userData.metal = true;
                grup.add(mentese);
            });

            const kilitKarsilik = new THREE.Mesh(new THREE.BoxGeometry(0.035, 0.3, 0.032), metalMat.clone());
            kilitKarsilik.position.set(0.72, 0, 0.074);
            kilitKarsilik.castShadow = true;
            kilitKarsilik.userData.metal = true;
            grup.add(kilitKarsilik);

            // Kapı kolu
            const kolGeo = new THREE.CylinderGeometry(0.015, 0.015, 0.3, 16);
            const kolMat = metalMat.clone();
            const kol = new THREE.Mesh(kolGeo, kolMat);
            kol.rotation.z = Math.PI / 2;
            kol.position.set(0.5, 0, 0.1);
            kol.castShadow = true;
            kol.userData.metal = true;
            grup.add(kol);

            // Kol tutucu - üst
            const tutucuGeo = new THREE.CylinderGeometry(0.025, 0.025, 0.05, 16);
            const tutucuMat = kolMat.clone();
            const tutucuUst = new THREE.Mesh(tutucuGeo, tutucuMat);
            tutucuUst.position.set(0.35, 0, 0.1);
            tutucuUst.castShadow = true;
            tutucuUst.userData.metal = true;
            grup.add(tutucuUst);

            const tutucuSag = new THREE.Mesh(tutucuGeo.clone(), tutucuMat.clone());
            tutucuSag.position.set(0.65, 0, 0.1);
            tutucuSag.castShadow = true;
            tutucuSag.userData.metal = true;
            grup.add(tutucuSag);

            _teknikKenarCizgisiEkle(grup);
            sahne.add(grup);
            return grup;
        },

        /**
         * Dışarıdan GLB/GLTF model dosyası yükler.
         * Yükleme tamamlandığında mevcut parametrik model kaldırılır.
         */
        modeli_yukle: function (kanvasId, modelYolu, yedekDenemesiMi) {
            const veri = _sahneGetir(kanvasId);
            if (!veri) return;

            if (typeof THREE.GLTFLoader === 'undefined') {
                console.warn('[UcBoyutMotoru] GLTFLoader bulunamadı, parametrik model kullanılıyor.');
                return;
            }

            const yukleme = new THREE.GLTFLoader();

            // DRACO sıkıştırılmış model desteği
            if (typeof THREE.DRACOLoader !== 'undefined') {
                const dracoYukleyici = new THREE.DRACOLoader();
                dracoYukleyici.setDecoderPath('https://cdn.jsdelivr.net/npm/three@0.128.0/examples/js/libs/draco/');
                yukleme.setDRACOLoader(dracoYukleyici);
                console.log('[UcBoyutMotoru] DRACO dekompresyon aktif.');
            }

            yukleme.load(
                modelYolu,
                (gltf) => {
                    // Eski modeli kaldır
                    if (veri.model) {
                        veri.sahne.remove(veri.model);
                    }

                    const yeniModel = gltf.scene;
                    yeniModel.castShadow = true;

                    // Modeli ortala ve boyutlandır
                    const kutu = new THREE.Box3().setFromObject(yeniModel);
                    const merkez = kutu.getCenter(new THREE.Vector3());
                    const boyut = kutu.getSize(new THREE.Vector3());
                    const maxBoyut = Math.max(boyut.x, boyut.y, boyut.z);
                    if (!Number.isFinite(maxBoyut) || maxBoyut <= 0) {
                        console.warn('[UcBoyutMotoru] Model boyutu okunamadi:', modelYolu);
                        return;
                    }

                    const olcek = 2.45 / maxBoyut;

                    yeniModel.position.sub(merkez.multiplyScalar(olcek));
                    yeniModel.scale.setScalar(olcek);

                    // GLB malzeme fix: atanmamış (yeşil) mesh'leri gri/bej rengiyle başlat
                    yeniModel.traverse((nesne) => {
                        if (nesne.isMesh && nesne.material) {
                            const mats = Array.isArray(nesne.material) ? nesne.material : [nesne.material];
                            mats.forEach(mat => {
                                // Eğer materyal rengi varsayılan yeşil (#008000) ise düzült
                                if (mat.color) {
                                    const r = mat.color.r, g = mat.color.g, b = mat.color.b;
                                    const isDefaultGreen = (r < 0.1 && g > 0.4 && b < 0.1);
                                    if (isDefaultGreen) {
                                        mat.color.set(new THREE.Color('#D0CBC4').convertSRGBToLinear());
                                    }
                                }
                                // roughness/metalness yoksa varsayılan ata
                                if (mat.roughness === undefined || mat.roughness === 0) mat.roughness = 0.22;
                                if (mat.metalness === undefined) mat.metalness = 0.0;
                                if (mat.envMapIntensity === undefined) mat.envMapIntensity = 0.75;
                                mat.needsUpdate = true;
                            });
                        }
                    });

                    veri.sahne.add(yeniModel);
                    veri.model = yeniModel;
                    veri.patlatilmisMi = false;
                    _orijinalPozisyonlariKaydet(yeniModel);
                    _teknikKenarCizgisiEkle(yeniModel);
                    _modeliKamerayaSigdir(kanvasId, true);

                    // Başlangıç rengini uygula (eğer belirtilmişse)
                    if (veri.secilenRenk && veri.secilenRenk !== '#E8E4DF') {
                        this.renk_uygula(kanvasId, veri.secilenRenk);
                    }

                    console.log('[UcBoyutMotoru] Model yüklendi:', modelYolu);
                },
                (progress) => {
                    const yuzde = (progress.loaded / progress.total * 100).toFixed(0);
                    console.log('[UcBoyutMotoru] Yüklenıyor:', yuzde + '%');
                },
                (hata) => {
                    console.error('[UcBoyutMotoru] Model yüklenemedi:', hata);
                    const veri = _sahneGetir(kanvasId);
                    if (veri && veri.konteyner) {
                        veri.konteyner.innerHTML = '<div class="gb-hata-kutu" style="height:100%;display:flex;align-items:center;justify-content:center;background:#f5f5f5;color:#666;"><p>3D model yüklenemedi.</p></div>';
                    }
                }
            );
        },

        /**
         * 3D modelin rengini anında değiştirir.
         * Tüm mesh'lerin materyali güncellenir.
         *
         * @param {string} kanvasId - Hangi sahnenin rengi değişecek
         * @param {string} renkHex - Hex renk kodu (örn: "#FF5733")
         */
        renk_uygula: function (kanvasId, renkHex) {
            const veri = _sahneGetir(kanvasId);
            if (!veri || !veri.model) return;

            veri.secilenRenk = renkHex;
            const yeniRenk = _ralRengiOlustur(renkHex);

            // Grup içindeki tüm mesh'leri gez
            veri.model.traverse((nesne) => {
                if (nesne.isMesh && nesne.material) {
                    // Çerçeve ve gövde rengi değişir, metal kol değişmez
                    if (nesne.userData.anaRenk || nesne.userData.cerceve) {
                        if (Array.isArray(nesne.material)) {
                            nesne.material.forEach(mat => {
                                _ralMateryalUygula(mat, yeniRenk);
                            });
                        } else {
                            _ralMateryalUygula(nesne.material, yeniRenk);
                        }
                    } else if (!nesne.userData.metal) {
                        // GLB modelinde userData yoksa tüm mesh'lere uygula
                        if (Array.isArray(nesne.material)) {
                            nesne.material.forEach(mat => {
                                _ralMateryalUygula(mat, yeniRenk);
                            });
                        } else if (nesne.material.color) {
                            _ralMateryalUygula(nesne.material, yeniRenk);
                        }
                    }
                }
            });

            console.log('[UcBoyutMotoru] Renk uygulandı:', renkHex);
        },

        /**
         * Otomatik döndürme animasyonunu açar veya kapatır.
         * Mouse ile döndürme sırasında otomatik döndürme durur,
         * 2 saniye sonra tekrar başlar.
         *
         * @param {string} kanvasId - Hedef sahne
         * @param {boolean} aktifMi - true = açık, false = kapalı
         */
        otomatik_dondur: function (kanvasId, aktifMi) {
            const veri = _sahneGetir(kanvasId);
            if (!veri) return;

            veri.otomatikDondurmeMi = aktifMi;

            if (veri.kontroller) {
                veri.kontroller.autoRotate = aktifMi;
                veri.kontroller.autoRotateSpeed = 0.7;
            }
        },

        /**
         * Endustriyel olcu izgarasini acar veya kapatir.
         */
        izgara_goster: function (kanvasId, gorunurMu) {
            const veri = _sahneGetir(kanvasId);
            if (!veri || !veri.izgaraGrubu) return;

            veri.izgaraGrubu.visible = gorunurMu;
        },

        /**
         * Modeli teknik inceleme icin parcali gorunume alir.
         */
        patlatilmis_gorunum: function (kanvasId, aktifMi) {
            const veri = _sahneGetir(kanvasId);
            if (!veri || !veri.model) return;

            veri.patlatilmisMi = aktifMi;
            _patlatilmisGorunumUygula(veri.model, aktifMi);
        },

        /**
         * Görüntüleyiciyi tam ekran moduna alır.
         * Tarayıcının Fullscreen API'si kullanılır.
         *
         * @param {string} kanvasId - Tam ekrana alınacak konteyner
         */
        tam_ekran: function (kanvasId) {
            const konteyner = document.getElementById(kanvasId);
            if (!konteyner) return;

            if (document.fullscreenElement) {
                document.exitFullscreen();
            } else {
                konteyner.requestFullscreen().catch(err => {
                    console.warn('[UcBoyutMotoru] Tam ekran başlatılamadı:', err);
                });
            }
        },

        /**
         * Kamerayı başlangıç pozisyonuna sıfırlar.
         * Kullanıcı çok uzaklaştı veya kaybettiyse kullanışlıdır.
         *
         * @param {string} kanvasId - Hedef sahne
         */
        kamera_sifirla: function (kanvasId) {
            const veri = _sahneGetir(kanvasId);
            if (!veri) return;

            _modeliKamerayaSigdir(kanvasId, true);
        },

        /**
         * Belirtilen sahneyi tamamen temizler ve belleği serbest bırakır.
         * Bileşen kaldırıldığında (OnAfterRenderAsync/Dispose) çağrılmalıdır.
         * Bellek sızıntısını önler.
         *
         * @param {string} kanvasId - Temizlenecek sahne
         */
        temizle: function (kanvasId) {
            const veri = _sahneGetir(kanvasId);
            if (!veri) return;

            veri.durduruldu = true;

            if (veri.animKaresi) {
                cancelAnimationFrame(veri.animKaresi);
            }

            if (veri.boyutFonk) {
                window.removeEventListener('resize', veri.boyutFonk);
            }

            if (veri.boyutIzleyici) {
                veri.boyutIzleyici.disconnect();
            }

            if (veri.parcaSecTiklayici && veri.renderer?.domElement) {
                veri.renderer.domElement.removeEventListener('click', veri.parcaSecTiklayici);
            }

            if (veri.kontroller) {
                veri.kontroller.dispose();
            }

            // Tüm geometri ve materyalleri temizle
            veri.sahne.traverse((nesne) => {
                if (nesne.isMesh) {
                    nesne.geometry.dispose();
                    if (Array.isArray(nesne.material)) {
                        nesne.material.forEach(m => m.dispose());
                    } else if (nesne.material) {
                        nesne.material.dispose();
                    }
                }
            });

            veri.renderer.dispose();

            if (veri.konteyner) {
                veri.konteyner.innerHTML = '';
            }

            delete _sahneler[kanvasId];
            console.log('[UcBoyutMotoru] Sahne temizlendi:', kanvasId);
        },

        /**
         * 3D Model uzerinde tiklanabilir noktalar (Hotspot) sistemini etkinlestirir.
         * Raycaster ile tiklanan mesh tespit edilir ve .NET callback'i cagrilir.
         *
         * @param {string} kanvasId - Hedef sahne
         * @param {string} dotNetRef - .NET nesne referansi (DotNetObjectReference)
         * @param {string} metodAdi - Cagrilacak C# metodu
         */
        hotspot_etkinlestir: function (kanvasId, dotNetRef, metodAdi) {
            const veri = _sahneGetir(kanvasId);
            if (!veri || !veri.konteyner) return;

            // Raycaster olustur
            const isaretci = new THREE.Vector2();
            const isinIzgarasi = new THREE.Raycaster();

            veri.konteyner.addEventListener('click', function (olay) {
                const rect = veri.konteyner.getBoundingClientRect();
                isaretci.x = ((olay.clientX - rect.left) / rect.width) * 2 - 1;
                isaretci.y = -((olay.clientY - rect.top) / rect.height) * 2 + 1;

                isinIzgarasi.setFromCamera(isaretci, veri.kamera);

                if (veri.model) {
                    const kesismeNoktalari = isinIzgarasi.intersectObjects(veri.model.children, true);
                    if (kesismeNoktalari.length > 0) {
                        const tiklanan = kesismeNoktalari[0].object;
                        const noktaAdi = tiklanan.userData.hotspot || tiklanan.name || 'Bilinmeyen';
                        const nokta = kesismeNoktalari[0].point;

                        // .NET callback'ini cagir
                        if (dotNetRef && metodAdi) {
                            dotNetRef.invokeMethodAsync(metodAdi, noktaAdi, {
                                x: nokta.x.toFixed(3),
                                y: nokta.y.toFixed(3),
                                z: nokta.z.toFixed(3)
                            });
                        }
                    }
                }
            });

            console.log('[UcBoyutMotoru] Hotspot sistemi etkinlestirildi:', kanvasId);
        },

        /**
         * Yüklenen 3D modelin tüm parçalarını (node isimlerini) analiz eder.
         * Her mesh/node için: isim, tip, vertex/triangle sayısı döner.
         * Sonuç JSON string olarak döndürülür.
         */
        model_analiz_et: function (kanvasId) {
            const veri = _sahneGetir(kanvasId);
            if (!veri || !veri.model) return '[]';

            const parcalar = [];
            veri.model.traverse(function (nesne) {
                if (nesne.isMesh) {
                    parcalar.push({
                        isim: nesne.name || '(isimsiz)',
                        tip: 'Mesh',
                        ucgenSayisi: nesne.geometry.index
                            ? Math.floor(nesne.geometry.index.count / 3)
                            : Math.floor((nesne.geometry.attributes.position?.count || 0) / 3),
                        gorunur: nesne.visible
                    });
                } else if (nesne.isObject3D && nesne !== veri.model && nesne.children.length > 0) {
                    parcalar.push({
                        isim: nesne.name || '(isimsiz)',
                        tip: 'Grup',
                        cocukSayisi: nesne.children.length,
                        gorunur: nesne.visible
                    });
                }
            });

            console.log('[UcBoyutMotoru] Model analizi:', parcalar);
            return JSON.stringify(parcalar);
        },

        /**
         * Belirtilen isimdeki mesh/grubun görünürlüğünü değiştirir.
         * @param {string} kanvasId - Hedef sahne
         * @param {string} parcaIsmi - Gizlenecek/gösterilecek parça adı
         * @param {boolean} gorunurMu - true = göster, false = gizle
         */
        parca_gorunurluk: function (kanvasId, parcaIsmi, gorunurMu) {
            const veri = _sahneGetir(kanvasId);
            if (!veri || !veri.model) return;

            veri.model.traverse(function (nesne) {
                if (nesne.name === parcaIsmi) {
                    nesne.visible = gorunurMu;
                }
            });
            console.log('[UcBoyutMotoru] Parça görünürlük:', parcaIsmi, gorunurMu ? 'göster' : 'gizle');
        },

        /**
         * Belirtilen isimdeki mesh'in rengini değiştirir.
         */
        parca_renk: function (kanvasId, parcaIsmi, renkHex) {
            const veri = _sahneGetir(kanvasId);
            if (!veri || !veri.model) return;

            const yeniRenk = _ralRengiOlustur(renkHex);
            veri.model.traverse(function (nesne) {
                if (nesne.name === parcaIsmi && nesne.isMesh && nesne.material) {
                    if (Array.isArray(nesne.material)) {
                        nesne.material.forEach(m => {
                            _ralMateryalUygula(m, yeniRenk);
                        });
                    } else if (nesne.material.color) {
                        _ralMateryalUygula(nesne.material, yeniRenk);
                    }
                }
            });
        },

        /**
         * Belirtilen isimdeki mesh'i derece cinsinden döndürür (Y ekseninde).
         * Kapak açma/kapama için kullanılır.
         * @param {number} derece — 0 = kapalı, 30-90 = açık
         */
        kapak_derece: function (kanvasId, parcaIsmi, derece) {
            const veri = _sahneGetir(kanvasId);
            if (!veri || !veri.model) return;

            const radyan = (derece * Math.PI) / 180;
            veri.model.traverse(function (nesne) {
                if (nesne.name === parcaIsmi) {
                    nesne.rotation.y = radyan;
                }
            });
        },

        /**
         * Belirtilen isimdeki mesh'in malzemesini değiştirir (PBR).
         * @param {string} malzeme — "krom_parlak", "krom_mat", "ayna", "cam", "mdf_mat", "mdf_parlak", "ahsap_kaplama", vs.
         */
        parca_malzeme: function (kanvasId, parcaIsmi, malzeme) {
            const veri = _sahneGetir(kanvasId);
            if (!veri || !veri.model) return;

            // PBR malzeme ayarlari
            const malzemeAyarlari = {
                krom_parlak:  { roughness: 0.05, metalness: 1.0, color: '#FFFFFF', envMapIntensity: 1.5 },
                krom_mat:     { roughness: 0.4,  metalness: 0.9, color: '#D0D0D0', envMapIntensity: 0.8 },
                ayna:         { roughness: 0.0,  metalness: 1.0, color: '#FFFFFF', envMapIntensity: 2.0 },
                metal:        { roughness: 0.2,  metalness: 0.9, color: '#B0B0B8' },
                cam:          { roughness: 0.0,  metalness: 0.1, color: '#FFFFFF', opacity: 0.3, transparent: true, transmission: 0.9, ior: 1.5 },
                ahsap:        { roughness: 0.6,  metalness: 0.0, color: '#8B5A2B' },
                ahsap_kaplama:{ roughness: 0.5,  metalness: 0.0 }, // Sadece roughness ayarlanır, doku korunur
                mdf_mat:      { roughness: 0.6,  metalness: 0.0 },
                mdf_parlak:   { roughness: 0.1,  metalness: 0.0, clearcoat: 1.0, clearcoatRoughness: 0.1 },
                plastik:      { roughness: 0.4,  metalness: 0.0, color: '#F0F0F0' },
                porselen:     { roughness: 0.1,  metalness: 0.0, color: '#FFFFF5', envMapIntensity: 0.5 }
            };

            const ayar = malzemeAyarlari[malzeme] || malzemeAyarlari.plastik;

            veri.model.traverse(function (nesne) {
                if (nesne.name === parcaIsmi && nesne.isMesh && nesne.material) {
                    const uygula = function(mat) {
                        if (ayar.roughness !== undefined) mat.roughness = ayar.roughness;
                        if (ayar.metalness !== undefined) mat.metalness = ayar.metalness;

                        if (ayar.color && mat.color) {
                            mat.color.set(new THREE.Color(ayar.color).convertSRGBToLinear());
                        }

                        if (ayar.envMapIntensity !== undefined) mat.envMapIntensity = ayar.envMapIntensity;
                        if (ayar.transmission !== undefined) mat.transmission = ayar.transmission;
                        if (ayar.ior !== undefined) mat.ior = ayar.ior;
                        if (ayar.clearcoat !== undefined) mat.clearcoat = ayar.clearcoat;
                        if (ayar.clearcoatRoughness !== undefined) mat.clearcoatRoughness = ayar.clearcoatRoughness;

                        if (ayar.opacity !== undefined) {
                            mat.opacity = ayar.opacity;
                            mat.transparent = ayar.transparent;
                            mat.depthWrite = false;
                        } else {
                            mat.transparent = false;
                            mat.opacity = 1.0;
                            mat.depthWrite = true;
                        }

                        mat.needsUpdate = true;
                    };

                    if (Array.isArray(nesne.material)) {
                        nesne.material.forEach(uygula);
                    } else {
                        uygula(nesne.material);
                    }
                }
            });
        },

        /**
         * 3D Sahne icin HDR ortam haritasini degistirir.
         * @param {string} hdriKey - "banyo", "mutfak", "fabrika", "icmekan"
         */
        cevre_ayarla: function(kanvasId, hdriKey) {
            const veri = _sahneGetir(kanvasId);
            if (!veri || !veri.renderer || !veri.sahne) return;

            if (typeof THREE.RGBELoader === 'undefined' || typeof THREE.PMREMGenerator === 'undefined') return;

            // Yaygin PolyHaven 1K HDRI linkleri
            const hdriMap = {
                'banyo': 'https://dl.polyhaven.org/file/ph-assets/HDRIs/hdr/1k/brown_photostudio_02_1k.hdr',
                'mutfak': 'https://dl.polyhaven.org/file/ph-assets/HDRIs/hdr/1k/kitchen_1k.hdr',
                'fabrika': 'https://dl.polyhaven.org/file/ph-assets/HDRIs/hdr/1k/machine_shop_02_1k.hdr',
                'icmekan': 'https://dl.polyhaven.org/file/ph-assets/HDRIs/hdr/1k/studio_country_hall_1k.hdr'
            };

            const url = hdriMap[hdriKey] || hdriMap['icmekan'];

            const pmrem = new THREE.PMREMGenerator(veri.renderer);
            pmrem.compileEquirectangularShader();
            
            new THREE.RGBELoader()
                .setDataType(THREE.HalfFloatType)
                .load(url,
                    function (doku) {
                        const envHarita = pmrem.fromEquirectangular(doku).texture;
                        veri.sahne.environment = envHarita;
                        doku.dispose();
                        console.log('[UcBoyutMotoru] Cevre ayarlandi:', hdriKey);
                    }
                );
        },

        /**
         * Sahne ışık şiddetini ayarlar.
         * @param {number} seviye — 0.0 (karanlık) ile 2.0 (çok parlak) arası
         */
        isik_ayar: function (kanvasId, seviye) {
            const veri = _sahneGetir(kanvasId);
            if (!veri || !veri.sahne) return;

            veri.sahne.traverse(function (nesne) {
                if (nesne.isLight) {
                    nesne.intensity = nesne.userData.orijinalSiddet
                        ? nesne.userData.orijinalSiddet * seviye
                        : nesne.intensity;
                }
            });
        },

        /**
         * Admin sahne panelinden kaydedilen ayarlari acik viewer'a canli uygular.
         */
        sahne_ayar_uygula: function (kanvasId, ayarTipi, ayarJson) {
            const veri = _sahneGetir(kanvasId);
            if (!veri || !ayarJson) return;

            let ayar = null;
            if (typeof ayarJson === 'object') {
                ayar = ayarJson;
            } else {
                try {
                    ayar = JSON.parse(ayarJson);
                } catch (e) {
                    console.warn('[UcBoyutMotoru] Sahne ayari JSON okunamadi:', e);
                    return;
                }
            }

            if (ayarTipi === 'kamera') {
                const x = Number(ayar.baslangicAciX ?? ayar.BaslangicAciX ?? 0);
                const y = Number(ayar.baslangicAciY ?? ayar.BaslangicAciY ?? 0.5);
                const z = Number(ayar.baslangicAciZ ?? ayar.BaslangicAciZ ?? 4);
                veri.kamera.position.set(x, y, z);
                veri.kamera.lookAt(0, Number(ayar.hedefYukseklik ?? ayar.HedefYukseklik ?? 0), 0);

                if (veri.kontroller) {
                    veri.kontroller.target.set(0, Number(ayar.hedefYukseklik ?? ayar.HedefYukseklik ?? 0), 0);
                    veri.kontroller.minDistance = Number(ayar.zoomMin ?? ayar.ZoomMin ?? veri.kontroller.minDistance);
                    veri.kontroller.maxDistance = Number(ayar.zoomMax ?? ayar.ZoomMax ?? veri.kontroller.maxDistance);
                    const otomatikDonme = Boolean(ayar.otomatikDonme ?? ayar.OtomatikDonme ?? false);
                    veri.otomatikDondurmeMi = otomatikDonme;
                    veri.kontroller.autoRotate = otomatikDonme;
                    veri.kontroller.autoRotateSpeed = Number(ayar.donmeHizi ?? ayar.DonmeHizi ?? veri.kontroller.autoRotateSpeed);
                    veri.kontroller.update();
                }
                veri.kameraOzellestirildi = true;
            }

            if (ayarTipi === 'isik') {
                const siddet = Number(ayar.siddet ?? ayar.Siddet ?? 1);
                this.isik_kaydet(kanvasId);
                
                // Update light intensities
                veri.sahne.traverse(function (nesne) {
                    if (nesne.isLight) {
                        nesne.intensity = nesne.userData.orijinalSiddet
                            ? nesne.userData.orijinalSiddet * siddet
                            : nesne.intensity;
                            
                        // If it's a directional light, we can adjust its position based on angles
                        if (nesne.type === 'DirectionalLight' && nesne.castShadow) {
                            const aciX = Number(ayar.aciX ?? ayar.AciX ?? 45) * (Math.PI / 180);
                            const aciY = Number(ayar.aciY ?? ayar.AciY ?? 45) * (Math.PI / 180);
                            const r = 6.0;
                            // Update position based on spherical coordinates
                            nesne.position.set(
                                r * Math.cos(aciY) * Math.sin(aciX),
                                r * Math.sin(aciY),
                                r * Math.cos(aciY) * Math.cos(aciX)
                            );
                        }
                    }
                });

                veri.renderer.toneMappingExposure = Number(ayar.pozlama ?? ayar.Pozlama ?? veri.renderer.toneMappingExposure);
            }

            if (ayarTipi === 'cevre') {
                const arkaPlan = ayar.arkaPlanRengi ?? ayar.ArkaPlanRengi;
                if (arkaPlan) {
                    veri.sahne.background = new THREE.Color(arkaPlan);
                }

                const zeminRengi = ayar.zeminRengi ?? ayar.ZeminRengi;
                if (zeminRengi && veri.izgaraGrubu) {
                    veri.izgaraGrubu.children.forEach(c => {
                        if (c.geometry && c.geometry.type === 'PlaneGeometry') {
                            const zeminTipi = (ayar.zeminTipi ?? ayar.ZeminTipi ?? 'mat').toLowerCase();
                            const isParlak = zeminTipi === 'parlak';
                            const isYansimali = zeminTipi === 'yansimali';

                            if (c.material.type === 'ShadowMaterial') {
                                c.material = new THREE.MeshStandardMaterial({
                                    color: new THREE.Color(zeminRengi),
                                    roughness: isParlak ? 0.2 : (isYansimali ? 0.1 : 0.9),
                                    metalness: isYansimali ? 0.8 : (isParlak ? 0.3 : 0.1)
                                });
                            } else {
                                c.material.color.set(new THREE.Color(zeminRengi));
                                c.material.roughness = isParlak ? 0.2 : (isYansimali ? 0.1 : 0.9);
                                c.material.metalness = isYansimali ? 0.8 : (isParlak ? 0.3 : 0.1);
                                c.material.needsUpdate = true;
                            }
                        }
                    });
                }

                const hdrYolu = ayar.hdrYolu ?? ayar.HdrYolu;
                if (hdrYolu && typeof THREE.RGBELoader !== 'undefined') {
                    new THREE.RGBELoader().load(hdrYolu, function (doku) {
                        const pmrem = new THREE.PMREMGenerator(veri.renderer);
                        veri.sahne.environment = pmrem.fromEquirectangular(doku).texture;
                        doku.dispose();
                    });
                }
            }
        },

        /**
         * Işık orijinal şiddetini kaydeder (ilk isik_ayar çağrısı için).
         */
        isik_kaydet: function (kanvasId) {
            const veri = _sahneGetir(kanvasId);
            if (!veri || !veri.sahne) return;

            veri.sahne.traverse(function (nesne) {
                if (nesne.isLight && nesne.userData.orijinalSiddet === undefined) {
                    nesne.userData.orijinalSiddet = nesne.intensity;
                }
            });
        },

        /**
         * Parça seçildiğinde .NET tarafına bildirim için callback kaydeder.
         */
        parca_sec_callback_kaydet: function (kanvasId, dotNetRef) {
            const veri = _sahneGetir(kanvasId);
            if (veri) {
                veri.parcaSecildiCallback = dotNetRef;
            }
        },

        /**
         * Sahnede yuklu olan 3D modeli degistirir.
         * Eski model temizlenir, yeni GLB/GLTF dosyasi yuklenir.
         */
        model_degistir: function (kanvasId, modelYolu) {
            const veri = _sahneGetir(kanvasId);
            if (!veri) return;
            if (veri.model) {
                veri.sahne.remove(veri.model);
            }
            this.modeli_yukle(kanvasId, modelYolu);
        },

        /**
         * Kullanicinin gordugu sahnenin ekran goruntusunu PNG olarak alir.
         * Base64 formatinda geri doner.
         */
        ekran_goruntusu_al: function (kanvasId) {
            const veri = _sahneGetir(kanvasId);
            if (!veri || !veri.renderer) return null;
            return veri.renderer.domElement.toDataURL('image/png');
        },

        /**
         * 3D modeli olceklendirir. Genislik ve yukseklik mm cinsinden.
         * Mevcut model bounding box'ina gore olcek hesaplanir.
         */
        olcu_uygula: function (kanvasId, genislikMm, yukseklikMm) {
            const veri = _sahneGetir(kanvasId);
            if (!veri || !veri.model) return;

            const kutu = new THREE.Box3().setFromObject(veri.model);
            const boyut = kutu.getSize(new THREE.Vector3());
            const maxBoyut = Math.max(boyut.x, boyut.y, boyut.z);
            const hedefMax = Math.max(genislikMm / 1000, yukseklikMm / 1000);
            if (maxBoyut > 0) {
                const olcek = hedefMax / maxBoyut;
                veri.model.scale.setScalar(olcek);
            }
        }
    };
})();

// === DRACO Loader Desteği ===
// DRACO sıkıştırılmış GLB/GLTF modelleri için
window.initDracoLoader = function(dracoPath) {
    if (!window.THREE) return;
    
    try {
        const dracoLoader = new THREE.DRACOLoader();
        dracoLoader.setDecoderPath(dracoPath || '/js/draco/');
        dracoLoader.setDecoderConfig({ type: 'js' });
        
        const gltfLoader = new THREE.GLTFLoader();
        gltfLoader.setDRACOLoader(dracoLoader);
        
        window._dracoGltfLoader = gltfLoader;
        console.log('DRACO loader başlatıldı');
    } catch (e) {
        console.warn('DRACO loader başlatılamadı:', e);
    }
};

// DRACO destekli model yükleme
window.loadDracoModel = function(url, canvasId) {
    return new Promise((resolve, reject) => {
        if (!window._dracoGltfLoader) {
            reject('DRACO loader başlatılmamış');
            return;
        }
        window._dracoGltfLoader.load(
            url,
            (gltf) => resolve({ scene: gltf.scene, animations: gltf.animations }),
            (progress) => {
                if (progress.total > 0) {
                    const pct = Math.round((progress.loaded / progress.total) * 100);
                    window._modelProgress = pct;
                }
            },
            (error) => reject(error)
        );
    });
};

// === HDR Çevre Haritası Desteği ===
window.setEnvironmentMap = function(hdrUrl) {
    if (!window._renderer || !window._scene) return;
    
    try {
        const pmremGenerator = new THREE.PMREMGenerator(window._renderer);
        pmremGenerator.compileEquirectangularShader();
        
        new THREE.RGBELoader()
            .setDataType(THREE.HalfFloatType)
            .load(hdrUrl, function(texture) {
                const envMap = pmremGenerator.fromEquirectangular(texture).texture;
                window._scene.environment = envMap;
                window._scene.background = envMap;
                window._scene.backgroundIntensity = 0.4;
                
                // Tüm mesh'lere yansıma uygula
                window._scene.traverse(function(child) {
                    if (child.isMesh && child.material.isMeshStandardMaterial) {
                        child.material.envMapIntensity = 0.6;
                        child.material.needsUpdate = true;
                    }
                });
                
                texture.dispose();
            });
    } catch (e) {
        console.warn('HDR yüklenemedi:', e);
    }
};

// === Hotspot (Tıklanabilir Nokta) Sistemi ===
window._hotspots = [];

window.addHotspot = function(position, meshName, label) {
    if (!window._scene || !window._camera) return;
    
    // Küçük küre işaretçi
    const geometry = new THREE.SphereGeometry(0.05, 16, 16);
    const material = new THREE.MeshBasicMaterial({ 
        color: 0xc8952a,  // altın rengi
        transparent: true,
        opacity: 0.8
    });
    const marker = new THREE.Mesh(geometry, material);
    marker.position.copy(position);
    marker.userData = { meshName, label, isHotspot: true };
    window._scene.add(marker);
    window._hotspots.push(marker);
    
    return marker;
};

window.clearHotspots = function() {
    window._hotspots.forEach(function(h) {
        h.geometry.dispose();
        h.material.dispose();
        window._scene.remove(h);
    });
    window._hotspots = [];
};

window.checkHotspotClick = function(mouseX, mouseY) {
    if (!window._raycaster || !window._camera) return null;
    
    window._raycaster.setFromCamera(
        new THREE.Vector2(mouseX, mouseY), 
        window._camera
    );
    
    const intersects = window._raycaster.intersectObjects(window._hotspots);
    if (intersects.length > 0) {
        return intersects[0].object.userData;
    }
    return null;
};
