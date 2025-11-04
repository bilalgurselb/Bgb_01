// === 🔹 Lookup.js — SINTAN CHEMICALS (v2) ===

// --- Ana yükleyici ---
async function loadLookups() {
    await Promise.all([
        loadCustomers(),
        loadSalesReps(),
        loadCurrencies(),
        loadUnits(),
        loadTransports(),
        loadPaymentTerms(),
        loadDeliveryTerms(),
        loadShipFrom(),
        loadPorts("portOfDelivery"),
        loadPorts("placeOfDelivery")
    ]);
}

// === 🔹 MÜŞTERİLER (API'den veya cache'ten) ===
async function loadCustomers() {
    const select = document.getElementById("customerSelect");
    if (!select) return;

    select.disabled = true;
    select.innerHTML = `<option>Loading...</option>`;

    try {
        const cacheKey = "sintan_customers";
        let customers = JSON.parse(localStorage.getItem(cacheKey));

        if (!customers) {
            const res = await fetch("/api/lookups/customers");
            customers = await res.json();
            localStorage.setItem(cacheKey, JSON.stringify(customers));
        }

        select.innerHTML = `<option value="">Select Customer...</option>`;
        customers.forEach(c => {
            const opt = document.createElement("option");
            opt.value = c.id;
            opt.textContent = `${c.name} (${c.city}, ${c.country})`;
            opt.dataset.city = c.city;
            opt.dataset.country = c.country;
            opt.dataset.phone = c.phone;
            select.appendChild(opt);
        });

        // müşteri bilgisi gösterimi
        select.addEventListener("change", function () {
            const option = this.selectedOptions[0];
            if (option && option.value) {
                document.getElementById("customerInfo").innerHTML = `
                    <small>
                        <b>City:</b> ${option.dataset.city || '-'} |
                        <b>Country:</b> ${option.dataset.country || '-'} |
                        <b>Phone:</b> ${option.dataset.phone || '-'}
                    </small>`;
            } else {
                document.getElementById("customerInfo").innerHTML = "";
            }
        });
    } catch (err) {
        console.error("❌ Müşteri listesi yüklenemedi:", err);
        select.innerHTML = `<option>Error loading</option>`;
    } finally {
        select.disabled = false;
    }
}

// === 🔹 SATIŞ TEMSİLCİLERİ ===
async function loadSalesReps() {
    const select = document.getElementById("salesRepSelect");
    if (!select) return;

    select.disabled = true;
    select.innerHTML = `<option>Loading...</option>`;

    try {
        const cacheKey = "sintan_salesreps";
        let reps = JSON.parse(localStorage.getItem(cacheKey));

        if (!reps) {
            const res = await fetch("/api/lookups/salesreps");
            reps = await res.json();
            localStorage.setItem(cacheKey, JSON.stringify(reps));
        }

        select.innerHTML = `<option value="">Select Representative...</option>`;
        reps.forEach(r => {
            const opt = document.createElement("option");
            opt.value = r.id;
            opt.textContent = r.name;
            select.appendChild(opt);
        });
    } catch (err) {
        console.error("❌ Satış temsilcileri yüklenemedi:", err);
        select.innerHTML = `<option>Error loading</option>`;
    } finally {
        select.disabled = false;
    }
}

// === 🔹 SHIP FROM (Sabit, İzmir / İstanbul) ===
function loadShipFrom() {
    const container = document.getElementById("shipFromContainer");
    if (!container) return;
    container.innerHTML = `
        <label class="form-label">Ship From:</label><br>
        <div class="form-check form-check-inline">
            <input class="form-check-input" type="radio" name="shipFrom" id="shipFromIzmir" value="Sintan Kimya İzmir" checked>
            <label class="form-check-label" for="shipFromIzmir">İzmir</label>
        </div>
        <div class="form-check form-check-inline">
            <input class="form-check-input" type="radio" name="shipFrom" id="shipFromIstanbul" value="Sintan Kimya İstanbul">
            <label class="form-check-label" for="shipFromIstanbul">İstanbul</label>
        </div>
    `;
}
// Product //
function loadProducts(select) {
    if (!select) return;
    select.innerHTML = `<option value="">Loading...</option>`;

    // örnek simülasyon (ileride SQL’den API’ye geçecek)
    const products = [
        { id: 1, name: "TURAX PH (25*40) P", net: 1000, gross: 1035 },
        { id: 2, name: "TURAX DLM (25*42) P", net: 1050, gross: 1085 },
        { id: 3, name: "TURAX LQ (IBC)", net: 1000, gross: 1040 }
    ];

    select.innerHTML = `<option value="">Seçiniz...</option>`;
    products.forEach(p => {
        const opt = document.createElement("option");
        opt.value = p.id;
        opt.textContent = p.name;
        opt.dataset.net = p.net;
        opt.dataset.gross = p.gross;
        select.appendChild(opt);
    });
}


// === 🔹 PARA BİRİMLERİ ===
function loadCurrencies() {
    const list = ["USD", "EURO", "RUBLE", "TL"];
    fillSelect("currency", list);
}

// === 🔹 ÖLÇÜ BİRİMLERİ ===
function loadUnits(selectId = null) {
    const list = ["pieces", "pallets", "IBC"];
    if (!target) fillSelect(selectId, list);
    else fillSelect("unit", list);
}

// === 🔹 TAŞIMA TÜRLERİ ===
function loadTransports() {
    const list = ["Seaway", "Truck", "Airway", "Railway"];
    fillSelect("transport", list);
}

// === 🔹 ÖDEME ŞARTLARI ===
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

// === 🔹 TESLİMAT ŞARTLARI ===
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

// === 🔹 LİMANLAR (ports.json’dan veya cache’ten) ===
async function loadPorts(selectId) {
    const select = document.getElementById(selectId);
    if (!select) return;

    select.disabled = true;
    select.innerHTML = `<option>Loading...</option>`;

    try {
        const cacheKey = "sintan_ports";
        let ports = JSON.parse(localStorage.getItem(cacheKey));

        if (!ports) {
            const response = await fetch("/js/ports.json");
            ports = await response.json();
            localStorage.setItem(cacheKey, JSON.stringify(ports));
        }

        select.innerHTML = `<option value="">Select Port...</option>`;
        ports.slice(0, 2000).forEach(port => {
            const opt = document.createElement("option");
            opt.value = port.code || port.name;
            opt.textContent = `${port.name} (${port.country})`;
            select.appendChild(opt);
        });
    } catch (err) {
        console.error("❌ Liman listesi yüklenemedi:", err);
        select.innerHTML = `<option>Error loading ports</option>`;
    } finally {
        select.disabled = false;
    }
}

// === 🔹 Genel Yardımcı ===
function fillSelect(id, list) {
    const select = document.getElementById(id);
    if (!select) return;
    select.innerHTML = `<option value="">Select...</option>`;
    list.forEach(x => {
        const opt = document.createElement("option");
        opt.value = x;
        opt.textContent = x;
        select.appendChild(opt);
    });
}
