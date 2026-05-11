using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Infrastructure
{
    public class LogWatcherContext : DbContext
    {
        public LogWatcherContext(DbContextOptions<LogWatcherContext> options) : base(options)
        {
        }

        public DbSet<LogFile> Logfiles => Set<LogFile>();
        public DbSet<LogError> LogErrors => Set<LogError>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LogFile>(entity =>
            {
                  entity.HasKey(e => e.Id);
                  entity.Property(e => e.FilePath).IsRequired();
                  entity.HasIndex(e => e.FilePath).IsUnique();
                  entity.HasMany(e => e.Errors)
                        .WithOne(e => e.LogFile)
                        .HasForeignKey(e => e.LogFileId);
            });

            modelBuilder.Entity<LogError>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Level).IsRequired();
                entity.Property(e => e.Message).IsRequired();
                entity.Property(e => e.Created).HasDefaultValueSql("sysutcdatetime()");
            });
        }
    }
}