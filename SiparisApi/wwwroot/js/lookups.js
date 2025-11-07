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
        loadCities("placeOfDelivery")
    ]);
}

// === 🔹 MÜŞTERİLER (dbo.SintanCari) ===
async function loadCustomers() {
    const select = document.getElementById("customerSelect");
    if (!select) return;
    select.disabled = true;
    select.innerHTML = `<option>Loading customers...</option>`;

    try {
        const cacheKey = "sintan_customers_v4";
        let customers = JSON.parse(localStorage.getItem(cacheKey));

        // cache yoksa veya 24 saati geçtiyse API'den al
        const lastFetch = localStorage.getItem(cacheKey + "_time");
        const expired = !lastFetch || Date.now() - parseInt(lastFetch) > 86400000;

        if (!customers || expired) {
            const res = await fetch("/api/orders/lookups/customers");
            if (!res.ok) throw new Error("Failed to load customers");
            customers = await res.json();
            localStorage.setItem(cacheKey, JSON.stringify(customers));
            localStorage.setItem(cacheKey + "_time", Date.now().toString());
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

        // seçildiğinde müşteri bilgisi yazdır
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

// === 🔹 ÜRÜNLER (dbo.SintanStok) ===
async function loadProducts(selectElement = null) {
    const cacheKey = "sintan_products_v4";
    let products = JSON.parse(localStorage.getItem(cacheKey));

    const lastFetch = localStorage.getItem(cacheKey + "_time");
    const expired = !lastFetch || Date.now() - parseInt(lastFetch) > 86400000;

    try {
        if (!products || expired) {
            const res = await fetch("/api/orders/lookups/products");
            if (!res.ok) throw new Error("Failed to load products");
            products = await res.json();
            localStorage.setItem(cacheKey, JSON.stringify(products));
            localStorage.setItem(cacheKey + "_time", Date.now().toString());
        }

        const targetSelects = selectElement
            ? [selectElement]
            : document.querySelectorAll(".product-select");

        targetSelects.forEach(sel => {
            sel.innerHTML = `<option value="">Select Product...</option>`;
            products.forEach(p => {
                const opt = document.createElement("option");
                opt.value = p.id;
                opt.textContent = p.name;
                sel.appendChild(opt);
            });
        });
    } catch (err) {
        console.error("❌ Products failed:", err);
        if (selectElement)
            selectElement.innerHTML = `<option>Error loading products</option>`;
    }
}
// === 🔹 ÜRÜN DETAYLARI (AMBALAJ/PALET) ===
document.addEventListener("change", async (e) => {
    if (!e.target.classList.contains("product-select")) return;
    const select = e.target;
    const row = select.closest(".item-row");
    const id = select.value;
    if (!id) return;

    try {
        const res = await fetch(`/api/orders/lookups/productdetails/${id}`);
        if (!res.ok) throw new Error("Ürün bilgisi alınamadı.");
        const d = await res.json();

        row.dataset.packWeight = parseFloat(d.AMBALAJ_AGIRLIGI || 0);
        row.dataset.palletCount = parseFloat(d.PALET_AMBALAJ_ADEDI || 0);
        row.dataset.palletNet = parseFloat(d.PALET_NET_AGIRLIGI || 0);
        row.dataset.productName = d.STOK_ADI || "-";

        // Net Weight etiketi oluştur (Product altına)
        let lbl = row.querySelector(".net-weight-info");
        if (!lbl) {
            lbl = document.createElement("small");
            lbl.className = "text-muted net-weight-info d-block mt-1";
            select.insertAdjacentElement("afterend", lbl);
        }
        lbl.textContent = "";

        recalcRowTotal(row);
    } catch (err) {
        console.error("Ürün detay yüklenemedi:", err);
    }
});

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
    const select = document.getElementById("transport");
    if (select) select.value = "Seaway"; // 🔹 varsayılan seçili değer
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
        "CFR--Cost and Freight",
        "CIF--Cost, Insurance and Freigh",
        "CIP--Carriage and Insured Paid To",
        "CPT--Carriage Paid To",
        "DAP--Delivered At Place",
        "DDP--Delivered Duty Paid",
        "EXW--Ex Works",
        "FAS--Free Alongside Ship",
        "FCA--Free Carrier",
        "FOB--Free On Board"
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
            const res = await fetch("/js/ports.json", { cache: "no-store" });
            if (!res.ok) throw new Error("Ports not found");
            ports = await res.json();
            localStorage.setItem(cacheKey, JSON.stringify(ports));
        }

        select.innerHTML = `<option value="">Select Port...</option>`;
        ports.forEach(p => {
            const opt = document.createElement("option");
            opt.value = p.code || p.name;
            opt.textContent = `${p.name} (${p.country || "-"})`;
            select.appendChild(opt);
        });

        // olası tema/kontrast sorunlarına karşı
        select.style.color = "#000";
        select.style.backgroundColor = "#fff";
    } catch (err) {
        console.error("❌ Ports failed:", err);
        select.innerHTML = `<option>Error loading ports</option>`;
    } finally {
        select.disabled = false;
    }
}

// === 🔹 PLACE OF DELIVERY (SQL: dbo.SintanCari.IL) ===
async function loadCities(selectId = "placeOfDelivery") {
    const select = document.getElementById(selectId);
    if (!select) return;
    select.disabled = true;
    select.innerHTML = `<option>Loading cities...</option>`;

    try {
        const cacheKey = "sintan_cities_v4";
        let cities = JSON.parse(localStorage.getItem(cacheKey));

        const lastFetch = localStorage.getItem(cacheKey + "_time");
        const expired = !lastFetch || Date.now() - parseInt(lastFetch) > 86400000;

        if (!cities || expired) {
            const res = await fetch("/api/orders/lookups/cities");
            if (!res.ok) throw new Error("Cities not found");
            cities = await res.json();
            localStorage.setItem(cacheKey, JSON.stringify(cities));
            localStorage.setItem(cacheKey + "_time", Date.now().toString());
        }

        select.innerHTML = `<option value="">Select City...</option>`;
        cities.forEach(city => {
            const opt = document.createElement("option");
            opt.value = city;
            opt.textContent = city;
            select.appendChild(opt);
        });

        select.style.color = "#000";
        select.style.backgroundColor = "#fff";
    } catch (err) {
        console.error("❌ Cities failed:", err);
        select.innerHTML = `<option>Error loading cities</option>`;
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


