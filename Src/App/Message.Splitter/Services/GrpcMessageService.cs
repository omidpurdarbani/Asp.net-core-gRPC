using Grpc.Core;
using Grpc.Net.Client;
using GrpcMessage;
using Message.Processor.Persistence.Interfaces;
using Message.Splitter.Store;
using Microsoft.Extensions.Logging;

namespace Message.Splitter.Services
{
    public class GrpcMessageService : MessageSplitter.MessageSplitterBase
    {
        private readonly IMessageService _messageService;
        private readonly ILogger<GrpcMessageService> _logger;
        private readonly MessageProcessor.MessageProcessorClient _client;

        public GrpcMessageService(IMessageService messageService, ILogger<GrpcMessageService> logger)
        {
            _messageService = messageService;
            _logger = logger;

            var channel = GrpcChannel.ForAddress("http://localhost:5001");
            _client = new MessageProcessor.MessageProcessorClient(channel);
        }

        public override async Task RequestMessage(IAsyncStreamReader<MessageRequest> requestStream, IServerStreamWriter<MessageResponse> responseStream, ServerCallContext context)
        {
            //this will read incoming requests from a call until its marked as completed 
            await foreach (var request in requestStream.ReadAllAsync())
            {
                //checks for application's status
                if (!ApplicationStore.IsEnabled || ApplicationStore.ExpirationTime < DateTime.Now)
                {
                    //send error for calling service
                    _logger.LogWarning("Application is not enabled. Skipping request ID: {request.Id}", request.Id);
                    throw new RpcException(new Status(StatusCode.PermissionDenied, "Application is not enabled."));
                }

                try
                {

                    #region Register or check a processor

                    if (ApplicationStore.ProcessClientsList.All(p => p.Id != request.Id))
                    {
                        ApplicationStore.ProcessClientsList.Add(new ProcessClients
                        {
                            Id = request.Id,
                            Type = request.Type,
                            LastTransactionTime = DateTime.Now,
                            IsEnabled = false
                        });
                        continue;
                    }

                    var storeProcess = ApplicationStore.ProcessClientsList.FirstOrDefault(p => p.Id == request.Id)!;

                    if (!storeProcess.IsEnabled && ApplicationStore.ProcessClientsList.Count(p => p.IsEnabled && DateTime.Now <= p.LastTransactionTime.AddMinutes(5)) < ApplicationStore.NumberOfMaximumActiveClients)
                    {
                        storeProcess.IsEnabled = true;
                    }

                    if (!storeProcess.IsEnabled)
                    {
                        continue;
                    }


                    #endregion

                    _logger.LogInformation("Message Splitter: Received message request with ID: {request.Id}", request.Id);
                    var message = await _messageService.GetMessageFromQueue();

                    #region Get results from Message.Processor

                    //starting call for Message.Processor
                    using var call = _client.ProcessMessage();
                    var requestStreamProcess = call.RequestStream;
                    var responseStreamProcess = call.ResponseStream;

                    _logger.LogInformation("Message Splitter: Sending message to process: {message.Message}", message.Message);
                    await requestStreamProcess.WriteAsync(message);
                    await requestStreamProcess.CompleteAsync();

                    //this will run until the request is closed by ProcessMessage on message.processor
                    await foreach (var response in responseStreamProcess.ReadAllAsync())
                    {
                        _logger.LogInformation("Message Splitter: Processed message: MessageLength: {response.MessageLength}, IsValid: {response.IsValid}", response.MessageLength, response.IsValid);

                        var messageResponse = new MessageResponse()
                        {
                            Id = response.Id,
                            MessageLength = response.MessageLength,
                            Engine = response.Engine,
                            IsValid = response.IsValid,
                            AdditionalFields = { response.AdditionalFields }
                        };

                        _logger.LogInformation("Message Splitter: Sending response for ID: {request.Id}", request.Id);

                        await responseStream.WriteAsync(messageResponse);
                    }

                    #endregion 
                }
                catch (RpcException ex)
                {
                    _logger.LogError("RPC Error: {ex.Status}, {ex.Message}", ex.Status, ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error in RequestMessage: {ex.Message}", ex.Message);
                }
            }
        }

    }
}