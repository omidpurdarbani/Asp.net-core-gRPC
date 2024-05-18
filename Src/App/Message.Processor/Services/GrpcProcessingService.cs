using Grpc.Core;
using GrpcMessage;
using Message.Processor.Persistence.Interfaces;
using Microsoft.Extensions.Logging;

namespace Message.Processor.Services
{
    public class GrpcProcessingService : MessageProcessor.MessageProcessorBase
    {
        private readonly IProcessorService _processorService;
        private readonly ILogger<GrpcProcessingService> _logger;

        public GrpcProcessingService(IProcessorService processorService, ILogger<GrpcProcessingService> logger)
        {
            _processorService = processorService;
            _logger = logger;
        }

        public override async Task ProcessMessage(IAsyncStreamReader<MessageQueueRequest> requestStream, IServerStreamWriter<ProcessResponse> responseStream, ServerCallContext context)
        {
            //this will read incoming requests from a call until its marked as completed 
            await foreach (var request in requestStream.ReadAllAsync().ConfigureAwait(false))
            {
                try
                {
                    var res = _processorService.ProcessMessage(request);
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