using Grpc.Core;
using Grpc.Net.Client;
using GrpcMessage;
using Message.Processor.Persistence.Interfaces;
using Microsoft.Extensions.Logging;

namespace Message.Processor
{
    public class Processor
    {
        private readonly ILogger<Processor> _logger;
        private readonly IProcessorService _processorService;
        private readonly MessageSplitter.MessageSplitterClient _client;

        public Processor(ILogger<Processor> logger, IProcessorService processorService)
        {
            _logger = logger;
            _processorService = processorService;

            var channel = GrpcChannel.ForAddress("http://localhost:6001");
            _client = new MessageSplitter.MessageSplitterClient(channel);
        }

        public async Task StartTask(string instanceId)
        {
            _logger.LogInformation("Message Processor[{instanceId}]: Created", instanceId);
            var isCanceled = false;
            var firstTime = true;
            try
            {
                do
                {
                    using var call = _client.RequestMessage();
                    var requestStream = call.RequestStream;
                    var responseStream = call.ResponseStream;
                    isCanceled = false;

                    //log responses when received
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            //this will run until the call iis marked completed ( in this case it will remain running until some exceptions is thrown ) 
                            await foreach (var response in responseStream.ReadAllAsync().ConfigureAwait(false))
                            {
                                _logger.LogInformation("Message Processor[{instanceId}]: Received response: {response.Id}, {response.Engine}, {response.MessageLength}, {response.IsValid}, {response.AdditionalFields.Count}", instanceId, response.Id, response.Engine, response.MessageLength, response.IsValid, response.AdditionalFields);
                            }
                        }
                        catch (RpcException ex)
                        {
                            if (ex.StatusCode == StatusCode.Cancelled) isCanceled = true;

                            _logger.LogWarning("Message Processor[{instanceId}]: No Response, RPC Error: {ex.Status}, {ex.Message}", instanceId, ex.Status, ex.Message);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Receiving response Error: {ex.Message}", ex.Message);
                        }

                    });

                    if (firstTime)
                    {
                        var request = await _processorService.InitialRequest(instanceId).ConfigureAwait(false);

                        await requestStream.WriteAsync(request);
                        _logger.LogInformation("Message Processor[{instanceId}]: Initial request", instanceId);

                        firstTime = false;
                    }

                    while (true)
                    {
                        try
                        {
                            var newRequest = await _processorService.RequestMessage(instanceId).ConfigureAwait(false);

                            await requestStream.WriteAsync(newRequest).ConfigureAwait(false);

                            _logger.LogInformation("Message Processor[{newRequest.Id}]: Requesting for a new message", newRequest.Id);
                        }
                        catch (RpcException ex)
                        {
                            _logger.LogWarning("Message Processor[{instanceId}]: RPC Error: {ex.Status}, {ex.Message}", instanceId, ex.Status, ex.Message);

                            if (ex.StatusCode == StatusCode.Cancelled) isCanceled = true;

                            break;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Requesting message Error: {ex.Message}", ex.Message);
                            break;
                        }
                    }

                } while (isCanceled);
            }
            catch (Exception ex)
            {
                _logger.LogError("Message Processor[{instanceId}]: Error in StartTask: {ex.Message}", instanceId, ex.Message);
            }
        }
    }
}
