using Grpc.Net.Client;
using GrpcMessage;
using Microsoft.Extensions.Logging;

namespace MessageDispatcher.Services
{
    public class MessageService
    {
        private readonly ILogger<MessageService> _logger;
        private readonly Random _random;
        private readonly MessageProcessor.MessageProcessorClient _client;

        public MessageService(ILogger<MessageService> logger)
        {
            _logger = logger;
            _random = new Random();
            var channel = GrpcChannel.ForAddress("http://localhost:5001");
            _client = new MessageProcessor.MessageProcessorClient(channel);
        }

        public async Task StartProcessingMessages()
        {
            while (true)
            {
                await Task.Delay(200);
                var message = GenerateRandomMessage();
                _logger.LogInformation($"Message received: {message}");

                var request = new MessageRequest
                {
                    Id = message.Id.ToString(),
                    Sender = message.Sender,
                    Message = message.Message
                };

                var response = await _client.ProcessMessageAsync(request);
                _logger.LogInformation($"Processed message: {response}");
            }
        }

        private dynamic GenerateRandomMessage()
        {
            return new
            {
                Id = _random.Next(1000),
                Sender = "Legal",
                Message = "lorem ipsum"
            };
        }
    }
}