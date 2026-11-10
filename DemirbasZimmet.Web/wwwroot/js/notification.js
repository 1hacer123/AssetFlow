
const notificationApi = window.ASSETFLOW_API_BASE_URL;

document.addEventListener("DOMContentLoaded", () => {
    const notificationList = document.getElementById("notificationList");
    const notificationBadge = document.getElementById("notificationBadge");
    const notificationSummary = document.getElementById("notificationSummary");
    const markAllButton = document.getElementById("markAllNotificationsRead");

    const assetModalElement = document.getElementById("assetNotificationModal");
    const assetDetailBody = document.getElementById("assetNotificationDetailBody");
    const assetModalTitle = document.getElementById("assetNotificationTitle");

    if (!notificationList || !notificationBadge) return;

    let notifications = [];
    // Bildirim sesi için ses motoru.
    let audioContext = null;
    let ilkYukleme = true;

    // Kullanıcının ses tercihini kontrol et.
    function bildirimSesiAcikMi() {
        try {
            const ayarlar = JSON.parse(
                localStorage.getItem("assetFlowAyarlar") || "{}"
            );

            return ayarlar.bildirimSesi === true;
        } catch {
            return false;
        }
    }

    // Tarayıcının ses motorunu kullanıcı etkileşimiyle etkinleştir.
    document.addEventListener("click", async () => {
        try {
            const AudioContextClass =
                window.AudioContext || window.webkitAudioContext;

            if (!AudioContextClass) return;

            if (!audioContext) {
                audioContext = new AudioContextClass();
            }

            if (audioContext.state === "suspended") {
                await audioContext.resume();
            }
        } catch (error) {
            console.warn("Ses motoru etkinleştirilemedi:", error);
        }
    }, { once: true });

    // Kısa bildirim sesi oluştur.
    function bildirimSesiCal() {
        if (!bildirimSesiAcikMi()) return;
        if (!audioContext || audioContext.state !== "running") return;

        try {
            const oscillator = audioContext.createOscillator();
            const gain = audioContext.createGain();

            oscillator.type = "sine";
            oscillator.frequency.setValueAtTime(
                880,
                audioContext.currentTime
            );

            gain.gain.setValueAtTime(
                0.0001,
                audioContext.currentTime
            );

            gain.gain.exponentialRampToValueAtTime(
                0.15,
                audioContext.currentTime + 0.02
            );

            gain.gain.exponentialRampToValueAtTime(
                0.0001,
                audioContext.currentTime + 0.25
            );

            oscillator.connect(gain);
            gain.connect(audioContext.destination);

            oscillator.start();
            oscillator.stop(audioContext.currentTime + 0.26);
        } catch (error) {
            console.warn("Bildirim sesi çalınamadı:", error);
        }
    }

    const assetModal = assetModalElement && window.bootstrap
        ? new bootstrap.Modal(assetModalElement)
        : null;

    function formatNotificationDate(dateValue) {
        if (!dateValue) return "-";

        return new Date(dateValue).toLocaleString("tr-TR", {
            day: "2-digit",
            month: "2-digit",
            year: "numeric",
            hour: "2-digit",
            minute: "2-digit"
        });
    }

    function formatAssetDate(dateValue) {
        if (!dateValue) return "-";

        return new Date(dateValue).toLocaleDateString("tr-TR");
    }

    function formatPrice(price) {
        if (price === null || price === undefined) return "-";

        return Number(price).toLocaleString("tr-TR", {
            style: "currency",
            currency: "TRY"
        });
    }

    function getAssetStatus(status) {
        if (status === 1) return "Kullanılabilir";
        return status ?? "-";
    }

    function renderNotifications() {
        notificationList.replaceChildren();

        const unreadCount = notifications.filter(
            item => !item.okunduMu
        ).length;

        notificationBadge.textContent =
            unreadCount > 99 ? "99+" : unreadCount;

        notificationBadge.classList.toggle(
            "d-none",
            unreadCount === 0
        );

        if (notificationSummary) {
            notificationSummary.textContent =
                `${notifications.length} bildirim, ${unreadCount} okunmamış`;
        }

        if (notifications.length === 0) {
            const empty = document.createElement("div");
            empty.className = "notification-empty";
            empty.textContent = "Henüz bildiriminiz bulunmuyor.";
            notificationList.appendChild(empty);
            return;
        }

        notifications.forEach(item => {
            const button = document.createElement("button");
            button.type = "button";
            button.className = "notification-item";

            if (!item.okunduMu) {
                button.classList.add("unread");
            }

            button.dataset.id = item.bildirimId;

            const icon = document.createElement("span");
            icon.className = "notification-item-icon";

            const iconElement = document.createElement("i");

            if (item.tur === "Basari") {
                iconElement.className = "bi bi-check-circle";
            } else if (item.tur === "Hata") {
                iconElement.className = "bi bi-exclamation-circle";
            } else {
                iconElement.className = "bi bi-info-circle";
            }

            icon.appendChild(iconElement);

            const content = document.createElement("span");
            content.className = "notification-item-content";

            const title = document.createElement("strong");
            title.textContent = item.baslik || "Bildirim";

            const message = document.createElement("span");
            message.textContent = item.mesaj || "";

            const date = document.createElement("small");
            date.textContent = formatNotificationDate(
                item.olusturmaTarihi
            );

            content.append(title, message, date);
            button.append(icon, content);

            if (!item.okunduMu) {
                const dot = document.createElement("span");
                dot.className = "notification-unread-dot";
                button.appendChild(dot);
            }

            notificationList.appendChild(button);
        });
    }

    async function loadNotifications() {
        try {
            const response = await fetch(
                `${notificationApi}/api/Bildirim`,
                {
                    method: "GET",
                    headers: {
                        "Accept": "application/json"
                    }
                }
            );

            if (!response.ok) {
                throw new Error(`Bildirimler alınamadı: ${response.status}`);
            }

            const yeniBildirimler = await response.json();

            // İlk yüklemede eski bildirimler için ses çalma.
            if (!ilkYukleme) {
                const eskiBildirimIdleri = new Set(
                    notifications.map(item => String(item.bildirimId))
                );

                const yeniGelenBildirimVar = yeniBildirimler.some(
                    item =>
                        !eskiBildirimIdleri.has(String(item.bildirimId)) &&
                        !item.okunduMu
                );

                if (yeniGelenBildirimVar) {
                    bildirimSesiCal();
                }
            }

            notifications = yeniBildirimler;
            ilkYukleme = false;

            renderNotifications();

        } catch (error) {
            console.error("Bildirim yükleme hatası:", error);

            notificationList.replaceChildren();

            const errorMessage = document.createElement("div");
            errorMessage.className = "notification-empty text-danger";
            errorMessage.textContent = "Bildirimler yüklenemedi.";

            notificationList.appendChild(errorMessage);
        }
    }

    // Bildirimi okundu olarak işaretle.
    async function markNotificationAsRead(item) {
        if (item.okunduMu) return;

        const response = await fetch(
            `${notificationApi}/api/Bildirim/${item.bildirimId}/okundu`,
            {
                method: "PATCH"
            }
        );

        if (!response.ok) {
            throw new Error("Bildirim okundu olarak işaretlenemedi.");
        }

        item.okunduMu = true;
        renderNotifications();
    }

    // Demirbaş detaylarını modal içinde göster.
    function showAssetDetails(asset) {
        if (!assetModal || !assetDetailBody) {
            throw new Error("Demirbaş detay penceresi bulunamadı.");
        }

        assetDetailBody.replaceChildren();

        const details = [
            ["Demirbaş ID", asset.demirbasId],
            ["Demirbaş kodu", asset.demirbasKodu],
            ["Demirbaş adı", asset.ad],
            ["Kategori", asset.kategori],
            ["Marka", asset.marka],
            ["Model", asset.model],
            ["Seri numarası", asset.seriNo],
            ["Satın alma tarihi", formatAssetDate(asset.satinAlmaTarihi)],
            ["Satın alma fiyatı", formatPrice(asset.satinAlmaFiyati)],
            ["Durum", getAssetStatus(asset.durum)]
        ];

        details.forEach(([label, value]) => {
            const row = document.createElement("tr");

            const heading = document.createElement("th");
            heading.scope = "row";
            heading.textContent = label;

            const cell = document.createElement("td");
            cell.textContent =
                value === null || value === undefined || value === ""
                    ? "-"
                    : String(value);

            row.append(heading, cell);
            assetDetailBody.appendChild(row);
        });

        if (assetModalTitle) {
            assetModalTitle.textContent = asset.ad || "Demirbaş Detayları";
        }

        assetModal.show();
    }

    // İlgili kaydın detayını getir.
    async function openRelatedRecord(item) {
        console.log("Tıklanan bildirim:", item);
        console.log("İlgili kayıt türü:", item.ilgiliKayitTuru);
        console.log("İlgili kayıt ID:", item.ilgiliKayitId);
        if (!item.ilgiliKayitTuru || !item.ilgiliKayitId) {
            return;
        }

        if (item.ilgiliKayitTuru === "Demirbas") {
            const response = await fetch(
                `${notificationApi}/api/Demirbas/${item.ilgiliKayitId}`,
                {
                    method: "GET",
                    headers: {
                        "Accept": "application/json"
                    }
                }
            );

            if (!response.ok) {
                throw new Error(
                    response.status === 404
                        ? "İlgili demirbaş bulunamadı."
                        : "Demirbaş detayları alınamadı."
                );
            }

            const asset = await response.json();
            showAssetDetails(asset);
            return;
        }

        alert("Bu bildirim türü için detay ekranı henüz eklenmedi.");
    }

    // Bildirime tıklanınca okundu yap ve ilgili kaydı aç.
    notificationList.addEventListener("click", async event => {
        const button = event.target.closest(".notification-item");

        if (!button) return;

        const id = button.dataset.id;

        const selectedNotification = notifications.find(
            item => String(item.bildirimId) === String(id)
        );

        if (!selectedNotification) return;

        button.disabled = true;

        try {
            await markNotificationAsRead(selectedNotification);

            await openRelatedRecord(selectedNotification);

        } catch (error) {
            console.error("Bildirim detay hatası:", error);
            alert(error.message || "Bildirim detayı açılamadı.");
        } finally {
            button.disabled = false;
        }
    });

    // Tüm bildirimleri okundu olarak işaretle.
    if (markAllButton) {
        markAllButton.addEventListener("click", async () => {
            try {
                const response = await fetch(
                    `${notificationApi}/api/Bildirim/okundu-tumu`,
                    {
                        method: "PATCH"
                    }
                );

                if (!response.ok) {
                    throw new Error("Bildirimler güncellenemedi.");
                }

                notifications.forEach(item => {
                    item.okunduMu = true;
                });

                renderNotifications();

            } catch (error) {
                console.error(error);
                alert("Bildirimler güncellenemedi. Lütfen tekrar deneyin.");
            }
        });
    }

    loadNotifications();

    // Her 30 saniyede bir yeni bildirimleri kontrol et.
    setInterval(loadNotifications, 30000);
});
