using Microsoft.EntityFrameworkCore;
using SiparisApi.Models;

namespace SiparisApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // --- DbSets ---
        public DbSet<User> Users { get; set; }
        public DbSet<AllowedEmail> AllowedEmails { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }
        public DbSet<SintanCari> SintanCari { get; set; }
        public DbSet<SintanStok> SintanStok { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---- AllowedEmail ----
            modelBuilder.Entity<AllowedEmail>()
                .HasIndex(x => x.Email)
                .IsUnique();

            // ---- User ----
            modelBuilder.Entity<User>()
                .HasIndex(x => x.Email)
                .IsUnique();

            // ---- OrderHeader ----
            modelBuilder.Entity<OrderHeader>()
                .HasKey(h => h.Id);

            // CustomerId → SintanCari
            modelBuilder.Entity<OrderHeader>()
                .HasOne(h => h.Customer)                 // navigation
                .WithMany()
                .HasForeignKey(h => h.CustomerId)        // FK sütunu
                .OnDelete(DeleteBehavior.NoAction);

            // SalesRepId → Users
            modelBuilder.Entity<OrderHeader>()
                .HasOne(h => h.SalesRep)
                .WithMany()
                .HasForeignKey(h => h.SalesRepId)
                .OnDelete(DeleteBehavior.NoAction);

            // CreatedById → Users
            modelBuilder.Entity<OrderHeader>()
                .HasOne(h => h.CreatedBy)
                .WithMany()
                .HasForeignKey(h => h.CreatedById)
                .OnDelete(DeleteBehavior.NoAction);

            // Performans indexleri
            modelBuilder.Entity<OrderHeader>().HasIndex(h => h.CreatedAt);
            modelBuilder.Entity<OrderHeader>().HasIndex(h => h.Status);
            modelBuilder.Entity<OrderHeader>().HasIndex(h => h.CreatedById);

            // ---- OrderItem ----
            modelBuilder.Entity<OrderItem>()
                .HasKey(i => i.Id);

            // OrderHeaderId → OrderHeaders (cascade)
            modelBuilder.Entity<OrderItem>()
                .HasOne(i => i.OrderHeader)
                .WithMany(h => h.Items)                   // Header.Items koleksiyonu varsa; yoksa .WithMany()
                .HasForeignKey(i => i.OrderHeaderId)
                .OnDelete(DeleteBehavior.Cascade);

            // ProductId → SintanStok
            modelBuilder.Entity<OrderItem>()
                .HasOne(i => i.Product)                   // !!! navigation (SintanStok)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            // ---- OrderStatusHistory ----
            modelBuilder.Entity<OrderStatusHistory>()
                .HasKey(s => s.Id);

            // OrderHeaderId → OrderHeaders (cascade)
            modelBuilder.Entity<OrderStatusHistory>()
                .HasOne(s => s.OrderHeader)
                .WithMany()
                .HasForeignKey(s => s.OrderHeaderId)
                .OnDelete(DeleteBehavior.Cascade);

            // ChangedById → Users
            modelBuilder.Entity<OrderStatusHistory>()
                .HasOne(s => s.ChangedBy)
                .WithMany()
                .HasForeignKey(s => s.ChangedById)
                .OnDelete(DeleteBehavior.NoAction);

            // ---- Logs ----
            modelBuilder.Entity<Log>()
                .HasKey(l => l.Id);

            // UserId → Users
            modelBuilder.Entity<Log>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // ---- SintanCari / SintanStok ----
            modelBuilder.Entity<SintanCari>()
                .HasKey(c => c.Id);
            modelBuilder.Entity<SintanCari>()
                .Property(c => c.CARI_KOD)
                .IsRequired();

            modelBuilder.Entity<SintanStok>()
                .HasKey(s => s.Id);
            modelBuilder.Entity<SintanStok>()
                .Property(s => s.STOK_KODU)
                .IsRequired();
        }
    }
}
