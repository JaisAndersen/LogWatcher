using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces
{
    public interface ILogFileRepository
    {
        Task<LogFile?> GetByPathAsync(string filePath);
        Task<LogFile> AddAsync(LogFile logFile);
        Task UpdateOffsetAsync(Guid id, long newOffset);
    }
}
