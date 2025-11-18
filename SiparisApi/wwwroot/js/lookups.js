// === 🔹 Lookup.js — SINTAN CHEMICALS (v3, DB + Cache + Fast) ===

// --- Ana yükleyici ---
async function loadLookups() {
    const token = localStorage.getItem("token");

    const headers = {
        "Authorization": `Bearer ${token}`
    };

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
        loadCities("placeOfDelivery"),
        preloadProducts()
    ]);
    
}

// === 🔹 MÜŞTERİLER (dbo.SintanCari - Autocomplete + Cache) ===
async function loadCustomers() {
    const select = document.getElementById("customerSelect");
    if (!select) return;
    select.disabled = true;
    select.innerHTML = `<option>Loading customers...</option>`;

    try {
        const cacheKey = "sintan_customers_v5";
        let customers = JSON.parse(localStorage.getItem(cacheKey));
        const lastFetch = localStorage.getItem(cacheKey + "_time");
        const expired = !lastFetch || Date.now() - parseInt(lastFetch) > 86400000;

        if (!customers || expired) {
            const res = await fetch("/api/orders/lookups/customers");
            if (!res.ok) throw new Error("Failed to load customers");
            customers = await res.json();
            localStorage.setItem(cacheKey, JSON.stringify(customers));
            localStorage.setItem(cacheKey + "_time", Date.now().toString());
        }

        // Normalize tüm olası alan isimlerine göre
        const norm = customers.map(c => ({
            id: c.id ?? c.CARI_KOD ?? "",
            name: c.name ?? c.CARI_ISIM ?? "",
            city: c.city ?? c.ILCE ?? c.Ilce ?? "",
            country: c.country ?? c.IL ?? c.Ulke ?? "",
            phone: c.phone ?? c.TELEFON ?? ""
        }));

        select.innerHTML = `<option value="">Select Customer...</option>`;
        norm.forEach(c => {
            const opt = document.createElement("option");
            opt.value = c.id;
            opt.textContent = `${c.name} (${c.city || "N/A"}, ${c.country || "-"})`;
            opt.dataset.city = c.city || "";
            opt.dataset.country = c.country || "";
            opt.dataset.phone = c.phone || "";
            select.appendChild(opt);
        });

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
// New CUSTOMER          //
document.getElementById("btnAddCustomer").addEventListener("click", () => {
    const modal = new bootstrap.Modal(document.getElementById("addCustomerModal"));
    modal.show();
});

document.getElementById("addCustomerForm").addEventListener("submit", async (e) => {
    e.preventDefault();

    const name = document.getElementById("newCustomerName").value.trim();
    const country = document.getElementById("newCustomerCountry").value.trim();

    if (!name || !country) return;

    const payload = {
        CARI_ISIM: name,
        IL: country
    };
    const token = sessionStorage.getItem("AccessToken");
    try {
        const res = await fetch("/api/orders/lookups/add", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(payload)
        });

        if (!res.ok) throw new Error(await res.text());

        const created = await res.json();

        // Cache temizle ve tekrar yükle
        localStorage.removeItem("sintan_customers_v5");
        localStorage.removeItem("sintan_customers_v5_time");

        await loadCustomers(); // Güncel listeyi getir

        // Yeni eklenen müşteriyi otomatik seç
        document.getElementById("customerSelect").value = created.CARI_KOD;

        bootstrap.Modal.getInstance(document.getElementById("addCustomerModal")).hide();
        showToast("✅ Yeni müşteri eklendi!");
      

    } catch (err) {
        showToast("❌ Kayıt başarısız: " + err.message);
    }
});


async function loadSalesReps() {
    return new Promise(async resolve => {    // <---- Eklenen satır
        const select = document.getElementById("salesRepSelect");
        if (!select) return resolve();

        select.disabled = true;
        select.innerHTML = `<option>Loading...</option>`;

        try {
            const cacheKey = "sintan_salesreps_v3";
            let reps = JSON.parse(localStorage.getItem(cacheKey));

            if (!reps) {
                const token = sessionStorage.getItem("AccessToken");
                const res = await fetch("/api/orders/lookups/salesreps", {
                    headers: { "Authorization": "Bearer " + token }
                });

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
            console.error(err);
            select.innerHTML = `<option>Error loading reps</option>`;
        } finally {
            select.disabled = false;
            resolve();     // <---- Burası kritik
        }
    });
}




    // === 🔹 ÜRÜNLER ---//
    let productListCache = [];

async function preloadProducts() {
    try {
        const res = await fetch("/api/orders/lookups/products");
        if (!res.ok) throw new Error("Ürün verisi alınamadı");
        const list = await res.json();
        productListCache = Array.isArray(list) ? list : [];
    } catch (err) {
        console.error("❌ Ürünler alınamadı:", err);
        productListCache = [];
    }
}

function fillProductSelect(selectElement) {
    if (!selectElement) return;
    selectElement.innerHTML = `<option value="">Ürün seçiniz...</option>`;

    productListCache.forEach(p => {
        const opt = document.createElement("option");
        opt.value = p.id ?? "";
        opt.textContent = `${p.name ?? "(isimsiz)"} (${p.id})`;
        opt.dataset.packWeight = p.packWeight ?? 0;
        opt.dataset.palletCount = p.palletCount ?? 0;
        opt.dataset.palletNet = p.palletNet ?? 0;
        opt.dataset.transportCost = p.transportCost ?? 0;
        selectElement.appendChild(opt);
    });

    selectElement.disabled = false;
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

// === 🔹 LİMANLAR (ports.json’dan - Basit ve Hızlı Sürüm) ===
async function loadPorts(selectId = "portOfDelivery") {
    const select = document.getElementById(selectId);
    if (!select) return;

    select.disabled = true;
    select.innerHTML = `<option>Loading ports...</option>`;

    try {
        const cacheKey = "sintan_ports_v4";
        let ports = JSON.parse(localStorage.getItem(cacheKey));
        const lastFetch = localStorage.getItem(cacheKey + "_time");
        const expired = !lastFetch || Date.now() - parseInt(lastFetch) > 86400000;

        // 🔹 JSON’dan veri çek
        if (!ports || expired) {
            const res = await fetch("/js/ports.json", { cache: "no-store" });
            if (!res.ok) throw new Error("Ports file not found");
            ports = await res.json();
            localStorage.setItem(cacheKey, JSON.stringify(ports));
            localStorage.setItem(cacheKey + "_time", Date.now().toString());
        }

        // 🔹 Dropdown doldur
        select.innerHTML = `<option value="">Select Port...</option>`;
        ports.forEach(p => {
            const opt = document.createElement("option");
            opt.value = p.code || p.name;
            opt.textContent = `${p.name} (${p.country || "-"})`;
            select.appendChild(opt);
        });

        // 🔹 Görsel düzen
        select.style.color = "#000";
        select.style.backgroundColor = "#fff";
        select.style.border = "1px solid #47AAC6"; // Sintan mavisi
        select.style.borderRadius = "6px";
        select.style.padding = "6px";
    } catch (err) {
        console.error("❌ Port listesi yüklenemedi:", err);
        select.innerHTML = `<option>Error loading ports</option>`;
    } finally {
        select.disabled = false;
    }
}



document.addEventListener('change', function (e) {
    if (e.target.classList.contains('product-select')) {
        const opt = e.target.selectedOptions[0];
        const row = e.target.closest('.item-row');
        // loadProducts sırasında set ettiğiniz packWeight ve palletNet verilerini kullanın
        row.dataset.packWeight = opt.dataset.packWeight || 0;
        row.dataset.palletNet = opt.dataset.palletNet || 0;
        recalcRowTotal(row);
    }
});

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
///*-------yaRDIMCI OLMAK-------*/// 
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


