using GrpcMessage;
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
                .ConfigureServices((_, services) =>
                {
                    services.AddGrpcClient<MessageProcessor.MessageProcessorClient>(o =>
                    {
                        o.Address = new Uri("http://localhost:5001");
                    });
                    services.AddHostedService<HealthChecker>();
                    services.AddHostedService<Worker>();
                    services.AddHostedService<ClientChecker>();
                    services.AddSingleton<GrpcMessageService>();
                    services.AddSingleton<IMessageService, MessageService>();
                    services.AddHttpClient();
                })
                .Build();

            await host.RunAsync();
        }
    }
}