using System.Text.RegularExpressions;
using Grpc.Core;
using GrpcMessage;

namespace MessageProcessor.Services
{
    public class ProcessingService : GrpcMessage.MessageProcessor.MessageProcessorBase
    {
        public override Task<MessageResponse> ProcessMessage(MessageRequest request, ServerCallContext context)
        {
            var message = request.Message;
            var messageLength = message.Length;
            var isValid = true; // Placeholder for actual validation logic

            var additionalFields = new Dictionary<string, bool>
            {
                { "HasNumbers", Regex.IsMatch(message, @"\d") },
                { "HasLetters", Regex.IsMatch(message, @"[a-zA-Z]") }
            };

            var response = new MessageResponse
            {
                Id = request.Id,
                Engine = "RegexEngine",
                MessageLength = messageLength,
                IsValid = isValid,
                AdditionalFields = { additionalFields }
            };

            return Task.FromResult(response);
        }
    }
}