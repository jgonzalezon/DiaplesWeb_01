using DiaplesWeb.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DiaplesWeb.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<EventItem> Events => Set<EventItem>();
        public DbSet<Attendance> Attendances => Set<Attendance>();


        public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Attendance: PK compuesta y FKs
            builder.Entity<Attendance>()
                .HasKey(a => new { a.EventId, a.UserId });

            builder.Entity<Attendance>()
                .HasOne(a => a.Event)
                .WithMany(e => e.Attendances!)
                .HasForeignKey(a => a.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Attendance>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Valor por defecto
            builder.Entity<Attendance>()
                .Property(a => a.Status)
                .HasConversion<int>()               // guarda el enum como int
                .HasDefaultValue(AttendanceStatus.No);

            builder.Entity<Attendance>()
                .Property(a => a.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");


            builder.Entity<ContactMessage>()
                .Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}