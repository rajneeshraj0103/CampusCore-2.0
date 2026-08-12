
using CampusCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CampusCore.Infrastructure.Data
{
    public class CampusCoreDbContext : DbContext
    {
        public CampusCoreDbContext(
            DbContextOptions<CampusCoreDbContext> options)
            : base(options)
        {
        }

        public DbSet<Role> Roles { get; set;}
        public DbSet<User> Users { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Role>()
                .ToTable("Role");

            modelBuilder.Entity<User>()
                .ToTable("User");

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
