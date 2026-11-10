const DEMIRBAS_API =
    `${window.ASSETFLOW_API_BASE_URL}/api/Demirbas`;
let demirbasListesi = [];
let demirbasModal;
let toast;

// Sayfa açıldığında
document.addEventListener("DOMContentLoaded", () => {
    demirbasModal = new bootstrap.Modal(
        document.getElementById("demirbasModal")
    );

    toast = new bootstrap.Toast(
        document.getElementById("assetToast")
    );

    demirbaslariGetir();

    document.getElementById("demirbasArama")
        .addEventListener("input", tabloyuFiltrele);

    document.getElementById("durumFiltre")
        .addEventListener("change", tabloyuFiltrele);

    document.getElementById("demirbasForm")
        .addEventListener("submit", demirbasKaydet);
});


// API İSTEKLERİ
async function demirbasApiIstegi(url, options = {}) {
    const response = await fetch(url, {
        ...options,
        headers: {
            "Content-Type": "application/json",
            ...options.headers
        }
    });

    const responseText = await response.text();

    let data = null;

    if (responseText) {
        try {
            data = JSON.parse(responseText);
        } catch {
            data = responseText;
        }
    }

    if (!response.ok) {
        let hataMesaji = "İşlem gerçekleştirilemedi.";

        if (typeof data === "string") {
            hataMesaji = data;
        } else if (data?.errors) {
            hataMesaji = Object.values(data.errors)
                .flat()
                .join(" ");
        } else if (data?.message) {
            hataMesaji = data.message;
        } else if (data?.title) {
            hataMesaji = data.title;
        }

        throw new Error(hataMesaji);
    }

    return data;
}


// DEMİRBAŞLARI GETİR
async function demirbaslariGetir() {
    const tablo = document.getElementById("demirbasTablosu");

    tablo.innerHTML = `
        <tr>
            <td colspan="8" class="text-center py-4">
                <div class="spinner-border spinner-border-sm text-primary"></div>
                Demirbaşlar yükleniyor...
            </td>
        </tr>
    `;

    try {
        const data = await demirbasApiIstegi(DEMIRBAS_API);

        demirbasListesi = Array.isArray(data) ? data : [];

        istatistikleriGuncelle();
        tabloyuFiltrele();

    } catch (error) {
        tablo.innerHTML = `
            <tr>
                <td colspan="8" class="text-center text-danger py-4">
                    Demirbaşlar yüklenemedi: ${htmlGuvenli(error.message)}
                </td>
            </tr>
        `;

        bildirimGoster("Hata", error.message, true);
    }
}


// İSTATİSTİK KARTLARI
function istatistikleriGuncelle() {
    document.getElementById("toplamDemirbas").textContent =
        demirbasListesi.length;

    document.getElementById("kullanilabilirDemirbas").textContent =
        demirbasListesi.filter(d => durumDegeri(d.durum) === 1).length;

    document.getElementById("zimmetliDemirbas").textContent =
        demirbasListesi.filter(d => durumDegeri(d.durum) === 2).length;

    document.getElementById("arizaliDemirbas").textContent =
        demirbasListesi.filter(d => durumDegeri(d.durum) === 3).length;

    document.getElementById("hurdaDemirbas").textContent =
        demirbasListesi.filter(d => durumDegeri(d.durum) === 4).length;
}


// DURUM DEĞERİNİ SAYIYA ÇEVİR
function durumDegeri(durum) {
    if (typeof durum === "number") {
        return durum;
    }

    const degerler = {
        "Kullanilabilir": 1,
        "Kullanılabilir": 1,
        "Zimmetli": 2,
        "Arizali": 3,
        "Arızalı": 3,
        "Hurda": 4
    };

    return degerler[durum] ?? Number(durum);
}


// DURUM ADI
function durumAdi(durum) {
    const durumlar = {
        1: "Kullanılabilir",
        2: "Zimmetli",
        3: "Arızalı",
        4: "Hurda"
    };

    return durumlar[durumDegeri(durum)] || "Bilinmiyor";
}


// DURUM BADGE SINIFI
function durumSinifi(durum) {
    const siniflar = {
        1: "asset-status-available",
        2: "asset-status-assigned",
        3: "asset-status-faulty",
        4: "asset-status-scrap"
    };

    return siniflar[durumDegeri(durum)] || "";
}


