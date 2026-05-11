using LogWatcher.Services;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<DataAccess.Infrastructure.LogWatcherContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<DataAccess.Repositories.Interfaces.ILogErrorRepository, DataAccess.Repositories.LogErrorRepository>();

builder.Services.AddHostedService<LogPollingService>();

var host = builder.Build();
host.Run();
