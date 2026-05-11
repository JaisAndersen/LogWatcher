using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogWatcher.Services.Interfaces
{
    public interface ILogParser
    {
        IReadOnlyList<LogError> Parse(IReadOnlyList<string> lines, Guid logFileId, string filePath);
    }
}
