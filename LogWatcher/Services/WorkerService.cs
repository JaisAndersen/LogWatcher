namespace LogWatcher.Services
{
    public class LogPollingService : BackgroundService
    {
        private readonly ILogger<LogPollingService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);

        public LogPollingService(ILogger<LogPollingService> logger)
        {
            _logger = logger;
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
            var files = Directory.GetFiles(@"C:\sites\hk\Data\logs", "*.txt", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                try
                {
                    _logger.LogInformation("Polling {file}...", file);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Couldn't read {file}: {msg}", file, ex.Message);
                }
            }
        }
    }
}