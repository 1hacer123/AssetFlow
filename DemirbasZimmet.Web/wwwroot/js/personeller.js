const PERSONEL_API =
    `${window.ASSETFLOW_API_BASE_URL}/api/Personel`;

let personeller = [];

const tablo = document.getElementById("personelTablosu");
const aramaInput = document.getElementById("personelArama");
const personelForm = document.getElementById("personelForm");

const personelModalElement = document.getElementById("personelModal");
const personelModal = new bootstrap.Modal(personelModalElement);

// API isteği
async function apiIstegi(url, options = {}) {
    const response = await fetch(url, {
        ...options,
        headers: {
            "Content-Type": "application/json",
            ...options.headers
        }
    });

    // API cevabını önce metin olarak oku
    const responseText = await response.text();

    let data = null;

    // Cevap JSON ise JSON'a dönüştür
    if (responseText) {
        try {
            data = JSON.parse(responseText);
        } catch {
            // JSON değilse düz metin olarak sakla
            data = responseText;
        }
    }

    // HTTP hata kodu varsa anlamlı hata mesajı göster
    if (!response.ok) {
        const hataMesaji =
            typeof data === "string"
                ? data
                : data?.message || data?.title || "İşlem başarısız oldu.";

        throw new Error(hataMesaji);
    }

    return data;
}

// Personelleri getir
async function personelleriGetir() {
    try {
        tablo.innerHTML = `
            <tr>
                <td colspan="6" class="text-center py-4">
                    <div class="spinner-border spinner-border-sm"></div>
                    Personeller yükleniyor...
                </td>
            </tr>`;

        personeller = await apiIstegi(PERSONEL_API);

        ozetleriGuncelle();
        tabloyuOlustur(personeller);

    } catch (error) {
        console.error("Personel listeleme hatası:", error);

        tablo.innerHTML = `
            <tr>
                <td colspan="6" class="text-center text-danger py-4">
                    Personeller yüklenemedi. API bağlantısını kontrol edin.
                </td>
            </tr>`;
    }
}

// Özet kartlarını güncelle
function ozetleriGuncelle() {
    const toplam = personeller.length;
    const aktif = personeller.filter(p => p.aktifMi).length;
    const pasif = toplam - aktif;

    document.getElementById("toplamPersonel").textContent = toplam;
    document.getElementById("aktifPersonelSayisi").textContent = aktif;
    document.getElementById("pasifPersonelSayisi").textContent = pasif;
}

// Tabloyu oluştur
function tabloyuOlustur(liste) {
    tablo.replaceChildren();

    if (liste.length === 0) {
        const row = document.createElement("tr");
        const cell = document.createElement("td");

        cell.colSpan = 6;
        cell.className = "text-center py-4 text-muted";
        cell.textContent = "Aramanızla eşleşen personel bulunamadı.";

        row.appendChild(cell);
        tablo.appendChild(row);
    }

    liste.forEach(personel => {
        const row = document.createElement("tr");

        const adSoyad = `${personel.ad} ${personel.soyad}`;

        const alanlar = [
            personel.personelId,
            adSoyad,
            personel.departman,
            personel.eposta
        ];

        alanlar.forEach(deger => {
            const cell = document.createElement("td");
            cell.textContent = deger ?? "-";
            row.appendChild(cell);
        });

        // Durum
        const durumCell = document.createElement("td");
        const durumBadge = document.createElement("span");

        durumBadge.className = personel.aktifMi
            ? "status-badge status-active"
            : "status-badge status-passive";

        durumBadge.textContent = personel.aktifMi ? "Aktif" : "Pasif";

        durumCell.appendChild(durumBadge);
        row.appendChild(durumCell);

        // İşlemler
        const islemCell = document.createElement("td");

        const duzenleBtn = document.createElement("button");
        duzenleBtn.className = "btn btn-sm btn-outline-primary me-2";
        duzenleBtn.innerHTML = '<i class="bi bi-pencil"></i>';
        duzenleBtn.title = "Düzenle";
        duzenleBtn.addEventListener("click", () => personelDuzenle(personel));

        islemCell.appendChild(duzenleBtn);

        const durumBtn = document.createElement("button");
        durumBtn.className = "btn btn-sm btn-outline-danger";
        durumBtn.title = personel.aktifMi ? "Pasife Al" : "Aktifleştir";
        durumBtn.innerHTML = personel.aktifMi
            ? '<i class="bi bi-person-dash"></i>'
            : '<i class="bi bi-person-check"></i>';

        durumBtn.addEventListener("click", () => personelDurumDegistir(personel));

        islemCell.appendChild(durumBtn);
        row.appendChild(islemCell);

        tablo.appendChild(row);
    });

    document.getElementById("personelKayitBilgisi").textContent =
        `Toplam ${liste.length} kayıt gösteriliyor.`;
}

