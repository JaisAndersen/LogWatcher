using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace LogWatcher.Services
{
    public class LogPollingService : BackgroundService
    {
        private readonly ILogger<LogPollingService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(60);
        private readonly IServiceProvider _services;

        public LogPollingService(ILogger<LogPollingService> logger, IServiceProvider services)
        {
            _logger = logger;
            _services = services;
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
            string path = @"C:\sites\hk\Data\logs";
            string[] files;
            try
            {
                files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to enumerate files in {path}: {msg}", path, ex.Message);
                return;
            }

            foreach (var file in files)
            {
                try
                {
                    _logger.LogInformation("Polling {file}...", file);
                    using var scope = _services.CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<ILogErrorRepository>();
                    var logError = new LogError { FilePath = file };
                    await repo.AddAsync(logError);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Couldn't process {file}: {msg}", file, ex.Message);
                }
            }
        }
    }
}