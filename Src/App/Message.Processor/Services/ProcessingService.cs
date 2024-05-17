using Grpc.Core;
using Grpc.Net.Client;
using GrpcMessage;
using Message.Processor.Persistence.Interfaces;
using Microsoft.Extensions.Logging;

namespace Message.Processor.Services
{
    public class ProcessingService
    {
        private readonly ILogger<ProcessingService> _logger;
        private readonly IMessageService _messageService;
        private readonly MessageSplitter.MessageSplitterClient _client;

        public ProcessingService(ILogger<ProcessingService> logger, IMessageService messageService)
        {
            _logger = logger;
            _messageService = messageService;

            var channel = GrpcChannel.ForAddress("http://localhost:6001");
            _client = new MessageSplitter.MessageSplitterClient(channel);
        }

        public async Task StartTask(string instanceId)
        {
            _logger.LogInformation("Message Processor[{instanceId}]: Created", instanceId);

            try
            {
                //start call
                using var call = _client.RequestMessage();
                var requestStream = call.RequestStream;
                var responseStream = call.ResponseStream;

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

                var request = _messageService.InitialRequest(instanceId);

                await requestStream.WriteAsync(request);
                _logger.LogInformation("Message Processor[{instanceId}]: Initial request", instanceId);

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

            }
            catch (Exception ex)
            {
                _logger.LogError("Message Processor[{instanceId}]: Error in StartTask: {ex.Message}", instanceId, ex.Message);
            }
        }
    }
}
