using Grpc.Core;
using GrpcMessage;
using Message.Processor.Persistence.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Message.Splitter.Tests;

public class MessageServiceTests
{
    [Fact]
    public async Task MessageService_ProcessesMessageCorrectly()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MessageService>>();
        var messageService = new MessageService(loggerMock.Object);

        var messageQueueRequest = new MessageQueueRequest { Id = "test" };
        var messageRequest = new MessageRequest { Id = "test" };
        var responseStreamMock = new Mock<IServerStreamWriter<MessageResponse>>();

        // Act
        await messageService.ProcessMessageAndSendResponse(messageQueueRequest, messageRequest, responseStreamMock.Object);

        // Assert
        // Verify that the message processing and response sending logic was called
        responseStreamMock.Verify(rs => rs.WriteAsync(It.IsAny<MessageResponse>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
}