using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces
{
    public interface ILogErrorRepository
    {
        Task AddRangeAsync(IEnumerable<LogError> errors);
        Task<PagedResult<LogError>> GetAsync(LogErrorQuery query);
        Task AcknowledgeAsync(Guid id);
    }
}
