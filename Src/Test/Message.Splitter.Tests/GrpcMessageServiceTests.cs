using Grpc.Core;
using GrpcMessage;
using Message.Processor.Persistence.Interfaces;
using Message.Splitter.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Message.Splitter.Tests;

public class GrpcMessageServiceTests
{
    [Fact]
    public async Task GrpcMessageService_HandlesMessageRequestCorrectly()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<GrpcMessageService>>();
        var messageServiceMock = new Mock<IMessageService>();
        var grpcService = new GrpcMessageService(messageServiceMock.Object, loggerMock.Object);

        var requestStreamMock = new Mock<IAsyncStreamReader<MessageRequest>>();
        var responseStreamMock = new Mock<IServerStreamWriter<MessageResponse>>();
        var serverCallContextMock = new Mock<ServerCallContext>();

        requestStreamMock.SetupSequence(r => r.MoveNext(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        var messageRequest = new MessageRequest { Id = "test" };
        requestStreamMock.Setup(r => r.Current).Returns(messageRequest);
        messageServiceMock.Setup(r => r.IsApplicationEnabled()).Returns(true);
        messageServiceMock.Setup(r => r.IsClientEnabled(It.IsAny<string>())).Returns(true); messageServiceMock.Setup(r => r.ProcessClient(It.IsAny<MessageRequest>())).Returns(false);

        // Act
        await grpcService.RequestMessage(requestStreamMock.Object, responseStreamMock.Object, serverCallContextMock.Object);

        // Assert
        messageServiceMock.Verify(ms => ms.ProcessMessageAndSendResponse(It.IsAny<MessageQueueRequest>(), messageRequest, responseStreamMock.Object), Times.Once);
    }
}