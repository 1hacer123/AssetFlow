const ZIMMET_API =
    `${window.ASSETFLOW_API_BASE_URL}/api/Zimmet`;

const PERSONEL_API =
    `${window.ASSETFLOW_API_BASE_URL}/api/Personel`;

const DEMIRBAS_API =
    `${window.ASSETFLOW_API_BASE_URL}/api/Demirbas`;
let tumZimmetler = [];
let aktifZimmetler = [];
let personeller = [];
let demirbaslar = [];

const zimmetModal = new bootstrap.Modal(document.getElementById("zimmetModal"));
const iadeModal = new bootstrap.Modal(document.getElementById("iadeModal"));

async function apiIstegi(url, options = {}) {
    const response = await fetch(url, {
        ...options,
        headers: {
            "Content-Type": "application/json",
            ...options.headers
        }
    });

    const text = await response.text();

    let data = null;
    if (text) {
        try {
            data = JSON.parse(text);
        } catch {
            data = text;
        }
    }

    if (!response.ok) {
        throw new Error(
            typeof data === "string"
                ? data
                : data?.message || data?.title || `İşlem başarısız. HTTP ${response.status}`
        );
    }

    return data;
}

function tarihFormatla(tarih) {
    if (!tarih) return "-";

    return new Date(tarih).toLocaleString("tr-TR", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
        hour: "2-digit",
        minute: "2-digit"
    });
}

function bildirimGoster(mesaj, baslik = "AssetFlow") {
    document.getElementById("toastBaslik").textContent = baslik;
    document.getElementById("toastMesaj").textContent = mesaj;

    const toast = bootstrap.Toast.getOrCreateInstance(
        document.getElementById("zimmetToast")
    );

    toast.show();
}

async function verileriGetir() {
    try {
        const [zimmetSonuc, aktifSonuc, personelSonuc, demirbasSonuc] =
            await Promise.all([
                apiIstegi(ZIMMET_API),
                apiIstegi(`${ZIMMET_API}/aktif`),
                apiIstegi(PERSONEL_API),
                apiIstegi(DEMIRBAS_API)
            ]);

        tumZimmetler = Array.isArray(zimmetSonuc) ? zimmetSonuc : [];
        aktifZimmetler = Array.isArray(aktifSonuc) ? aktifSonuc : [];
        personeller = Array.isArray(personelSonuc) ? personelSonuc : [];
        demirbaslar = Array.isArray(demirbasSonuc) ? demirbasSonuc : [];

        istatistikleriGuncelle();
        tabloyuOlustur();
        personelSecenekleriniDoldur();
        demirbasSecenekleriniDoldur();

    } catch (error) {
        console.error("Zimmet verileri alınamadı:", error);

        document.getElementById("zimmetTablosu").innerHTML = `
            <tr>
                <td colspan="7" class="text-center text-danger py-4">
                    Veriler yüklenemedi: ${htmlGuvenli(error.message)}
                </td>
            </tr>`;

        bildirimGoster(error.message, "Yükleme Hatası");
    }
}

function istatistikleriGuncelle() {
    document.getElementById("toplamZimmet").textContent = tumZimmetler.length;
    document.getElementById("aktifZimmet").textContent = aktifZimmetler.length;

    const iadeSayisi = tumZimmetler.filter(
        z => z.durum === "İade Edildi"
    ).length;

    document.getElementById("iadeEdilen").textContent = iadeSayisi;
}

