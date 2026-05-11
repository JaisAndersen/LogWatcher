using DataAccess.Infrastructure;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Repositories
{
    public class LogFileRepository : ILogFileRepository
    {
        private readonly LogWatcherContext _context;

        public LogFileRepository(LogWatcherContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Task<LogFile?> GetByPathAsync(string filePath)
            => _context.LogFiles.FirstOrDefaultAsync(f => f.FilePath == filePath);


        public async Task<LogFile> AddAsync(LogFile logFile)
        {
            if (logFile.Id == Guid.Empty)
                logFile.Id = Guid.NewGuid();

            await _context.LogFiles.AddAsync(logFile);
            await _context.SaveChangesAsync();
            return logFile;
        }

        public async Task UpdateOffsetAsync(Guid id, long newOffset)
        {
            await _context.LogFiles
                .Where(f => f.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(f => f.ByteOffset, newOffset)
                    .SetProperty(f => f.LastPolled, DateTime.UtcNow));
        }
    }
}
