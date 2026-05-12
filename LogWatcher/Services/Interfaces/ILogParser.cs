using DataAccess.Models;

namespace LogWatcher.Services.Interfaces
{
    public interface ILogParser
    {
        IReadOnlyList<LogError> Parse(IReadOnlyList<string> lines, Guid logFileId, string filePath);
    }
}
