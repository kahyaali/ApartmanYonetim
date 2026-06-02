using ApartmanYonetim.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;



namespace ApartmanYonetim.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Apartment> Apartments { get; set; }
        public DbSet<Resident> Residents { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Block> Blocks { get; set; }
        public DbSet<AidatTanimi> AidatTanimlari { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<FavoriSayfa> FavoriSayfalar { get; set; }
        public DbSet<Demirbas> Demirbaslar { get; set; }
        public DbSet<DemirbasResim> DemirbasResimleri { get; set; }
        public DbSet<Kargo> Kargolar { get; set; }

        public DbSet<Mesaj> Mesajlar { get; set; }

        public DbSet<Toplanti> Toplantilar { get; set; }
        public DbSet<Anket> Anketler { get; set; }
        public DbSet<AnketSik> AnketSiklari { get; set; }
        public DbSet<AnketOy> AnketOylari { get; set; }
        public DbSet<Belge> Belgeler { get; set; }
        public DbSet<ButcePlan> ButcePlanlar { get; set; }
        public DbSet<ButceKalem> ButceKalemler { get; set; }
        public DbSet<YoneticiBlock> YoneticiBlocklar { get; set; }

        public DbSet<Ziyaretci> Ziyaretciler { get; set; }
     
        public DbSet<BakimGorev> BakimGorevler { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Apartment
            builder.Entity<Apartment>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Tipi).HasMaxLength(20);
                e.Property(x => x.MetreKare).HasColumnType("decimal(10,2)");
                e.HasOne(x => x.Block)
                 .WithMany(b => b.Apartments)
                 .HasForeignKey(x => x.BlockId)
                 .OnDelete(DeleteBehavior.SetNull)
                 .IsRequired(false);
            });

            // Resident → Apartment
            builder.Entity<Resident>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Ad).HasMaxLength(50).IsRequired();
                e.Property(x => x.Soyad).HasMaxLength(50).IsRequired();
                e.HasOne(x => x.Apartment)
                 .WithMany(a => a.Residents)
                 .HasForeignKey(x => x.ApartmentId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // AppUser → Apartment
            builder.Entity<AppUser>(e =>
            {
                e.Property(x => x.Ad).HasMaxLength(50);
                e.Property(x => x.Soyad).HasMaxLength(50);
                e.HasOne(x => x.Apartment)
                 .WithMany()
                 .HasForeignKey(x => x.ApartmentId)
                 .OnDelete(DeleteBehavior.SetNull)
                 .IsRequired(false);
            });

            // Payment → Apartment
            builder.Entity<Payment>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Tutar).HasColumnType("decimal(10,2)");
                e.Property(x => x.Aciklama).HasMaxLength(200);
                e.Ignore(x => x.OdenenTutar);
                e.Ignore(x => x.KalanTutar);
                e.HasOne(x => x.Apartment)
                 .WithMany(a => a.Payments)
                 .HasForeignKey(x => x.ApartmentId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.AidatTanimi)
                 .WithMany(a => a.Payments)
                 .HasForeignKey(x => x.AidatTanimiId)
                 .OnDelete(DeleteBehavior.SetNull)
                 .IsRequired(false);
            });

            // Expense
            builder.Entity<Expense>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.FirmaAdi).HasMaxLength(100).IsRequired();
                e.Property(x => x.CalismaTuru).HasMaxLength(100).IsRequired();
                e.Property(x => x.Aciklama).HasMaxLength(500);
                e.Property(x => x.Tutar).HasColumnType("decimal(10,2)");
                e.Property(x => x.FaturaNo).HasMaxLength(50);
            });

            // Announcement
            builder.Entity<Announcement>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Baslik).HasMaxLength(200).IsRequired();
                e.Property(x => x.Icerik).HasMaxLength(2000).IsRequired();
            });

            // Block
            builder.Entity<Block>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Ad).HasMaxLength(50).IsRequired();
                e.Property(x => x.Aciklama).HasMaxLength(200);
            });

            // AidatTanimi
            builder.Entity<AidatTanimi>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Baslik).HasMaxLength(100).IsRequired();
                e.Property(x => x.Tutar).HasColumnType("decimal(10,2)");
            });

            // PaymentTransaction → Payment
            builder.Entity<PaymentTransaction>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.OdenenTutar).HasColumnType("decimal(10,2)");
                e.Property(x => x.Aciklama).HasMaxLength(300);
                e.Property(x => x.OdeyenKisi).HasMaxLength(100);
                e.HasOne(x => x.Payment)
                 .WithMany(p => p.Transactions)
                 .HasForeignKey(x => x.PaymentId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Ticket>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Baslik).HasMaxLength(200).IsRequired();
                e.Property(x => x.Aciklama).HasMaxLength(1000).IsRequired();
                e.Property(x => x.Kategori).HasMaxLength(100);
                e.Property(x => x.AdminNotu).HasMaxLength(500);
                e.HasOne(x => x.Olusturan)
                 .WithMany()
                 .HasForeignKey(x => x.OlusturanId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Apartment)
                 .WithMany()
                 .HasForeignKey(x => x.ApartmentId)
                 .OnDelete(DeleteBehavior.SetNull)
                 .IsRequired(false);
            });

            builder.Entity<AuditLog>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.KullaniciId).HasMaxLength(450);
                e.Property(x => x.KullaniciAdi).HasMaxLength(200);
                e.Property(x => x.Islem).HasMaxLength(100);
                e.Property(x => x.Entity).HasMaxLength(100);
                e.Property(x => x.IpAdresi).HasMaxLength(50);
            });


            builder.Entity<Vehicle>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Plaka).HasMaxLength(20).IsRequired();
                e.Property(x => x.Marka).HasMaxLength(50);
                e.Property(x => x.Model).HasMaxLength(50);
                e.Property(x => x.Renk).HasMaxLength(30);
                e.Property(x => x.AracTipi).HasMaxLength(30);
                e.Property(x => x.OtoparkNo).HasMaxLength(20);
                e.Property(x => x.Aciklama).HasMaxLength(300);
                e.HasOne(x => x.Apartment)
                 .WithMany()
                 .HasForeignKey(x => x.ApartmentId)
                 .OnDelete(DeleteBehavior.SetNull)
                 .IsRequired(false);
                e.HasOne(x => x.Kaydeden)
                 .WithMany()
                 .HasForeignKey(x => x.KaydedenId)
                 .OnDelete(DeleteBehavior.SetNull)
                 .IsRequired(false);
            });

            builder.Entity<FavoriSayfa>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Baslik).HasMaxLength(100);
                e.Property(x => x.Url).HasMaxLength(300);
                e.Property(x => x.Ikon).HasMaxLength(50);
                e.HasOne(x => x.Kullanici)
                 .WithMany()
                 .HasForeignKey(x => x.KullaniciId)
                 .OnDelete(DeleteBehavior.Cascade);
            });
            builder.Entity<Demirbas>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Ad).HasMaxLength(200).IsRequired();
                e.Property(x => x.Aciklama).HasMaxLength(1000);
                e.Property(x => x.Konum).HasMaxLength(200);
                e.Property(x => x.SeriNo).HasMaxLength(100);
                e.Property(x => x.Marka).HasMaxLength(100);
                e.Property(x => x.Model).HasMaxLength(100);
                e.Property(x => x.Fiyat).HasColumnType("decimal(10,2)");
            });

            builder.Entity<DemirbasResim>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.ResimYolu).HasMaxLength(500).IsRequired();
                e.Property(x => x.Aciklama).HasMaxLength(300);
                e.HasOne(x => x.Demirbas)
                 .WithMany(d => d.Resimler)
                 .HasForeignKey(x => x.DemirbasId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Kargo>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.AliciAd).HasMaxLength(100).IsRequired();
                e.Property(x => x.KargoFirma).HasMaxLength(100);
                e.Property(x => x.TakipNo).HasMaxLength(100);
                e.Property(x => x.Aciklama).HasMaxLength(500);
                e.Property(x => x.TeslimAlan).HasMaxLength(100);
                e.HasOne(x => x.Apartment)
                 .WithMany()
                 .HasForeignKey(x => x.ApartmentId)
                 .OnDelete(DeleteBehavior.SetNull)
                 .IsRequired(false);
            });

            builder.Entity<Mesaj>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Icerik).HasMaxLength(2000).IsRequired();
                e.HasOne(x => x.Gonderen)
                 .WithMany()
                 .HasForeignKey(x => x.GonderenId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Alici)
                 .WithMany()
                 .HasForeignKey(x => x.AliciId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Toplanti>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Baslik).HasMaxLength(200).IsRequired();
                e.Property(x => x.Aciklama).HasMaxLength(1000);
                e.Property(x => x.Konum).HasMaxLength(200);
                e.Property(x => x.Notlar).HasMaxLength(2000);
                e.HasOne(x => x.Olusturan)
                 .WithMany()
                 .HasForeignKey(x => x.OlusturanId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Anket>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Baslik).HasMaxLength(200).IsRequired();
                e.Property(x => x.Aciklama).HasMaxLength(1000);
                e.HasOne(x => x.Olusturan)
                 .WithMany()
                 .HasForeignKey(x => x.OlusturanId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<AnketSik>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Metin).HasMaxLength(500).IsRequired();
                e.HasOne(x => x.Anket)
                 .WithMany(a => a.Siklar)
                 .HasForeignKey(x => x.AnketId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<AnketOy>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasOne(x => x.Anket)
                 .WithMany(a => a.Oylar)
                 .HasForeignKey(x => x.AnketId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Sik)
                 .WithMany(s => s.Oylar)
                 .HasForeignKey(x => x.AnketSikId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Kullanici)
                 .WithMany()
                 .HasForeignKey(x => x.KullaniciId)
                 .OnDelete(DeleteBehavior.Restrict);
                // Bir kullanıcı bir ankete bir kez oy verebilir
                e.HasIndex(x => new { x.AnketId, x.KullaniciId }).IsUnique();
            });

            builder.Entity<Belge>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Ad).HasMaxLength(200).IsRequired();
                e.Property(x => x.Aciklama).HasMaxLength(500);
                e.Property(x => x.DosyaYolu).HasMaxLength(500);
                e.Property(x => x.DosyaAdi).HasMaxLength(200);
                e.Property(x => x.DosyaTipi).HasMaxLength(50);
                e.HasOne(x => x.Yukleyen)
                 .WithMany()
                 .HasForeignKey(x => x.YukleyenId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ButcePlan>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Aciklama).HasMaxLength(500);
                e.HasOne(x => x.Olusturan)
                 .WithMany()
                 .HasForeignKey(x => x.OlusturanId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ButceKalem>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Kategori).HasMaxLength(100).IsRequired();
                e.Property(x => x.Aciklama).HasMaxLength(500).IsRequired();
                e.Property(x => x.PlanlananTutar).HasColumnType("decimal(10,2)");
                e.Property(x => x.GerceklesenTutar).HasColumnType("decimal(10,2)");
                e.HasOne(x => x.ButcePlan)
                 .WithMany(b => b.Kalemler)
                 .HasForeignKey(x => x.ButcePlanId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<YoneticiBlock>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => new { x.YoneticiId, x.BlockId }).IsUnique();
                e.HasOne(x => x.Yonetici)
                 .WithMany()
                 .HasForeignKey(x => x.YoneticiId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.Block)
                 .WithMany()
                 .HasForeignKey(x => x.BlockId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Ziyaretci>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.AdSoyad).HasMaxLength(100).IsRequired();
                e.Property(x => x.Telefon).HasMaxLength(20);
                e.Property(x => x.AracPlaka).HasMaxLength(20);
                e.Property(x => x.Aciklama).HasMaxLength(500);
                e.HasOne(x => x.Apartment)
                 .WithMany()
                 .HasForeignKey(x => x.ApartmentId)
                 .OnDelete(DeleteBehavior.SetNull)
                 .IsRequired(false);
                e.HasOne(x => x.DavetEden)
                 .WithMany()
                 .HasForeignKey(x => x.DavetEdenId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Onaylayan)
                 .WithMany()
                 .HasForeignKey(x => x.OnaylayanId)
                 .OnDelete(DeleteBehavior.Restrict)
                 .IsRequired(false);
            });

            builder.Entity<BakimGorev>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Baslik).HasMaxLength(200).IsRequired();
                e.Property(x => x.Aciklama).HasMaxLength(1000);
                e.Property(x => x.Kategori).HasMaxLength(100);
                e.Property(x => x.Firma).HasMaxLength(200);
                e.Property(x => x.Notlar).HasMaxLength(1000);
                e.Property(x => x.Maliyet).HasColumnType("decimal(10,2)");
                e.HasOne(x => x.Demirbas)
                 .WithMany()
                 .HasForeignKey(x => x.DemirbasId)
                 .OnDelete(DeleteBehavior.SetNull)
                 .IsRequired(false);
                e.HasOne(x => x.Olusturan)
                 .WithMany()
                 .HasForeignKey(x => x.OlusturanId)
                 .OnDelete(DeleteBehavior.Restrict);
            });


            // Identity tablo isimlerini Türkçeleştir (opsiyonel ama şık)
            builder.Entity<AppUser>().ToTable("Kullanicilar");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityRole>().ToTable("Roller");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>().ToTable("KullaniciRoller");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>().ToTable("KullaniciClaim");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>().ToTable("KullaniciLogin");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>>().ToTable("RolClaim");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<string>>().ToTable("KullaniciToken");
        }
    }
}
