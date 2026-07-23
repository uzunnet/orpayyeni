using Microsoft.JSInterop;

namespace VizitLink3D.UI.Servisler
{
    /// <summary>
    /// Üç Boyut Servisi — Blazor bileşenlerinden JavaScript'teki
    /// UcBoyutMotoru (Three.js wrapper) fonksiyonlarını güvenli şekilde
    /// çağırmaya yarayan köprü servisidir.
    ///
    /// Bu servis KURALLAR.md §2 uyarınca Three.js'e doğrudan erişim sağlamaz;
    /// tüm işlemler Türkçe isimlendirilmiş bu sınıf üzerinden yürütülür.
    /// Blazor bileşeni yalnızca UcBoyutServisi'ni bilir.
    /// </summary>
    public class UcBoyutServisi : IAsyncDisposable
    {
        /// <summary>
        /// .NET 10 Blazor WebAssembly'nin JavaScript çalışma zamanı arayüzü.
        /// JS çağrıları bu nesne üzerinden yapılır.
        /// </summary>
        private readonly IJSRuntime _jsCalismZamani;

        /// <summary>
        /// Servis ömrü boyunca aktif olan kanvas → sahne kayıt listesi.
        /// Hangi kanvas ID'lerinin başlatıldığını tutar, temizleme için gereklidir.
        /// </summary>
        private readonly List<string> _aktifKanvaslar = new();

        public UcBoyutServisi(IJSRuntime jsCalismZamani)
        {
            _jsCalismZamani = jsCalismZamani;
        }

        /// <summary>
        /// Belirtilen kanvas kimliğine sahip HTML elementi üzerinde Three.js
        /// sahnesini başlatır. Kamera, ışıklandırma, renderer ve OrbitControls
        /// kurulumu JavaScript tarafında gerçekleştirilir.
        ///
        /// Eğer modelYolu sağlanırsa GLTF/GLB formatında 3D model yüklenir.
        /// Aksi hâlde programatik kapak geometrisi gösterilir.
        /// </summary>
        /// <param name="kanvasId">Three.js sahnesi için hedef HTML element ID'si</param>
        /// <param name="modelYolu">GLB/GLTF model dosya yolu (opsiyonel, null olabilir)</param>
        /// <param name="baslangicRenk">Başlangıç rengi (hex kodu, örn: "#E8E4DF")</param>
        public async Task Baslat(string kanvasId, string? modelYolu = null, string baslangicRenk = "#E8E4DF", string? cevreJson = null)
        {
            try
            {
                await _jsCalismZamani.InvokeVoidAsync(
                    "UcBoyutMotoru.baslat",
                    kanvasId,
                    modelYolu,
                    baslangicRenk,
                    cevreJson
                );

                if (!_aktifKanvaslar.Contains(kanvasId))
                    _aktifKanvaslar.Add(kanvasId);
            }
            catch (Exception hata)
            {
                Console.Error.WriteLine($"[UcBoyutServisi] Başlatma hatası ({kanvasId}): {hata.Message}");
            }
        }

        /// <summary>
        /// 3D modelin rengini anında değiştirir.
        /// Tüm yüzeylerin malzeme rengi belirtilen hex renkle güncellenir.
        /// Metal aksesuarlar (kol vb.) bu değişimden etkilenmez.
        /// </summary>
        /// <param name="kanvasId">Rengi değiştirilecek sahnenin kanvas ID'si</param>
        /// <param name="renkHex">Yeni renk kodu (örn: "#FF5733" veya "#FFFFFF")</param>
        public async Task RenkUygula(string kanvasId, string renkHex)
        {
            try
            {
                await _jsCalismZamani.InvokeVoidAsync(
                    "UcBoyutMotoru.renk_uygula",
                    kanvasId,
                    renkHex
                );
            }
            catch (Exception hata)
            {
                Console.Error.WriteLine($"[UcBoyutServisi] Renk uygulama hatası: {hata.Message}");
            }
        }

