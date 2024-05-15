using Message.Splitter.Services;
using MessageProcessor.Services;
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
                    services.AddHostedService<Worker>();
                    services.AddSingleton<GrpcMessageService>();
                    services.AddSingleton<MessageService>();
                })
                .Build();

            await host.RunAsync();
        }
    }
}