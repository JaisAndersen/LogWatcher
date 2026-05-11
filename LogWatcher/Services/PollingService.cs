using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using LogWatcher.Services.Interfaces;

namespace LogWatcher.Services
{
    public class LogPollingService : BackgroundService
    {
        private readonly ILogger<LogPollingService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(60);
        private readonly IServiceProvider _services;
        private readonly ILogFileReader _fileReader;
        private readonly ILogParser _parser;
        private readonly string _logPath;

        public LogPollingService(
            ILogger<LogPollingService> logger,
            IServiceProvider services,
            ILogFileReader fileReader,
            ILogParser parser,
            IConfiguration configuration)
        {
            _logger = logger;
            _services = services;
            _fileReader = fileReader;
            _parser = parser;
            _logPath = configuration["LogWatcher:LogPath"]
                ?? throw new InvalidOperationException("LogWatcher:LogPath is not configured.");
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                await PollAllLogsAsync(ct);
                await Task.Delay(_interval, ct);
            }
        }

        private async Task PollAllLogsAsync(CancellationToken ct)
        {
            string[] files;
            try
            {
                files = Directory.GetFiles(_logPath, "*.txt", SearchOption.AllDirectories);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to enumerate files in {Path}: {Message}", _logPath, ex.Message);
                return;
            }

            using var scope = _services.CreateScope();
            var logFileRepo = scope.ServiceProvider.GetRequiredService<ILogFileRepository>();
            var logErrorRepo = scope.ServiceProvider.GetRequiredService<ILogErrorRepository>();

            foreach (var file in files)
            {
                try
                {
                    var logFile = await logFileRepo.GetByPathAsync(file)
                        ?? await logFileRepo.AddAsync(new LogFile { FilePath = file });

                    var (lines, newOffset) = await _fileReader.ReadNewLinesAsync(file, logFile.ByteOffset, ct);

                    if (lines.Count == 0)
                    {
                        _logger.LogInformation("No new content in {File}", file);
                        continue;
                    }

                    var errors = _parser.Parse(lines, logFile.Id, file);
                    await logErrorRepo.AddRangeAsync(errors);
                    await logFileRepo.UpdateOffsetAsync(logFile.Id, newOffset);

                    _logger.LogInformation("Processed {File}: {ErrorCount} new error(s) found", file, errors.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Couldn't process {File}: {Message}", file, ex.Message);
                }
            }
        }
    }
}