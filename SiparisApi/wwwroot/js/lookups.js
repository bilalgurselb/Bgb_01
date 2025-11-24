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
        norm.sort((a, b) => (a.name ?? "").localeCompare(b.name ?? "", "tr"));


        select.innerHTML = `<option value="">Select Customer...</option>`;
        norm.forEach(c => {
            const opt = document.createElement("option");
            opt.value = c.id;
            opt.textContent = `${c.name} (${c.country})`;
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
// === 🔹 CUSTOMER: Mobile-Friendly Search & Filter ===
(function makeCustomerFieldEditable() {
    const originalSelect = document.getElementById("customerSelect");
    if (!originalSelect) return;

    // 1️⃣ Yeni input oluştur
    const input = document.createElement("input");
    input.type = "text";
    input.placeholder = "Select or search customer...";
    input.classList.add("form-control", "mb-2");
    input.setAttribute("autocomplete", "off");

    // 2️⃣ Liste container
    const dropdown = document.createElement("div");
    dropdown.classList.add("customer-dropdown");
    Object.assign(dropdown.style, {
        display: "none",
        position: "absolute",
        width: "100%",
        maxHeight: "200px",
        overflowY: "auto",
        border: "1px solid #ccc",
        background: "#fff",
        zIndex: "1000",
        borderRadius: "6px",
    });

    // 3️⃣ Kapsayıcı oluştur
    const wrapper = document.createElement("div");
    wrapper.style.position = "relative";
    wrapper.style.width = "100%";

    // 4️⃣ Select gizle, wrapper içine al
    originalSelect.parentNode.insertBefore(wrapper, originalSelect);
    wrapper.appendChild(input);
    wrapper.appendChild(dropdown);
    originalSelect.style.display = "none";

    // 5️⃣ Dropdown'u doldur
    function populateDropdown() {
        dropdown.innerHTML = "";
        Array.from(originalSelect.options).forEach(opt => {
            if (!opt.value) return;
            const item = document.createElement("div");
            item.textContent = opt.textContent;
            item.dataset.value = opt.value;
            Object.assign(item.style, {
                padding: "6px 10px",
                cursor: "pointer",
            });

            item.addEventListener("mouseenter", () => {
                item.style.background = "#e6f4f8"; 
            });
            item.addEventListener("mouseleave", () => {
                item.style.background = "#fff";
            });

            item.addEventListener("click", () => {
                input.value = opt.textContent;
                originalSelect.value = opt.value;
                dropdown.style.display = "none";
                // Seçim olayı tetikle
                originalSelect.dispatchEvent(new Event("change", { bubbles: true }));
            });
            dropdown.appendChild(item);
        });
    }
    populateDropdown();

    // 6️⃣ 🔍 Filtreleme (filter değişkeni burada)
    input.addEventListener("input", () => {
        const filter = input.value.toLowerCase().trim();
        dropdown.style.display = "block";

        Array.from(dropdown.children).forEach(item => {
            const text = item.textContent.toLowerCase();
            item.style.display = text.includes(filter) ? "block" : "none";
        });
    });

    // 7️⃣ Focus & blur
    input.addEventListener("focus", () => {
        dropdown.style.display = "block";
        input.select();
    });

    input.addEventListener("blur", () => {
        setTimeout(() => (dropdown.style.display = "none"), 200);
    });

    // 8️⃣ Yeni müşteri eklendiğinde listeyi yenile
    const observer = new MutationObserver(populateDropdown);
    observer.observe(originalSelect, { childList: true });
})();


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

        await loadCustomers(); 

        /*--Yeni müşteri seçimi--*/
        const select = document.getElementById("customerSelect");
        const newOpt = [...select.options].find(o =>
            o.value === created.id?.toString() || o.value === created.kod?.toString()
        );

        if (newOpt) {
            select.value = newOpt.value;
            select.dispatchEvent(new Event("change", { bubbles: true }));
        }

        bootstrap.Modal.getInstance(document.getElementById("addCustomerModal")).hide();
        showToast(`✅ ${created.CARI_ISIM || "Yeni müşteri"} eklendi ve seçildi.`);
      

    } catch (err) {
        showToast("❌ Kayıt başarısız: " + err.message);
    }
});
/* 🔍 Customer Search (içeren arama)
document.getElementById("customerSearch")?.addEventListener("input", function () {
    const filter = this.value.toLowerCase();
    const options = document.querySelectorAll("#customerSelect option");

    options.forEach(opt => {
        const text = opt.textContent.toLowerCase();
        opt.style.display = (filter === "" || text.includes(filter)) ? "block" : "none";
    });
});
*/


