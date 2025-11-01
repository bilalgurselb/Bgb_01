
# 🧾 SİPARİŞ YÖNETİM SİSTEMİ – ORDER FLOW DOKÜMANI

### 📅 Sürüm:
**v1.0 – 30.10.2025**  
**Hazırlayan:** Azure Architect & Bilal Börekçi  
**Amaç:** Sipariş oluşturma, onaylama ve yönetim süreçlerinin standart hale getirilmesi  

---

## 🧩 1. GENEL MANTIK

Kullanıcı giriş yaptıktan sonra **öncelikle sipariş listesini (Index)** görür.  
Bu liste hem yöneticilerin hem kullanıcıların sistem genel durumunu kolayca izlemesini sağlar.  
Yalnızca “Yeni Sipariş” oluşturma veya “Mevcut siparişi düzenleme” işlemleri kullanıcı bazlı yetkiyle yapılır.

---

## 👤 2. ROLLER VE YETKİLER

| Rol | Yetkiler |
|------|-----------|
| **Admin / Yönetici** | Tüm siparişleri görebilir, düzenleyebilir, onay kaldırabilir, fiyatları görüntüler. |
| **Kullanıcı / Satış** | Kendi siparişlerini oluşturabilir, yalnızca kendi fiyatlarını görebilir, onay bekleyen siparişini düzenleyebilir. |
| **Üretim / Operasyon** | Sadece “Onaylı” veya “Üretimde” olan siparişleri görebilir, “Üretildi” durumuna geçirebilir. |

---

## 🧾 3. SİPARİŞ DURUMLARI (ORDER STATUS)

| Kod | Durum | Renk | Açıklama | Düzenlenebilir | Admin Müdahalesi |
|------|--------|--------|-----------|------------------|--------------------|
| 🟡 0 | **Onay Bekleniyor** | #e6a700 | Müşteri / ödeme / üretim onayı bekliyor. | ✅ | ✅ |
| 🟢 1 | **Onaylandı / Üretimde** | #009879 | Üretim veya sevkiyat aşamasında. | ❌ | ✅ |
| 🔵 2 | **Üretildi** | #007BFF | Üretim tamamlandı, teslimat bekleniyor. | ❌ | ✅ |
| 🟣 3 | **Tamamlandı / Teslim Edildi** | #6f42c1 | Ürün teslim edildi, süreç kapandı. | ❌ | ✅ Geri alabilir |
| 🔴 4 | **İptal** | #dc3545 | Sipariş iptal edildi. | ❌ | ✅ Onay kaldırabilir |

> **Not:** “Onay Bekleniyor” tüm bekleme sebeplerini kapsar (müşteri, ödeme, üretim yoğunluğu vs.)

---

## 🎨 4. DURUM RENKLERİ (LEGEND)

Sipariş listesi sayfasının üst kısmında yer alır:

```html
<div class="status-legend">
    <div><span class="dot yellow"></span> <strong>Onay Bekleniyor</strong> – Beklemede</div>
    <div><span class="dot green"></span> <strong>Onaylandı / Üretimde</strong> – Üretim veya sevkiyat aşamasında</div>
    <div><span class="dot blue"></span> <strong>Üretildi</strong> – Ürün üretimi tamamlandı</div>
    <div><span class="dot purple"></span> <strong>Tamamlandı / Teslim Edildi</strong> – Ürün müşteriye teslim edildi</div>
    <div><span class="dot red"></span> <strong>İptal</strong> – Sipariş iptal edildi</div>
</div>
```

CSS tanımları:
```css
.status-legend { background:#f9f9f9; border:1px solid #ddd; border-radius:6px; padding:10px 15px; margin-bottom:20px; font-size:14px; }
.dot { display:inline-block; width:14px; height:14px; border-radius:50%; margin-right:8px; vertical-align:middle; }
.dot.yellow { background-color:#e6a700; }
.dot.green { background-color:#009879; }
.dot.blue { background-color:#007BFF; }
.dot.purple { background-color:#6f42c1; }
.dot.red { background-color:#dc3545; }
```

