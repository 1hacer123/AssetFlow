const BAKIM_API =
    `${window.ASSETFLOW_API_BASE_URL}/api/Bakim`;

const DEMIRBAS_API =
    `${window.ASSETFLOW_API_BASE_URL}/api/Demirbas`;

let bakimKayitlari = [];
let demirbaslar = [];

const ekleModal = new bootstrap.Modal(
    document.getElementById("bakimEkleModal")
);

const tamamlaModal = new bootstrap.Modal(
    document.getElementById("bakimTamamlaModal")
);

async function apiIstek(url, options = {}) {
    const response = await fetch(url, {
        ...options,
        headers: {
            "Content-Type": "application/json",
            ...options.headers
        }
    });

    const text = await response.text();

    let data = null;

    try {
        data = text ? JSON.parse(text) : null;
    } catch {
        data = text;
    }

    if (!response.ok) {
        const mesaj = typeof data === "string"
            ? data
            : data?.message || data?.title || "İşlem gerçekleştirilemedi.";

        throw new Error(mesaj);
    }

    return data;
}

document.addEventListener("DOMContentLoaded", async () => {
    await verileriYukle();

    document.getElementById("bakimEkleForm")
        .addEventListener("submit", bakimOlustur);

    document.getElementById("bakimTamamlaForm")
        .addEventListener("submit", bakimTamamla);
});

async function verileriYukle() {
    try {
        const [bakimSonuc, demirbasSonuc] = await Promise.all([
            apiIstek(BAKIM_API),
            apiIstek(DEMIRBAS_API)
        ]);

        bakimKayitlari = Array.isArray(bakimSonuc) ? bakimSonuc : [];
        demirbaslar = Array.isArray(demirbasSonuc) ? demirbasSonuc : [];

        istatistikleriGuncelle();
        bakimlariFiltrele();
        demirbasSecenekleriniDoldur();

    } catch (error) {
        console.error(error);

        document.getElementById("bakimTablosu").innerHTML = `
            <tr>
                <td colspan="8" class="text-center text-danger py-4">
                    Bakım kayıtları yüklenemedi: ${htmlGuvenli(error.message)}
                </td>
            </tr>`;
    }
}

function istatistikleriGuncelle() {
    const tamamlanan = bakimKayitlari.filter(
        b => b.durum === "Tamamlandi"
    ).length;

    const devamEden = bakimKayitlari.length - tamamlanan;

    document.getElementById("toplamBakim").textContent =
        bakimKayitlari.length;

    document.getElementById("devamEdenBakim").textContent =
        devamEden;

    document.getElementById("tamamlananBakim").textContent =
        tamamlanan;
}

function bakimlariFiltrele() {
    const arama = document.getElementById("bakimArama")
        .value.toLocaleLowerCase("tr-TR");

    const durum = document.getElementById("bakimDurumFiltre").value;

    const filtrelenmis = bakimKayitlari.filter(b => {
        const metin = [
            b.demirbasAdi,
            b.demirbasKodu,
            b.arizaAciklamasi,
            b.bakimiYapan
        ].join(" ").toLocaleLowerCase("tr-TR");

        const aramaUygun = metin.includes(arama);
        const durumUygun = !durum || b.durum === durum;

        return aramaUygun && durumUygun;
    });

    tabloyuOlustur(filtrelenmis);
}

function tabloyuOlustur(kayitlar) {
    const tbody = document.getElementById("bakimTablosu");

    if (!kayitlar.length) {
        tbody.innerHTML = `
            <tr>
                <td colspan="8" class="text-center text-muted py-4">
                    Gösterilecek bakım kaydı bulunamadı.
                </td>
            </tr>`;
        return;
    }

    tbody.innerHTML = kayitlar.map(b => {
        const tamamlandi = b.durum === "Tamamlandi";

        return `
            <tr>
                <td><strong>#${b.bakimKaydiId}</strong></td>

                <td>
                    <div class="maintenance-asset-name">
                        ${htmlGuvenli(b.demirbasAdi)}
                    </div>
                    <small class="text-muted">
                        ${htmlGuvenli(b.demirbasKodu)}
                    </small>
                </td>

                <td class="maintenance-description">
                    ${htmlGuvenli(b.arizaAciklamasi)}
                </td>

                <td>${tarihFormatla(b.baslangicTarihi)}</td>

                <td>
                    ${b.bitisTarihi
                ? tarihFormatla(b.bitisTarihi)
                : '<span class="text-muted">-</span>'}
                </td>

                <td>${paraFormatla(b.maliyet)}</td>

                <td>
                    <span class="maintenance-status ${tamamlandi ? "completed" : "pending"}">
                        ${tamamlandi ? "Tamamlandı" : "Devam Ediyor"}
                    </span>
                </td>

                <td>
                    ${!tamamlandi
                ? `<button class="btn btn-sm btn-outline-success"
                            onclick="tamamlamaFormuAc(${b.bakimKaydiId})">
                            <i class="bi bi-check-lg"></i> Tamamla
                           </button>`
                : `<span class="text-muted small">
                            <i class="bi bi-check-circle"></i> Tamamlandı
                           </span>`
            }
                </td>
            </tr>`;
    }).join("");
}

