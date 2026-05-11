using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Infrastructure
{
    public class LogWatcherContext : DbContext
    {
        public LogWatcherContext(DbContextOptions<LogWatcherContext> options) : base(options)
        {
        }
        public DbSet<LogError> LogErrors => Set<LogError>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LogError>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FilePath).IsRequired();
                entity.Property(e => e.Created).HasDefaultValueSql("sysutcdatetime()");
                entity.Property(e => e.Updated).HasDefaultValueSql("sysutcdatetime()");
            });
        }
    }
}