// Arama
aramaInput.addEventListener("input", function () {
    const arama = this.value.toLocaleLowerCase("tr-TR").trim();

    const filtrelenmis = personeller.filter(p => {
        const metin = `${p.ad} ${p.soyad} ${p.departman} ${p.eposta}`
            .toLocaleLowerCase("tr-TR");

        return metin.includes(arama);
    });

    tabloyuOlustur(filtrelenmis);
});

// Yeni personel formu
document.getElementById("yeniPersonelBtn").addEventListener("click", () => {
    personelForm.reset();

    document.getElementById("personelId").value = "";
    document.getElementById("modalBaslik").textContent = "Yeni Personel Ekle";
    document.getElementById("formHata").classList.add("d-none");
});

// Personel düzenle
function personelDuzenle(personel) {
    personelForm.reset();

    document.getElementById("personelId").value = personel.personelId;
    document.getElementById("ad").value = personel.ad;
    document.getElementById("soyad").value = personel.soyad;
    document.getElementById("departman").value = personel.departman;
    document.getElementById("eposta").value = personel.eposta;

    document.getElementById("modalBaslik").textContent = "Personel Düzenle";
    document.getElementById("formHata").classList.add("d-none");

    personelModal.show();
}

// Form gönderimi: ekleme veya güncelleme
personelForm.addEventListener("submit", async function (event) {
    event.preventDefault();

    const id = document.getElementById("personelId").value;

    const veri = {
        ad: document.getElementById("ad").value.trim(),
        soyad: document.getElementById("soyad").value.trim(),
        departman: document.getElementById("departman").value.trim(),
        eposta: document.getElementById("eposta").value.trim()
    };

    const kaydetBtn = document.getElementById("kaydetBtn");
    const hataAlani = document.getElementById("formHata");

    kaydetBtn.disabled = true;
    kaydetBtn.textContent = "Kaydediliyor...";
    hataAlani.classList.add("d-none");

    try {
        if (id) {
            await apiIstegi(`${PERSONEL_API}/${id}`, {
                method: "PUT",
                body: JSON.stringify(veri)
            });

            bildirimGoster("Personel bilgileri güncellendi.");
        } else {
            await apiIstegi(PERSONEL_API, {
                method: "POST",
                body: JSON.stringify(veri)
            });

            bildirimGoster("Yeni personel başarıyla eklendi.");
        }

        personelModal.hide();
        await personelleriGetir();

    } catch (error) {
        console.error(error);
        hataAlani.textContent = error.message;
        hataAlani.classList.remove("d-none");

    } finally {
        kaydetBtn.disabled = false;
        kaydetBtn.textContent = "Kaydet";
    }
});

// Personeli pasife alma / aktifleştirme
async function personelDurumDegistir(personel) {
    if (!personel.aktifMi) {
        bildirimGoster(
            "Pasif personeli aktifleştirme işlemi mevcut API'de desteklenmiyor.",
            "warning"
        );
        return;
    }

    const onay = confirm(
        `${personel.ad} ${personel.soyad} adlı personeli pasife almak istiyor musunuz?`
    );

    if (!onay) return;

    try {
        await apiIstegi(`${PERSONEL_API}/${personel.personelId}`, {
            method: "DELETE"
        });

        bildirimGoster("Personel pasife alındı.");
        await personelleriGetir();

    } catch (error) {
        console.error(error);
        bildirimGoster(error.message || "Personel pasife alınamadı.", "danger");
    }
}

// Bildirim
function bildirimGoster(mesaj, tur = "success") {
    const toastElement = document.getElementById("bildirimToast");
    const mesajElement = document.getElementById("bildirimMesaji");

    mesajElement.textContent = mesaj;

    toastElement.classList.remove("text-bg-success", "text-bg-danger", "text-bg-warning");
    toastElement.classList.add(`text-bg-${tur}`);

    bootstrap.Toast.getOrCreateInstance(toastElement).show();
}

// Sayfa açılışında listele
personelleriGetir();
