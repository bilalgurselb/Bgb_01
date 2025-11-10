using Microsoft.EntityFrameworkCore;
using SiparisApi.Models;

namespace SiparisApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

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

            // AllowedEmail
            modelBuilder.Entity<AllowedEmail>()
                .HasKey(a => a.Id);

            modelBuilder.Entity<AllowedEmail>()
                .HasIndex(a => a.Email)
                .IsUnique();

            // User
            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasOne(u => u.AllowedEmail)
                .WithMany()
                .HasForeignKey(u => u.AllowedId)
                .OnDelete(DeleteBehavior.NoAction);

            // Log
            modelBuilder.Entity<Log>()
                .HasKey(l => l.Id);

            modelBuilder.Entity<Log>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // OrderHeader
            modelBuilder.Entity<OrderHeader>()
                .HasKey(h => h.Id);

            modelBuilder.Entity<OrderHeader>()
                .HasOne(h => h.SalesRep)
                .WithMany()
                .HasForeignKey(h => h.SalesRepId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<OrderHeader>()
                .HasOne(h => h.CreatedBy)
                .WithMany()
                .HasForeignKey(h => h.CreatedById)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<OrderHeader>()
                .HasIndex(h => h.CreatedAt);

            modelBuilder.Entity<OrderHeader>()
                .HasIndex(h => h.Status);

            modelBuilder.Entity<OrderHeader>()
                .HasIndex(h => h.CreatedById);

            // OrderItem
            modelBuilder.Entity<OrderItem>()
                .HasKey(i => i.Id);

            modelBuilder.Entity<OrderItem>()
                .HasOne(i => i.OrderHeader)
                .WithMany(h => h.Items)
                .HasForeignKey(i => i.OrderHeaderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .Property(i => i.Quantity).HasColumnType("decimal(13,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(i => i.Price).HasColumnType("decimal(13,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(i => i.NetWeight).HasColumnType("decimal(13,2)");

            // OrderStatusHistory
            modelBuilder.Entity<OrderStatusHistory>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<OrderStatusHistory>()
                .HasOne(s => s.OrderHeader)
                .WithMany()
                .HasForeignKey(s => s.OrderHeaderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderStatusHistory>()
                .HasOne(s => s.ChangedBy)
                .WithMany()
                .HasForeignKey(s => s.ChangedById)
                .OnDelete(DeleteBehavior.NoAction);

            // SintanCari (no relationships, just for reading)
            modelBuilder.Entity<SintanCari>()
                .HasKey(c => c.CARI_KOD);

            modelBuilder.Entity<SintanCari>()
                .Property(c => c.CARI_KOD)
                .IsRequired()
                .HasMaxLength(100);

            // SintanStok (no relationships, just for reading)
            modelBuilder.Entity<SintanStok>()
                .HasKey(s => s.STOK_KODU);

            modelBuilder.Entity<SintanStok>()
                .Property(s => s.STOK_KODU)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<SintanStok>().Property(s => s.PAY1).HasColumnType("decimal(13,2)");
            modelBuilder.Entity<SintanStok>().Property(s => s.AMBALAJ_AGIRLIGI).HasColumnType("decimal(13,2)");
            modelBuilder.Entity<SintanStok>().Property(s => s.PALET_AMBALAJ_ADEDI).HasColumnType("decimal(13,2)");
            modelBuilder.Entity<SintanStok>().Property(s => s.PALET_NET_AGIRLIGI).HasColumnType("decimal(13,2)");
            modelBuilder.Entity<SintanStok>().Property(s => s.PAY2).HasColumnType("decimal(13,2)");
            modelBuilder.Entity<SintanStok>().Property(s => s.CEVRIM_DEGERI_1).HasColumnType("decimal(13,2)");
            modelBuilder.Entity<SintanStok>().Property(s => s.ASGARI_STOK).HasColumnType("decimal(13,2)");
            modelBuilder.Entity<SintanStok>().Property(s => s.BIRIM_AGIRLIK).HasColumnType("decimal(13,2)");
            modelBuilder.Entity<SintanStok>().Property(s => s.NAKLIYET_TUT).HasColumnType("decimal(13,2)");
        }
    }
}
