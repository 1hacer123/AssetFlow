const API_BASE_URL = "http://localhost:5235";

const loginForm = document.getElementById("loginForm");
const loginButton = document.getElementById("loginButton");
const loginError = document.getElementById("loginError");

loginForm.addEventListener("submit", async function (event) {
    event.preventDefault();

    loginError.classList.add("d-none");
    loginButton.disabled = true;
    loginButton.textContent = "Giriş yapılıyor...";

    const eposta = document.getElementById("eposta").value.trim();
    const parola = document.getElementById("parola").value;

    try {
        const response = await fetch(`${API_BASE_URL}/api/Auth/giris`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Accept": "application/json"
            },
            body: JSON.stringify({
                eposta: eposta,
                parola: parola
            })
        });

        const data = await response.json();

        if (!response.ok) {
            throw new Error(data.mesaj || data.message || "E-posta veya parola hatalı.");
        }

        if (!data.token || !data.kullanici) {
            throw new Error("Sunucudan beklenen giriş bilgileri alınamadı.");
        }

        // JWT ve kullanıcı bilgilerini sakla.
        sessionStorage.setItem("assetflow_token", data.token);
        sessionStorage.setItem("assetflow_user", JSON.stringify(data.kullanici));

        // Dashboard'a yönlendir.
        window.location.replace("index.html");

    } catch (error) {
        loginError.textContent =
            error.message || "Sunucuya bağlanırken bir hata oluştu.";

        loginError.classList.remove("d-none");
    } finally {
        loginButton.disabled = false;
        loginButton.textContent = "Giriş Yap";
    }
});