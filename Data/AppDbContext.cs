using Microsoft.EntityFrameworkCore;
using EventManagementServer.Models;

namespace EventManagementServer.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CourseCategory> CourseCategories { get; set; }
        public DbSet<Registration> Registrations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CourseCategory>()
                .HasKey(cc => new { cc.CourseId, cc.CategoryId });

            modelBuilder.Entity<CourseCategory>()
                .HasOne(cc => cc.Course)
                .WithMany(c => c.CourseCategories)
                .HasForeignKey(cc => cc.CourseId);

            modelBuilder.Entity<CourseCategory>()
                .HasOne(cc => cc.Category)
                .WithMany(c => c.CourseCategories)
                .HasForeignKey(cc => cc.CategoryId);
        }
    }
}