// ARAMA VE FİLTRE
function tabloyuFiltrele() {
    const arama = document.getElementById("demirbasArama")
        .value.toLocaleLowerCase("tr-TR")
        .trim();

    const secilenDurum = document.getElementById("durumFiltre").value;

    const filtrelenmis = demirbasListesi.filter(d => {
        const aramaMetni = [
            d.demirbasKodu,
            d.ad,
            d.kategori,
            d.marka,
            d.model,
            d.seriNo
        ].filter(Boolean).join(" ").toLocaleLowerCase("tr-TR");

        const aramaUygun = aramaMetni.includes(arama);

        const durumUygun =
            secilenDurum === "" ||
            durumDegeri(d.durum) === Number(secilenDurum);

        return aramaUygun && durumUygun;
    });

    tabloyuOlustur(filtrelenmis);
}


// TABLOYU OLUŞTUR
function tabloyuOlustur(liste) {
    const tablo = document.getElementById("demirbasTablosu");

    if (liste.length === 0) {
        tablo.innerHTML = `
            <tr>
                <td colspan="8" class="text-center text-muted py-5">
                    <i class="bi bi-inbox fs-3 d-block mb-2"></i>
                    Gösterilecek demirbaş bulunamadı.
                </td>
            </tr>
        `;

        document.getElementById("demirbasKayitSayisi").textContent =
            "0 kayıt gösteriliyor";

        return;
    }

    tablo.innerHTML = liste.map(d => {
        const durum = durumDegeri(d.durum);

        const fiyat = d.satinAlmaFiyati != null
            ? Number(d.satinAlmaFiyati).toLocaleString("tr-TR", {
                style: "currency",
                currency: "TRY"
            })
            : "-";

        const markaModel = [d.marka, d.model]
            .filter(Boolean)
            .join(" / ") || "-";

        return `
            <tr>
                <td>${d.demirbasId}</td>

                <td>
                    <span class="asset-code">
                        ${htmlGuvenli(d.demirbasKodu)}
                    </span>
                </td>

                <td class="fw-semibold">
                    ${htmlGuvenli(d.ad)}
                </td>

                <td>${htmlGuvenli(d.kategori)}</td>

                <td>${htmlGuvenli(markaModel)}</td>

                <td>${fiyat}</td>

                <td>
                    <span class="asset-status ${durumSinifi(durum)}">
                        ${durumAdi(durum)}
                    </span>
                </td>

                <td>
                    <div class="d-flex gap-1">

                        <button class="btn btn-sm btn-outline-primary"
                                title="Düzenle"
                                onclick="demirbasDuzenle(${d.demirbasId})">
                            <i class="bi bi-pencil"></i>
                        </button>

                        <button class="btn btn-sm btn-outline-danger"
                                title="Sil"
                                onclick="demirbasSil(${d.demirbasId})">
                            <i class="bi bi-trash"></i>
                        </button>

                    </div>
                </td>
            </tr>
        `;
    }).join("");

    document.getElementById("demirbasKayitSayisi").textContent =
        `${liste.length} kayıt gösteriliyor`;
}


// YENİ DEMİRBAŞ FORMU
function yeniDemirbasFormu() {
    document.getElementById("demirbasForm").reset();

    document.getElementById("demirbasId").value = "";
    document.getElementById("modalBaslik").textContent =
        "Yeni Demirbaş Ekle";

    document.getElementById("durumAlanı").style.display = "none";
}


// DEMİRBAŞ DÜZENLE
async function demirbasDuzenle(id) {
    try {
        const d = await demirbasApiIstegi(`${DEMIRBAS_API}/${id}`);

        document.getElementById("demirbasId").value = d.demirbasId;
        document.getElementById("demirbasKodu").value = d.demirbasKodu ?? "";
        document.getElementById("demirbasAdi").value = d.ad ?? "";
        document.getElementById("kategori").value = d.kategori ?? "";
        document.getElementById("marka").value = d.marka ?? "";
        document.getElementById("model").value = d.model ?? "";
        document.getElementById("seriNo").value = d.seriNo ?? "";

        document.getElementById("satinAlmaTarihi").value =
            d.satinAlmaTarihi
                ? d.satinAlmaTarihi.substring(0, 10)
                : "";

        document.getElementById("satinAlmaFiyati").value =
            d.satinAlmaFiyati ?? "";

        const durum = durumDegeri(d.durum);

        const durumAlani = document.getElementById("durumAlanı");
        const durumSecimi = document.getElementById("demirbasDurumu");
        const durumBilgisi = document.getElementById("durumBilgisi");

        durumAlani.style.display = "block";

        if (durum === 2) {
            durumSecimi.disabled = true;
            durumSecimi.value = "1";

            durumBilgisi.textContent =
                "Bu demirbaş zimmetli. Durumu zimmet iade işlemi üzerinden değiştirilmelidir.";
        } else {
            durumSecimi.disabled = false;
            durumSecimi.value = String(durum);

            durumBilgisi.textContent =
                "Durum değişikliği ayrı bir API isteğiyle kaydedilir.";
        }

        document.getElementById("modalBaslik").textContent =
            "Demirbaş Bilgilerini Düzenle";

        demirbasModal.show();

    } catch (error) {
        bildirimGoster("Hata", error.message, true);
    }
}


