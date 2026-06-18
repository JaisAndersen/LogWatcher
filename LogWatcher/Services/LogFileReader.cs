using LogWatcher.Services.Interfaces;

namespace LogWatcher.Services
{
    public class LogFileReader : ILogFileReader
    {
        /// <summary>
        /// Reads all new lines from a log file starting at the given byte offset.
        /// Opens the file with read/write sharing to avoid conflicts with active writers.
        /// Returns the new lines and the updated byte offset for the next read.
        /// </summary>
        public async Task<(IReadOnlyList<string> Lines, long NewOffset)> ReadNewLinesAsync(string filePath, long fromOffset, CancellationToken cancellationToken = default)
        {
            var lines = new List<string>();

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            stream.Seek(fromOffset, SeekOrigin.Begin);

            using var reader = new StreamReader(stream);
            string? line;
            while ((line = await reader.ReadLineAsync(cancellationToken)) is not null)
                lines.Add(line);

            return (lines, stream.Position);
        }
    }
}
