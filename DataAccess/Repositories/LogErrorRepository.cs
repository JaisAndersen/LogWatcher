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

        public async Task AddRangeAsync(IEnumerable<LogError> errors)
        {
            var list = errors.ToList();
            if (list.Count == 0) return;

            var utcNow = DateTime.UtcNow;
            foreach (var error in list)
            {
                if (error.Id == Guid.Empty)
                    error.Id = Guid.NewGuid();

                if (error.Created == default)
                    error.Created = utcNow;
            }

            await _context.LogErrors.AddRangeAsync(list);
            await _context.SaveChangesAsync();
        }
    }
}
