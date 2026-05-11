using DataAccess.Models;
using DataAccess.Repositories.Interfaces;

namespace LogWatcher.Services
{
    public class LogPollingService : BackgroundService
    {
        private readonly ILogger<LogPollingService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(60);
        private readonly IServiceProvider _services;
        private readonly string _logPath;

        public LogPollingService(ILogger<LogPollingService> logger, IServiceProvider services, IConfiguration configuration)
        {
            _logger = logger;
            _services = services;
            _logPath = configuration["LogWatcher:LogPath"] ?? throw new InvalidOperationException("LogWatcher:LogPath is not configured.");
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                await PollAllLogsAsync();
                await Task.Delay(_interval, ct);
            }
        }

        private async Task PollAllLogsAsync()
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
            var repo = scope.ServiceProvider.GetRequiredService<ILogErrorRepository>();

            foreach (var file in files)
            {
                try
                {
                    _logger.LogInformation("Polling {File}...", file);
                    var logError = new LogError { FilePath = file };
                    await repo.AddAsync(logError);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Couldn't process {File}: {Message}", file, ex.Message);
                }
            }
        }
    }
}