using GrpcMessage;

namespace Message.Processor.Persistence.Interfaces
{
    public interface IMessageService
    {
        public Task<MessageQueueRequest> GetMessageFromQueue();
    }
}
