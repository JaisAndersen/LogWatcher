namespace LogWatcher.Services.Interfaces
{
    public interface ILogFileReader
    {
        Task<(IReadOnlyList<string> Lines, long NewOffset)> ReadNewLinesAsync(string filePath, long fromOffset, CancellationToken cancellationToken = default);
    }
}
