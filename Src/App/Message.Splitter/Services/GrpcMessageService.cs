using Grpc.Core;
using Grpc.Net.Client;
using GrpcMessage;
using Message.Processor.Persistence.Interfaces;
using Microsoft.Extensions.Logging;

namespace Message.Splitter.Services
{
    public class GrpcMessageService : MessageSplitter.MessageSplitterBase
    {
        private readonly IMessageService _messageService;
        private readonly ILogger<GrpcMessageService> _logger;
        private readonly GrpcChannel _channel;

        public GrpcMessageService(IMessageService messageService, ILogger<GrpcMessageService> logger)
        {
            _messageService = messageService;
            _logger = logger;

        }

        public override async Task RequestMessage(IAsyncStreamReader<MessageRequest> requestStream, IServerStreamWriter<MessageResponse> responseStream, ServerCallContext context)
        {
            await foreach (var request in requestStream.ReadAllAsync().ConfigureAwait(false))
            {
                if (!_messageService.IsApplicationEnabled())
                {
                    LogAndThrowPermissionDenied(request.Id);
                }

                if (_messageService.ProcessClient(request))
                {
                    _logger.LogInformation("Message Splitter: Registered process with ID: {request.Id}", request.Id);
                    continue;
                }


                if (!_messageService.IsClientEnabled(request.Id))
                {
                    LogAndThrowCancelled(request.Id);
                }

                try
                {

                    _logger.LogInformation("Message Splitter: Received message request with ID: {request.Id}", request.Id);

                    var message = await _messageService.GetMessageFromQueue().ConfigureAwait(false);

                    await _messageService.ProcessMessageAndSendResponse(message, request, responseStream).ConfigureAwait(false);
                }
                catch (RpcException ex)
                {
                    _logger.LogError(ex, "RPC Error: {Status}, {Message}", ex.Status, ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in RequestMessage: {Message}", ex.Message);
                }
            }
        }

        private void LogAndThrowPermissionDenied(string requestId)
        {
            _logger.LogWarning("Application is not enabled. Skipping request ID: {requestId}", requestId);
            throw new RpcException(new Status(StatusCode.PermissionDenied, "Application is not enabled."));
        }

        private void LogAndThrowCancelled(string requestId)
        {
            _logger.LogWarning("Processor is not enabled. Skipping request ID: {requestId}", requestId);
            throw new RpcException(new Status(StatusCode.Cancelled, "Processor is not enabled."));
        }

    }
}