function demirbasSecenekleriniDoldur() {
    const select = document.getElementById("bakimDemirbasId");

    // Durum 3 = Arızalı
    const arizaliDemirbaslar = demirbaslar.filter(d => d.durum === 3);

    select.innerHTML = '<option value="">Demirbaş seçin</option>';

    arizaliDemirbaslar.forEach(d => {
        select.innerHTML += `
            <option value="${d.demirbasId}">
                ${htmlGuvenli(d.demirbasAdi || d.ad)}
                (${htmlGuvenli(d.demirbasKodu)})
            </option>`;
    });

    if (!arizaliDemirbaslar.length) {
        select.innerHTML = '<option value="">Arızalı demirbaş bulunamadı</option>';
    }
}

function yeniBakimFormu() {
    document.getElementById("bakimEkleForm").reset();
    demirbasSecenekleriniDoldur();
    ekleModal.show();
}

async function bakimOlustur(event) {
    event.preventDefault();

    const demirbasId = Number(
        document.getElementById("bakimDemirbasId").value
    );

    if (!demirbasId) {
        bildirimGoster("Lütfen bir demirbaş seçin.", false);
        return;
    }

    const veri = {
        demirbasId: demirbasId,
        arizaAciklamasi: document.getElementById("arizaAciklamasi").value.trim(),
        bakimiYapan: document.getElementById("bakimiYapan").value.trim(),
        maliyet: Number(document.getElementById("bakimMaliyet").value || 0)
    };

    try {
        await apiIstek(BAKIM_API, {
            method: "POST",
            body: JSON.stringify(veri)
        });

        ekleModal.hide();
        bildirimGoster("Bakım kaydı başarıyla oluşturuldu.");
        await verileriYukle();

    } catch (error) {
        bildirimGoster(error.message, false);
    }
}

function tamamlamaFormuAc(id) {
    const kayit = bakimKayitlari.find(
        b => b.bakimKaydiId === id
    );

    if (!kayit) return;

    document.getElementById("tamamlanacakBakimId").value = id;
    document.getElementById("yapilanIslem").value = "";
    document.getElementById("tamamlananMaliyet").value = kayit.maliyet ?? 0;
    document.getElementById("tamamlanmaSonrasiDurum").value = "1";

    tamamlaModal.show();
}

async function bakimTamamla(event) {
    event.preventDefault();

    const id = Number(
        document.getElementById("tamamlanacakBakimId").value
    );

    const veri = {
        yapilanIslem: document.getElementById("yapilanIslem").value.trim(),
        maliyet: Number(document.getElementById("tamamlananMaliyet").value),
        tamamlanmaSonrasiDurum: Number(
            document.getElementById("tamamlanmaSonrasiDurum").value
        )
    };

    try {
        await apiIstek(`${BAKIM_API}/${id}/tamamla`, {
            method: "PUT",
            body: JSON.stringify(veri)
        });

        tamamlaModal.hide();
        bildirimGoster("Bakım başarıyla tamamlandı.");
        await verileriYukle();

    } catch (error) {
        bildirimGoster(error.message, false);
    }
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

function paraFormatla(tutar) {
    return Number(tutar || 0).toLocaleString("tr-TR", {
        style: "currency",
        currency: "TRY"
    });
}

function htmlGuvenli(deger) {
    return String(deger ?? "").replace(/[&<>"']/g, karakter => ({
        "&": "&amp;",
        "<": "&lt;",
        ">": "&gt;",
        '"': "&quot;",
        "'": "&#039;"
    })[karakter]);
}

function bildirimGoster(mesaj, basarili = true) {
    const toastElement = document.getElementById("bakimToast");

    document.getElementById("bakimToastMesaj").textContent = mesaj;

    toastElement.classList.toggle("text-bg-success", basarili);
    toastElement.classList.toggle("text-bg-danger", !basarili);

    bootstrap.Toast.getOrCreateInstance(toastElement).show();
}

document.addEventListener("DOMContentLoaded", () => {
    const rolAlani = document.getElementById("bakimSayfaKullaniciRol");

    if (rolAlani && window.assetFlowUser) {
        rolAlani.textContent =
            window.assetFlowUser.rol === "Yonetici"
                ? "Yönetici"
                : "Bilgi İşlem";
    }
});
