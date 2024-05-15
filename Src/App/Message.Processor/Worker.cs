using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Message.Processor.Services
{
    public class Worker : BackgroundService
    {
        private readonly ProcessingService _processingService;
        private readonly IWebHost _host;
        private readonly ILogger<Worker> _logger;

        public Worker(ProcessingService processingService, GrpcProcessingService grpcProcessingService, ILogger<Worker> logger)
        {
            _processingService = processingService;
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
                    services.AddSingleton(grpcProcessingService);
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGrpcService<GrpcProcessingService>();
                    });
                })
                .Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Message processor started...");
            await Task.Run(() =>
            {
                _host.RunAsync(stoppingToken);
                _ = _processingService.StartTask();
            }, stoppingToken);
        }
    }
}