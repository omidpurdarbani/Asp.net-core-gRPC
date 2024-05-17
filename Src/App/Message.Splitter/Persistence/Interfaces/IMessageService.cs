using Grpc.Core;
using GrpcMessage;

namespace Message.Processor.Persistence.Interfaces
{
    public interface IMessageService
    {
        public Task<MessageQueueRequest> GetMessageFromQueue();
        public Task ProcessMessageAndSendResponse(MessageQueueRequest message, MessageRequest request,
            IServerStreamWriter<MessageResponse> responseStream);

        public bool IsApplicationEnabled();
        public bool IsClientEnabled(string requestId);
        public void ProcessClient(MessageRequest request);

    }
}