function tabloyuOlustur() {
    const tbody = document.getElementById("zimmetTablosu");
    const arama = document.getElementById("zimmetArama").value
        .toLocaleLowerCase("tr-TR");

    const filtre = document.getElementById("zimmetFiltre").value;

    const filtrelenmis = tumZimmetler.filter(z => {
        const aranacak = [
            z.personelAdSoyad,
            z.demirbasAdi,
            z.zimmetId
        ].join(" ").toLocaleLowerCase("tr-TR");

        const aramayaUygun = aranacak.includes(arama);
        const durumaUygun = !filtre || z.durum === filtre;

        return aramayaUygun && durumaUygun;
    });

    if (filtrelenmis.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="7" class="text-center py-4 text-muted">
                    Gösterilecek zimmet kaydı bulunamadı.
                </td>
            </tr>`;
    } else {
        tbody.innerHTML = filtrelenmis.map(z => {
            const aktif = z.durum !== "İade Edildi";

            return `
                <tr>
                    <td>${z.zimmetId}</td>
                    <td>${htmlGuvenli(z.personelAdSoyad)}</td>
                    <td>${htmlGuvenli(z.demirbasAdi)}</td>
                    <td>${tarihFormatla(z.teslimTarihi)}</td>
                    <td>${tarihFormatla(z.iadeTarihi)}</td>
                    <td>
                        <span class="status-badge ${aktif ? "status-active" : "status-passive"}">
                            ${htmlGuvenli(z.durum)}
                        </span>
                    </td>
                    <td>
                        ${aktif
                    ? `<button class="btn btn-sm btn-primary-custom"
                                     onclick="iadeFormuAc(${z.zimmetId})">
                                     <i class="bi bi-box-arrow-in-left"></i> İade Al
                                   </button>`
                    : `<span class="text-muted">Tamamlandı</span>`
                }
                    </td>
                </tr>`;
        }).join("");
    }

    document.getElementById("zimmetKayitSayisi").textContent =
        `${filtrelenmis.length} kayıt gösteriliyor`;
}

function personelSecenekleriniDoldur() {
    const select = document.getElementById("personelId");

    select.innerHTML = '<option value="">Personel seçin</option>';

    personeller.forEach(p => {
        if (p.aktifMi) {
            const adSoyad = `${p.ad} ${p.soyad}`;

            select.innerHTML += `
                <option value="${p.personelId}">
                    ${htmlGuvenli(adSoyad)}
                </option>`;
        }
    });
}

function demirbasSecenekleriniDoldur() {
    const select = document.getElementById("demirbasId");

    select.innerHTML = '<option value="">Demirbaş seçin</option>';

    demirbaslar.forEach(d => {
        if (d.durum === 1) {
            select.innerHTML += `
                <option value="${d.demirbasId}">
                    ${htmlGuvenli(d.demirbasKodu)} - ${htmlGuvenli(d.ad)}
                </option>`;
        }
    });
}

function yeniZimmetFormu() {
    document.getElementById("zimmetForm").reset();
    document.getElementById("teslimEden").value = "Bilgi İşlem";
    document.getElementById("teslimAlan").value = "";

    personelSecenekleriniDoldur();
    demirbasSecenekleriniDoldur();

    zimmetModal.show();
}

document.getElementById("personelId").addEventListener("change", function () {
    const secilen = personeller.find(
        p => p.personelId === Number(this.value)
    );

    document.getElementById("teslimAlan").value = secilen
        ? `${secilen.ad} ${secilen.soyad}`
        : "";
});

document.getElementById("zimmetForm").addEventListener("submit", async function (e) {
    e.preventDefault();

    const personelId = Number(document.getElementById("personelId").value);
    const demirbasId = Number(document.getElementById("demirbasId").value);

    const veri = {
        personelId: personelId,
        demirbasId: demirbasId,
        aciklama: document.getElementById("aciklama").value.trim(),
        teslimEden: document.getElementById("teslimEden").value.trim(),
        teslimAlan: document.getElementById("teslimAlan").value.trim()
    };

    if (!personelId || !demirbasId || !veri.teslimEden || !veri.teslimAlan) {
        bildirimGoster("Lütfen zorunlu alanları doldurun.", "Eksik Bilgi");
        return;
    }

    const buton = document.getElementById("zimmetKaydet");
    buton.disabled = true;

    try {
        await apiIstegi(ZIMMET_API, {
            method: "POST",
            body: JSON.stringify(veri)
        });

        zimmetModal.hide();
        bildirimGoster("Yeni zimmet başarıyla oluşturuldu.");
        await verileriGetir();

    } catch (error) {
        bildirimGoster(error.message, "Zimmet Oluşturulamadı");
    } finally {
        buton.disabled = false;
    }
});

function iadeFormuAc(id) {
    document.getElementById("iadeZimmetId").value = id;
    document.getElementById("iadeSonrasiDurum").value = "1";

    iadeModal.show();
}

document.getElementById("iadeForm").addEventListener("submit", async function (e) {
    e.preventDefault();

    const id = document.getElementById("iadeZimmetId").value;

    const veri = {
        iadeSonrasiDurum: Number(
            document.getElementById("iadeSonrasiDurum").value
        )
    };

    try {
        await apiIstegi(`${ZIMMET_API}/${id}/iade`, {
            method: "PUT",
            body: JSON.stringify(veri)
        });

        iadeModal.hide();
        bildirimGoster("Demirbaş iade işlemi tamamlandı.");
        await verileriGetir();

    } catch (error) {
        bildirimGoster(error.message, "İade İşlemi Başarısız");
    }
});

function htmlGuvenli(deger) {
    return String(deger ?? "").replace(/[&<>"']/g, karakter => ({
        "&": "&amp;",
        "<": "&lt;",
        ">": "&gt;",
        '"': "&quot;",
        "'": "&#039;"
    })[karakter]);
}

document.getElementById("zimmetArama").addEventListener("input", tabloyuOlustur);
document.getElementById("zimmetFiltre").addEventListener("change", tabloyuOlustur);

document.addEventListener("DOMContentLoaded", verileriGetir);