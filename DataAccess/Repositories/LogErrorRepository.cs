using DataAccess.Infrastructure;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;

namespace DataAccess.Repositories
{
    public class LogErrorRepository : ILogErrorRepository
    {
        private readonly LogWatcherContext _context;

        public LogErrorRepository(LogWatcherContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddAsync(LogError logError)
        {
            ArgumentNullException.ThrowIfNull(logError);

            if (logError.Id == Guid.Empty)
            {
                logError.Id = Guid.NewGuid();
            }

            var utcNow = DateTime.UtcNow;
            if (logError.Created == default)
            { 
                logError.Created = utcNow;
                logError.Updated = utcNow;
            }

            await _context.LogErrors.AddAsync(logError);
            await _context.SaveChangesAsync();
        }
    }
}
