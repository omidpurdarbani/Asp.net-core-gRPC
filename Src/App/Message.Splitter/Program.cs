using Message.Processor.Persistence.Interfaces;
using Message.Processor.Persistence.Services;
using Message.Processor.Services;
using Message.Splitter.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Message.Splitter
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<HealthChecker>();
                    services.AddHostedService<Worker>();
                    services.AddHostedService<ClientChecker>();
                    services.AddSingleton<GrpcMessageService>();
                    services.AddHttpClient();
                    services.AddSingleton<IMessageService, MessageService>();
                })
                .Build();

            await host.RunAsync();
        }
    }
}