using DataAccess.Infrastructure;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

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

        public async Task<PagedResult<LogError>> GetAsync(LogErrorQuery query)
        {
            var q = _context.LogErrors.AsQueryable();

            if (query.From.HasValue)
                q = q.Where(e => e.OccurredAt >= query.From.Value);

            if (query.To.HasValue)
                q = q.Where(e => e.OccurredAt <= query.To.Value);

            var totalCount = await q.CountAsync();

            var items = await q
                .OrderByDescending(e => e.Created)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<LogError>
            {
                Items = items,
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }

        public async Task AcknowledgeAsync(Guid id)
        {
            await _context.LogErrors
                .Where(e => e.Id == id)
                .ExecuteUpdateAsync(e => e.SetProperty(x => x.Acknowledged, true));
        }
    }
}


