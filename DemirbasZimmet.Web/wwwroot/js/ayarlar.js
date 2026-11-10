
document.addEventListener("DOMContentLoaded", () => {

    const kayitSayisi = document.getElementById("kayitSayisi");
    const bildirimSesi = document.getElementById("bildirimSesi");
    const kaydetBtn = document.getElementById("kaydetBtn");
    const cikisBtn = document.getElementById("cikisBtn");

    // Giriş yapan kullanıcı bilgileri
    const kullanici = window.assetFlowUser;

    if (kullanici) {
        document.getElementById("kullaniciAdi").textContent =
            kullanici.adSoyad ||
            kullanici.kullaniciAdi ||
            kullanici.email ||
            "Kullanıcı";

        document.getElementById("kullaniciRol").textContent =
            kullanici.rol || "Belirtilmemiş";
    }

    // Kayıtlı ayarları yükle
    let kayitliAyarlar = {};

    try {
        kayitliAyarlar = JSON.parse(
            localStorage.getItem("assetFlowAyarlar") || "{}"
        );
    } catch (error) {
        console.error("Ayarlar okunamadı:", error);
    }

    // Eski koyu tema tercihini temizle
    delete kayitliAyarlar.koyuTema;

    localStorage.setItem(
        "assetFlowAyarlar",
        JSON.stringify(kayitliAyarlar)
    );

    // Kayıtlı tercihleri kontrollere aktar
    if (kayitliAyarlar.kayitSayisi) {
        kayitSayisi.value = kayitliAyarlar.kayitSayisi;
    }

    bildirimSesi.checked = kayitliAyarlar.bildirimSesi === true;

    // Ayarları kaydet
    kaydetBtn.addEventListener("click", () => {

        const ayarlar = {
            kayitSayisi: kayitSayisi.value,
            bildirimSesi: bildirimSesi.checked
        };

        localStorage.setItem(
            "assetFlowAyarlar",
            JSON.stringify(ayarlar)
        );

        if (typeof bildirimGoster === "function") {
            bildirimGoster(
                "Başarılı",
                "Ayarlarınız kaydedildi.",
                "success"
            );
        } else {
            alert("Ayarlarınız başarıyla kaydedildi.");
        }
    });

    // Güvenli çıkış
    cikisBtn.addEventListener("click", () => {

        const onay = confirm(
            "Hesabınızdan çıkış yapmak istediğinize emin misiniz?"
        );

        if (onay) {
            if (typeof window.assetFlowLogout === "function") {
                window.assetFlowLogout();
            } else {
                sessionStorage.removeItem("assetflow_token");
                sessionStorage.removeItem("assetflow_user");
                window.location.replace("login.html");
            }
        }
    });

    // Şifre değiştirme
    const sifreForm = document.getElementById("sifreForm");

    sifreForm.addEventListener("submit", async (e) => {
        e.preventDefault();

        const mevcutSifre =
            document.getElementById("mevcutSifre").value;

        const yeniSifre =
            document.getElementById("yeniSifre").value;

        const yeniSifreTekrar =
            document.getElementById("yeniSifreTekrar").value;

        const sifreMesaj =
            document.getElementById("sifreMesaj");

        sifreMesaj.innerHTML = "";

        if (yeniSifre.length < 8) {
            sifreMesaj.innerHTML =
                '<div class="alert alert-warning">Yeni şifre en az 8 karakter olmalıdır.</div>';
            return;
        }

        if (yeniSifre !== yeniSifreTekrar) {
            sifreMesaj.innerHTML =
                '<div class="alert alert-warning">Yeni şifreler eşleşmiyor.</div>';
            return;
        }

        try {
            const response = await fetch(
                `${window.ASSETFLOW_API_BASE_URL}/api/Hesap/sifre-degistir`,
                {
                    method: "PATCH",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify({
                        mevcutSifre: mevcutSifre,
                        yeniSifre: yeniSifre
                    })
                }
            );

            const sonuc = await response.json();

            if (!response.ok) {
                throw new Error(
                    sonuc.message ||
                    sonuc.mesaj ||
                    "Şifre değiştirilemedi."
                );
            }

            sifreMesaj.innerHTML =
                '<div class="alert alert-success">Şifreniz başarıyla değiştirildi.</div>';

            sifreForm.reset();

        } catch (error) {
            sifreMesaj.innerHTML =
                `<div class="alert alert-danger">${error.message}</div>`;
        }
    });

});
