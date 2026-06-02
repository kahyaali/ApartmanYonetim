using ApartmanYonetim.Data;
using ApartmanYonetim.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ApartmanYonetim.Services
{
    public class PaymentReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PaymentReminderService> _logger;

        public PaymentReminderService(IServiceProvider serviceProvider,
            ILogger<PaymentReminderService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SendReminders();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Mail hatırlatma hatası");
                }

                // Her gün saat 09:00'da çalış
                var now = DateTime.Now;
                var nextRun = now.Date.AddDays(1).AddHours(9);
                var delay = nextRun - now;
                await Task.Delay(delay, stoppingToken);
            }
        }

        private async Task SendReminders()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            //var userManager = scope.ServiceProvider
            //    .GetRequiredService < Microsoft.AspNetCore.Identity.UserManager<ApartmanYonetim.Models.Entities.AppUser >> ();

            var userManager = scope.ServiceProvider
             .GetRequiredService<UserManager<AppUser>>();


            var bugun = DateTime.Today;
            var besGunSonra = bugun.AddDays(5);

            // Son 5 gün içinde olan ve ödenmemiş ödemeleri bul
            var bekleyenOdemeler = await context.Payments
                .Include(p => p.Apartment)
                .Include(p => p.Transactions)
                .Where(p => !p.Odendi
                         && p.SonOdemeTarihi.Date >= bugun
                         && p.SonOdemeTarihi.Date <= besGunSonra)
                .ToListAsync();

            foreach (var payment in bekleyenOdemeler)
            {
                // O dairedeki kullanıcıyı bul
                var user = await userManager.Users
                    .FirstOrDefaultAsync(u => u.ApartmentId == payment.ApartmentId);

                if (user?.Email == null) continue;

                var kalanGun = (payment.SonOdemeTarihi.Date - bugun).Days;
                var kalanTutar = payment.KalanTutar;

                var htmlBody = $@"
                <div style='font-family:Segoe UI,sans-serif;max-width:520px;margin:auto;
                            border:1px solid #e2e8f0;border-radius:12px;overflow:hidden;'>
                    <div style='background:#1e3a5f;padding:24px;text-align:center;'>
                        <h2 style='color:#fff;margin:0;font-size:20px;'>
                            Ödeme Hatırlatması
                        </h2>
                    </div>
                    <div style='padding:28px;'>
                        <p style='color:#2d3748;'>Merhaba <strong>{user.Ad} {user.Soyad}</strong>,</p>
                        <p style='color:#4a5568;'>
                            <strong>Daire {payment.Apartment?.DaireNo}</strong> için 
                            <strong>{payment.Aciklama}</strong> ödemeniz 
                            <strong style='color:{(kalanGun <= 3 ? "#e74c3c" : "#e67e22")};'>
                                {(kalanGun == 0 ? "BUGÜN" : $"{kalanGun} gün içinde")}
                            </strong> son bulmaktadır.
                        </p>
                        <div style='background:#f8fafc;border-radius:10px;padding:16px;margin:16px 0;'>
                            <div style='display:flex;justify-content:space-between;margin-bottom:8px;'>
                                <span style='color:#718096;'>Toplam Borç:</span>
                                <strong>{payment.Tutar:N2} ₺</strong>
                            </div>
                            <div style='display:flex;justify-content:space-between;margin-bottom:8px;'>
                                <span style='color:#718096;'>Ödenen:</span>
                                <strong style='color:#27ae60;'>{payment.OdenenTutar:N2} ₺</strong>
                            </div>
                            <div style='display:flex;justify-content:space-between;
                                        border-top:1px solid #e2e8f0;padding-top:8px;'>
                                <span style='color:#718096;'>Kalan:</span>
                                <strong style='color:#e74c3c;font-size:16px;'>
                                    {kalanTutar:N2} ₺
                                </strong>
                            </div>
                        </div>
                        <p style='color:#4a5568;font-size:13px;'>
                            Son ödeme tarihi: 
                            <strong>{payment.SonOdemeTarihi:dd MMMM yyyy}</strong>
                        </p>
                    </div>
                    <div style='background:#f8fafc;padding:16px;text-align:center;
                                border-top:1px solid #e2e8f0;'>
                        <p style='color:#a0aec0;font-size:12px;margin:0;'>
                            Apartman Yönetim Sistemi — Otomatik Bildirim
                        </p>
                    </div>
                </div>";

                await emailService.SendAsync(
                    user.Email,
                    $"⚠️ Ödeme Hatırlatması — {payment.Aciklama} ({kalanGun} gün kaldı)",
                    htmlBody);

                _logger.LogInformation(
                    $"Hatırlatma gönderildi: {user.Email} — Daire {payment.Apartment?.DaireNo}");
            }
        }
    }
}