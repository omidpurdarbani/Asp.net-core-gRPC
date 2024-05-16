using Grpc.Net.Client;
using GrpcMessage;
using Microsoft.Extensions.Logging;

namespace Message.Processor.Services
{
    public class ProcessingService
    {
        private readonly ILogger<ProcessingService> _logger;
        private readonly MessageSplitter.MessageSplitterClient _client;
        private readonly Random _random;

        public ProcessingService(ILogger<ProcessingService> logger)
        {
            _logger = logger;
            _random = new Random();
            var channel = GrpcChannel.ForAddress("http://localhost:6001");
            _client = new MessageSplitter.MessageSplitterClient(channel);
        }

        public async Task StartTask(string instanceId)
        {
            _logger.LogInformation($"Message Processor[{instanceId}]: Created");

            var wait = _random.Next(6000, 10000);
            await Task.Delay(wait);
            var initConnection = new MessageRequest
            {
                Id = instanceId,
                Type = "RegexEngine"
            };

            _logger.LogInformation($"Message Processor[{initConnection.Id}]: Initial request");

            await _client.RequestMessageAsync(initConnection);

            while (true)
            {
                wait = _random.Next(6000, 600000);
                await Task.Delay(wait);

                var newRequest = new MessageRequest()
                {
                    Id = instanceId,
                    Type = "RegexEngine"
                };

                _logger.LogInformation($"Message Processor[{newRequest.Id}]: Requesting for a new message");

                await _client.RequestMessageAsync(newRequest);
            }
        }
    }
}