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
        private readonly IMessageService _messageService;
        private readonly MessageSplitter.MessageSplitterClient _client;

        public Processor(ILogger<Processor> logger, IMessageService messageService)
        {
            _logger = logger;
            _messageService = messageService;

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
                    //start the call
                    using var call = _client.RequestMessage();
                    var requestStream = call.RequestStream;
                    var responseStream = call.ResponseStream;
                    isCanceled = false;

                    #region Get Responses

                    //this will run on different threads
                    //log responses when received
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            //this will run until the request is closed by RequestMessage on message.splitter
                            await foreach (var response in responseStream.ReadAllAsync())
                            {
                                _logger.LogInformation("Message Processor[{instanceId}]: Received response: {response.Id}, {response.Engine}, {response.MessageLength}, {response.IsValid}, {response.AdditionalFields.Count}", instanceId, response.Id, response.Engine, response.MessageLength, response.IsValid, response.AdditionalFields);
                            }
                        }
                        catch (RpcException ex)
                        {
                            if (ex.StatusCode == StatusCode.PermissionDenied)
                            {
                                _logger.LogError("Receiving response RPC Error: {ex.Status}, {ex.Message}", ex.Status, ex.Message);
                            }
                            if (ex.StatusCode == StatusCode.Cancelled)
                            {
                                _logger.LogWarning("Message Processor[{instanceId}]: RPC Error: {ex.Status}, {ex.Message}", instanceId, ex.Status, ex.Message);
                                isCanceled = true;
                            }

                            _logger.LogError("Receiving response RPC Error: {ex.Status}, {ex.Message}", ex.Status, ex.Message);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Receiving response Error: {ex.Message}", ex.Message);
                        }

                    });

                    #endregion

                    #region Send Requests

                    #region Initial Request

                    if (firstTime)
                    {
                        var request = await _messageService.InitialRequest(instanceId);

                        await requestStream.WriteAsync(request);
                        _logger.LogInformation("Message Processor[{instanceId}]: Initial request", instanceId);

                        firstTime = false;
                    }

                    #endregion

                    #region Request new messages

                    //sending requests on a loop
                    //this will run on main thread so the call will never end
                    while (true)
                    {
                        try
                        {
                            var newRequest = await _messageService.RequestMessage(instanceId);

                            await requestStream.WriteAsync(newRequest);
                            _logger.LogInformation("Message Processor[{newRequest.Id}]: Requesting for a new message",
                                newRequest.Id);

                        }
                        catch (RpcException ex)
                        {
                            if (ex.StatusCode == StatusCode.PermissionDenied)
                            {
                                _logger.LogError("Receiving response RPC Error: {ex.Status}, {ex.Message}", ex.Status, ex.Message);
                                break;
                            }
                            if (ex.StatusCode == StatusCode.Cancelled)
                            {
                                _logger.LogWarning("Message Processor[{instanceId}]: RPC Error: {ex.Status}, {ex.Message}", instanceId, ex.Status, ex.Message);
                                isCanceled = true;
                                break;
                            }

                            _logger.LogError("Requesting message RPC Error: {ex.Status}, {ex.Message}", ex.Status,
                                ex.Message);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Requesting message Error: {ex.Message}", ex.Message);
                        }

                    }


                    #endregion

                    #endregion

                } while (isCanceled);
            }
            catch (Exception ex)
            {
                _logger.LogError("Message Processor[{instanceId}]: Error in StartTask: {ex.Message}", instanceId, ex.Message);
            }
        }
    }
}
