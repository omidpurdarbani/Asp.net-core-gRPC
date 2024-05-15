using Grpc.Core;
using GrpcMessage;
using Message.Splitter.Services;

namespace MessageProcessor.Services
{
    public class GrpcMessageService : MessageSplitter.MessageSplitterBase
    {
        private readonly MessageService _messageService;

        public GrpcMessageService(MessageService messageService)
        {
            _messageService = messageService;
        }

        public override async Task<MessageResponse> RequestMessage(MessageRequest request, ServerCallContext context)
        {
            await _messageService.StartTask();
            return await Task.FromResult(new MessageResponse());
        }
    }
}