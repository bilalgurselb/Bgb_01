using Microsoft.EntityFrameworkCore;
using SiparisApi.Models;

namespace SiparisApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
		{
			
		}

        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<AllowedEmail> AllowedEmails { get; set; }
        public DbSet<Log> Logs { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
            modelBuilder.Entity<AllowedEmail>().HasIndex(x => x.Email).IsUnique();
        }
    }
}
