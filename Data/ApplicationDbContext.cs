using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DiaplesWeb.Models;

namespace DiaplesWeb.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

        public DbSet<EventItem> Events => Set<EventItem>();

        public DbSet<EventAttendance> EventAttendances => Set<EventAttendance>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<EventAttendance>()
                .HasIndex(a => new { a.EventItemId, a.UserId })
                .IsUnique();
        }

    }
}
