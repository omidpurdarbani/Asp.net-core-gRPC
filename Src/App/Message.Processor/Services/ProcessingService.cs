using Grpc.Core;
using Grpc.Net.Client;
using GrpcMessage;
using Microsoft.Extensions.Logging;

namespace Message.Processor.Services
{
    public class ProcessingService
    {
        private readonly ILogger<ProcessingService> _logger;
        private readonly Random _random;

        public ProcessingService(ILogger<ProcessingService> logger)
        {
            _logger = logger;
            _random = new Random();
        }

        public async Task StartTask(string instanceId)
        {
            _logger.LogInformation($"Message Processor[{instanceId}]: Created");

            try
            {
                var channel = GrpcChannel.ForAddress("http://localhost:6001");
                var client = new MessageSplitter.MessageSplitterClient(channel);

                var wait = _random.Next(1000, 1000);
                await Task.Delay(wait);
                var initConnection = new MessageRequest
                {
                    Id = instanceId,
                    Type = "RegexEngine"
                };

                _logger.LogInformation($"Message Processor[{initConnection.Id}]: Initial request");

                using var call = client.RequestMessage();

                var requestStream = call.RequestStream;
                var responseStream = call.ResponseStream;

                await requestStream.WriteAsync(initConnection);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await foreach (var response in responseStream.ReadAllAsync())
                        {
                            _logger.LogInformation($"Message Processor[{instanceId}]: Received response: {response.Id}, {response.Engine}, {response.MessageLength}, {response.IsValid}");
                        }
                    }
                    catch (RpcException ex)
                    {
                        _logger.LogError($"RPC Error: {ex.Status}, {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error receiving response: {ex.Message}");
                    }
                });

                while (true)
                {
                    wait = _random.Next(200, 200);
                    await Task.Delay(wait);

                    var newRequest = new MessageRequest()
                    {
                        Id = instanceId,
                        Type = "RegexEngine"
                    };

                    _logger.LogInformation($"Message Processor[{newRequest.Id}]: Requesting for a new message");

                    await requestStream.WriteAsync(newRequest);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in StartTask: {ex.Message}");
            }
        }
    }
}
