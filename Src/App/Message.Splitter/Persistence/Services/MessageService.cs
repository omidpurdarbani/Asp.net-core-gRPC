using GrpcMessage;
using Message.Processor.Persistence.Interfaces;
using Message.Splitter.Helper;

namespace Message.Processor.Persistence.Services
{
    public class MessageService : IMessageService
    {
        private readonly Tools _tools;

        public MessageService()
        {
            _tools = new Tools();
        }
        public async Task<MessageQueueRequest> GetMessageFromQueue()
        {
            var message = await _tools.GenerateRandomMessage();
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
    }
}
