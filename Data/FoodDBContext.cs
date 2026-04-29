using Microsoft.EntityFrameworkCore;
using QuanLyQuanAn.Web.Models;
using System.Reflection.Emit;

namespace QuanLyQuanAn.Web.Data
{
    public class FoodDBContext : DbContext
    {
        public FoodDBContext(DbContextOptions<FoodDBContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
        }
    }
}
