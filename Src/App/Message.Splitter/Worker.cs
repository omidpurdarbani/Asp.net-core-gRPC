using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Message.Splitter.Services
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IWebHost _host;

        public Worker(GrpcMessageService grpcMessageService, MessageService messageService, ILogger<Worker> logger)
        {
            _logger = logger;
            _host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    options.ListenLocalhost(6001, o => o.Protocols = HttpProtocols.Http2);
                })
                .ConfigureServices(services =>
                {
                    services.AddGrpc();
                    services.AddSingleton(grpcMessageService);
                    services.AddSingleton(messageService);
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGrpcService<GrpcMessageService>();
                    });
                })
                .Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogWarning("Message Splitter has started...");
            await _host.RunAsync(stoppingToken);
        }
    }
}