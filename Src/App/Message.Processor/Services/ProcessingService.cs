using Grpc.Net.Client;
using GrpcMessage;
using Microsoft.Extensions.Logging;

namespace Message.Processor.Services
{
    public class ProcessingService
    {
        private readonly ILogger<ProcessingService> _logger;
        private readonly MessageSplitter.MessageSplitterClient _client;

        public ProcessingService(ILogger<ProcessingService> logger)
        {
            _logger = logger;
            var channel = GrpcChannel.ForAddress("http://localhost:6001");
            _client = new MessageSplitter.MessageSplitterClient(channel);
        }

        public async Task StartTask()
        {
            while (true)
            {
                await Task.Delay(2000);
                var request = new MessageRequest()
                {
                    Id = "",
                    Type = ""
                };

                _logger.LogInformation($"Message Processor: requesting new message for id: {request.Id}");

                await _client.RequestMessageAsync(request);
            }
        }
    }
}