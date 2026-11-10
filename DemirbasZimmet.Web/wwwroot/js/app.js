
// API adresimiz
const API_URL = "http://localhost:5235/api/Dashboard";

// Grafik nesnesini tekrar oluşturmayı önlemek için
let demirbasChart = null;

// Sayfadaki tarih bilgisini göster
const currentDateElement = document.getElementById("currentDate");

if (currentDateElement) {
    currentDateElement.textContent = new Date().toLocaleDateString("tr-TR", {
        day: "2-digit",
        month: "long",
        year: "numeric"
    });
}

// Tarih formatlama
function formatDate(dateValue) {
    if (!dateValue) return "-";

    return new Date(dateValue).toLocaleDateString("tr-TR", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric"
    });
}

// KPI kartlarını güncelle
function updateDashboard(data) {
    document.getElementById("toplamDemirbas").textContent =
        data.toplamDemirbas ?? 0;

    document.getElementById("kullanilabilirDemirbas").textContent =
        data.kullanilabilirDemirbas ?? 0;

    document.getElementById("zimmetliDemirbas").textContent =
        data.zimmetliDemirbas ?? 0;

    document.getElementById("aktifPersonel").textContent =
        data.aktifPersonel ?? 0;

    document.getElementById("arizaliDemirbas").textContent =
        data.arizaliDemirbas ?? 0;

    document.getElementById("hurdaDemirbas").textContent =
        data.hurdaDemirbas ?? 0;

    document.getElementById("devamEdenBakim").textContent =
        data.devamEdenBakim ?? 0;

    // Alt özet kartları
    document.getElementById("zimmetOzetSayisi").textContent =
        data.zimmetliDemirbas ?? 0;

    document.getElementById("bosDemirbas").textContent =
        data.kullanilabilirDemirbas ?? 0;

    document.getElementById("bakimOzetSayisi").textContent =
        data.devamEdenBakim ?? 0;

    // Grafik ve tablo
    createDemirbasChart(data);
    updateRecentAssignments(data.sonZimmetler ?? []);
}

// Demirbaş durum grafiği
function createDemirbasChart(data) {
    const canvas = document.getElementById("demirbasGrafik");

    if (!canvas || typeof Chart === "undefined") {
        console.error("Grafik alanı veya Chart.js bulunamadı.");
        return;
    }

    if (demirbasChart) {
        demirbasChart.destroy();
    }

    demirbasChart = new Chart(canvas, {
        type: "doughnut",
        data: {
            labels: [
                "Kullanılabilir",
                "Zimmetli",
                "Arızalı",
                "Hurda"
            ],
            datasets: [{
                data: [
                    data.kullanilabilirDemirbas ?? 0,
                    data.zimmetliDemirbas ?? 0,
                    data.arizaliDemirbas ?? 0,
                    data.hurdaDemirbas ?? 0
                ],
                backgroundColor: [
                    "#22a477",
                    "#5269bd",
                    "#e6a23c",
                    "#e65b67"
                ],
                borderWidth: 0,
                hoverOffset: 5
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: "bottom"
                }
            }
        }
    });
}

// Son zimmet hareketlerini tabloya ekle

function updateRecentAssignments(assignments) {
    const tableBody = document.getElementById("sonZimmetler");

    if (!tableBody) return;

    tableBody.replaceChildren();

    if (assignments.length === 0) {
        const row = document.createElement("tr");
        const cell = document.createElement("td");

        cell.colSpan = 6;
        cell.textContent = "Henüz zimmet kaydı bulunmuyor.";
        cell.className = "text-center";

        row.appendChild(cell);
        tableBody.appendChild(row);
        return;
    }

    assignments.forEach(item => {
        const row = document.createElement("tr");

        const values = [
            item.personelAdi,
            item.demirbasAdi,
            item.demirbasKodu,
            formatDate(item.teslimTarihi),
            formatDate(item.iadeTarihi),
            item.durum
        ];

        values.forEach(value => {
            const cell = document.createElement("td");
            cell.textContent = value ?? "-";
            row.appendChild(cell);
        });

        tableBody.appendChild(row);
    });
}

// API'den Dashboard verilerini getir
async function loadDashboard() {
    try {
        const token = sessionStorage.getItem("assetflow_token");

        if (!token) {
            throw new Error("Oturum token'ı bulunamadı. Tekrar giriş yapın.");
        }

        const response = await fetch(API_URL, {
            method: "GET",
            headers: {
                "Accept": "application/json",
                "Authorization": `Bearer ${token}`
            }
        });

        console.log("Dashboard HTTP Status:", response.status);

        if (!response.ok) {
            const errorText = await response.text();

            throw new Error(
                `API hatası: ${response.status} - ${errorText}`
            );
        }

        const data = await response.json();

        console.log("Dashboard verileri:", data);

        updateDashboard(data);

        console.log("Dashboard verileri başarıyla yüklendi.");

    } catch (error) {
        console.error("Dashboard yükleme hatası:", error);

        const tableBody = document.getElementById("sonZimmetler");

        if (tableBody) {
            tableBody.replaceChildren();

            const row = document.createElement("tr");
            const cell = document.createElement("td");

            cell.colSpan = 6;
            cell.textContent = error.message;
            cell.className = "text-center text-danger";

            row.appendChild(cell);
            tableBody.appendChild(row);
        }
    }
}

loadDashboard();