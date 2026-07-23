var SOM_APP_CACHE = "som-app-v1";
var SOM_APP_ASSETS = [
    "/SomApp",
    "/MeraKshetra",
    "/Content/css/som-app.css",
    "/Content/css/mera-kshetra.css",
    "/Content/images/logo.png"
];

self.addEventListener("install", function (event) {
    event.waitUntil(
        caches.open(SOM_APP_CACHE).then(function (cache) {
            return cache.addAll(SOM_APP_ASSETS);
        }).catch(function () { })
    );
    self.skipWaiting();
});

self.addEventListener("activate", function (event) {
    event.waitUntil(
        caches.keys().then(function (keys) {
            return Promise.all(keys.map(function (key) {
                if (key !== SOM_APP_CACHE) {
                    return caches.delete(key);
                }
                return null;
            }));
        })
    );
    self.clients.claim();
});

self.addEventListener("fetch", function (event) {
    if (event.request.method !== "GET") {
        return;
    }

    event.respondWith(
        fetch(event.request).catch(function () {
            return caches.match(event.request).then(function (response) {
                return response || caches.match("/SomApp");
            });
        })
    );
});
