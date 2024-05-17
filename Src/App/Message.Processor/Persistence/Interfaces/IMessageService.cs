using GrpcMessage;

namespace Message.Processor.Persistence.Interfaces
{
    public interface IMessageService
    {
        public MessageRequest InitialRequest(string instanceId);
        public Task<MessageRequest> RequestMessage(string instanceId);
        public ProcessResponse ProcessMessage(MessageQueueRequest request);
    }
}
