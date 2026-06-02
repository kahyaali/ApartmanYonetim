// Controllers/OdemeController.cs
using ApartmanYonetim.Data;
using ApartmanYonetim.Models.Entities;
using ApartmanYonetim.Models.ViewModels;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApartmanYonetim.Controllers
{
    [Authorize]
    public class OdemeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _config;

        public OdemeController(ApplicationDbContext context,
            UserManager<AppUser> userManager,
            IConfiguration config)
        {
            _context = context;
            _userManager = userManager;
            _config = config;
        }

        // Ödeme sayfası
        [HttpGet]
        public async Task<IActionResult> Odeme(int paymentId)
        {
            var user = await _userManager.GetUserAsync(User);
            var payment = await _context.Payments
                .Include(p => p.Apartment).ThenInclude(a => a.Block)
                .Include(p => p.Transactions)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                TempData["Error"] = "Ödeme kaydı bulunamadı.";
                return RedirectToAction("MyPayments", "Payment");
            }

            // Sadece kendi dairesi
            if (payment.ApartmentId != user!.ApartmentId && !User.IsInRole("Admin"))
            {
                TempData["Error"] = "Bu ödemeye erişim yetkiniz yok.";
                return RedirectToAction("MyPayments", "Payment");
            }

            if (payment.Odendi || payment.KalanTutar <= 0)
            {
                TempData["Error"] = "Bu ödeme zaten tamamlanmış.";
                return RedirectToAction("MyPayments", "Payment");
            }

            var model = new OdemeBaslatViewModel
            {
                PaymentId = paymentId,
                Tutar = payment.KalanTutar,
                KartSahibi = $"{user.Ad} {user.Soyad}"
            };

            ViewBag.Payment = payment;
            return View(model);
        }

        // Ödemeyi işle
        [HttpPost]
        public async Task<IActionResult> OdemeYap(OdemeBaslatViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var payment = await _context.Payments
                .Include(p => p.Apartment).ThenInclude(a => a.Block)
                .Include(p => p.Transactions)
                .FirstOrDefaultAsync(p => p.Id == model.PaymentId);

            if (payment == null)
            {
                TempData["Error"] = "Ödeme kaydı bulunamadı.";
                return RedirectToAction("MyPayments", "Payment");
            }

            try
            {
                var iyzicoSettings = _config.GetSection("IyzicoSettings");

                var options = new Options
                {
                    ApiKey = iyzicoSettings["ApiKey"],
                    SecretKey = iyzicoSettings["SecretKey"],
                    BaseUrl = iyzicoSettings["BaseUrl"]
                };

                var request = new CreatePaymentRequest
                {
                    Locale = Locale.TR.ToString(),
                    ConversationId = $"payment_{payment.Id}_{DateTime.Now.Ticks}",
                    Price = payment.KalanTutar.ToString("F2",
                        System.Globalization.CultureInfo.InvariantCulture),
                    PaidPrice = payment.KalanTutar.ToString("F2",
                        System.Globalization.CultureInfo.InvariantCulture),
                    Currency = Currency.TRY.ToString(),
                    Installment = model.Taksit,
                    BasketId = $"basket_{payment.Id}",
                    PaymentChannel = PaymentChannel.WEB.ToString(),
                    PaymentGroup = PaymentGroup.PRODUCT.ToString()
                };

                // Kart bilgileri
                var paymentCard = new PaymentCard
                {
                    CardHolderName = model.KartSahibi,
                    CardNumber = model.KartNumarasi.Replace(" ", ""),
                    ExpireMonth = model.SonKullanmaAy,
                    ExpireYear = model.SonKullanmaYil,
                    Cvc = model.Cvv,
                    RegisterCard = 0
                };
                request.PaymentCard = paymentCard;

                // Alıcı bilgileri
                var buyer = new Buyer
                {
                    Id = user!.Id,
                    Name = user.Ad,
                    Surname = user.Soyad,
                    GsmNumber = user.PhoneNumber ?? "+905000000000",
                    Email = user.Email ?? "",
                    IdentityNumber = "11111111111",
                    RegistrationAddress = "Türkiye",
                    Ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                    City = "İstanbul",
                    Country = "Turkey"
                };
                request.Buyer = buyer;

                var shippingAddress = new Address
                {
                    ContactName = $"{user.Ad} {user.Soyad}",
                    City = "İstanbul",
                    Country = "Turkey",
                    Description = $"Daire {payment.Apartment?.DaireNo}"
                };
                request.ShippingAddress = shippingAddress;
                request.BillingAddress = shippingAddress;

                // Sepet
                var basketItem = new BasketItem
                {
                    Id = $"item_{payment.Id}",
                    Name = payment.Aciklama,
                    Category1 = "Aidat",
                    ItemType = BasketItemType.VIRTUAL.ToString(),
                    Price = payment.KalanTutar.ToString("F2",
                        System.Globalization.CultureInfo.InvariantCulture)
                };
                request.BasketItems = new List<BasketItem> { basketItem };

                // iyzico'ya gönder
                var response =await Iyzipay.Model.Payment.Create(request, options);

                if (response.Status == "success")
                {
                    // Transaction kaydet
                    var transaction = new PaymentTransaction
                    {
                        PaymentId = payment.Id,
                        OdenenTutar = payment.KalanTutar,
                        OdemeTarihi = DateTime.Now,
                        Aciklama = $"Online Ödeme (iyzico) — {response.PaymentId}",
                        OdeyenKisi = $"{user.Ad} {user.Soyad}"
                    };

                    _context.PaymentTransactions.Add(transaction);
                    payment.Odendi = true;
                    await _context.SaveChangesAsync();

                    TempData["Success"] =
                        $"Ödeme başarıyla tamamlandı! {payment.KalanTutar:N2} ₺";
                    return RedirectToAction("Detail", "Payment",
                        new { id = payment.Id });
                }
                else
                {
                    TempData["Error"] =
                        $"Ödeme başarısız: {response.ErrorMessage}";
                    ViewBag.Payment = payment;
                    return View("Odeme", model);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ödeme işlemi sırasında hata: {ex.Message}";
                ViewBag.Payment = payment;
                return View("Odeme", model);
            }
        }

        // Ödeme sonucu
        public IActionResult Sonuc(bool basarili, string mesaj)
        {
            ViewBag.Basarili = basarili;
            ViewBag.Mesaj = mesaj;
            return View();
        }
    }
}