        /// <summary>
        /// Modelin otomatik yavaş döndürme animasyonunu açar veya kapatır.
        /// Kullanıcı modeli eliyle döndürdüğünde otomatik döndürme duraklar,
        /// 2 saniye hareketsizlik sonrası tekrar devreye girer.
        /// </summary>
        /// <param name="kanvasId">Hedef sahne kanvas ID'si</param>
        /// <param name="aktifMi">true → döndürme açık, false → kapalı</param>
        public async Task OtomatikDondur(string kanvasId, bool aktifMi)
        {
            try
            {
                await _jsCalismZamani.InvokeVoidAsync(
                    "UcBoyutMotoru.otomatik_dondur",
                    kanvasId,
                    aktifMi
                );
            }
            catch (Exception hata)
            {
                Console.Error.WriteLine($"[UcBoyutServisi] Döndürme hatası: {hata.Message}");
            }
        }

        /// <summary>
        /// Endustriyel olcu izgarasini acar veya kapatir.
        /// </summary>
        public async Task IzgaraGoster(string kanvasId, bool gorunurMu)
        {
            try
            {
                await _jsCalismZamani.InvokeVoidAsync(
                    "UcBoyutMotoru.izgara_goster",
                    kanvasId,
                    gorunurMu
                );
            }
            catch (Exception hata)
            {
                Console.Error.WriteLine($"[UcBoyutServisi] Izgara hatasi: {hata.Message}");
            }
        }

        /// <summary>
        /// Model parcalarini kontrollu sekilde ayirarak teknik gorunum saglar.
        /// </summary>
        public async Task PatlatilmisGorunum(string kanvasId, bool aktifMi)
        {
            try
            {
                await _jsCalismZamani.InvokeVoidAsync(
                    "UcBoyutMotoru.patlatilmis_gorunum",
                    kanvasId,
                    aktifMi
                );
            }
            catch (Exception hata)
            {
                Console.Error.WriteLine($"[UcBoyutServisi] Patlatilmis gorunum hatasi: {hata.Message}");
            }
        }

        /// <summary>
        /// Görüntüleyiciyi tam ekran moduna alır.
        /// Tarayıcının yerleşik Fullscreen API'si kullanılır.
        /// Tekrar çağrıldığında tam ekrandan çıkılır.
        /// </summary>
        /// <param name="kanvasId">Tam ekrana alınacak kanvas ID'si</param>
        public async Task TamEkran(string kanvasId)
        {
            try
            {
                await _jsCalismZamani.InvokeVoidAsync(
                    "UcBoyutMotoru.tam_ekran",
                    kanvasId
                );
            }
            catch (Exception hata)
            {
                Console.Error.WriteLine($"[UcBoyutServisi] Tam ekran hatası: {hata.Message}");
            }
        }

        /// <summary>
        /// Kamerayı başlangıç pozisyonuna (varsayılan açı ve uzaklığa) sıfırlar.
        /// Kullanıcı modeli çok yaklaştırdı veya garip bir açıya getirdiyse
        /// bu fonksiyon ile orijinal görünüme dönülür.
        /// </summary>
        /// <param name="kanvasId">Kamerası sıfırlanacak sahne ID'si</param>
        public async Task KameraSifirla(string kanvasId)
        {
            try
            {
                await _jsCalismZamani.InvokeVoidAsync(
                    "UcBoyutMotoru.kamera_sifirla",
                    kanvasId
                );
            }
            catch (Exception hata)
            {
                Console.Error.WriteLine($"[UcBoyutServisi] Kamera sıfırlama hatası: {hata.Message}");
            }
        }

        /// <summary>
        /// Sahnede yuklu olan 3D modeli degistirir.
        /// Eski model temizlenir, yeni GLB/GLTF dosyasi yuklenir.
        /// </summary>
        public async Task ModelDegistir(string kanvasId, string modelYolu)
        {
            try
            {
                await _jsCalismZamani.InvokeVoidAsync(
                    "UcBoyutMotoru.model_degistir",
                    kanvasId,
                    modelYolu
                );
            }
            catch (Exception hata)
            {
                Console.Error.WriteLine($"[UcBoyutServisi] Model degistirme hatasi: {hata.Message}");
            }
        }

        /// <summary>
        /// Kullanicinin gordugu sahnenin ekran goruntusunu PNG olarak alir.
        /// Base64 formatinda geri doner, dogrudan indirilebilir veya kaydedilebilir.
        /// </summary>
        public async Task<string?> EkranGoruntusuAl(string kanvasId)
        {
            try
            {
                return await _jsCalismZamani.InvokeAsync<string>(
                    "UcBoyutMotoru.ekran_goruntusu_al",
                    kanvasId
                );
            }
            catch (Exception hata)
            {
                Console.Error.WriteLine($"[UcBoyutServisi] Ekran goruntusu hatasi: {hata.Message}");
                return null;
            }
        }

