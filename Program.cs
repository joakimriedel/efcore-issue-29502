using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Data;

#nullable enable

public static class Your
{
    public static string ConnectionString = @"Server=(localdb)\mssqllocaldb;Database=Issue29502;Trusted_Connection=True;MultipleActiveResultSets=true;Connect Timeout=30";
}

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            services.AddTransient<Processor>();
            services.AddHostedService<Worker>();
            services.AddPooledDbContextFactory<ApplicationDbContext>(options =>
            {
                options
                    .UseSqlServer(Your.ConnectionString)
                    .EnableSensitiveDataLogging();
            });
        });
}

public class Worker : IHostedService
{
    private readonly ILogger _logger;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly Processor _processor;

    public Worker(ILogger<Worker> logger, IHostApplicationLifetime appLifetime, Processor processor)
    {
        _logger = logger;
        _appLifetime = appLifetime;
        _processor = processor;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _appLifetime.ApplicationStarted.Register(() =>
        {
            Task.Run(async () =>
            {
                try
                {
                    _logger.LogInformation("Process starting");
                    await _processor.Process(_appLifetime.ApplicationStopping);
                    _logger.LogInformation("Process done");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception!");
                }
                finally
                {
                    _appLifetime.StopApplication();
                }
            });
        });

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
