using Message.Processor.Persistence.Interfaces;
using Message.Processor.Persistence.Services;
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
                    services.AddHostedService<Worker>();
                    services.AddSingleton<GrpcMessageService>();
                    services.AddSingleton<IMessageService, MessageService>();
                })
                .Build();

            await host.RunAsync();
        }
    }
}