        /// <summary>
        /// 3D modeli olceklendirir. Genislik ve yukseklik mm cinsinden verilir.
        /// </summary>
        public async Task OlcuUygula(string kanvasId, double genislikMm, double yukseklikMm)
        {
            try
            {
                await _jsCalismZamani.InvokeVoidAsync(
                    "UcBoyutMotoru.olcu_uygula",
                    kanvasId,
                    genislikMm,
                    yukseklikMm
                );
            }
            catch (Exception hata)
            {
                Console.Error.WriteLine($"[UcBoyutServisi] Olcu uygulama hatasi: {hata.Message}");
            }
        }

        /// <summary>
        /// Belirtilen kanvasa ait Three.js sahnesini tamamen kaldırır.
        /// Animasyon döngüsü durdurulur, tüm geometri ve materyal nesneleri
        /// bellekten temizlenir. Bileşen kaldırıldığında mutlaka çağrılmalıdır.
        /// </summary>
        /// <param name="kanvasId">Temizlenecek sahne kanvas ID'si</param>
        public async Task Temizle(string kanvasId)
        {
            try
            {
                await _jsCalismZamani.InvokeVoidAsync(
                    "UcBoyutMotoru.temizle",
                    kanvasId
                );
                _aktifKanvaslar.Remove(kanvasId);
            }
            catch (Exception hata)
            {
                Console.Error.WriteLine($"[UcBoyutServisi] Temizleme hatası: {hata.Message}");
            }
        }

        /// <summary>
        /// 3D model uzerinde tiklanabilir hotspot (etkilesimli nokta) sistemini etkinlestirir.
        /// Tiklanan nokta bilgisi callback ile .NET tarafina iletilir.
        /// </summary>
        public async Task HotspotEtkinlestir(string kanvasId, object dotNetRef, string metodAdi = "HotspotTiklandi")
        {
            try
            {
                await _jsCalismZamani.InvokeVoidAsync("UcBoyutMotoru.hotspot_etkinlestir", kanvasId, dotNetRef, metodAdi);
            }
            catch (Exception hata)
            {
                Console.Error.WriteLine($"[UcBoyutServisi] Hotspot etkinleştirme hatası: {hata.Message}");
            }
        }

        /// <summary>
        /// Yüklenen 3D modelin tüm mesh/grup isimlerini JSON olarak döndürür.
        /// </summary>
        public async Task<string> ModelAnalizEt(string kanvasId)
        {
            return await _jsCalismZamani.InvokeAsync<string>("UcBoyutMotoru.model_analiz_et", kanvasId);
        }

        /// <summary>
        /// Belirtilen isimdeki parçanın görünürlüğünü değiştirir.
        /// </summary>
        public async Task ParcaGorunurluk(string kanvasId, string parcaIsmi, bool gorunurMu)
        {
            await _jsCalismZamani.InvokeVoidAsync("UcBoyutMotoru.parca_gorunurluk", kanvasId, parcaIsmi, gorunurMu);
        }

        /// <summary>
        /// Belirtilen isimdeki parçanın rengini değiştirir.
        /// </summary>
        public async Task ParcaRenk(string kanvasId, string parcaIsmi, string renkHex)
        {
            await _jsCalismZamani.InvokeVoidAsync("UcBoyutMotoru.parca_renk", kanvasId, parcaIsmi, renkHex);
        }

        /// <summary>
        /// Belirtilen isimdeki parçayı derece cinsinden döndürür (Y ekseni — kapak aç/kapa).
        /// </summary>
        public async Task KapakDerece(string kanvasId, string parcaIsmi, int derece)
        {
            await _jsCalismZamani.InvokeVoidAsync("UcBoyutMotoru.kapak_derece", kanvasId, parcaIsmi, derece);
        }

        /// <summary>
        /// Belirtilen isimdeki parçaya malzeme uygular (metal, cam, ayna, ahsap, plastik).
        /// </summary>
        public async Task ParcaMalzeme(string kanvasId, string parcaIsmi, string malzeme)
        {
            await _jsCalismZamani.InvokeVoidAsync("UcBoyutMotoru.parca_malzeme", kanvasId, parcaIsmi, malzeme);
        }

