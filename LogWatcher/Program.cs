using DataAccess.Repositories;
using DataAccess.Repositories.Interfaces;
using LogWatcher.Services;
using LogWatcher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DataAccess.Infrastructure.LogWatcherContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ILogFileRepository, LogFileRepository>();
builder.Services.AddScoped<ILogErrorRepository, LogErrorRepository>();

builder.Services.AddSingleton<ILogFileReader, LogFileReader>();
builder.Services.AddSingleton<ILogParser, SitecoreLogParser>();

builder.Services.AddHostedService<LogPollingService>();

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();