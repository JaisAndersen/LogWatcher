using DataAccess.Models;
using LogWatcher.Services.Interfaces;
using System.Text.RegularExpressions;

namespace LogWatcher.Services
{
    public partial class SitecoreLogParser : ILogParser
    {
        // Sitecore logs have levels: DEBUG, INFO, WARN, ERROR, FATAL. We only care about errors and above.
        private static readonly string[] ErrorLevels = new[] { "DEBUG", "WARN", "ERROR", "FATAL" };
        // Matches: <thread> <HH:mm:ss> <LEVEL>  <message>
        [GeneratedRegex(@"^.+?\s(\d{2}:\d{2}:\d{2})\s(DEBUG|INFO|WARN|ERROR|FATAL)\s{1,2}(.+)$")]
        private static partial Regex EntryPattern();

        // Extracts date from filename: log.20260511.txt → 2026-05-11
        [GeneratedRegex(@"\.(\d{4})(\d{2})(\d{2})\.")]
        private static partial Regex FileDatePattern();

        public IReadOnlyList<LogError> Parse(IReadOnlyList<string> lines, Guid logFileId, string filePath)
        {
            var fileDate = ExtractDateFromFilename(filePath);
            var errors = new List<LogError>();
            var entries = GroupIntoEntries(lines, fileDate);

            foreach (var entry in entries)
            {
                if (!ErrorLevels.Contains(entry.Level))
                    continue;

                errors.Add(new LogError
                {
                    LogFileId = logFileId,
                    Level = entry.Level,
                    Message = entry.Message,
                    StackTrace = entry.StackTrace,
                    OccurredAt = entry.OccurredAt,
                });
            }

            return errors;
        }
        private static List<ParsedEntry> GroupIntoEntries(IReadOnlyList<string> lines, DateOnly fileDate)
        {
            var entries = new List<ParsedEntry>();
            ParsedEntry? current = null;
            var stackLines = new List<string>();

            void FlushCurrent()
            {
                if (current is null) return;
                current.StackTrace = stackLines.Count > 0 ? string.Join(Environment.NewLine, stackLines) : null;
                entries.Add(current);
                stackLines.Clear();
            }

            foreach (var line in lines)
            {
                var match = EntryPattern().Match(line);
                if (match.Success)
                {
                    FlushCurrent();

                    var time = TimeOnly.Parse(match.Groups[1].Value);
                    current = new ParsedEntry
                    {
                        Level = match.Groups[2].Value,
                        Message = match.Groups[3].Value,
                        OccurredAt = fileDate.ToDateTime(time, DateTimeKind.Utc),
                    };
                }
                else if (current is not null && !string.IsNullOrWhiteSpace(line))
                {
                    stackLines.Add(line);
                }
            }

            FlushCurrent();
            return entries;
        }
        private static DateOnly ExtractDateFromFilename(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            var match = FileDatePattern().Match(fileName);
            if (match.Success)
            {
                var year = int.Parse(match.Groups[1].Value);
                var month = int.Parse(match.Groups[2].Value);
                var day = int.Parse(match.Groups[3].Value);
                return new DateOnly(year, month, day);
            }

            return DateOnly.FromDateTime(DateTime.UtcNow);
        }

        private sealed class ParsedEntry
        {
            public string Level { get; init; } = string.Empty;
            public string Message { get; init; } = string.Empty;
            public DateTime OccurredAt { get; init; }
            public string? StackTrace { get; set; }
        }
    }
}
