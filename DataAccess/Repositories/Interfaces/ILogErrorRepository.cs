using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces
{
    public interface ILogErrorRepository
    {
        Task AddRangeAsync(IEnumerable<LogError> errors);
    }
}
