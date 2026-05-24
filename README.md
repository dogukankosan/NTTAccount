
# 🗡️ NTTAccountUI

![License](https://img.shields.io/github/license/dogukankosan/NTTAccountUI)
![Stars](https://img.shields.io/github/stars/dogukankosan/NTTAccountUI)
![Issues](https://img.shields.io/github/issues/dogukankosan/NTTAccountUI)
![Last Commit](https://img.shields.io/github/last-commit/dogukankosan/NTTAccountUI)

> **NTTAccountUI**, Knight Online private server hesap yönetimi için geliştirilmiş, ASP.NET Core MVC tabanlı tam kapsamlı bir web uygulamasıdır. Kullanıcı tarafında modern bir vitrin sunarken, arka planda güçlü bir admin paneli ile site yönetimini tek çatı altında toplar.

---
<img width="1600" height="775" alt="35051906-965a-4836-927e-c618ff643676" src="https://github.com/user-attachments/assets/3cad3942-c5b9-4615-88f4-5c3a3d94b0e3" />


## 🚀 Özellikler

- 🔐 Cookie tabanlı admin oturum sistemi (IP bağlı, revoke edilebilir)
- 🛡️ Rol bazlı yetkilendirme (Super Admin / Moderatör)
- 🔒 AES-256 şifreleme servisi (IV + ciphertext, Base64)
- 📧 SMTP mail ayarları yönetimi (test zorunluluğu ile kayıt)
- 📩 İletişim formu (honeypot, IP spam koruması, input sanitization)
- 📬 Admin'e otomatik HTML bildirim maili (WhatsApp linki entegreli)
- 📰 Haber/duyuru yönetimi
- 🖼️ Banner/slide yönetimi
- ⚙️ Site ayarları (logo, ikon, sosyal medya linkleri — base64 DB'de)
- 👤 Kullanıcı yönetimi (lockout, şifre hash, profil resmi)
- 📋 Sistem log yönetimi (Info / Error / Critical)
- 🛒 Sipariş yönetimi (CRUD, durum takibi, satır bazlı kapatma/açma)
- 📦 Ürün yönetimi (stok, kod tekrar kontrolü, sipariş bağlantı koruması)
- 📊 Raporlama paneli (gelir, sipariş, ürün ve kullanıcı istatistikleri)
- 📥 ZIP indirme (tekli ve toplu, bilgi TXT ile birlikte paketli)
- 👁️ Kullanıcı sipariş paneli (sadece teslim edilmiş satırlar indirilebilir)
- ❄️ Kullanıcı arayüzü (kar efekti, çerez banner, OG/Twitter meta tag)
- 🌐 Dinamik footer (WhatsApp, Telegram, Discord, Facebook, YouTube)
- 🛠️ Global exception middleware → otomatik log + yönlendirme

---

## 🗂 Proje Yapısı

```
NTTAccountUI/
├── Controllers/
│   ├── AdminBaseController.cs          # Admin base: site ayarları, okunmamış mesaj, profil inject
│   ├── AdminLoginController.cs         # Giriş, lockout, RememberMe, session yönetimi
│   ├── AdminMailSettingsController.cs  # SMTP ayarları, test mail zorunluluğu
│   ├── AdminSiteSettingsController.cs  # Site adı, logo, sosyal medya ayarları
│   ├── AdminContactController.cs       # İletişim mesajları yönetimi
│   ├── AdminUserController.cs          # Kullanıcı yönetimi
│   ├── AdminLogController.cs           # Sistem logları
│   ├── AdminBannerSlideController.cs   # Banner/slide yönetimi
│   ├── AdminHomeController.cs          # Admin dashboard
│   ├── AdminOrderController.cs         # Sipariş yönetimi (CRUD, durum, ZIP indirme, satır toggle)
│   ├── AdminProductController.cs       # Ürün yönetimi (stok, kod kontrolü, sipariş koruması)
│   ├── AdminReportController.cs        # Raporlama (gelir, sipariş, ürün, kullanıcı istatistikleri)
│   ├── OrderController.cs              # Kullanıcı sipariş paneli (görüntüleme, ZIP indirme)
│   ├── ContactController.cs            # Kullanıcı iletişim formu + bildirim maili
│   ├── NewsController.cs               # Haber sayfası
│   └── UserBaseController.cs           # Kullanıcı tarafı base controller
├── Middleware/
│   ├── AdminAuthMiddleware.cs          # Session doğrulama, IP kontrolü, rol kısıtlaması
│   └── ExceptionMiddleware.cs          # Global hata yakalama → log + redirect
├── Data/
│   └── Repositories/
│       ├── AdminSessionRepository.cs   # Session CRUD (Dapper)
│       ├── UserRepository.cs           # Kullanıcı işlemleri
│       ├── ContactRepository.cs        # İletişim mesajları + spam kontrolü
│       ├── MailSettingsRepository.cs   # Mail ayarları
│       ├── SiteSettingsRepository.cs   # Site ayarları
│       ├── LogRepository.cs            # Sistem logları
│       ├── OrderRepository.cs          # Sipariş + satır CRUD, durum yönetimi
│       └── ProductRepository.cs        # Ürün CRUD, stok, kod ve sipariş kontrolü
├── Business/
│   └── Validators/
│       ├── AdminLoginValidator.cs      # Email & şifre doğrulama
│       ├── ContactValidator.cs         # Form alanı doğrulama
│       └── SiteSettingsValidator.cs    # Site ayarları doğrulama
├── Models/
│   ├── Entities/                       # DB entity sınıfları
│   └── ViewModels/                     # View model sınıfları
├── Security/
│   ├── EncryptionService.cs            # AES-256 şifreleme/çözme
│   ├── PasswordHasher.cs               # Şifre hash & verify
│   └── InputSanitizer.cs              # XSS önleme, input temizleme
├── Services/
│   ├── MailService.cs                  # SMTP mail gönderimi
│   └── LogService.cs                  # Info / Error / Critical loglama
├── Views/
│   ├── Shared/
│   │   ├── _UserLayout.cshtml          # Kullanıcı tarafı layout (kar efekti, footer, cookie banner)
│   │   └── _AdminLayout.cshtml         # Admin panel layout
│   └── ...                             # Diğer view dosyaları
└── appsettings.json                    # Encryption key, DB bağlantısı
```

---

## 🛠️ Kurulum & Çalıştırma

1. **Projeyi Klonla:**
   ```bash
   git clone https://github.com/dogukankosan/NTTAccountUI.git
   cd NTTAccountUI
   ```

2. **`appsettings.json` Dosyasını Düzenle:**
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=...;Database=...;User Id=...;Password=...;"
     },
     "Encryption": {
       "Key": "32-karakter-gizli-anahtar-buraya"
     }
   }
   ```

3. **Veritabanını Hazırla:**
   - SQL Server üzerinde gerekli tabloları oluştur:
     `Users`, `AdminSessions`, `Contacts`, `SiteSettings`, `MailSettings`, `Logs`, `BannerSlides`, `News`, `Orders`, `OrderItems`, `Products`

4. **Projeyi Çalıştır:**
   ```bash
   dotnet run
   ```
   veya Visual Studio ile açıp `F5` ile başlat.

5. **İlk Girişi Yap:**
   - `/AdminLogin` adresine git.
   - Veritabanına manuel olarak eklediğin süper admin (RoleId=1) hesabıyla giriş yap.

---

## ⚡ Kullanım Senaryosu

1. Uygulamayı başlat, `/AdminLogin` üzerinden giriş yap.
2. Admin panelinde site ayarlarını, logo ve sosyal medya linklerini düzenle.
3. Mail ayarlarına git → SMTP bilgilerini gir → **önce test maili gönder**, sonra kaydet.
4. Ürünleri `/AdminProduct` üzerinden ekle (kod, stok, fiyat).
5. Yeni sipariş oluştur → her satıra ZIP dosyası ekle → karakter/mail/OTP bilgileri AES-256 ile şifrelenir.
6. Sipariş satırlarını teslim ettikçe `/AdminOrder` üzerinden kapat; kullanıcı tarafında indirme aktif olur.
7. İletişim formundan gelen mesajları `/AdminContact` üzerinden yönet.
8. Kullanıcı tarafı vitrin otomatik olarak DB'deki ayarları yansıtır (site adı, footer, meta tag'ler).
9. Sistem loglarını `/AdminLog`, gelir ve sipariş istatistiklerini `/AdminReport` üzerinden takip et.

---

## 🔐 Güvenlik Mimarisi

| Katman | Yöntem |
|---|---|
| Oturum | Cookie tabanlı, IP'ye bağlı, revoke edilebilir token |
| Yetkilendirme | Middleware seviyesinde rol kontrolü (RoleId=1 / 2) |
| Şifreleme | AES-256 CBC, her şifrelemede farklı IV |
| Şifre | PBKDF2 / BCrypt hash & verify |
| Brute-force | Login lockout + `Task.Delay` ile timing attack önlemi |
| Spam | IP + telefon bazlı spam kontrolü, honeypot alanı |
| XSS | `InputSanitizer` ile form alanı temizleme |
| CSRF | `ValidateAntiForgeryToken` tüm POST action'larında |
| Hassas veri | Karakter şifresi, mail şifresi, OTP → AES-256 şifreli DB'de |
| Hata | Global exception middleware → log + kullanıcı dostu yönlendirme |

---

## 🤝 Katkı

Katkı sağlamak için projeyi forklayabilir ve pull request gönderebilirsiniz.

---

## 📄 Lisans

MIT License

---

## 📬 İletişim

- 👨‍💻 Geliştirici: [@dogukankosan](https://github.com/dogukankosan)
- 🐞 Suggestions or issues: [Issues sekmesi](https://github.com/dogukankosan/NTTAccountUI/issues)

---

<p align="center">
  <img src="https://img.shields.io/badge/ASP.NET_Core-MVC-512BD4?logo=dotnet" alt="aspnet" />
  <img src="https://img.shields.io/badge/SQL_Server-Dapper-CC2927?logo=microsoftsqlserver" alt="sqlserver" />
  <img src="https://img.shields.io/badge/AES--256-Encrypted-green?logo=letsencrypt" alt="encryption" />
  <img src="https://img.shields.io/badge/Knight_Online-Private_Server-red" alt="ko" />
</p>
