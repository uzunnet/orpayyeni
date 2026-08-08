(function () {
    var destekleniyor = typeof window !== "undefined"
        && typeof window.WebAssembly !== "undefined"
        && typeof window.fetch !== "undefined";

    var urunler = [
        { slug: "hermes-120", ad: "Hermes 120", kod: "HERMES-120", koleksiyon: "Exclusive", sayfaNo: 6, fiyat: 122500, boyDolabiFiyati: 60000, renkler: ["Siyah", "Bej", "Kahve", "Krem"], ozellikler: ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Stone Lavabo"], olculer: [{ baslik: "Dolap", yukseklik: "85 cm", genislik: "120 cm", derinlik: "50 cm" }, { baslik: "Ayna", yukseklik: "75 cm", genislik: "75 cm", derinlik: "5 cm" }, { baslik: "Boy Dolap", yukseklik: "160 cm", genislik: "38 cm", derinlik: "33 cm" }], oneCikanMi: true, yeniMi: true },
        { slug: "hermes-80", ad: "Hermes 80", kod: "HERMES-80", koleksiyon: "Exclusive", sayfaNo: 7, fiyat: 102000, boyDolabiFiyati: 60000, renkler: ["Siyah", "Bej", "Kahve", "Krem"], ozellikler: ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Stone Lavabo"], olculer: [{ baslik: "Dolap", yukseklik: "85 cm", genislik: "80 cm", derinlik: "50 cm" }, { baslik: "Ayna", yukseklik: "75 cm", genislik: "75 cm", derinlik: "5 cm" }, { baslik: "Boy Dolap", yukseklik: "160 cm", genislik: "38 cm", derinlik: "33 cm" }], oneCikanMi: true, yeniMi: true },
        { slug: "bottega-120", ad: "Bottega 120", kod: "BOTTEGA-120", koleksiyon: "Exclusive", sayfaNo: 8, fiyat: 117500, boyDolabiFiyati: 60000, renkler: ["Beyaz", "Siyah"], ozellikler: ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Stone Lavabo"], olculer: [{ baslik: "Dolap", yukseklik: "85 cm", genislik: "120 cm", derinlik: "50 cm" }, { baslik: "Ayna", yukseklik: "90 cm", genislik: "110 cm", derinlik: "5 cm" }, { baslik: "Boy Dolap", yukseklik: "140 cm", genislik: "38 cm", derinlik: "33 cm" }], oneCikanMi: true, yeniMi: true },
        { slug: "bottega-100", ad: "Bottega 100", kod: "BOTTEGA-100", koleksiyon: "Exclusive", sayfaNo: 9, fiyat: 110000, boyDolabiFiyati: 60000, renkler: ["Beyaz", "Siyah"], ozellikler: ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Stone Lavabo"], olculer: [{ baslik: "Dolap", yukseklik: "85 cm", genislik: "100 cm", derinlik: "50 cm" }, { baslik: "Ayna", yukseklik: "90 cm", genislik: "100 cm", derinlik: "5 cm" }, { baslik: "Boy Dolap", yukseklik: "140 cm", genislik: "38 cm", derinlik: "33 cm" }], oneCikanMi: true, yeniMi: true },
        { slug: "bottega-80", ad: "Bottega 80", kod: "BOTTEGA-80", koleksiyon: "Exclusive", sayfaNo: 10, fiyat: 102000, boyDolabiFiyati: null, renkler: ["Beyaz", "Siyah"], ozellikler: ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Stone Lavabo"], olculer: [{ baslik: "Dolap", yukseklik: "85 cm", genislik: "80 cm", derinlik: "50 cm" }, { baslik: "Ayna", yukseklik: "90 cm", genislik: "80 cm", derinlik: "5 cm" }, { baslik: "Boy Dolap", yukseklik: "140 cm", genislik: "38 cm", derinlik: "33 cm" }], oneCikanMi: true, yeniMi: true },
        { slug: "giorgio-120", ad: "Giorgio 120", kod: "GIORGIO-120", koleksiyon: "Exclusive", sayfaNo: 12, fiyat: 117500, boyDolabiFiyati: 60000, renkler: ["Gri", "Siyah", "Ahşap"], ozellikler: ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Stone Lavabo"], olculer: [{ baslik: "Dolap", yukseklik: "85 cm", genislik: "120 cm", derinlik: "50 cm" }, { baslik: "Ayna", yukseklik: "90 cm", genislik: "100 cm", derinlik: "5 cm" }, { baslik: "Boy Dolap", yukseklik: "140 cm", genislik: "38 cm", derinlik: "33 cm" }], oneCikanMi: true, yeniMi: true },
        { slug: "giorgio-100", ad: "Giorgio 100", kod: "GIORGIO-100", koleksiyon: "Exclusive", sayfaNo: 13, fiyat: 110000, boyDolabiFiyati: 60000, renkler: ["Gri", "Siyah", "Ahşap"], ozellikler: ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Stone Lavabo"], olculer: [{ baslik: "Dolap", yukseklik: "85 cm", genislik: "100 cm", derinlik: "50 cm" }, { baslik: "Ayna", yukseklik: "90 cm", genislik: "100 cm", derinlik: "5 cm" }, { baslik: "Boy Dolap", yukseklik: "140 cm", genislik: "38 cm", derinlik: "33 cm" }], oneCikanMi: true, yeniMi: true },
        { slug: "giorgio-80", ad: "Giorgio 80", kod: "GIORGIO-80", koleksiyon: "Exclusive", sayfaNo: 14, fiyat: 102000, boyDolabiFiyati: null, renkler: ["Gri", "Siyah", "Ahşap"], ozellikler: ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Stone Lavabo"], olculer: [{ baslik: "Dolap", yukseklik: "85 cm", genislik: "80 cm", derinlik: "50 cm" }, { baslik: "Ayna", yukseklik: "90 cm", genislik: "70 cm", derinlik: "5 cm" }, { baslik: "Boy Dolap", yukseklik: "140 cm", genislik: "38 cm", derinlik: "33 cm" }], oneCikanMi: true, yeniMi: true },
        { slug: "diago-120", ad: "Diago 120", kod: "DIAGO-120", koleksiyon: "Exclusive", sayfaNo: 16, fiyat: 112500, boyDolabiFiyati: 55000, renkler: ["Siyah", "Kahve Rengi"], ozellikler: ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Cam Lavabo"], olculer: [{ baslik: "Dolap", yukseklik: "85 cm", genislik: "120 cm", derinlik: "46 cm" }, { baslik: "Ayna", yukseklik: "75 cm", genislik: "75 cm", derinlik: "5 cm" }, { baslik: "Boy Dolap", yukseklik: "140 cm", genislik: "38 cm", derinlik: "33 cm" }], oneCikanMi: true, yeniMi: true },
        { slug: "diago-100", ad: "Diago 100", kod: "DIAGO-100", koleksiyon: "Exclusive", sayfaNo: 17, fiyat: 99500, boyDolabiFiyati: null, renkler: ["Siyah", "Kahve Rengi"], ozellikler: ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Cam Lavabo"], olculer: [{ baslik: "Dolap", yukseklik: "85 cm", genislik: "100 cm", derinlik: "46 cm" }, { baslik: "Ayna", yukseklik: "75 cm", genislik: "75 cm", derinlik: "5 cm" }, { baslik: "Boy Dolap", yukseklik: "140 cm", genislik: "38 cm", derinlik: "33 cm" }], oneCikanMi: true, yeniMi: true },
        { slug: "capelli-150", ad: "Capelli 150", kod: "CAPELLI-150", koleksiyon: "Premium", sayfaNo: 19, fiyat: 97000, boyDolabiFiyati: null, renkler: ["Mocha", "Gri"], ozellikler: ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna"], olculer: [{ baslik: "Dolap", yukseklik: "85 cm", genislik: "150 cm", derinlik: "46 cm" }, { baslik: "Ayna", yukseklik: "75 cm", genislik: "75 cm", derinlik: "5 cm" }], oneCikanMi: true, yeniMi: true },
        { slug: "capelli-100", ad: "Capelli 100", kod: "CAPELLI-100", koleksiyon: "Premium", sayfaNo: 20, fiyat: 67500, boyDolabiFiyati: null, renkler: ["Mocha", "Gri"], ozellikler: ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna"], olculer: [{ baslik: "Dolap", yukseklik: "85 cm", genislik: "100 cm", derinlik: "46 cm" }, { baslik: "Ayna", yukseklik: "75 cm", genislik: "75 cm", derinlik: "5 cm" }], oneCikanMi: true, yeniMi: true },
        { slug: "hera-120", ad: "Hera 120", kod: "HERA-120", koleksiyon: "Premium", sayfaNo: 21, fiyat: 110000, boyDolabiFiyati: null, renkler: ["Hareli Meşe", "Antik Ceviz"], ozellikler: ["Soft Kapak", "Doğal Ağaç", "Dokunmatik Ledli Ayna"], olculer: [{ baslik: "Dolap", yukseklik: "85 cm", genislik: "120 cm", derinlik: "46,5 cm" }, { baslik: "Ayna", yukseklik: "69 cm", genislik: "120 cm", derinlik: "5 cm" }], oneCikanMi: true, yeniMi: true },
        { slug: "hera-80", ad: "Hera 80", kod: "HERA-80", koleksiyon: "Premium", sayfaNo: 23, fiyat: 104000, boyDolabiFiyati: null, renkler: ["Hareli Meşe", "Antik Ceviz"], ozellikler: ["Soft Kapak", "Doğal Ağaç", "Dokunmatik Ledli Ayna"], olculer: [{ baslik: "Dolap", yukseklik: "85 cm", genislik: "80 cm", derinlik: "46,5 cm" }, { baslik: "Ayna", yukseklik: "69 cm", genislik: "80 cm", derinlik: "5 cm" }], oneCikanMi: true, yeniMi: true },
        { slug: "cavalli-110", ad: "Cavalli 110", kod: "CAVALLI-110", koleksiyon: "Premium", sayfaNo: 24, fiyat: 72000, boyDolabiFiyati: 40000, renkler: ["Antrasit", "Beyaz"], ozellikler: ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna"], olculer: [{ baslik: "Dolap", yukseklik: "85 cm", genislik: "110 cm", derinlik: "46 cm" }, { baslik: "Ayna", yukseklik: "90 cm", genislik: "70 cm", derinlik: "5 cm" }, { baslik: "Boy Dolap", yukseklik: "140 cm", genislik: "45 cm", derinlik: "32 cm" }], oneCikanMi: true, yeniMi: true },
        { slug: "cavalli-90", ad: "Cavalli 90", kod: "CAVALLI-90", koleksiyon: "Premium", sayfaNo: 26, fiyat: 66000, boyDolabiFiyati: null, renkler: ["Antrasit", "Beyaz"], ozellikler: ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna"], olculer: [{ baslik: "Dolap", yukseklik: "85 cm", genislik: "90 cm", derinlik: "46 cm" }, { baslik: "Ayna", yukseklik: "90 cm", genislik: "70 cm", derinlik: "5 cm" }, { baslik: "Boy Dolap", yukseklik: "140 cm", genislik: "45 cm", derinlik: "32 cm" }], oneCikanMi: true, yeniMi: true },
        { slug: "tiffany-120", ad: "Tiffany 120", kod: "TIFFANY-120", koleksiyon: "Premium", sayfaNo: 28, fiyat: 67500, boyDolabiFiyati: 30000, renkler: ["Antrasit", "Beyaz", "Mocha"], ozellikler: ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna"], olculer: [{ baslik: "Dolap", yukseklik: "85 cm", genislik: "120 cm", derinlik: "45 cm" }, { baslik: "Ayna", yukseklik: "90 cm", genislik: "70 cm", derinlik: "5 cm" }, { baslik: "Boy Dolap", yukseklik: "120 cm", genislik: "36 cm", derinlik: "36 cm" }], oneCikanMi: true, yeniMi: true },
        { slug: "tiffany-100", ad: "Tiffany 100", kod: "TIFFANY-100", koleksiyon: "Premium", sayfaNo: 29, fiyat: 62000, boyDolabiFiyati: null, renkler: ["Antrasit", "Beyaz", "Mocha"], ozellikler: ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna"], olculer: [{ baslik: "Dolap", yukseklik: "85 cm", genislik: "100 cm", derinlik: "45 cm" }, { baslik: "Ayna", yukseklik: "90 cm", genislik: "70 cm", derinlik: "5 cm" }, { baslik: "Boy Dolap", yukseklik: "120 cm", genislik: "36 cm", derinlik: "36 cm" }], oneCikanMi: true, yeniMi: true },
        { slug: "siena-80", ad: "Siena 80", kod: "SIENA-80", koleksiyon: "Trend", sayfaNo: 31, fiyat: 97500, boyDolabiFiyati: 30000, renkler: ["Bej", "Gri"], ozellikler: ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna"], olculer: [{ baslik: "Dolap", yukseklik: "85 cm", genislik: "80 cm", derinlik: "50 cm" }, { baslik: "Ayna", yukseklik: "90 cm", genislik: "60 cm", derinlik: "5 cm" }, { baslik: "Boy Dolap", yukseklik: "140 cm", genislik: "38 cm", derinlik: "33 cm" }], oneCikanMi: true, yeniMi: true },
        { slug: "valentino-100", ad: "Valentino 100", kod: "VALENTINO-100", koleksiyon: "Trend", sayfaNo: 33, fiyat: 67500, boyDolabiFiyati: 27500, renkler: ["Spesiyal Gri", "Krem"], ozellikler: ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna"], olculer: [{ baslik: "Dolap", yukseklik: "85 cm", genislik: "100 cm", derinlik: "50 cm" }, { baslik: "Ayna", yukseklik: "70 cm", genislik: "90 cm", derinlik: "5 cm" }, { baslik: "Boy Dolap", yukseklik: "140 cm", genislik: "38 cm", derinlik: "33 cm" }], oneCikanMi: true, yeniMi: true },
        { slug: "zanussi-100", ad: "Zanussi 100", kod: "ZANUSSI-100", koleksiyon: "Trend", sayfaNo: 35, fiyat: 75000, boyDolabiFiyati: 27500, renkler: ["Spesiyal Gri", "Krem"], ozellikler: ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna"], olculer: [{ baslik: "Dolap", yukseklik: "85 cm", genislik: "100 cm", derinlik: "50 cm" }, { baslik: "Ayna", yukseklik: "75 cm", genislik: "75 cm", derinlik: "5 cm" }, { baslik: "Boy Dolap", yukseklik: "140 cm", genislik: "38 cm", derinlik: "33 cm" }], oneCikanMi: true, yeniMi: true },
        { slug: "zanussi-80", ad: "Zanussi 80", kod: "ZANUSSI-80", koleksiyon: "Trend", sayfaNo: 36, fiyat: 67500, boyDolabiFiyati: 27500, renkler: ["Spesiyal Gri", "Krem"], ozellikler: ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna"], olculer: [{ baslik: "Dolap", yukseklik: "85 cm", genislik: "80 cm", derinlik: "50 cm" }, { baslik: "Ayna", yukseklik: "75 cm", genislik: "75 cm", derinlik: "5 cm" }, { baslik: "Boy Dolap", yukseklik: "140 cm", genislik: "38 cm", derinlik: "33 cm" }], oneCikanMi: true, yeniMi: true },
        { slug: "perla-80", ad: "Perla 80", kod: "PERLA-80", koleksiyon: "Trend", sayfaNo: 37, fiyat: 57500, boyDolabiFiyati: 27500, renkler: ["Siyah", "Yeşil", "Turuncu"], ozellikler: ["Soft Kapak", "MDF Ahşap", "Cam Lavabo"], olculer: [{ baslik: "Dolap", yukseklik: "85 cm", genislik: "80 cm", derinlik: "46 cm" }, { baslik: "Ayna", yukseklik: "90 cm", genislik: "70 cm", derinlik: "5 cm" }, { baslik: "Boy Dolap", yukseklik: "140 cm", genislik: "38 cm", derinlik: "33 cm" }], oneCikanMi: true, yeniMi: true },
        { slug: "valencia-90", ad: "Valencia 90", kod: "VALENCIA-90", koleksiyon: "Standart", sayfaNo: 40, fiyat: 49000, boyDolabiFiyati: null, renkler: ["Amerikan Ceviz"], ozellikler: ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna"], olculer: [{ baslik: "Dolap", yukseklik: "85 cm", genislik: "90 cm", derinlik: "46 cm" }, { baslik: "Ayna", yukseklik: "75 cm", genislik: "75 cm", derinlik: "5 cm" }], oneCikanMi: true, yeniMi: true },
        { slug: "otto-100", ad: "Otto 100", kod: "OTTO-100", koleksiyon: "Standart", sayfaNo: 41, fiyat: 59900, boyDolabiFiyati: 37500, renkler: ["Gri", "Kum Beji"], ozellikler: ["Soft Kapak", "MDF Ahşap", "Renk Seçenekleri"], olculer: [{ baslik: "Dolap", yukseklik: "85 cm", genislik: "100 cm", derinlik: "46 cm" }, { baslik: "Ayna", yukseklik: "75 cm", genislik: "100 cm", derinlik: "16 cm" }, { baslik: "Boy Dolap", yukseklik: "140 cm", genislik: "38 cm", derinlik: "33 cm" }], oneCikanMi: true, yeniMi: true },
        { slug: "otto-80", ad: "Otto 80", kod: "OTTO-80", koleksiyon: "Standart", sayfaNo: 42, fiyat: 57500, boyDolabiFiyati: 37500, renkler: ["Gri", "Kum Beji"], ozellikler: ["Soft Kapak", "MDF Ahşap", "Renk Seçenekleri"], olculer: [{ baslik: "Dolap", yukseklik: "85 cm", genislik: "80 cm", derinlik: "46 cm" }, { baslik: "Ayna", yukseklik: "75 cm", genislik: "80 cm", derinlik: "16 cm" }, { baslik: "Boy Dolap", yukseklik: "140 cm", genislik: "38 cm", derinlik: "33 cm" }], oneCikanMi: true, yeniMi: true },
        { slug: "otto-65", ad: "Otto 65", kod: "OTTO-65", koleksiyon: "Standart", sayfaNo: 43, fiyat: 55000, boyDolabiFiyati: 37500, renkler: ["Gri", "Kum Beji"], ozellikler: ["Soft Kapak", "MDF Ahşap", "Renk Seçenekleri"], olculer: [{ baslik: "Dolap", yukseklik: "85 cm", genislik: "65 cm", derinlik: "46 cm" }, { baslik: "Ayna", yukseklik: "75 cm", genislik: "65 cm", derinlik: "16 cm" }, { baslik: "Boy Dolap", yukseklik: "140 cm", genislik: "38 cm", derinlik: "33 cm" }], oneCikanMi: true, yeniMi: true },
        { slug: "lugano-100", ad: "Lugano 100", kod: "LUGANO-100", koleksiyon: "Standart", sayfaNo: 44, fiyat: 49000, boyDolabiFiyati: 28800, renkler: ["Gri", "Beyaz"], ozellikler: ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna"], olculer: [{ baslik: "Dolap", yukseklik: "85 cm", genislik: "100 cm", derinlik: "46 cm" }, { baslik: "Ayna", yukseklik: "75 cm", genislik: "75 cm", derinlik: "5 cm" }, { baslik: "Boy Dolap", yukseklik: "140 cm", genislik: "38 cm", derinlik: "33 cm" }], oneCikanMi: true, yeniMi: true },
        { slug: "lugano-80", ad: "Lugano 80", kod: "LUGANO-80", koleksiyon: "Standart", sayfaNo: 45, fiyat: 45000, boyDolabiFiyati: 28800, renkler: ["Gri", "Beyaz"], ozellikler: ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna"], olculer: [{ baslik: "Dolap", yukseklik: "85 cm", genislik: "80 cm", derinlik: "46 cm" }, { baslik: "Ayna", yukseklik: "75 cm", genislik: "75 cm", derinlik: "5 cm" }, { baslik: "Boy Dolap", yukseklik: "140 cm", genislik: "38 cm", derinlik: "33 cm" }], oneCikanMi: true, yeniMi: true },
        { slug: "lugano-plus-100", ad: "Lugano Plus 100", kod: "LUGANO-PLUS-100", koleksiyon: "Standart", sayfaNo: 46, fiyat: 49000, boyDolabiFiyati: 28800, renkler: ["Gri", "Beyaz"], ozellikler: ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna"], olculer: [{ baslik: "Dolap", yukseklik: "85 cm", genislik: "100 cm", derinlik: "46 cm" }, { baslik: "Ayna", yukseklik: "75 cm", genislik: "100 cm", derinlik: "5 cm" }, { baslik: "Boy Dolap", yukseklik: "140 cm", genislik: "38 cm", derinlik: "33 cm" }], oneCikanMi: true, yeniMi: true },
        { slug: "lugano-plus-80", ad: "Lugano Plus 80", kod: "LUGANO-PLUS-80", koleksiyon: "Standart", sayfaNo: 47, fiyat: 45000, boyDolabiFiyati: 28800, renkler: ["Gri", "Beyaz"], ozellikler: ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna"], olculer: [{ baslik: "Dolap", yukseklik: "85 cm", genislik: "80 cm", derinlik: "46 cm" }, { baslik: "Ayna", yukseklik: "75 cm", genislik: "80 cm", derinlik: "5 cm" }, { baslik: "Boy Dolap", yukseklik: "140 cm", genislik: "38 cm", derinlik: "33 cm" }], oneCikanMi: true, yeniMi: true }
    ];

    var koleksiyonAciklamalari = {
        Exclusive: "İmza koleksiyonları. Geniş modül yapısı, yüksek yüzey kalitesi ve premium sunum diliyle öne çıkar.",
        Premium: "Showroom etkisi veren malzeme, renk ve oran dengesiyle seçkin seri grubu.",
        Trend: "Çağdaş banyolara sıcak renk paleti ve dengeli fiyat yapısı sunan seri.",
        Standart: "Günlük kullanıma uygun, ölçü ve fiyat dengesi güçlü Orpay banyo dolapları."
    };

    var koleksiyonSirasi = { Exclusive: 1, Premium: 2, Trend: 3, Standart: 4 };

    function htmlKodla(deger) {
        return String(deger)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/\"/g, "&quot;")
            .replace(/'/g, "&#39;");
    }

    function fiyatYaz(fiyat) {
        return Number(fiyat).toLocaleString("tr-TR") + " TL";
    }

    function gorselYolu(sayfaNo, tip) {
        var sayfa = String(sayfaNo).padStart(3, "0");
        return "/medya/sayfa-" + sayfa + "-" + tip + ".png";
    }

    function metaAyarla(baslik, aciklama) {
        document.title = baslik;
        var meta = document.querySelector('meta[name="description"]');
        if (meta) {
            meta.setAttribute("content", aciklama);
        }
    }

    function kabukBaslangici() {
        return '<div class="wasm-uyumluluk-kabugu"><div class="wasm-uyumluluk-kapsayici"><div class="wasm-uyumluluk-ust"><div class="wasm-uyumluluk-marka"><span>Orpay Katalog Vitrini</span><strong>Orpay</strong></div><div class="wasm-uyumluluk-bant">Tarayıcı uyumluluk görünümü aktif</div></div>';
    }

    function kabukBitisi() {
        return "</div></div>";
    }

    function listeKartHtml(urun) {
        var etiketler = [];
        if (urun.oneCikanMi) {
            etiketler.push('<span class="wasm-kart-etiket">Öne Çıkan</span>');
        }
        if (urun.yeniMi) {
            etiketler.push('<span class="wasm-kart-etiket">Yeni</span>');
        }

        var aciklama = urun.koleksiyon + " koleksiyonunda " + urun.renkler.length + " renk seçeneği ve " + urun.olculer.length + " ölçü modülü sunar.";

        return '<a class="wasm-kart" href="/banyo-dolabi/' + htmlKodla(urun.slug) + '">'
            + '<div class="wasm-kart-gorsel"><img src="' + gorselYolu(urun.sayfaNo, "hero") + '" alt="' + htmlKodla(urun.ad) + '" /></div>'
            + '<div class="wasm-kart-icerik">'
            + '<div class="wasm-kart-ustbilgi"><span>' + htmlKodla(urun.kod) + '</span><span>' + htmlKodla(urun.koleksiyon) + '</span></div>'
            + '<div class="wasm-kart-etiketler">' + etiketler.join("") + '</div>'
            + '<div class="wasm-kart-baslik">' + htmlKodla(urun.ad) + '</div>'
            + '<div class="wasm-kart-aciklama">' + htmlKodla(aciklama) + '</div>'
            + '<div class="wasm-kart-fiyat">' + htmlKodla(fiyatYaz(urun.fiyat)) + '</div>'
            + '</div></a>';
    }

    function listeSayfasiHtml() {
        var gruplar = {};
        urunler.forEach(function (urun) {
            if (!gruplar[urun.koleksiyon]) {
                gruplar[urun.koleksiyon] = [];
            }
            gruplar[urun.koleksiyon].push(urun);
        });

        var koleksiyonlar = Object.keys(gruplar).sort(function (a, b) {
            return (koleksiyonSirasi[a] || 99) - (koleksiyonSirasi[b] || 99);
        });

        var toplamOneCikan = urunler.filter(function (urun) { return urun.oneCikanMi; }).length;

        var html = kabukBaslangici();
        html += '<section class="banyo-vitrin-hero">'
            + '<div class="banyo-vitrin-hero__icerik">'
            + '<div class="banyo-vitrin-hero__etiket">Orpay 2026 Koleksiyonları</div>'
            + '<h1>Orpay Banyo Dolabı Koleksiyonları</h1>'
            + '<p class="banyo-vitrin-hero__aciklama">Hermes’ten Lugano Plus’a kadar tüm Orpay banyo dolabı serilerini tek ekranda inceleyin. Bu görünüm, Blazor açılamayan tarayıcılarda katalog gezinmesini canlı tutar.</p>'
            + '</div>'
            + '<div class="banyo-vitrin-hero__ozetler">'
            + '<article class="banyo-vitrin-ozet-kart"><span>Koleksiyon</span><strong>' + koleksiyonlar.length + '</strong></article>'
            + '<article class="banyo-vitrin-ozet-kart"><span>Model</span><strong>' + urunler.length + '</strong></article>'
            + '<article class="banyo-vitrin-ozet-kart"><span>Öne Çıkan</span><strong>' + toplamOneCikan + '</strong></article>'
            + '</div></section>';

        koleksiyonlar.forEach(function (koleksiyon) {
            html += '<section class="urun-koleksiyon-grubu">'
                + '<div class="wasm-koleksiyon-baslik">'
                + '<div><h2>' + htmlKodla(koleksiyon) + '</h2><p>' + htmlKodla(koleksiyonAciklamalari[koleksiyon] || "") + '</p></div>'
                + '<div class="wasm-koleksiyon-sayisi">' + gruplar[koleksiyon].length + ' model</div>'
                + '</div>'
                + '<div class="wasm-kart-grid">'
                + gruplar[koleksiyon].map(listeKartHtml).join("")
                + '</div></section>';
        });

        html += kabukBitisi();
        return html;
    }

    function detaySayfasiHtml(slug) {
        var urun = urunler.find(function (kayit) { return kayit.slug === slug; });

        if (!urun) {
            return kabukBaslangici()
                + '<div class="wasm-bos-durum"><h1>Banyo dolabı bulunamadı</h1><p>İstediğiniz ürün fallback kataloğunda yer almıyor.</p><p><a class="wasm-buton" href="/banyo-dolaplari">Koleksiyona dön</a></p></div>'
                + kabukBitisi();
        }

        var rozetler = ['<span>' + htmlKodla(urun.koleksiyon) + ' Koleksiyonu</span>'];
        if (urun.oneCikanMi) {
            rozetler.push("<span>Öne Çıkan Seri</span>");
        }
        if (urun.yeniMi) {
            rozetler.push("<span>2026 Sunumu</span>");
        }

        var teknikKartlar = urun.olculer.map(function (olcu) {
            return '<div class="wasm-olcu-kart"><strong>' + htmlKodla(olcu.baslik) + '</strong>'
                + '<div>Yükseklik: ' + htmlKodla(olcu.yukseklik) + '</div>'
                + '<div>Genişlik: ' + htmlKodla(olcu.genislik) + '</div>'
                + '<div>Derinlik: ' + htmlKodla(olcu.derinlik) + '</div></div>';
        }).join("");

        var fiyatAlani = '<div class="wasm-detay-fiyat">' + htmlKodla(fiyatYaz(urun.fiyat)) + '</div>';
        if (urun.boyDolabiFiyati) {
            fiyatAlani += '<div class="wasm-detay-metin">Boy dolabı: ' + htmlKodla(fiyatYaz(urun.boyDolabiFiyati)) + '</div>';
        }

        return kabukBaslangici()
            + '<a class="wasm-geri-link" href="/banyo-dolaplari">← Koleksiyona geri dön</a>'
            + '<div class="wasm-detay-grid">'
            + '<div class="wasm-detay-medya"><img src="' + gorselYolu(urun.sayfaNo, "spread") + '" alt="' + htmlKodla(urun.ad) + '" /></div>'
            + '<section class="wasm-detay-panel">'
            + '<div class="wasm-rozet-satiri">' + rozetler.join("") + '</div>'
            + '<div class="wasm-kart-ustbilgi"><span>' + htmlKodla(urun.kod) + '</span><span>Sayfa ' + urun.sayfaNo + '</span></div>'
            + '<h1 class="wasm-kart-baslik">' + htmlKodla(urun.ad) + '</h1>'
            + '<p class="wasm-detay-spot">' + htmlKodla(urun.ad + ", Orpay katalog düzeninden beslenen, showroom odaklı banyo dolabı sunumudur.") + '</p>'
            + fiyatAlani
            + '<div><strong>Renkler</strong><ul class="wasm-liste">' + urun.renkler.map(function (renk) { return "<li>" + htmlKodla(renk) + "</li>"; }).join("") + '</ul></div>'
            + '<div><strong>Öne çıkan özellikler</strong><ul class="wasm-liste">' + urun.ozellikler.map(function (ozellik) { return "<li>" + htmlKodla(ozellik) + "</li>"; }).join("") + '</ul></div>'
            + '<div><strong>Teknik ölçüler</strong><div class="wasm-olcu-tablosu">' + teknikKartlar + '</div></div>'
            + '<a class="wasm-buton" href="/iletisim">BANYO DOLABI İÇİN TEKLİF AL</a>'
            + '</section></div>'
            + kabukBitisi();
    }

    function fallbackGoster() {
        if (destekleniyor) {
            return;
        }

        var yol = window.location.pathname || "/";
        var uygulama = document.getElementById("app");
        var hataAlani = document.getElementById("blazor-error-ui");

        if (!uygulama) {
            return;
        }

        if (yol === "/banyo-dolaplari" || yol === "/urunler") {
            metaAyarla("Orpay Banyo Dolabı Koleksiyonları", "Orpay 2026 koleksiyonundaki banyo dolabı modellerini fallback katalog görünümünde inceleyin.");
            uygulama.innerHTML = listeSayfasiHtml();
        } else if (yol.indexOf("/banyo-dolabi/") === 0 || yol.indexOf("/urun/") === 0) {
            var slug = yol.split("/").filter(Boolean).pop();
            metaAyarla("Orpay Banyo Dolabı Detayı", "Orpay banyo dolabı detay görünümü.");
            uygulama.innerHTML = detaySayfasiHtml(slug);
        } else {
            return;
        }

        document.documentElement.setAttribute("data-wasm-fallback", "aktif");
        if (hataAlani) {
            hataAlani.style.display = "none";
        }
    }

    window.vizitlink3dWasmFallbackYukle = fallbackGoster;

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", fallbackGoster);
    } else {
        fallbackGoster();
    }
})();
