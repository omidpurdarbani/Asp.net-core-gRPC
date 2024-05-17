using Grpc.Core;
using GrpcMessage;
using Message.Processor.Persistence.Interfaces;

namespace Message.Processor.Services
{
    public class GrpcProcessingService : MessageProcessor.MessageProcessorBase
    {
        private readonly IMessageService _messageService;

        public GrpcProcessingService(IMessageService messageService)
        {
            _messageService = messageService;
        }

        public override async Task ProcessMessage(IAsyncStreamReader<MessageQueueRequest> requestStream, IServerStreamWriter<ProcessResponse> responseStream, ServerCallContext context)
        {
            await foreach (var request in requestStream.ReadAllAsync())
            {
                var res = _messageService.ProcessMessage(request);
                await responseStream.WriteAsync(res);
            }
        }
    }
}