// EKLEME VE GÜNCELLEME
async function demirbasKaydet(event) {
    event.preventDefault();

    const id = document.getElementById("demirbasId").value;

    const dto = {
        demirbasKodu: document.getElementById("demirbasKodu").value.trim(),
        ad: document.getElementById("demirbasAdi").value.trim(),
        kategori: document.getElementById("kategori").value.trim(),
        marka: document.getElementById("marka").value.trim(),
        model: document.getElementById("model").value.trim(),
        seriNo: document.getElementById("seriNo").value.trim(),
        satinAlmaTarihi:
            document.getElementById("satinAlmaTarihi").value || null,
        satinAlmaFiyati:
            document.getElementById("satinAlmaFiyati").value === ""
                ? null
                : Number(document.getElementById("satinAlmaFiyati").value)
    };

    const kaydetButonu = document.getElementById("kaydetButonu");
    const eskiButonMetni = kaydetButonu.innerHTML;

    kaydetButonu.disabled = true;
    kaydetButonu.textContent = "Kaydediliyor...";

    try {
        if (id) {
            // Önce demirbaş bilgilerini güncelle
            await demirbasApiIstegi(`${DEMIRBAS_API}/${id}`, {
                method: "PUT",
                body: JSON.stringify(dto)
            });

            // Durum değişmişse ayrı endpoint'e gönder
            const eskiDemirbas = demirbasListesi.find(
                d => d.demirbasId === Number(id)
            );

            const yeniDurum = Number(
                document.getElementById("demirbasDurumu").value
            );

            if (
                eskiDemirbas &&
                durumDegeri(eskiDemirbas.durum) !== 2 &&
                yeniDurum !== durumDegeri(eskiDemirbas.durum)
            ) {
                await demirbasApiIstegi(`${DEMIRBAS_API}/${id}/durum`, {
                    method: "PATCH",
                    body: JSON.stringify({
                        durum: yeniDurum
                    })
                });
            }

            bildirimGoster(
                "Başarılı",
                "Demirbaş bilgileri güncellendi."
            );

        } else {
            // Yeni demirbaş ekle
            await demirbasApiIstegi(DEMIRBAS_API, {
                method: "POST",
                body: JSON.stringify(dto)
            });

            bildirimGoster(
                "Başarılı",
                "Yeni demirbaş başarıyla eklendi."
            );
        }

        demirbasModal.hide();

        await demirbaslariGetir();

    } catch (error) {
        bildirimGoster("İşlem Başarısız", error.message, true);

    } finally {
        kaydetButonu.disabled = false;
        kaydetButonu.innerHTML = eskiButonMetni;
    }
}


// DEMİRBAŞ SİL
async function demirbasSil(id) {
    const demirbas = demirbasListesi.find(
        d => d.demirbasId === id
    );

    if (!demirbas) {
        bildirimGoster("Hata", "Demirbaş kaydı bulunamadı.", true);
        return;
    }

    const onay = confirm(
        `"${demirbas.ad}" demirbaşını silmek istediğinize emin misiniz?`
    );

    if (!onay) return;

    try {
        await demirbasApiIstegi(`${DEMIRBAS_API}/${id}`, {
            method: "DELETE"
        });

        bildirimGoster(
            "Başarılı",
            "Demirbaş kaydı silindi."
        );

        await demirbaslariGetir();

    } catch (error) {
        bildirimGoster("Silme Başarısız", error.message, true);
    }
}


// HTML GÜVENLİ METİN
function htmlGuvenli(deger) {
    return String(deger ?? "").replace(/[&<>"']/g, karakter => {
        const karakterler = {
            "&": "&amp;",
            "<": "&lt;",
            ">": "&gt;",
            '"': "&quot;",
            "'": "&#39;"
        };

        return karakterler[karakter];
    });
}


// BİLDİRİM GÖSTER
function bildirimGoster(baslik, mesaj, hata = false) {
    document.getElementById("toastBaslik").textContent = baslik;
    document.getElementById("toastMesaj").textContent = mesaj;

    const toastElement = document.getElementById("assetToast");

    toastElement.classList.remove("text-bg-danger", "text-bg-success");

    toastElement.classList.add(
        hata ? "text-bg-danger" : "text-bg-success"
    );

    toast.show();
}