using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces
{
    public interface ILogErrorRepository
    {
        Task AddAsync(LogError logError);
    }
}