---

## 🧱 5. FORM ALANLARI (CREATE / EDIT)

| Alan Adı | Tür | Zorunlu | Açıklama |
|------------|------|-----------|-------------|
| Müşteri Adı | Text | ✅ | Siparişi veren müşteri |
| Ürün Adı | Text | ✅ | Sipariş edilen ürün |
| Miktar | Number | ✅ | Adet, kg, ton vb. |
| Fiyat | Decimal | ✅ | Kullanıcı yalnızca kendi siparişlerinin fiyatını görür |
| Durum | Dropdown | ✅ | Yukarıdaki statülerden biri |
| Açıklama | Multiline Text | ❌ | Siparişe özel not / detay |
| Ekleyen Kullanıcı | Hidden | ✅ | Login olan kullanıcı ID’si |
| Oluşturulma Tarihi | Hidden | ✅ | Otomatik atanır |

---

## 🧭 6. SİPARİŞ LİSTESİ (INDEX SAYFASI)

- Tabloda her satırda renkli durum etiketi (`badge`) gösterilir.  
- Kullanıcı yalnızca **kendi siparişlerini düzenleyebilir.**
- Admin tüm siparişleri düzenleyebilir veya statüsünü değiştirebilir.
- Satır tıklanarak `Edit` sayfasına gidilir.
- Onaylanan veya “Üretimde” olan siparişler **renkli arka plan** veya **ikon** ile işaretlenir.

---

## 🔔 7. BİLDİRİM & E-POSTA AKIŞI

| Olay | Bildirim Alan | Açıklama |
|------|----------------|----------|
| Yeni sipariş oluşturuldu | Admin | “Yeni sipariş eklendi” |
| Sipariş onaylandı | Kullanıcı | “Siparişiniz onaylandı” |
| Sipariş düzenlendi | Admin | “Sipariş revize edildi” |
| Sipariş üretimde / tamamlandı | Kullanıcı | “Siparişiniz üretim aşamasında / teslim edildi” |

---

## 🧭 8. ARAYÜZ (UI/UX) TASARIM PRENSİPLERİ

- Mobil uyumlu (%90 kullanım mobilde)
- Kurumsal renk paleti (Logo’daki tonlar + beyaz + turkuaz)
- Temiz, sade form (Login sayfası çizgisinde)
- Responsive grid yapısı
- `Bootstrap 5` kullanımı önerilir
- Tüm form kontrolleri tek dikey akışta
- Giriş alanları net kenarlıklı ve kontrastlı

---

## 🧱 9. TEKNİK NOTLAR

- **HTTPS Only** aktif
- **UseForwardedHeaders** middleware kullanılıyor
- Veri tabanı: `AppDbContext`
- Ortam: Azure App Service (North Europe)
- Ücretsiz plan — `wwwroot/wwwroot/css` gibi yol karışıklıkları göz önünde bulundurulmalı

---

## 🔒 10. GELECEK PLANLARI

| Aşama | Açıklama |
|--------|------------|
| 🔸 Form adım adım (ileri/geri) doldurma versiyonu | Opsiyonel |
| 🔸 Kullanıcı bazlı dashboard (KPI, sipariş toplamları) | Planlı |
| 🔸 Teslim onayını lojistik rolüne ayırma | İleri aşama |
| 🔸 Çoklu dil desteği (TR/EN) | Gerektiğinde |

---

📍**Sonuç:**  
Bu yapı, kullanıcı dostu bir arayüzle hem üretim hem satış sürecinin takibini kolaylaştırır.  
Her renk, her statü, her yetki **bilinçli olarak sade ama güçlü** bir deneyim sunar.  
Admin müdahaleleri açıkça tanımlanmıştır, veri bütünlüğü korunur.

---

💾 Dosyayı kaydederken:
> `/Docs/OrderFlow_Design.md`
