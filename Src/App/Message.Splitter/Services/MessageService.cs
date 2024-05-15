using System.Text;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcMessage;
using Microsoft.Extensions.Logging;

namespace Message.Splitter.Services
{
    public class MessageService
    {
        private readonly ILogger<MessageService> _logger;
        private readonly Random _random;
        private readonly GrpcMessage.MessageProcessor.MessageProcessorClient _client;

        public MessageService(ILogger<MessageService> logger)
        {
            _logger = logger;
            _random = new Random();
            var channel = GrpcChannel.ForAddress("http://localhost:5001");
            _client = new GrpcMessage.MessageProcessor.MessageProcessorClient(channel);
        }

        public async Task StartTask()
        {
            try
            {
                var message = await GenerateRandomMessage();
                _logger.LogInformation($"Message Splitter: Message received: {message}");

                var request = new MessageQueueResponse
                {
                    Id = message.Id,
                    Sender = message.Sender,
                    Message = message.Message
                };

                var response = await _client.ProcessMessageAsync(request);
                _logger.LogInformation($"Message Splitter: Processed message: {response}");
            }
            catch (RpcException ex)
            {
                _logger.LogError($"RPC Error: {ex.Status}, {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing message: {ex.Message}");
            }
        }

        private async Task<MessageQueueResponse> GenerateRandomMessage()
        {
            await Task.Delay(200);
            return new MessageQueueResponse()
            {
                Id = _random.Next(1000).ToString(),
                Sender = "Legal",
                Message = LoremIpsum(10 + _random.Next(30), 41 + _random.Next(30), 1, 1, 1)
            };
        }

        private static string LoremIpsum(int minWords, int maxWords, int minSentences, int maxSentences, int numLines)
        {
            var words = new[] { "lorem", "ipsum", "dolor", "sit", "amet", "consectetuer", "adipiscing", "elit", "sed", "diam", "nonummy", "nibh", "euismod", "tincidunt", "ut", "laoreet", "dolore", "magna", "aliquam", "erat" };

            var rand = new Random();
            int numSentences = rand.Next(maxSentences - minSentences)
                               + minSentences + 1;
            int numWords = rand.Next(maxWords - minWords) + minWords + 1;

            var sb = new StringBuilder();
            for (int p = 0; p < numLines; p++)
            {
                for (int s = 0; s < numSentences; s++)
                {
                    for (int w = 0; w < numWords; w++)
                    {
                        if (w > 0) { sb.Append(" "); }
                        sb.Append(words[rand.Next(words.Length)]);
                    }
                    sb.Append(". ");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
