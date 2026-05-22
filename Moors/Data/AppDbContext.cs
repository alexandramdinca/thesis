using Microsoft.EntityFrameworkCore;
using Moors.Models;

namespace Moors.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Movie> Movies => Set<Movie>();
        public DbSet<UserRating> UserRatings => Set<UserRating>();
        public DbSet<WatchlistEntry> WatchlistEntries => Set<WatchlistEntry>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserRating>()
                .HasIndex(r => new { r.UserId, r.MovieId })
                .IsUnique();

            modelBuilder.Entity<WatchlistEntry>()
                .HasIndex(w => new { w.UserId, w.MovieId })
                .IsUnique();
        }
    }
}