        /// <summary>
        /// 3D sahnesi için HDRI çevre haritasını ayarlar.
        /// </summary>
        public async Task CevreAyarla(string kanvasId, string hdriKey)
        {
            try
            {
                await _jsCalismZamani.InvokeVoidAsync("UcBoyutMotoru.cevre_ayarla", kanvasId, hdriKey);
            }
            catch (Exception hata)
            {
                Console.Error.WriteLine($"[UcBoyutServisi] Çevre ayarlama hatası: {hata.Message}");
            }
        }

        /// <summary>
        /// Sahne ışık şiddetini ayarlar (0.0 — 2.0 arası).
        /// </summary>
        public async Task IsikAyar(string kanvasId, double seviye)
        {
            await _jsCalismZamani.InvokeVoidAsync("UcBoyutMotoru.isik_ayar", kanvasId, seviye);
        }

        /// <summary>
        /// Admin panelinden gelen canlı sahne ayarını açık görüntüleyiciye uygular.
        /// </summary>
        public async Task SahneAyarUygula(string kanvasId, string ayarTipi, string ayarJson)
        {
            await _jsCalismZamani.InvokeVoidAsync("UcBoyutMotoru.sahne_ayar_uygula", kanvasId, ayarTipi, ayarJson);
        }

        /// <summary>
        /// Işık orijinal şiddetini kaydeder.
        /// </summary>
        public async Task IsikKaydet(string kanvasId)
        {
            await _jsCalismZamani.InvokeVoidAsync("UcBoyutMotoru.isik_kaydet", kanvasId);
        }

        /// <summary>
        /// Model üzerinde tıklanan parçanın .NET callback'ini kaydeder.
        /// </summary>
        public async Task ParcaSecCallbackKaydet(string kanvasId, object dotNetRef)
        {
            await _jsCalismZamani.InvokeVoidAsync("UcBoyutMotoru.parca_sec_callback_kaydet", kanvasId, dotNetRef);
        }

        /// <summary>
        /// Servis bellekten kaldırıldığında tüm aktif sahneleri otomatik temizler.
        /// IAsyncDisposable arayüzü gereği uygulanmıştır; bellek sızıntısını önler.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            foreach (var kanvasId in _aktifKanvaslar.ToList())
            {
                await Temizle(kanvasId);
            }
        }

        /// <summary>
        /// JS modül referansı (IJSObjectReference) — Blazor bileşeni tarafından
        /// atanır. Null ise modül-bazlı çağrılar sessizce başarısız olur.
        /// </summary>
        private IJSObjectReference? _modul;

        /// <summary>JS modül referansını ayarlar</summary>
        public void ModulAta(IJSObjectReference modul) => _modul = modul;

        /// <summary>Modeldeki tüm mesh/grup adlarını döndürür</summary>
        public async Task<List<string>> ModelParcaListesiAlAsync()
        {
            try
            {
                return await _modul!.InvokeAsync<List<string>>("modelParcaListesiAl");
            }
            catch { /* 3D motor hazır değilse parça listesi alınamaz */ }
            return [];
        }

        /// <summary>Belirtilen parçayı vurgular</summary>
        public async Task ParcaVurgulaAsync(string meshAdi)
        {
            try { await _modul!.InvokeVoidAsync("parcaVurgula", meshAdi); }
            catch { /* 3D etkileşimi başarısız olursa sayfa çalışmaya devam eder */ }
        }

        /// <summary>Parça vurgusunu temizler</summary>
        public async Task ParcaVurguTemizleAsync()
        {
            try { await _modul!.InvokeVoidAsync("parcaVurguTemizle"); }
            catch { /* 3D etkileşimi başarısız olursa sayfa çalışmaya devam eder */ }
        }

        /// <summary>Parçaya renk uygular (HexKod ile)</summary>
        public async Task ParcaRenkAsync(string meshAdi, string hexKod)
        {
            try { await _modul!.InvokeVoidAsync("parcaRenk", meshAdi, hexKod); }
            catch { /* 3D etkileşimi başarısız olursa sayfa çalışmaya devam eder */ }
        }

