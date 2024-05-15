using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MessageProcessor.Services
{
    public class Worker : BackgroundService
    {
        private readonly IWebHost _host;
        private readonly ILogger<Worker> _logger;

        public Worker(ProcessingService processingService, ILogger<Worker> logger)
        {
            _logger = logger;

            _host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    options.ListenLocalhost(5001, o => o.Protocols = HttpProtocols.Http2);
                })
                .ConfigureServices(services =>
                {
                    services.AddGrpc();
                    services.AddSingleton(processingService);
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGrpcService<ProcessingService>();
                    });
                })
                .Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Message processor started.");
            await _host.RunAsync(stoppingToken);
        }
    }
}