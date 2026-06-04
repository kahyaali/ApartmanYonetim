# ApartmanYonetimV1
Özellikler
🏗️ Bina Yönetimi

Çoklu Blok Desteği — A, B, C Blok gibi birden fazla blok yönetimi, blok bazlı yönetici ataması
Daire Yönetimi — Kat, tip (1+1/2+1/3+1), metrekare, doluluk takibi
Sakin Yönetimi — Taşınma/ayrılma tarihleri, aktif/pasif durumu, geçmiş kayıtları
Araç & Otopark — Plaka bazlı araç kaydı, daire eşleştirme
Kargo & Paket Takibi — Gelen kargo bildirimi, teslim durumu
Demirbaş Yönetimi — Fotoğraflı demirbaş kaydı, lightbox görünümü
Ziyaretçi Yönetimi — Davet sistemi, giriş/çıkış takibi, araç plakası
Bakım Takvimi — Periyodik bakım görevleri, gecikme uyarıları, maliyet takibi

💰 Finans

Aidat Tanımlama — Aylık aidat, su, elektrik vb. tanımla, tüm dolu dairelere otomatik uygula
Kısmi Ödeme Sistemi — PaymentTransaction modeli ile çok adımlı ödeme takibi
Renkli Ödeme Grid — Ödenmiş (yeşil), gecikmiş (kırmızı), yaklaşan (sarı) görsel durum
Gider Takibi — Firma, çalışma türü, fatura no ile gider kaydı
Bütçe Planlama — Yıllık bütçe planlaması ve hedef takibi
Excel Export — Sakinler, ödemeler, giderler için 3 sayfalı Excel raporu (ClosedXML)
PDF Makbuz — Ödeme makbuzu oluşturma ve indirme (QuestPDF)
İyzico Online Ödeme — Sakinler kredi kartıyla online ödeme yapabilir

📡 Gerçek Zamanlı & İletişim

SignalR Anlık Bildirim — Admin işlem yapınca sakin anında popup bildirimi alır
Sakin-Admin Mesajlaşma — AJAX polling ile anlık mesajlaşma, okundu işaretleme, silme
Toplu Bildirim — Tüm sakinlere SignalR + e-posta ile toplu bildirim
E-posta Hatırlatma — Ödeme tarihi yaklaşınca otomatik SMTP bildirimi (BackgroundService)
Duyuru Sistemi — Aktif/pasif duyuru yönetimi
Arıza & Talep Takibi — Ticket sistemi, durum güncellemeleri
Toplantı Takvimi — Sakin & admin toplantı planlama
Anket / Oylama — Sakinlerin oy kullanabileceği anket modülü
Belge Yönetimi — Apartmana ait belgelerin dijital arşivlenmesi

🎨 Arayüz & UX

Dark Mode — Tek tıkla karanlık tema, tercih localStorage'da saklanır
Global Arama — Sakin, daire, ödeme, kargo arasında anlık arama
Favori / Pin Sayfalar — Sık kullanılan sayfaları sidebar'a sabitleme
Ödeme Takvimi — FullCalendar.js ile görsel ödeme takvimi
Profil Fotoğrafı — Kullanıcı profil fotoğrafı yükleme/güncelleme
Responsive Tasarım — Mobil uyumlu Bootstrap 5 arayüz
Yazdırma Desteği — Tüm listeler yazdırılabilir format

🔐 Güvenlik & Sistem

ASP.NET Core Identity — Cookie tabanlı kimlik doğrulama
Rol Tabanlı Yetkilendirme — SuperAdmin / Admin / Sakin rolleri
Blok Yetki Servisi — Admin sadece atandığı bloğun verilerini görür (IBlockAuthService)
Audit Log — Tüm kritik işlemler loglanır, kim ne zaman ne yaptı takip edilir
SuperAdmin Paneli — Yönetici oluşturma, blok atama, tüm kullanıcı yönetimi
Şifremi Unuttum — SMTP üzerinden şifre sıfırlama e-postası

🖥️ Teknoloji Stack
KatmanTeknolojiBackendASP.NET Core 8 MVCORMEntity Framework Core 8 (Code-First)VeritabanıMicrosoft SQL ServerKimlikASP.NET Core IdentityGerçek ZamanlıSignalRE-postaMailKit / SMTPExcelClosedXMLPDFQuestPDFOnline ÖdemeİyzicoFrontendBootstrap 5, Font Awesome 6, Chart.js, DataTablesTakvimFullCalendar.jsArka Plan İşlerIHostedService (BackgroundService)

🚀 Kurulum
Gereksinimler

.NET 8 SDK
SQL Server (LocalDB veya Express)
Visual Studio 2022 veya VS Code

🔑 Giriş Bilgileri
Role           eposta                     Şifre
SuperAdmin     superadmin@apartman.com    SuperAdmin123!

