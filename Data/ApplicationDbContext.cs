using Microsoft.EntityFrameworkCore;
using MeetFlow_Backend.Models;

namespace MeetFlow_Backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options)
        {
        }
        
        public DbSet<User> Users { get; set; }
        public DbSet<EventType> EventTypes { get; set; }
        public DbSet<Availability> Availabilities { get; set; } 
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<GoogleIntegration> GoogleIntegrations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // User - unique indexes
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
            
            // ===================================
            // EventType - indexes
            // ===================================
    
            modelBuilder.Entity<EventType>()
                .HasIndex(e => new { e.UserId, e.Slug })
                .IsUnique(); // Slug must be unique for each user

            modelBuilder.Entity<EventType>()
                .HasIndex(e => e.IsActive);
            
            // ===================================
            // Availability - indexes
            // ===================================
    
            modelBuilder.Entity<Availability>()
                .HasIndex(a => a.UserId);

            modelBuilder.Entity<Availability>()
                .HasIndex(a => new { a.UserId, a.DayOfWeek });
            
            // ===================================
            // Booking - indexes
            // ===================================

            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.EventTypeId);

            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.GuestEmail);

            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.EventTypeId, b.StartTime });

            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.Status);

            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.CreatedAt);
            
            // ===================================
            // GoogleIntegration - indexes
            // ===================================

            modelBuilder.Entity<GoogleIntegration>()
                .HasIndex(g => g.UserId)
                .IsUnique(); // One integration per user

            modelBuilder.Entity<GoogleIntegration>()
                .HasIndex(g => g.Email);

            modelBuilder.Entity<GoogleIntegration>()
                .HasIndex(g => g.IsActive);

            // ===================================
            // Relationships
            // ===================================
    
            // User -> EventTypes (1:N)
            modelBuilder.Entity<EventType>()
                .HasOne(e => e.User)
                .WithMany(u => u.EventTypes)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Delete event types when user deleted
            
            // User -> Availabilities (1:N)
            modelBuilder.Entity<Availability>()
                .HasOne(a => a.User)
                .WithMany(u => u.Availabilities)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Delete event types when user deleted
            
            // EventType -> Bookings (1:N)
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.EventType)
                .WithMany(e => e.Bookings)
                .HasForeignKey(b => b.EventTypeId)
                .OnDelete(DeleteBehavior.Cascade); // Delete bookings when event type deleted
            
            // User -> GoogleIntegration (1:1)
            modelBuilder.Entity<GoogleIntegration>()
                .HasOne(g => g.User)
                .WithOne(u => u.GoogleIntegration)
                .HasForeignKey<GoogleIntegration>(g => g.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}