async function loadSalesReps() {
    return new Promise(async resolve => {   
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
            resolve();   
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
        console.log("✅ Ürün listesi yüklendi:", productListCache.length);
    } catch (err) {
        console.error("❌ Ürünler alınamadı:", err);
        productListCache = [];
    }
}
function fillProductSelect(selectElement) {
    if (!selectElement) return;

    selectElement.innerHTML = `<option value="">Ürün seçiniz...</option>`;

    const groups = {};
    productListCache.forEach(p => {
        const groupName = (p.name ?? "").split(" ")[0];
        if (!groups[groupName]) groups[groupName] = [];
        groups[groupName].push(p);
    });

    const groupNames = Object.keys(groups).sort((a, b) => a.localeCompare(b, "tr"));

    for (const group of groupNames) {
        const optGroup = document.createElement("optgroup");
        optGroup.label = group.toUpperCase();

        groups[group].forEach(p => {
            const opt = document.createElement("option");
            opt.value = p.id ?? "";
            opt.textContent = `${p.name ?? "(isimsiz)"} (${p.id})`;

            // normalize dataset
            opt.dataset.packWeight = p.packWeight ?? p.PACK_WEIGHT ?? 0;
            opt.dataset.palletCount = p.palletCount ?? p.PALLET_COUNT ?? 0;
            opt.dataset.palletNet = p.palletNet ?? p.PALLET_NET ?? 0;
            opt.dataset.transportCost = p.transportCost ?? p.TRANSPORT_COST ?? 0;

            optGroup.appendChild(opt);
        });

        selectElement.appendChild(optGroup);
    }

    selectElement.disabled = false;
}

document.addEventListener("change", e => {
    if (e.target.classList.contains("product-select")) {
        const opt = e.target.querySelector(`option[value='${e.target.value}']`);

        if (!opt) return;

        const row = e.target.closest(".item-row");
        if (!row) return;

        row.dataset.packWeight = opt.dataset.packWeight || 0;
        row.dataset.palletNet = opt.dataset.palletNet || 0;
        recalcRowTotal(row);
        if (typeof calculateOrderTotals === "function") calculateOrderTotals();
    }
});

/*--Köprü--*/
document.addEventListener("productSelected", e => {
    const { selectId, productId } = e.detail;
    const select = document.getElementById(selectId);
    if (!select) return;

    select.value = productId;
    select.dispatchEvent(new Event("change", { bubbles: true }));
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

// ============================================================
// SMART PRODUCT DROPDOWN
// ============================================================

let smartDropdownActiveSelect = null;
let smartDropdownScrollY = 0; 
// Ürün select'i tıklanınca özel dropdown aç
document.addEventListener("click", function (e) {
    if (e.target.classList.contains("product-select")) {
        e.preventDefault(); 
        const select = e.target;
        smartDropdownActiveSelect = select;
        openSmartProductDropdown(select);
    }
});

function openSmartProductDropdown(selectEl) {
    const dropdown = document.getElementById("smartProductDropdown");
    if (!dropdown) return;

    // Mevcut scroll konumunu kaydet (mobil için önemli)
    smartDropdownScrollY =
        window.scrollY || document.documentElement.scrollTop || 0;

    dropdown.classList.remove("hidden");

    // Desktop'ta select'in altına konumla
    if (window.innerWidth > 768) {
        const rect = selectEl.getBoundingClientRect();
        dropdown.style.top = rect.bottom + window.scrollY + "px";
        dropdown.style.left = rect.left + window.scrollX + "px";
        dropdown.style.width = rect.width + "px";
    } else {
        // Mobile: tam ekran, soldan başlasın
        dropdown.style.top = "0";
        dropdown.style.left = "0";
        dropdown.style.width = "100%";
    }

    // Listeyi doldur
    buildSmartProductList(productListCache);
}


// Dropdown kapatma
document.getElementById("smartProductClose").addEventListener("click", function () {
    document.getElementById("smartProductDropdown").classList.add("hidden");
    window.scrollTo({ top: smartDropdownScrollY, left: 0, behavior: "auto" });
});

// Arama
document.getElementById("smartProductSearch").addEventListener("input", function () {
    const term = this.value.toLowerCase();
    const filtered = productListCache.filter(p => p.name.toLowerCase().includes(term));
    buildSmartProductList(filtered);
});
// KATEGORİ + ÜRÜN LİSTESİNİ KUR (alfabetik)
function buildSmartProductList(list) {
    const container = document.getElementById("smartProductList");
    if (!container) return;

    container.innerHTML = "";

    const groups = {};

    list.forEach(p => {
        const name = (p.name ?? "").trim();
        const groupName = (name.split(" ")[0] || "DİĞER").toUpperCase();

        if (!groups[groupName]) groups[groupName] = [];
        groups[groupName].push(p);
    });

    const groupNames = Object.keys(groups).sort((a, b) =>
        a.localeCompare(b, "tr")
    );

    groupNames.forEach(groupName => {
        const groupEl = document.createElement("div");
        groupEl.classList.add("smart-category");
        groupEl.innerHTML = `
            ${groupName}
            <i class="fas fa-chevron-right"></i>
        `;
        container.appendChild(groupEl);

        const productWrapper = document.createElement("div");
        productWrapper.classList.add("smart-products-wrapper");
        productWrapper.style.display = "none";

        // Grup içi ürünleri alfabetik sırala
        const products = groups[groupName].slice().sort((a, b) =>
            (a.name ?? "").localeCompare(b.name ?? "", "tr")
        );

        products.forEach(p => {
            const item = document.createElement("div");
            item.classList.add("smart-product");
            item.textContent = p.name;

            item.addEventListener("click", () => {
                smartDropdownActiveSelect.value = p.id;

                smartDropdownActiveSelect.dispatchEvent(
                    new Event("change", { bubbles: true }));
                // Paneli kapat
                const dd = document.getElementById("smartProductDropdown");
                dd.classList.add("hidden");

                // Scroll'u eski yerine geri al
                window.scrollTo({ top: smartDropdownScrollY, left: 0, behavior: "auto" });
            });


            productWrapper.appendChild(item);
        });

        container.appendChild(productWrapper);

        // Grup başlığı tıkla → aç/kapat
        groupEl.addEventListener("click", function () {
            const isOpen = productWrapper.style.display === "block";
            productWrapper.style.display = isOpen ? "none" : "block";
            groupEl.classList.toggle("expanded", !isOpen);
        });
    });
}

document.addEventListener("smart-select-product", function (e) {
    const select = e.detail.select;
    const productId = e.detail.id;

    let matched = false;
    [...select.options].forEach((o, i) => {
        o.selected = (o.value == productId);
        if (o.selected) {
            select.selectedIndex = i;  
            select.value = productId;
            matched = true;
        }
    });

    if (!matched) return;

    const opt = select.selectedOptions[0];
    const row = select.closest(".item-row");

    if (!row || !opt) return;

    // Dataset değerlerini yükle
    row.dataset.packWeight = opt.dataset.packWeight || 0;
    row.dataset.palletNet = opt.dataset.palletNet || 0;

    // Hesaplamayı tetikle
    recalcRowTotal(row);
    if (typeof calculateOrderTotals === "function") {
        calculateOrderTotals();
    }
    select.dispatchEvent(new Event("change"));
});
document.addEventListener("mousedown", function (e) {
    if (e.target.classList.contains("product-select")) {
        e.preventDefault(); // native dropdown’u engeller
        const select = e.target;
        smartDropdownActiveSelect = select;
        openSmartProductDropdown(select);
    }
});


