using System.Text.RegularExpressions;
using GrpcMessage;
using Message.Processor.Persistence.Interfaces;
using Microsoft.Extensions.Logging;

namespace Message.Processor.Persistence.Services
{
    public class MessageService : IMessageService
    {
        private readonly ILogger<MessageService> _logger;
        private readonly Random _random;

        public MessageService(ILogger<MessageService> logger)
        {
            _logger = logger;
            _random = new Random();
        }

        public async Task<MessageRequest> InitialRequest(string instanceId)
        {
            var wait = _random.Next(2000, 600000);
            await Task.Delay(wait);
            var initConnection = new MessageRequest
            {
                Id = instanceId,
                Type = "RegexEngine"
            };

            return initConnection;
        }

        public async Task<MessageRequest> RequestMessage(string instanceId)
        {
            var wait = _random.Next(200, 500000);
            await Task.Delay(wait);

            var newRequest = new MessageRequest()
            {
                Id = instanceId,
                Type = "RegexEngine"
            };
            return newRequest;
        }

        public ProcessResponse ProcessMessage(MessageQueueRequest request)
        {
            var message = request.Message;
            var messageLength = message.Length;
            var isValid = true;

            var additionalFields =
                request.AdditionalFields
                    .ToDictionary(
                        additionalField => additionalField.Key, additionalField => Regex.IsMatch(request.Message, additionalField.Value));

            var response = new ProcessResponse
            {
                Id = request.Id,
                Engine = "RegexEngine",
                MessageLength = messageLength,
                IsValid = isValid,
                AdditionalFields = { additionalFields }
            };
            return response;
        }

    }
}