📁 Proje Yapısı
ApartmanYonetim/
├── Controllers/
│   ├── HomeController.cs          # Dashboard
│   ├── AuthController.cs          # Giriş/Kayıt/Şifre
│   ├── AdminController.cs         # Daire/Sakin/Kullanıcı
│   ├── PaymentController.cs       # Ödeme yönetimi
│   ├── AidatController.cs         # Aidat tanımlama
│   ├── ExpenseController.cs       # Gider takibi
│   ├── BlockController.cs         # Blok yönetimi
│   ├── MesajController.cs         # Mesajlaşma
│   ├── AnnouncementController.cs  # Duyurular
│   ├── TicketController.cs        # Arıza/Talep
│   ├── VehicleController.cs       # Araç/Otopark
│   ├── KargoController.cs         # Kargo takibi
│   ├── DemirbasController.cs      # Demirbaşlar
│   ├── ZiyaretciController.cs     # Ziyaretçi
│   ├── BakimController.cs         # Bakım takvimi
│   ├── BildirimController.cs      # Toplu bildirim
│   ├── ToplantiController.cs      # Toplantılar
│   ├── AnketController.cs         # Anket/Oylama
│   ├── BelgeController.cs         # Belge yönetimi
│   ├── ButceController.cs         # Bütçe planlama
│   ├── ProfileController.cs       # Profil/Şifre
│   ├── ExportController.cs        # Excel export
│   ├── PdfController.cs           # PDF makbuz
│   ├── AuditController.cs         # Sistem logları
│   ├── SearchController.cs        # Global arama
│   ├── FavoriController.cs        # Favori sayfalar
│   ├── SuperAdminController.cs    # SuperAdmin paneli
│   ├── YoneticiBlockController.cs # Blok atamaları
│   └── OdemeController.cs         # İyzico ödeme
│
├── Models/
│   ├── Entities/                  # DB entity modelleri
│   │   ├── AppUser.cs
│   │   ├── Apartment.cs
│   │   ├── Block.cs
│   │   ├── Resident.cs
│   │   ├── Payment.cs
│   │   ├── PaymentTransaction.cs
│   │   ├── AidatTanimi.cs
│   │   ├── Expense.cs
│   │   ├── Announcement.cs
│   │   ├── Ticket.cs
│   │   ├── Vehicle.cs
│   │   ├── Mesaj.cs
│   │   ├── Kargo.cs
│   │   ├── Demirbase.cs
│   │   ├── Toplanti.cs
│   │   ├── Anket.cs + AnketSik.cs + AnketOy.cs
│   │   ├── Belge.cs
│   │   ├── ButcePlan.cs + ButceKalem.cs
│   │   ├── FavoriSayfa.cs
│   │   ├── AuditLog.cs
│   │   ├── YoneticiBlock.cs
│   │   ├── Ziyaretci.cs
│   │   └── BakimGorev.cs
│   └── ViewModels/                # Form/görünüm modelleri
│
├── Services/
│   ├── EmailService.cs            # SMTP mail
│   ├── AuditService.cs            # Log servisi
│   ├── NotificationService.cs     # SignalR bildirimi
│   ├── PaymentReminderService.cs  # Arka plan hatırlatma
│   └── BlockAuthService.cs        # Blok yetki kontrolü
│
├── Hubs/
│   └── NotificationHub.cs         # SignalR hub
│
├── Data/
│   └── ApplicationDbContext.cs    # EF Core DbContext
│
├── Views/                         # Razor sayfaları
└── wwwroot/                       # Statik dosyalar
    ├── css/site.css
    ├── js/site.js
    └── uploads/                   # Yüklenen dosyalar

    📦 Modüller
Admin Paneli
ModülAçıklamaDashboardİstatistik kartları, son ödemeler, duyurular, Chart.js grafikleriBloklarBlok ekleme/düzenleme, doluluk oranı görselleştirmeDairelerKat/tip/m² yönetimi, blok bazlı filtrelemeSakinlerTaşınma/ayrılma takibi, aktif/pasif durumuKullanıcılarHesap yönetimi, rol atamaBlok AtamalarıYöneticilere blok yetki atamasıAidat TanımlarıTüm dairelere otomatik ödeme oluşturmaÖdemelerKısmi ödeme, transaction geçmişi, renkli gridGiderlerFirma/fatura bazlı gider kaydıBütçeYıllık bütçe planlama ve takipDemirbaşlarFotoğraflı demirbaş envanteriAraç & OtoparkPlaka/daire eşleştirmeKargo & PaketTeslim durumu takibiZiyaretçilerGiriş/çıkış onaylama, reddetmeBakım TakvimiPeriyodik bakım, gecikme uyarısıArıza & TaleplerTicket yönetimiDuyurularAktif/pasif yönetimToplantılarToplantı takvimiAnketlerOluşturma, oy sayımıBelgelerDijital arşivMesajlarSakinlerle birebir mesajlaşmaToplu BildirimSignalR + e-posta toplu gönderimSistem LoglarıAudit log görüntülemeExcel Raporları3 sayfalı Excel export

Sakin Paneli
ModülAçıklamaÖdemelerimBorç/ödeme durumu, transaction detayıOnline Ödemeİyzico ile kredi kartı ödemesiÖdeme TakvimiFullCalendar görünümüArıza & TaleplerTalep oluşturma, durum takibiAraç & OtoparkKendi araçlarını yönetmeKargolarımPaket durumu görüntülemeZiyaretçilerimZiyaretçi davet etmeMesajlarAdmin ile mesajlaşmaToplantılarToplantı takvimine erişimAnketlerOy kullanmaBelgelerBelge görüntülemeProfilAd/soyad güncelleme, fotoğraf yükleme, şifre değiştirme

🔑 Giriş Bilgileri
Role           eposta                     Şifre
SuperAdmin     superadmin@apartman.com    SuperAdmin123!
