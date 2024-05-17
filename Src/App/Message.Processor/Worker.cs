using Message.Processor.Persistence.Interfaces;
using Message.Processor.Persistence.Services;
using Message.Splitter.Helper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;

        public Worker(IConfiguration configuration, GrpcProcessingService grpcProcessingService, ProcessingService processingService, ILogger<Worker> logger)
        {
            _processingService = processingService;
            _logger = logger;
            _configuration = configuration;

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
                    services.AddSingleton<IMessageService, MessageService>();
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
            _logger.LogInformation("Message Processor has started...");

            //track of tasks
            var listOfTasks = new List<Task>
            {
                //run grpc server
                Task.Run(async () => await _host.RunAsync(stoppingToken), stoppingToken)
            };

            //run all service instances
            var numberOfInstances = int.Parse(_configuration["NumberOfInstances"] ?? "1");
            for (var i = 0; i < numberOfInstances; i++)
            {
                listOfTasks.Add(Task.Run(async () => await _processingService.StartTask(Tools.GenerateGuid().ToString()), stoppingToken));
            }

            //ensure all tasks are running
            await Task.WhenAll(listOfTasks).ConfigureAwait(false);
        }
    }
}