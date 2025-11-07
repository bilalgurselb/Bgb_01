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

// === 🔹 MÜŞTERİLER (dbo.SintanCari - Autocomplete + Cache) ===
async function loadCustomers() {
    const selectContainer = document.getElementById("customerSelect");
    if (!selectContainer) return;

    // 🔹 Input alanı oluştur
    const input = document.createElement("input");
    input.type = "text";
    input.className = "form-control";
    input.placeholder = "Type customer name...";
    input.autocomplete = "off";
    selectContainer.replaceWith(input);

    // 🔹 Müşteri verisini cache'den veya API'den al
    const cacheKey = "sintan_customers_v5";
    let customers = JSON.parse(localStorage.getItem(cacheKey));
    const lastFetch = localStorage.getItem(cacheKey + "_time");
    const expired = !lastFetch || Date.now() - parseInt(lastFetch) > 86400000;

    try {
        if (!customers || expired) {
            const res = await fetch("/api/orders/lookups/customers");
            if (!res.ok) throw new Error("Failed to load customers");
            customers = await res.json();
            localStorage.setItem(cacheKey, JSON.stringify(customers));
            localStorage.setItem(cacheKey + "_time", Date.now().toString());
        }
    } catch (err) {
        console.error("❌ Customer list failed:", err);
        input.placeholder = "Error loading customers";
        return;
    }

    // 🔹 Dropdown menü
    const dropdown = document.createElement("div");
    dropdown.className = "dropdown-menu show";
    dropdown.style.position = "absolute";
    dropdown.style.maxHeight = "220px";
    dropdown.style.overflowY = "auto";
    dropdown.style.width = "100%";
    dropdown.style.display = "none";
    input.parentNode.insertBefore(dropdown, input.nextSibling);

    // 🔹 Arama (yazdıkça filtreleme)
    input.addEventListener("input", () => {
        const query = input.value.trim().toLowerCase();
        dropdown.innerHTML = "";

        if (!query) {
            dropdown.style.display = "none";
            return;
        }

        const matches = customers
            .filter(c => (c.CARI_ISIM || "").toLowerCase().includes(query))
            .slice(0, 25); // 🔹 En fazla 25 sonuç

        matches.forEach(c => {
            const item = document.createElement("button");
            item.type = "button";
            item.className = "dropdown-item";
            item.textContent = `${c.CARI_ISIM} (${c.IL || "-"})`;
            item.onclick = () => {
                input.value = c.CARI_ISIM;
                input.dataset.value = c.CARI_KODU || c.id || c.Id || c.CARI_ISIM;
                dropdown.style.display = "none";

                // Müşteri bilgilerini göster
                let infoDiv = document.getElementById("customerInfo");
                if (infoDiv) {
                    infoDiv.innerHTML = `
                        <small><b>City:</b> ${c.ILCE || '-'} |
                        <b>Country:</b> ${c.IL || '-'} |
                        <b>Phone:</b> ${c.TELEFON || '-'}</small>
                    `;
                }

                // Değişiklik olayı tetikle (form için)
                input.dispatchEvent(new Event("change"));
            };
            dropdown.appendChild(item);
        });

        dropdown.style.display = matches.length ? "block" : "none";
    });

    // 🔹 Dışarı tıklanınca listeyi kapat
    document.addEventListener("click", e => {
        if (!dropdown.contains(e.target) && e.target !== input)
            dropdown.style.display = "none";
    });

    // 🔹 Mobilde klavye focus
    input.addEventListener("focus", () => {
        input.scrollIntoView({ behavior: "smooth", block: "center" });
    });

    // 🔹 Input'a class ekleyelim ki diğer fonksiyonlar etkilesin
    input.classList.add("customer-select");
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
// === 🔹 ÜRÜNLER (dbo.SintanStok - Autocomplete + Cache) ===
async function loadProducts(selectElement = null) {
    const cacheKey = "sintan_products_v5";
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
    } catch (err) {
        console.error("❌ Products failed:", err);
        return;
    }

    const selects = selectElement
        ? [selectElement]
        : document.querySelectorAll(".product-select");

    selects.forEach(sel => {
        const wrapper = document.createElement("div");
        wrapper.style.position = "relative";
        wrapper.classList.add("autocomplete-wrapper");

        const input = document.createElement("input");
        input.type = "text";
        input.className = "form-control";
        input.placeholder = "Type product name...";
        input.autocomplete = "off";

        const dropdown = document.createElement("div");
        dropdown.className = "dropdown-menu show";
        dropdown.style.position = "absolute";
        dropdown.style.maxHeight = "220px";
        dropdown.style.overflowY = "auto";
        dropdown.style.width = "100%";
        dropdown.style.display = "none";

        sel.replaceWith(wrapper);
        wrapper.appendChild(input);
        wrapper.appendChild(dropdown);

        // 🔹 Arama (her yerde arar)
        input.addEventListener("input", () => {
            const query = input.value.trim().toLowerCase();
            dropdown.innerHTML = "";

            if (!query) {
                dropdown.style.display = "none";
                return;
            }

            const matches = products
                .filter(p => (p.STOK_ADI || "").toLowerCase().includes(query))
                .slice(0, 25); // 🔹 en fazla 25 sonuç

            matches.forEach(p => {
                const item = document.createElement("button");
                item.type = "button";
                item.className = "dropdown-item";
                item.textContent = p.STOK_ADI;
                item.onclick = () => {
                    input.value = p.STOK_ADI;
                    input.dataset.value = p.id || p.Id;
                    dropdown.style.display = "none";
                    input.dispatchEvent(new Event("change"));
                };
                dropdown.appendChild(item);
            });

            dropdown.style.display = matches.length ? "block" : "none";
        });

        document.addEventListener("click", e => {
            if (!dropdown.contains(e.target) && e.target !== input)
                dropdown.style.display = "none";
        });

        input.classList.add("product-select");
    });
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

// === 🔹 LİMANLAR (ports.json’dan - Lazy Autocomplete Versiyonu) ===
async function loadPorts(selectId) {
    const container = document.getElementById(selectId);
    if (!container) return;

    // Input oluştur
    const input = document.createElement("input");
    input.type = "text";
    input.className = "form-control";
    input.placeholder = "Type port name...";
    input.autocomplete = "off";
    container.replaceWith(input);

    let ports = JSON.parse(localStorage.getItem("sintan_ports_v3"));
    if (!ports) {
        try {
            const res = await fetch("/js/ports.json", { cache: "no-store" });
            if (!res.ok) throw new Error("Ports not found");
            ports = await res.json();
            localStorage.setItem("sintan_ports_v3", JSON.stringify(ports));
        } catch (err) {
            console.error("❌ Ports failed:", err);
            input.placeholder = "Error loading ports";
            return;
        }
    }

    // Dropdown listesi oluştur
    const dropdown = document.createElement("div");
    dropdown.className = "dropdown-menu show";
    dropdown.style.position = "absolute";
    dropdown.style.maxHeight = "220px";
    dropdown.style.overflowY = "auto";
    dropdown.style.width = "100%";
    dropdown.style.display = "none";
    input.parentNode.insertBefore(dropdown, input.nextSibling);

    // Kullanıcı yazdıkça filtreleme
    input.addEventListener("input", () => {
        const query = input.value.trim().toLowerCase();
        dropdown.innerHTML = "";

        if (!query) {
            dropdown.style.display = "none";
            return;
        }

        const matches = ports
            .filter(p => (p.name || "").toLowerCase().includes(query))
            .slice(0, 20); // 🔹 En fazla 20 sonuç göster

        matches.forEach(p => {
            const item = document.createElement("button");
            item.type = "button";
            item.className = "dropdown-item";
            item.textContent = `${p.name} (${p.country || "-"})`;
            item.onclick = () => {
                input.value = `${p.name} (${p.country || "-"})`;
                input.dataset.value = p.code || p.name;
                dropdown.style.display = "none";
            };
            dropdown.appendChild(item);
        });

        dropdown.style.display = matches.length ? "block" : "none";
    });

    // Dışarı tıklanınca listeyi kapat
    document.addEventListener("click", e => {
        if (!dropdown.contains(e.target) && e.target !== input) {
            dropdown.style.display = "none";
        }
    });

    // 🔹 Klavye odaklanınca hemen açılacak
    input.addEventListener("focus", () => {
        input.scrollIntoView({ behavior: "smooth", block: "center" });
    });
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


