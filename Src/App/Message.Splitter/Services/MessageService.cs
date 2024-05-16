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
        private readonly MessageProcessor.MessageProcessorClient _client;

        public MessageService(ILogger<MessageService> logger)
        {
            _logger = logger;
            _random = new Random();
            var channel = GrpcChannel.ForAddress("http://localhost:5001");
            _client = new MessageProcessor.MessageProcessorClient(channel);
        }

        public async Task<ProcessResponse> StartTask()
        {
            var call = _client.ProcessMessage();
            var requestStream = call.RequestStream;
            var responseStream = call.ResponseStream;

            // Create a TaskCompletionSource to await the response
            var responseReceived = new TaskCompletionSource<ProcessResponse>();

            try
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await foreach (var response in responseStream.ReadAllAsync())
                        {
                            _logger.LogInformation($"Message Splitter: Processed message: MessageLength: {response.MessageLength}, IsValid: {response.IsValid}");

                            // Set the TaskCompletionSource result when response is received
                            responseReceived.SetResult(response);
                        }
                    }
                    catch (RpcException ex)
                    {
                        _logger.LogError($"RPC Error: {ex.Status}, {ex.Message}");
                        // Set the TaskCompletionSource exception if an error occurs
                        responseReceived.SetException(ex);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing message: {ex.Message}");
                        // Set the TaskCompletionSource exception if an error occurs
                        responseReceived.SetException(ex);
                    }
                });

                var message = await GenerateRandomMessage();
                _logger.LogInformation($"Message Splitter: Sending message to process: {message.Message}");

                var request = new MessageQueueResponse
                {
                    Id = message.Id,
                    Sender = message.Sender,
                    Message = message.Message
                };

                await requestStream.WriteAsync(request);
                await requestStream.CompleteAsync();

                // Wait for the response
                return await responseReceived.Task;
            }
            catch (RpcException ex)
            {
                _logger.LogError($"RPC Error: {ex.Status}, {ex.Message}");
                throw; // Rethrow the exception
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending message: {ex.Message}");
                throw; // Rethrow the exception
            }
            finally
            {
                // Ensure proper cleanup of resources
                await requestStream.CompleteAsync();
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
