using System.Text.RegularExpressions;
using Grpc.Core;
using GrpcMessage;

namespace Message.Processor.Services
{
    public class GrpcProcessingService : MessageProcessor.MessageProcessorBase
    {
        public override async Task ProcessMessage(IAsyncStreamReader<MessageQueueResponse> requestStream, IServerStreamWriter<ProcessResponse> responseStream, ServerCallContext context)
        {
            await foreach (var request in requestStream.ReadAllAsync())
            {
                var message = request.Message;
                var messageLength = message.Length;
                var isValid = true;

                var additionalFields = new Dictionary<string, bool>
                {
                    { "HasNumbers", Regex.IsMatch(message, @"\d") },
                    { "HasLetters", Regex.IsMatch(message, @"[a-zA-Z]") }
                };

                var response = new ProcessResponse
                {
                    Id = request.Id,
                    Engine = "RegexEngine",
                    MessageLength = messageLength,
                    IsValid = isValid,
                    AdditionalFields = { additionalFields }
                };

                await responseStream.WriteAsync(response);
            }
        }
    }
}