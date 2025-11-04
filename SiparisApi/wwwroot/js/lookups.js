// === 🔹 Lookup.js — SINTAN CHEMICALS ===

// --- Tüm dropdown’ları yükler ---
async function loadLookups() {
    loadCurrencies();
    loadUnits();
    loadTransports();
    loadPaymentTerms();
    loadDeliveryTerms();
    loadPorts("portOfDelivery");
    loadPorts("placeOfDelivery");
}

// --- Para Birimleri ---
function loadCurrencies() {
    const list = ["USD", "EURO", "RUBLE", "TL"];
    fillSelect("currency", list);
}

// --- Ölçü Birimleri ---
function loadUnits(selectId = null) {
    const list = ["pieces", "pallets", "IBC"];
    if (selectId) fillSelect(selectId, list);
}

// --- Taşıma Türleri ---
function loadTransports() {
    const list = ["Seaway", "Truck", "Airway", "Railway"];
    fillSelect("transport", list);
}

// --- Ödeme Şartları ---
function loadPaymentTerms() {
    const list = [
        "%30 Cash in Advance +%70 Cash Against Documents",
        "%40 Cash in Advance +%60 Cash Against Documents",
        "%50 Cash in Advance +%50 Cash Against Documents",
        "%50 Cash in Advance +%50 Cash Against Goods",
        "Acceptance Credit",
        "Cash Against Documents",
        "Cash Against Goods",
        "Cash in Advance",
        "Free of Charge",
        "Letter of Credit",
        "Letter of Credit at Sight",
        "Letter of Credit Deferred"
    ];
    fillSelect("paymentTerm", list);
}

// --- Teslim Şartları ---
function loadDeliveryTerms() {
    const list = [
        "CFR - Cost and Freight",
        "CIF - Cost, Insurance and Freight",
        "CIP - Carriage and Insurance Paid To",
        "CPT - Carriage Paid To",
        "DAP - Delivered At Place",
        "DDP - Delivered Duty Paid",
        "EXW - Ex Works",
        "FAS - Free Alongside Ship",
        "FCA - Free Carrier",
        "FOB - Free On Board"
    ];
    fillSelect("deliveryTerm", list);
}

// --- Limanlar ---
async function loadPorts(selectId) {
    const select = document.getElementById(selectId);
    if (!select) return;

    select.disabled = true;
    select.innerHTML = `<option>Yükleniyor...</option>`;

    try {
        const response = await fetch("/js/ports.json");
        const ports = await response.json();

        select.innerHTML = `<option value="">Seçiniz...</option>`;
        ports.slice(0, 2000).forEach(port => {
            const opt = document.createElement("option");
            opt.value = port.code;
            opt.textContent = `${port.name} (${port.code})`;
            select.appendChild(opt);
        });
    } catch (err) {
        console.error("Liman listesi yüklenemedi:", err);
        select.innerHTML = `<option>Yüklenemedi</option>`;
    } finally {
        select.disabled = false;
    }
}

// --- Genel Yardımcı ---
function fillSelect(id, list) {
    const select = document.getElementById(id);
    if (!select) return;
    select.innerHTML = `<option value="">Seçiniz...</option>`;
    list.forEach(x => {
        const opt = document.createElement("option");
        opt.value = x;
        opt.textContent = x;
        select.appendChild(opt);
    });
}
