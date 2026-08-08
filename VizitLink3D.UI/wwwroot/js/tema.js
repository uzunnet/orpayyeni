window.vizitlink3dTema = {
    temaYukle: function(s) { document.documentElement.setAttribute("data-tema-id", s || "orpay-gunduz"); },
    siteUygula: function(s) { document.documentElement.setAttribute("data-tema-id", s || "orpay-gunduz"); },
    adminTemaIzoleEt: function() { this.modUygula("koyu"); },
    adminModUygula: function(m) { this.modUygula(m || "acik"); },
    modUygula: function(mod) {
        localStorage.setItem("temaMod", mod);
        document.documentElement.setAttribute("data-tema-modu", mod);
        var r = document.documentElement;
        if (mod === "koyu") {
            r.classList.add("dark");
            r.style.colorScheme = "dark";
            r.style.setProperty("--tema-arkaplan","#0D0D0D");
            r.style.setProperty("--tema-arkaplan-2","#1A1A1A");
            r.style.setProperty("--tema-yuzey","#201f1f");
            r.style.setProperty("--tema-yuzey-hover","#2a2a2a");
            r.style.setProperty("--tema-metin","#e5e2e1");
            r.style.setProperty("--tema-metin-ikincil","#bccabc");
            r.style.setProperty("--tema-metin-soluk","#879487");
            r.style.setProperty("--tema-cizgi","rgba(255,255,255,0.10)");
            r.style.setProperty("--tema-cam-bg","rgba(26,26,26,0.7)");
            r.style.setProperty("--tema-cam-cizgi","rgba(255,255,255,0.1)");
            r.style.setProperty("--tema-kart-bg","#1A1A1A");
            r.style.setProperty("--tema-vurgu","#61de8a");
            r.style.setProperty("--tema-uyari","#e9c349");
        } else {
            r.classList.remove("dark");
            r.style.colorScheme = "light";
            r.style.setProperty("--tema-arkaplan","#f4fbf1");
            r.style.setProperty("--tema-arkaplan-2","#eff6ec");
            r.style.setProperty("--tema-yuzey","#e9f0e6");
            r.style.setProperty("--tema-yuzey-hover","#e3eae0");
            r.style.setProperty("--tema-metin","#191c19");
            r.style.setProperty("--tema-metin-ikincil","#414941");
            r.style.setProperty("--tema-metin-soluk","#727970");
            r.style.setProperty("--tema-cizgi","rgba(0,0,0,0.08)");
            r.style.setProperty("--tema-cam-bg","rgba(255,255,255,0.85)");
            r.style.setProperty("--tema-cam-cizgi","rgba(0,0,0,0.1)");
            r.style.setProperty("--tema-kart-bg","#ffffff");
            r.style.setProperty("--tema-vurgu","#27ae60");
            r.style.setProperty("--tema-uyari","#e9c349");
        }
    }
,
    uygula: function(birincil, vurgu, arkaPlan, yuzey, koyuTemaMi, slug) {
        var r = document.documentElement;
        if (slug) r.setAttribute("data-tema-id", slug);
        if (birincil) r.style.setProperty("--tema-ana", birincil);
        if (vurgu) r.style.setProperty("--tema-vurgu", vurgu);
        if (arkaPlan) r.style.setProperty("--tema-arkaplan", arkaPlan);
        if (yuzey) r.style.setProperty("--tema-yuzey", yuzey);
        this.modUygula(koyuTemaMi ? "koyu" : "acik");
    }
};
window.vizitlink3dDil = { htmlDiliniAyarla: function(d) { document.documentElement.setAttribute("lang", d || "tr"); } };
document.addEventListener("DOMContentLoaded", function() {
    localStorage.setItem("temaMod", "koyu");
    window.vizitlink3dTema.modUygula("koyu");
    window.vizitlink3dTema.siteUygula("orpay-luxe-industrial");
});
document.addEventListener('DOMContentLoaded', function() {
    var reveals = document.querySelectorAll('.orpay-reveal');
    if (reveals.length === 0) return;
    
    var observer = new IntersectionObserver(function(entries) {
        entries.forEach(function(entry) {
            if (entry.isIntersecting) {
                entry.target.classList.add('aktif');
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.1, rootMargin: '0px 0px -50px 0px' });
    
    reveals.forEach(function(el) { observer.observe(el); });
});
