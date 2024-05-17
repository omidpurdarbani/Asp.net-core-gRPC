using Grpc.Core;
using GrpcMessage;
using Message.Processor.Persistence.Interfaces;
using Microsoft.Extensions.Logging;

namespace Message.Processor.Services
{
    public class GrpcProcessingService : MessageProcessor.MessageProcessorBase
    {
        private readonly IMessageService _messageService;
        private readonly ILogger<GrpcProcessingService> _logger;

        public GrpcProcessingService(IMessageService messageService, ILogger<GrpcProcessingService> logger)
        {
            _messageService = messageService;
            _logger = logger;
        }

        public override async Task ProcessMessage(IAsyncStreamReader<MessageQueueRequest> requestStream, IServerStreamWriter<ProcessResponse> responseStream, ServerCallContext context)
        {
            //this will read incoming requests from a call until its marked as completed 
            await foreach (var request in requestStream.ReadAllAsync().ConfigureAwait(false))
            {
                try
                {
                    var res = _messageService.ProcessMessage(request);
                    await responseStream.WriteAsync(res).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error in ProcessMessage: {ex.Message}", ex.Message);
                }
            }
        }
    }
}