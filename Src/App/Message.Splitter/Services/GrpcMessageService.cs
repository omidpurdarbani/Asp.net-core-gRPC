using Grpc.Core;
using GrpcMessage;
using Microsoft.Extensions.Logging;

namespace Message.Splitter.Services
{
    public class GrpcMessageService : MessageSplitter.MessageSplitterBase
    {
        private readonly MessageService _messageService;
        private readonly ILogger<GrpcMessageService> _logger;

        public GrpcMessageService(MessageService messageService, ILogger<GrpcMessageService> logger)
        {
            _messageService = messageService;
            _logger = logger;
        }

        public override async Task RequestMessage(IAsyncStreamReader<MessageRequest> requestStream, IServerStreamWriter<MessageResponse> responseStream, ServerCallContext context)
        {
            await foreach (var request in requestStream.ReadAllAsync())
            {
                try
                {
                    _logger.LogInformation($"Message Splitter: Received message request with ID: {request.Id}");

                    var processResponse = await _messageService.StartTask();
                    var messageResponse = new MessageResponse()
                    {
                        Id = processResponse.Id,
                        MessageLength = processResponse.MessageLength,
                        Engine = processResponse.Engine,
                        IsValid = processResponse.IsValid
                    };
                    _logger.LogInformation($"Message Splitter: Sending response for ID: {request.Id}");

                    await responseStream.WriteAsync(messageResponse);
                }
                catch (RpcException ex)
                {
                    _logger.LogError($"RPC Error: {ex.Status}, {ex.Message}");
                    throw; // Rethrow the exception
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in RequestMessage: {ex.Message}");
                    throw; // Rethrow the exception
                }
            }
        }
    }
}