(function () {
    const sozlukler = {
        tr: {
            "Previous": "Onceki",
            "Previous Page": "Onceki Sayfa",
            "Next": "Sonraki",
            "Next Page": "Sonraki Sayfa",
            "Page": "Sayfa",
            "of": "/",
            "Zoom Out": "Uzaklastir",
            "Zoom In": "Yakinlastir",
            "Zoom": "Yakınlaştırma",
            "Automatic Zoom": "Otomatik Yakınlaştır",
            "Actual Size": "Gercek Boyut",
            "Page Fit": "Sayfaya Sigdir",
            "Page Width": "Sayfa Genisligi",
            "Print": "Yazdir",
            "Download": "Indir",
            "Save": "Kaydet",
            "Open File": "Dosya Ac",
            "Find": "Bul",
            "Find in Document": "Belgede Bul",
            "Tools": "Araclar",
            "More Information": "Daha Fazla Bilgi",
            "Document Properties": "Belge Ozellikleri",
            "Rotate Clockwise": "Saga Dondur",
            "Rotate Counterclockwise": "Sola Dondur",
            "Presentation Mode": "Sunum Modu",
            "Toggle Sidebar": "Yan Menuyu Ac/Kapat",
            "Show Thumbnails": "Kucuk Resimleri Goster",
            "Show Document Outline": "Belge Taslagini Goster",
            "Show Attachments": "Ekleri Goster",
            "Show Layers": "Katmanlari Goster",
            "Current Outline Item": "Mevcut Taslak Ogesi",
            "First Page": "Ilk Sayfa",
            "Last Page": "Son Sayfa",
            "Text Selection Tool": "Metin Secme Araci",
            "Hand Tool": "El Araci",
            "Vertical Scrolling": "Dikey Kaydirma",
            "Horizontal Scrolling": "Yatay Kaydirma",
            "Wrapped Scrolling": "Sarmal Kaydirma",
            "No Spreads": "Cift Sayfa Yok",
            "Odd Spreads": "Tek Sayfalar",
            "Even Spreads": "Cift Sayfalar",
            "Close": "Kapat",
            "Cancel": "Iptal",
            "OK": "Tamam"
        },
        en: {
            "Onceki": "Previous",
            "Onceki Sayfa": "Previous Page",
            "Sonraki": "Next",
            "Sonraki Sayfa": "Next Page",
            "Sayfa": "Page",
            "Uzaklastir": "Zoom Out",
            "Yakinlastir": "Zoom In",
            "Yakınlaştırma": "Zoom",
            "Otomatik Yakınlaştır": "Automatic Zoom",
            "Gercek Boyut": "Actual Size",
            "Sayfaya Sigdir": "Page Fit",
            "Sayfa Genisligi": "Page Width",
            "Yazdir": "Print",
            "Indir": "Download",
            "Kaydet": "Save",
            "Dosya Ac": "Open File",
            "Bul": "Find",
            "Belgede Bul": "Find in Document",
            "Araclar": "Tools",
            "Daha Fazla Bilgi": "More Information",
            "Belge Ozellikleri": "Document Properties",
            "Saga Dondur": "Rotate Clockwise",
            "Sola Dondur": "Rotate Counterclockwise",
            "Sunum Modu": "Presentation Mode",
            "Yan Menuyu Ac/Kapat": "Toggle Sidebar",
            "Kucuk Resimleri Goster": "Show Thumbnails",
            "Belge Taslagini Goster": "Show Document Outline",
            "Ekleri Goster": "Show Attachments",
            "Katmanlari Goster": "Show Layers",
            "Mevcut Taslak Ogesi": "Current Outline Item",
            "Ilk Sayfa": "First Page",
            "Son Sayfa": "Last Page",
            "Metin Secme Araci": "Text Selection Tool",
            "El Araci": "Hand Tool",
            "Dikey Kaydirma": "Vertical Scrolling",
            "Yatay Kaydirma": "Horizontal Scrolling",
            "Sarmal Kaydirma": "Wrapped Scrolling",
            "Cift Sayfa Yok": "No Spreads",
            "Tek Sayfalar": "Odd Spreads",
            "Cift Sayfalar": "Even Spreads",
            "Kapat": "Close",
            "Iptal": "Cancel",
            "Tamam": "OK"
        }
    };

    const l10nAnahtarlari = {
        tr: {
            previous: "Onceki Sayfa",
            next: "Sonraki Sayfa",
            page_label: "Sayfa",
            page_of_pages: "/",
            zoom_out: "Uzaklastir",
            zoom_in: "Yakinlastir",
            print: "Yazdir",
            download: "Indir",
            save: "Kaydet",
            open_file: "Dosya Ac",
            findbar: "Belgede Bul",
            tools: "Araclar",
            document_properties: "Belge Ozellikleri",
            presentation_mode: "Sunum Modu",
            first_page: "Ilk Sayfa",
            last_page: "Son Sayfa",
            page_rotate_cw: "Saga Dondur",
            page_rotate_ccw: "Sola Dondur",
            cursor_text_select_tool: "Metin Secme Araci",
            cursor_hand_tool: "El Araci",
            scroll_vertical: "Dikey Kaydirma",
            scroll_horizontal: "Yatay Kaydirma",
            scroll_wrapped: "Sarmal Kaydirma",
            spread_none: "Cift Sayfa Yok",
            spread_odd: "Tek Sayfalar",
            spread_even: "Cift Sayfalar"
        },
        en: {}
    };

    let gozlemci;
    const baslangicYakinlastirma = 0.9;

    function metniCevir(deger, dil) {
        if (!deger) {
            return deger;
        }

        const sozluk = sozlukler[dil] || sozlukler.en;
        const temiz = deger.trim();
        return sozluk[temiz] || deger;
    }

    function ogeyiCevir(oge, dil) {
        if (!(oge instanceof HTMLElement)) {
            return;
        }

        const l10nId = oge.getAttribute("data-l10n-id");
        const l10nMetin = l10nId ? l10nAnahtarlari[dil]?.[l10nId] : null;
        if (l10nMetin && oge.childElementCount === 0) {
            oge.textContent = l10nMetin;
        }

        for (const ozellik of ["title", "aria-label", "placeholder"]) {
            const deger = oge.getAttribute(ozellik);
            const ceviri = l10nMetin || metniCevir(deger, dil);
            if (deger && ceviri !== deger) {
                oge.setAttribute(ozellik, ceviri);
            }
        }

        if (oge.childElementCount === 0 && oge.textContent) {
            const ceviri = l10nMetin || metniCevir(oge.textContent, dil);
            if (ceviri !== oge.textContent) {
                oge.textContent = ceviri;
            }
        }
    }

    function uygula(dil) {
        const aktifDil = dil === "tr" ? "tr" : "en";
        const kok = document.querySelector(".pdf-gosterici-panel") || document.body;
        document.documentElement.lang = aktifDil;

        kok.querySelectorAll("[data-l10n-id], button, a, input, select, option, span, label, div[role='button']").forEach(oge => {
            ogeyiCevir(oge, aktifDil);
        });
    }

    function baslangicYakinlastirmasiniUygula() {
        const uygulama = window.PDFViewerApplication || window.pdfViewerApplication;
        if (uygulama?.pdfViewer) {
            uygulama.pdfViewer.currentScaleValue = String(baslangicYakinlastirma);
            return;
        }

        const secim = document.querySelector(".pdf-gosterici-panel #scaleSelect, .pdf-gosterici-panel select[title], .pdf-gosterici-panel select[aria-label]");
        if (secim instanceof HTMLSelectElement && Array.from(secim.options).some(option => option.value === String(baslangicYakinlastirma))) {
            secim.value = String(baslangicYakinlastirma);
            secim.dispatchEvent(new Event("change", { bubbles: true }));
        }
    }

    function davranisiUygula() {
        const panel = document.querySelector(".pdf-gosterici-panel");
        if (!(panel instanceof HTMLElement)) {
            return;
        }

        [300, 900, 1800].forEach(gecikme => {
            window.setTimeout(baslangicYakinlastirmasiniUygula, gecikme);
        });
    }

    window.VIZITLINK3DPdfDiliUygula = function (dil) {
        const aktifDil = dil === "tr" ? "tr" : "en";
        const kok = document.querySelector(".pdf-gosterici-panel") || document.body;

        if (gozlemci) {
            gozlemci.disconnect();
        }

        [0, 250, 750, 1500, 3000].forEach(gecikme => {
            window.setTimeout(() => uygula(aktifDil), gecikme);
        });

        davranisiUygula();

        gozlemci = new MutationObserver(() => uygula(aktifDil));
        gozlemci.observe(kok, {
            attributes: true,
            childList: true,
            subtree: true
        });
    };
})();
