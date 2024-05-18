using System.Text.RegularExpressions;
using GrpcMessage;
using Message.Processor.Persistence.Interfaces;
using Microsoft.Extensions.Logging;

namespace Message.Processor.Persistence.Services
{
    public class ProcessorService : IProcessorService
    {
        private readonly ILogger<ProcessorService> _logger;
        private readonly Random _random;

        public ProcessorService(ILogger<ProcessorService> logger)
        {
            _logger = logger;
            _random = new Random();
        }

        public async Task<MessageRequest> InitialRequest(string instanceId)
        {
            var wait = _random.Next(200, 100000);
            await Task.Delay(wait).ConfigureAwait(false);
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
            await Task.Delay(wait).ConfigureAwait(false);

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
