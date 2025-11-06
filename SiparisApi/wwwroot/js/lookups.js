// === 🔹 Lookup.js — SINTAN CHEMICALS (v3, DB + Cache + Fast) ===

// --- Ana yükleyici ---
async function loadLookups() {
    await Promise.all([
        loadCustomers(),
        loadSalesReps(),
        loadProducts(),
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

// === 🔹 MÜŞTERİLER (dbo.SintanCari) ===
async function loadCustomers() {
    const select = document.getElementById("customerSelect");
    if (!select) return;
    select.disabled = true;
    select.innerHTML = `<option>Loading customers...</option>`;

    try {
        const cacheKey = "sintan_customers_v3";
        let customers = JSON.parse(localStorage.getItem(cacheKey));

        if (!customers) {
            const res = await fetch("/api/orders/lookups/customers");
            if (!res.ok) throw new Error("Failed to load customers");
            customers = await res.json();
            localStorage.setItem(cacheKey, JSON.stringify(customers));
        }

        select.innerHTML = `<option value="">Select Customer...</option>`;
        customers.forEach(c => {
            const opt = document.createElement("option");
            opt.value = c.id;
            opt.textContent = `${c.name} (${c.city || "N/A"}, ${c.country || "-"})`;
            opt.dataset.city = c.city;
            opt.dataset.country = c.country;
            opt.dataset.phone = c.phone;
            select.appendChild(opt);
        });

        // Bilgi gösterimi
        select.addEventListener("change", function () {
            const o = this.selectedOptions[0];
            document.getElementById("customerInfo").innerHTML = o?.value
                ? `<small><b>City:</b> ${o.dataset.city || '-'} | <b>Country:</b> ${o.dataset.country || '-'} | <b>Phone:</b> ${o.dataset.phone || '-'}</small>`
                : "";
        });
    } catch (err) {
        console.error("❌ Customer list failed:", err);
        select.innerHTML = `<option>Error loading customers</option>`;
    } finally {
        select.disabled = false;
    }
}

// === 🔹 SATIŞ TEMSİLCİLERİ (AllowedUsers) ===
async function loadSalesReps() {
    const select = document.getElementById("salesRepSelect");
    if (!select) return;
    select.disabled = true;
    select.innerHTML = `<option>Loading sales reps...</option>`;

    try {
        const cacheKey = "sintan_salesreps_v3";
        let reps = JSON.parse(localStorage.getItem(cacheKey));

        if (!reps) {
            const res = await fetch("/api/orders/lookups/salesreps");
            if (!res.ok) throw new Error("Failed to load sales reps");
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
        console.error("❌ Sales reps failed:", err);
        select.innerHTML = `<option>Error loading reps</option>`;
    } finally {
        select.disabled = false;
    }
}

// === 🔹 STOKLAR (dbo.SintanStok) ===
async function loadProducts(selectElement = null) {
    const cacheKey = "sintan_products_v3";
    let products = JSON.parse(localStorage.getItem(cacheKey));

    try {
        if (!products) {
            const res = await fetch("/api/orders/lookups/products");
            if (!res.ok) throw new Error("Failed to load products");
            products = await res.json();
            localStorage.setItem(cacheKey, JSON.stringify(products));
        }

        // Eğer belirli select verildiyse onu doldur
        if (selectElement) {
            fillSelectElement(selectElement, products.map(p => ({ value: p.id, text: p.name })));
        } else {
            const allProductSelects = document.querySelectorAll(".product-select");
            allProductSelects.forEach(sel => fillSelectElement(sel, products.map(p => ({ value: p.id, text: p.name }))));
        }
    } catch (err) {
        console.error("❌ Products failed:", err);
        if (selectElement) selectElement.innerHTML = `<option>Error loading</option>`;
    }
}

// === 🔹 SHIP FROM (Sabit) ===
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

// === 🔹 PARA BİRİMLERİ ===
function loadCurrencies() {
    const list = ["USD", "EURO", "RUBLE", "TL"];
    fillSelect("currency", list);
}

// === 🔹 ÖLÇÜ BİRİMLERİ ===
function loadUnits(selectElement = null) {
    const list = ["Pallets", "Pieces", "IBC"];
    if (selectElement) fillSelectElement(selectElement, list.map(u => ({ value: u, text: u })));
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
        "%30 Cash in Advance + %70 CAD",
        "%40 Cash in Advance + %60 CAD",
        "%50 Cash in Advance + %50 CAD",
        "Cash Against Documents",
        "Letter of Credit at Sight",
        "Free of Charge"
    ];
    fillSelect("paymentTerm", list);
}

// === 🔹 TESLİMAT ŞARTLARI ===
function loadDeliveryTerms() {
    const list = [
        "CFR - Cost and Freight",
        "CIF - Cost, Insurance and Freight",
        "DAP - Delivered At Place",
        "FOB - Free On Board",
        "EXW - Ex Works"
    ];
    fillSelect("deliveryTerm", list);
}

// === 🔹 LİMANLAR (ports.json’dan veya cache’ten) ===
async function loadPorts(selectId) {
    const select = document.getElementById(selectId);
    if (!select) return;

    select.disabled = true;
    select.innerHTML = `<option>Loading ports...</option>`;

    try {
        const cacheKey = "sintan_ports_v3";
        let ports = JSON.parse(localStorage.getItem(cacheKey));

        if (!ports) {
            const res = await fetch("/js/ports.json");
            if (!res.ok) throw new Error("Ports not found");
            ports = await res.json();
            localStorage.setItem(cacheKey, JSON.stringify(ports));
        }

        select.innerHTML = `<option value="">Select Port...</option>`;
        ports.forEach(p => {
            const opt = document.createElement("option");
            opt.value = p.code || p.name;
            opt.textContent = `${p.name} (${p.country || '-'})`;
            select.appendChild(opt);
        });
    } catch (err) {
        console.error("❌ Ports failed:", err);
        select.innerHTML = `<option>Error loading ports</option>`;
    } finally {
        select.disabled = false;
    }
}

// === 🔹 Genel Yardımcılar ===
function fillSelect(id, list) {
    const select = document.getElementById(id);
    if (!select) return;
    select.innerHTML = `<option value="">Select...</option>`;
    list.forEach(x => {
        const opt = document.createElement("option");
        opt.value = typeof x === "string" ? x : x.value;
        opt.textContent = typeof x === "string" ? x : x.text;
        select.appendChild(opt);
    });
}

function fillSelectElement(select, list) {
    select.innerHTML = `<option value="">Select...</option>`;
    list.forEach(x => {
        const opt = document.createElement("option");
        opt.value = typeof x === "string" ? x : x.value;
        opt.textContent = typeof x === "string" ? x : x.text;
        select.appendChild(opt);
    });
}
