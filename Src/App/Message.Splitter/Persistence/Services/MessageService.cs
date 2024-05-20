using Grpc.Core;
using GrpcMessage;
using Message.Processor.Persistence.Interfaces;
using Message.Splitter.Helper;
using Message.Splitter.Store;
using Microsoft.Extensions.Logging;

namespace Message.Processor.Persistence.Services
{
    public class MessageService : IMessageService
    {
        private readonly Tools _tools;
        private readonly MessageProcessor.MessageProcessorClient _client;
        private readonly ILogger<MessageService> _logger;


        public MessageService(ILogger<MessageService> logger, MessageProcessor.MessageProcessorClient client)
        {
            _tools = new Tools();
            _logger = logger;
            _client = client;
        }

        public async Task<MessageQueueRequest> GetMessageFromQueue()
        {
            await Task.Delay(200);
            var message = _tools.GenerateRandomMessage();
            return new MessageQueueRequest
            {
                Id = message.Id,
                Message = message.Message,
                Sender = message.Sender,
                AdditionalFields =
                 {
                     { "HasNumbers", @"\d" },
                     { "HasLetters", @"[a-zA-Z]" }
                 }
            };
        }

        public async Task ProcessMessageAndSendResponse(MessageQueueRequest message, MessageRequest request,
            IServerStreamWriter<MessageResponse> responseStream)
        {
            using var call = _client.ProcessMessage();
            var requestStreamProcess = call.RequestStream;
            var responseStreamProcess = call.ResponseStream;

            _logger.LogInformation("Message Splitter: Sending message to process: {message.Message}", message.Message);
            await requestStreamProcess.WriteAsync(message).ConfigureAwait(false);
            await requestStreamProcess.CompleteAsync().ConfigureAwait(false);

            await foreach (var response in responseStreamProcess.ReadAllAsync().ConfigureAwait(false))
            {
                _logger.LogInformation("Message Splitter: Processed message: MessageLength: {response.MessageLength}, IsValid: {response.IsValid}", response.MessageLength, response.IsValid);

                var messageResponse = new MessageResponse
                {
                    Id = response.Id,
                    MessageLength = response.MessageLength,
                    Engine = response.Engine,
                    IsValid = response.IsValid,
                    AdditionalFields = { response.AdditionalFields }
                };

                _logger.LogInformation("Message Splitter: Sending response for ID: {request.Id}", request.Id);

                await responseStream.WriteAsync(messageResponse).ConfigureAwait(false);
            }
        }

        public bool IsApplicationEnabled()
        {
            return ApplicationStore.IsEnabled && ApplicationStore.ExpirationTime >= DateTime.Now;
        }

        public bool IsClientEnabled(string requestId)
        {
            var storeProcess = ApplicationStore.ProcessClientsList.FirstOrDefault(p => p.Id == requestId);
            return storeProcess is { IsEnabled: true };
        }

        public bool ProcessClient(MessageRequest request)
        {
            var storeProcess = ApplicationStore.ProcessClientsList.FirstOrDefault(p => p.Id == request.Id);
            if (storeProcess == null)
            {
                ApplicationStore.ProcessClientsList.Add(new ProcessClients
                {
                    Id = request.Id,
                    Type = request.Type,
                    LastTransactionTime = DateTime.Now,
                    IsEnabled = false
                });
                return true;
            }
            else if (!storeProcess.IsEnabled && ApplicationStore.ProcessClientsList.Count(p => p.IsEnabled && DateTime.Now <= p.LastTransactionTime.AddMinutes(5)) < ApplicationStore.NumberOfMaximumActiveClients)
            {
                storeProcess.IsEnabled = true;
            }
            return false;
        }
    }
}
