using MessageDispatcher.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MessageDispatcher
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<Worker>();
                    services.AddSingleton<MessageService>();
                })
                .Build();

            await host.RunAsync();
        }
    }
}