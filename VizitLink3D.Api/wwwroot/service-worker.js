// Önceki PWA sürümlerinden kalan önbelleği bir defaya mahsus temizler.
self.addEventListener("install", () => self.skipWaiting());

self.addEventListener("activate", (olay) => {
    olay.waitUntil((async () => {
        await self.registration.unregister();

        const istemciler = await self.clients.matchAll({ type: "window" });
        await Promise.all(istemciler.map((istemci) => istemci.navigate(istemci.url)));
    })());
});
