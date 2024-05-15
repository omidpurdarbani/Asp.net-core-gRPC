using MessageProcessor.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MessageProcessor
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<Worker>();
                    services.AddSingleton<ProcessingService>();
                })
                .Build();

            await host.RunAsync();
        }
    }
}