        /// <summary>Parçaya malzeme uygular (tip ile)</summary>
        public async Task ParcaMalzemeAsync(string meshAdi, string malzemeTipi)
        {
            try { await _modul!.InvokeVoidAsync("parcaMalzeme", meshAdi, malzemeTipi); }
            catch { /* 3D etkileşimi başarısız olursa sayfa çalışmaya devam eder */ }
        }

        /// <summary>Parça görünürlüğünü ayarlar</summary>
        public async Task ParcaGorunurlukAsync(string meshAdi, bool gorunur)
        {
            try { await _modul!.InvokeVoidAsync("parcaGorunurluk", meshAdi, gorunur); }
            catch { /* 3D etkileşimi başarısız olursa sayfa çalışmaya devam eder */ }
        }

        /// <summary>Hareketli parçayı belirtilen değere ayarlar (kapak açısı vb.)</summary>
        public async Task ParcaHareketAsync(string meshAdi, double deger)
        {
            try { await _modul!.InvokeVoidAsync("parcaHareket", meshAdi, deger); }
            catch { /* 3D etkileşimi başarısız olursa sayfa çalışmaya devam eder */ }
        }

        /// <summary>Kamera preset'ini uygular</summary>
        public async Task KameraPresetUygulaAsync(string presetAdi)
        {
            try { await _modul!.InvokeVoidAsync("kameraPresetUygula", presetAdi); }
            catch { /* 3D etkileşimi başarısız olursa sayfa çalışmaya devam eder */ }
        }

        /// <summary>Sahne ayarlarını uygular (JSON ile)</summary>
        public async Task SahneAyarUygulaAsync(string ayarJson)
        {
            try { await _modul!.InvokeVoidAsync("sahneAyarUygula", ayarJson); }
            catch { /* 3D etkileşimi başarısız olursa sayfa çalışmaya devam eder */ }
        }

        /// <summary>Ekran görüntüsü alır ve base64 döndürür</summary>
        public async Task<string?> EkranGoruntusuAlAsync()
        {
            try
            {
                return await _modul!.InvokeAsync<string>("ekranGoruntusuAl");
            }
            catch { /* 3D motor hazır değilse ekran görüntüsü alınamaz */ }
            return null;
        }

        /// <summary>Model optimizasyon bilgisini döndürür</summary>
        public async Task<string?> ModelOptimizasyonBilgisiAlAsync()
        {
            try
            {
                return await _modul!.InvokeAsync<string>("modelOptimizasyonBilgisiAl");
            }
            catch { /* 3D motor hazır değilse bilgi alınamaz */ }
            return null;
        }

        public async Task DracoLoaderBaslatAsync(string dracoPath = "/js/draco/")
        {
            try { await _jsCalismZamani.InvokeVoidAsync("initDracoLoader", dracoPath); }
            catch { /* DRACO decoder yüklenemezse normal GLTF loader kullanılır */ }
        }

        public async Task<string> DracoModelYukleAsync(string modelUrl, string canvasId)
        {
            try
            {
                return await _jsCalismZamani.InvokeAsync<string>("loadDracoModel", modelUrl, canvasId);
            }
            catch { /* DRACO model yüklenemezse hata döner */ }
            return "";
        }

        public async Task CevreHaritasiAtaAsync(string hdrUrl)
        {
            try { await _jsCalismZamani.InvokeVoidAsync("setEnvironmentMap", hdrUrl); }
            catch { /* HDR yüklenemezse varsayılan aydınlatma kullanılır */ }
        }

        public async Task HotspotEkleAsync(float x, float y, float z, string meshName, string label)
        {
            try { await _jsCalismZamani.InvokeVoidAsync("addHotspot", new { x, y, z }, meshName, label); }
            catch { /* Hotspot eklenemezse 3D görünüm etkilenmez */ }
        }

        public async Task HotspotlariTemizleAsync()
        {
            try { await _jsCalismZamani.InvokeVoidAsync("clearHotspots"); }
            catch { /* Hotspot temizlenemezse 3D görünüm etkilenmez */ }
        }

        public async Task<string?> HotspotTiklamaKontrolAsync(double mouseX, double mouseY)
        {
            try
            {
                return await _jsCalismZamani.InvokeAsync<string>("checkHotspotClick", mouseX, mouseY);
            }
            catch { /* Kontrol başarısız olursa null döner */ }
            return null;
        }
    }
}
