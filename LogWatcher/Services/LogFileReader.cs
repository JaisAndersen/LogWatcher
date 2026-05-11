using LogWatcher.Services.Interfaces;

namespace LogWatcher.Services
{
    public class LogFileReader : ILogFileReader
    {
        public async Task<(IReadOnlyList<string> Lines, long NewOffset)> ReadNewLinesAsync(string filePath, long fromOffset, CancellationToken cancellationToken = default)
        {
            var lines = new List<string>();

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            stream.Seek(fromOffset, SeekOrigin.Begin);

            using var reader = new StreamReader(stream);
            string? line;
            while ((line = await reader.ReadLineAsync(cancellationToken)) is not null)
                lines.Add(line);

            return (lines, stream.Position);
        }
    }
}
