using Message.Processor.Persistence.Interfaces;
using Message.Processor.Persistence.Services;
using Message.Processor.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Message.Processor
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                {
                    services.AddHostedService<Worker>();
                    services.AddSingleton<Processor>();
                    services.AddSingleton<GrpcProcessingService>();
                    services.AddSingleton<IProcessorService, ProcessorService>();
                })
                .Build();

            await host.RunAsync();
        }
    }
}