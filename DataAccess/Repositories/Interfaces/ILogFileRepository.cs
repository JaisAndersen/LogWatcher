using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Repositories.Interfaces
{
    public interface ILogFileRepository
    {
        Task<LogFile?> GetByPathAsync(string filePath);
        Task<LogFile> AddAsync(LogFile logFile);
        Task UpdateOffsetAsync(Guid id, long newOffset);
    }
}
