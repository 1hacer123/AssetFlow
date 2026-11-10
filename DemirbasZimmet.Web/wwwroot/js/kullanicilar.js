
const KULLANICI_API_URL =
    `${window.ASSETFLOW_API_BASE_URL}/api/Kullanicilar`;

document.addEventListener("DOMContentLoaded", () => {
    // Bu sayfa yalnızca yöneticiye açıktır.
    if (window.assetFlowUser?.rol !== "Yonetici") {
        window.location.replace("index.html");
        return;
    }

    kullanicilariYukle();

    document.getElementById("kullaniciForm")
        .addEventListener("submit", kullaniciOlustur);

    document.getElementById("kullaniciTablosu")
        .addEventListener("click", tabloIslemi);

    document.getElementById("kullaniciTablosu")
        .addEventListener("change", rolDegistir);
});

// API isteği
async function kullaniciApiIstek(url, options = {}) {
    const response = await fetch(url, {
        ...options,
        headers: {
            "Content-Type": "application/json",
            "Accept": "application/json",
            ...(options.headers || {})
        }
    });

    if (!response.ok) {
        const hataMetni = await response.text();
        throw new Error(hataMetni || `API hatası: ${response.status}`);
    }

    if (response.status === 204) {
        return null;
    }

    return response.json();
}

// Kullanıcıları getir
async function kullanicilariYukle() {
    const tablo = document.getElementById("kullaniciTablosu");

    try {
        tablo.innerHTML = `
            <tr>
                <td colspan="7" class="text-center py-4">
                    Kullanıcılar yükleniyor...
                </td>
            </tr>`;

        const kullanicilar = await kullaniciApiIstek(KULLANICI_API_URL);

        document.getElementById("toplamKullanici").textContent =
            kullanicilar.length;

        document.getElementById("aktifKullanici").textContent =
            kullanicilar.filter(x => x.aktifMi).length;

        document.getElementById("pasifKullanici").textContent =
            kullanicilar.filter(x => !x.aktifMi).length;

        tablo.innerHTML = "";

        if (kullanicilar.length === 0) {
            tablo.innerHTML = `
                <tr>
                    <td colspan="7" class="text-center text-muted py-4">
                        Henüz kayıtlı kullanıcı bulunmuyor.
                    </td>
                </tr>`;
            return;
        }

        kullanicilar.forEach(kullanici => {
            const kendiHesabim =
                kullanici.kullaniciId === window.assetFlowUser?.kullaniciId;

            const rolMetni =
                kullanici.rol === "Yonetici" ? "Yönetici" : "Bilgi İşlem";

            const satir = document.createElement("tr");

            satir.innerHTML = `
                <td>${kullanici.kullaniciId}</td>

                <td class="fw-semibold">
                    ${htmlGuvenli(kullanici.adSoyad)}
                    ${kendiHesabim
                    ? '<span class="badge bg-info text-dark ms-1">Siz</span>'
                    : ''}
                </td>

                <td>${htmlGuvenli(kullanici.eposta)}</td>

                <td>
                    <select class="form-select form-select-sm rol-secimi"
                            data-id="${kullanici.kullaniciId}"
                            ${kendiHesabim ? "disabled" : ""}>
                        <option value="BilgiIslem"
                            ${kullanici.rol === "BilgiIslem" ? "selected" : ""}>
                            Bilgi İşlem
                        </option>

                        <option value="Yonetici"
                            ${kullanici.rol === "Yonetici" ? "selected" : ""}>
                            Yönetici
                        </option>
                    </select>
                </td>

                <td>
                    <span class="badge ${kullanici.aktifMi
                    ? "bg-success"
                    : "bg-secondary"}">
                        ${kullanici.aktifMi ? "Aktif" : "Pasif"}
                    </span>
                </td>

                <td>${tarihFormatla(kullanici.olusturmaTarihi)}</td>

                <td>
                    <button class="btn btn-sm ${kullanici.aktifMi
                    ? "btn-outline-danger"
                    : "btn-outline-success"
                }"
                    data-islem="durum"
                    data-id="${kullanici.kullaniciId}"
                    data-aktif="${!kullanici.aktifMi}"
                    ${kendiHesabim ? "disabled" : ""}>
                        ${kullanici.aktifMi ? "Pasifleştir" : "Aktifleştir"}
                    </button>
                </td>
            `;

            tablo.appendChild(satir);
        });

    } catch (error) {
        console.error("Kullanıcılar yüklenemedi:", error);

        tablo.innerHTML = `
            <tr>
                <td colspan="7" class="text-center text-danger py-4">
                    Kullanıcılar yüklenemedi.
                    ${htmlGuvenli(error.message)}
                </td>
            </tr>`;
    }
}

