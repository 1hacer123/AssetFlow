
const API_BASE_URL = "http://localhost:5235";

window.ASSETFLOW_API_BASE_URL = API_BASE_URL;

const token = sessionStorage.getItem("assetflow_token");
const userJson = sessionStorage.getItem("assetflow_user");

// Giriş yapılmamışsa login ekranına gönder.
if (!token || !userJson) {
    window.location.replace("login.html");
}

// Kullanıcı bilgilerini al.
let currentUser = null;

try {
    currentUser = JSON.parse(userJson);
} catch {
    sessionStorage.clear();
    window.location.replace("login.html");
}

// Global kullanıcı bilgisi.
window.assetFlowUser = currentUser;

// Yönetici kontrolü.
window.isAdmin = currentUser?.rol === "Yonetici";

// API isteklerine JWT ekle.
const originalFetch = window.fetch.bind(window);

window.fetch = async function (input, init = {}) {
    const requestUrl = typeof input === "string" ? input : input.url;
    const url = new URL(requestUrl, window.location.origin);

    if (url.origin === new URL(API_BASE_URL).origin) {
        const headers = new Headers(
            input instanceof Request ? input.headers : undefined
        );

        new Headers(init.headers || {}).forEach((value, key) => {
            headers.set(key, value);
        });

        headers.set("Authorization", `Bearer ${token}`);

        init = {
            ...init,
            headers
        };
    }

    const response = await originalFetch(input, init);

    if (
        url.origin === new URL(API_BASE_URL).origin &&
        response.status === 401
    ) {
        sessionStorage.removeItem("assetflow_token");
        sessionStorage.removeItem("assetflow_user");
        window.location.replace("login.html");
    }

    return response;
};

document.addEventListener("DOMContentLoaded", () => {
    const kullanici = window.assetFlowUser;

    if (!kullanici) return;

    const rolMetni = kullanici.rol === "Yonetici"
        ? "Yönetici"
        : "Bilgi İşlem";

    const adAlani = document.getElementById("oturumKullaniciAdi");
    if (adAlani) {
        adAlani.textContent = kullanici.adSoyad || "Kullanıcı";
    }

    const rolAlani = document.getElementById("oturumKullaniciRol");
    if (rolAlani) {
        rolAlani.textContent = rolMetni;
    }

    const avatarAlani = document.getElementById("oturumKullaniciAvatar");
    if (avatarAlani) {
        avatarAlani.textContent =
            (kullanici.adSoyad || "K").trim()
                .charAt(0).toLocaleUpperCase("tr-TR");
    }

    const rolAlanlari = [
        "bakimSayfaKullaniciRol",
        "demirbasSayfaKullaniciRol",
        "personelSayfaKullaniciRol",
        "zimmetSayfaKullaniciRol"
    ];

    rolAlanlari.forEach(id => {
        const alan = document.getElementById(id);
        if (alan) alan.textContent = rolMetni;
    });

    document.querySelectorAll('[data-role="Yonetici"]').forEach(menu => {
        menu.classList.toggle("d-none", kullanici.rol !== "Yonetici");
    });
});

// Çıkış işlemi.
window.assetFlowLogout = function () {
    sessionStorage.removeItem("assetflow_token");
    sessionStorage.removeItem("assetflow_user");
    window.location.replace("login.html");
};
