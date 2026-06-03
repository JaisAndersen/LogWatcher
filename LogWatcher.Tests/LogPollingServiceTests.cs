using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using LogWatcher.Services;
using LogWatcher.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace LogWatcher.Tests;

[TestClass]
public class LogPollingServiceTests
{
    private Mock<ILogFileRepository> _logFileRepo = null!;
    private Mock<ILogErrorRepository> _logErrorRepo = null!;
    private Mock<ILogFileReader> _fileReader = null!;
    private Mock<ILogParser> _parser = null!;
    private Mock<ILogger<LogPollingService>> _logger = null!;
    private IServiceProvider _serviceProvider = null!;

    private LogPollingService CreateService(string logPath = @"C:\logs")
    {
        _logFileRepo = new Mock<ILogFileRepository>();
        _logErrorRepo = new Mock<ILogErrorRepository>();
        _fileReader = new Mock<ILogFileReader>();
        _parser = new Mock<ILogParser>();
        _logger = new Mock<ILogger<LogPollingService>>();

        var services = new ServiceCollection();
        services.AddSingleton(_logFileRepo.Object);
        services.AddSingleton(_logErrorRepo.Object);
        _serviceProvider = services.BuildServiceProvider();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["LogWatcher:LogPath"] = logPath })
            .Build();

        return new LogPollingService(_logger.Object, _serviceProvider, _fileReader.Object, _parser.Object, config);
    }

    //stops cleanly when cancellation is requested
    [TestMethod]
    public async Task ExecuteAsync_CancelledImmediately_StopsWithoutException()
    {
        var service = CreateService();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await service.StartAsync(cts.Token);
        await service.StopAsync(CancellationToken.None);
    }

    //new file is registered, errors stored, offset updated
    [TestMethod]
    public async Task ExecuteAsync_NewFile_RegistersAndPersistsErrors()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var filePath = Path.Combine(tempDir, "log.20260603.txt");
        File.WriteAllText(filePath, "");

        try
        {
            var service = CreateService(tempDir);

            var logFile = new LogFile { Id = Guid.NewGuid(), FilePath = filePath, ByteOffset = 0 };
            var errors = new List<LogError> { new() { Level = "ERROR", Message = "Boom" } };

            _logFileRepo.Setup(r => r.GetByPathAsync(filePath)).ReturnsAsync((LogFile?)null);
            _logFileRepo.Setup(r => r.AddAsync(It.IsAny<LogFile>())).ReturnsAsync(logFile);
            _fileReader
                .Setup(r => r.ReadNewLinesAsync(filePath, 0, It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<string> { "line1" }, 100L));
            _parser.Setup(p => p.Parse(It.IsAny<IReadOnlyList<string>>(), logFile.Id, filePath))
                .Returns(errors);
            _logErrorRepo.Setup(r => r.AddRangeAsync(errors)).Returns(Task.CompletedTask);
            _logFileRepo.Setup(r => r.UpdateOffsetAsync(logFile.Id, 100L)).Returns(Task.CompletedTask);

            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
            try { await service.StartAsync(cts.Token); await Task.Delay(200); } catch { }

            _logErrorRepo.Verify(r => r.AddRangeAsync(errors), Times.AtLeastOnce);
            _logFileRepo.Verify(r => r.UpdateOffsetAsync(logFile.Id, 100L), Times.AtLeastOnce);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    //missing config throws
    [TestMethod]
    public void Constructor_MissingLogPathConfig_Throws()
    {
        var services = new ServiceCollection().BuildServiceProvider();
        var config = new ConfigurationBuilder().Build();

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            new LogPollingService(
                new Mock<ILogger<LogPollingService>>().Object,
                services,
                new Mock<ILogFileReader>().Object,
                new Mock<ILogParser>().Object,
                config));
    }
}