// Yeni kullanıcı oluştur
async function kullaniciOlustur(event) {
    event.preventDefault();

    const hataAlani = document.getElementById("formHata");
    const kaydetButonu = document.getElementById("kullaniciKaydetBtn");

    hataAlani.classList.add("d-none");

    const kullanici = {
        adSoyad: document.getElementById("adSoyad").value.trim(),
        eposta: document.getElementById("eposta").value.trim(),
        parola: document.getElementById("parola").value,
        rol: document.getElementById("rol").value
    };

    if (kullanici.parola.length < 12) {
        hataAlani.textContent =
            "Parola en az 12 karakter olmalıdır.";
        hataAlani.classList.remove("d-none");
        return;
    }

    kaydetButonu.disabled = true;

    try {
        const sonuc = await kullaniciApiIstek(KULLANICI_API_URL, {
            method: "POST",
            body: JSON.stringify(kullanici)
        });

        bootstrap.Modal.getOrCreateInstance(
            document.getElementById("kullaniciEkleModal")
        ).hide();

        document.getElementById("kullaniciForm").reset();

        bildirimGoster(
            sonuc.mesaj || "Kullanıcı başarıyla oluşturuldu."
        );

        await kullanicilariYukle();

    } catch (error) {
        hataAlani.textContent = hataMesajiAl(error.message);
        hataAlani.classList.remove("d-none");

    } finally {
        kaydetButonu.disabled = false;
    }
}

// Aktif / pasif işlemi
async function tabloIslemi(event) {
    const buton = event.target.closest("button[data-islem='durum']");

    if (!buton) return;

    const id = buton.dataset.id;
    const aktifMi = buton.dataset.aktif === "true";

    const onay = confirm(
        aktifMi
            ? "Bu kullanıcıyı aktifleştirmek istiyor musunuz?"
            : "Bu kullanıcıyı pasifleştirmek istiyor musunuz?"
    );

    if (!onay) return;

    buton.disabled = true;

    try {
        const sonuc = await kullaniciApiIstek(
            `${KULLANICI_API_URL}/${id}/durum`,
            {
                method: "PATCH",
                body: JSON.stringify({ aktifMi })
            }
        );

        bildirimGoster(
            sonuc.mesaj || "Kullanıcı durumu güncellendi."
        );

        await kullanicilariYukle();

    } catch (error) {
        alert(hataMesajiAl(error.message));
        buton.disabled = false;
    }
}

// Rol değiştirme
async function rolDegistir(event) {
    const secim = event.target.closest(".rol-secimi");

    if (!secim) return;

    const id = secim.dataset.id;
    const yeniRol = secim.value;

    secim.disabled = true;

    try {
        const sonuc = await kullaniciApiIstek(
            `${KULLANICI_API_URL}/${id}/rol`,
            {
                method: "PATCH",
                body: JSON.stringify({ rol: yeniRol })
            }
        );

        bildirimGoster(
            sonuc.mesaj || "Kullanıcı rolü güncellendi."
        );

        await kullanicilariYukle();

    } catch (error) {
        alert(hataMesajiAl(error.message));
        await kullanicilariYukle();
    }
}

// XSS riskine karşı metinleri güvenli hale getir
function htmlGuvenli(deger) {
    return String(deger ?? "").replace(/[&<>"']/g, karakter => ({
        "&": "&amp;",
        "<": "&lt;",
        ">": "&gt;",
        '"': "&quot;",
        "'": "&#39;"
    })[karakter]);
}

// Tarih formatı
function tarihFormatla(tarih) {
    if (!tarih) return "-";

    return new Date(tarih).toLocaleString("tr-TR");
}

// API hata mesajını okunabilir hale getir
function hataMesajiAl(metin) {
    try {
        const veri = JSON.parse(metin);

        if (veri.mesaj) return veri.mesaj;
        if (veri.title) return veri.title;

        if (veri.errors) {
            return Object.values(veri.errors).flat().join("\n");
        }

        return metin;
    } catch {
        return metin;
    }
}

// Toast bildirimi
function bildirimGoster(mesaj) {
    document.getElementById("kullaniciToastMesaj").textContent = mesaj;

    const toast = bootstrap.Toast.getOrCreateInstance(
        document.getElementById("kullaniciToast")
    );

    toast.show();
}
