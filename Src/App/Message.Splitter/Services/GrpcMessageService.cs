using System.Text;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcMessage;
using Microsoft.Extensions.Logging;

namespace Message.Splitter.Services
{
    public class GrpcMessageService : MessageSplitter.MessageSplitterBase
    {
        private readonly MessageService _messageService;
        private readonly ILogger<GrpcMessageService> _logger;
        private readonly Random _random;



        public GrpcMessageService(MessageService messageService, ILogger<GrpcMessageService> logger)
        {
            _messageService = messageService;
            _random = new Random();
            _logger = logger;
        }

        public override async Task RequestMessage(IAsyncStreamReader<MessageRequest> requestStream, IServerStreamWriter<MessageResponse> responseStream, ServerCallContext context)
        {
            //this will run until request is closed by calling service
            await foreach (var request in requestStream.ReadAllAsync())
            {
                try
                {
                    _logger.LogInformation($"Message Splitter: Received message request with ID: {request.Id}");

                    #region get results

                    var channel = GrpcChannel.ForAddress("http://localhost:5001");
                    var client = new MessageProcessor.MessageProcessorClient(channel);

                    using var call = client.ProcessMessage();
                    var requestStreamProcess = call.RequestStream;
                    var responseStreamProcess = call.ResponseStream;

                    var message = await GenerateRandomMessage();
                    _logger.LogInformation($"Message Splitter: Sending message to process: {message.Message}");

                    var process = new MessageQueueResponse
                    {
                        Id = message.Id,
                        Sender = message.Sender,
                        Message = message.Message
                    };

                    await requestStreamProcess.WriteAsync(process);
                    await requestStreamProcess.CompleteAsync();

                    await foreach (var response in responseStreamProcess.ReadAllAsync())
                    {
                        _logger.LogInformation($"Message Splitter: Processed message: MessageLength: {response.MessageLength}, IsValid: {response.IsValid}");

                        var messageResponse = new MessageResponse()
                        {
                            Id = response.Id,
                            MessageLength = response.MessageLength,
                            Engine = response.Engine,
                            IsValid = response.IsValid,
                            AdditionalFields = { response.AdditionalFields }
                        };

                        _logger.LogInformation($"Message Splitter: Sending response for ID: {request.Id}");

                        await responseStream.WriteAsync(messageResponse);
                    }

                    #endregion 

                }
                catch (RpcException ex)
                {
                    _logger.LogError($"RPC Error: {ex.Status}, {ex.Message}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in RequestMessage: {ex.Message}");
                }
            }
        }
        private async Task<(string Id, string Sender, string Message)> GenerateRandomMessage()
        {
            await Task.Delay(200);
            return (Guid.NewGuid().ToString(), "Legal", LoremIpsum(10 + _random.Next(30), 41 + _random.Next(30), 1, 1, 1));
        }

        private static string LoremIpsum(int minWords, int maxWords, int minSentences, int maxSentences, int numLines)
        {
            var words = new[] { "lorem", "ipsum", "dolor", "sit", "amet", "consectetuer", "adipiscing", "elit", "sed", "diam", "nonummy", "nibh", "euismod", "tincidunt", "ut", "laoreet", "dolore", "magna", "aliquam", "erat" };

            var rand = new Random();
            int numSentences = rand.Next(maxSentences - minSentences) + minSentences + 1;
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