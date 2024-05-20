using FluentAssertions;
using Grpc.Core;
using GrpcMessage;
using Message.Processor.Persistence.Interfaces;
using Message.Splitter.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Message.Splitter.Tests;

public class GrpcMessageServiceTests
{
    private readonly Mock<ILogger<GrpcMessageService>> _loggerMock;
    private readonly Mock<IMessageService> _messageServiceMock;
    private readonly Mock<IAsyncStreamReader<MessageRequest>> _requestStreamMock;
    private readonly Mock<IServerStreamWriter<MessageResponse>> _responseStreamMock;
    private readonly Mock<ServerCallContext> _serverCallContextMock;

    private readonly GrpcMessageService _grpcService;

    public GrpcMessageServiceTests()
    {
        _loggerMock = new Mock<ILogger<GrpcMessageService>>();
        _messageServiceMock = new Mock<IMessageService>();
        _requestStreamMock = new Mock<IAsyncStreamReader<MessageRequest>>();
        _responseStreamMock = new Mock<IServerStreamWriter<MessageResponse>>();
        _serverCallContextMock = new Mock<ServerCallContext>();

        _grpcService = new GrpcMessageService(_messageServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GrpcMessageService_Handles_Message_Request_Correctly()
    {
        // Arrange
        _requestStreamMock.SetupSequence(r => r.MoveNext(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        var messageRequest = new MessageRequest { Id = "test" };

        _requestStreamMock.Setup(r => r.Current).Returns(messageRequest);

        _messageServiceMock.Setup(r => r.IsApplicationEnabled()).Returns(true);

        _messageServiceMock.Setup(r => r.ProcessClient(It.IsAny<MessageRequest>())).Returns(false);

        _messageServiceMock.Setup(r => r.IsClientEnabled(It.IsAny<string>())).Returns(true);


        // Act
        await _grpcService.RequestMessage(_requestStreamMock.Object, _responseStreamMock.Object, _serverCallContextMock.Object);


        // Assert
        _messageServiceMock.Verify(ms => ms.ProcessMessageAndSendResponse(It.IsAny<MessageQueueRequest>(), messageRequest, _responseStreamMock.Object), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Message Splitter: Received message request with ID: test")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task GrpcMessageService_Handles_Application_Not_Enabled_Correctly()
    {
        // Arrange
        _requestStreamMock.SetupSequence(r => r.MoveNext(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        var messageRequest = new MessageRequest { Id = "test" };

        _requestStreamMock.Setup(r => r.Current).Returns(messageRequest);

        //Application is not enabled
        _messageServiceMock.Setup(r => r.IsApplicationEnabled()).Returns(false);

        _messageServiceMock.Setup(r => r.ProcessClient(It.IsAny<MessageRequest>())).Returns(false);

        _messageServiceMock.Setup(r => r.IsClientEnabled(It.IsAny<string>())).Returns(true);


        // Act
        try
        {
            await _grpcService.RequestMessage(_requestStreamMock.Object, _responseStreamMock.Object, _serverCallContextMock.Object);
        }


        //Assert
        catch (RpcException e)
        {
            e.StatusCode.Should().Be(StatusCode.PermissionDenied);
            e.Message.Should().Contain("Application is not enabled.");
        }
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Application is not enabled. Skipping request ID: test")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!
            ),
            Times.Once
        );
        _messageServiceMock.Verify(ms => ms.ProcessMessageAndSendResponse(It.IsAny<MessageQueueRequest>(), messageRequest, _responseStreamMock.Object), Times.Never);
    }

    [Fact]
    public async Task GrpcMessageService_Handles_New_Client_Correctly()
    {
        // Arrange
        _requestStreamMock.SetupSequence(r => r.MoveNext(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        var messageRequest = new MessageRequest { Id = "test" };

        _requestStreamMock.Setup(r => r.Current).Returns(messageRequest);

        _messageServiceMock.Setup(r => r.IsApplicationEnabled()).Returns(true);

        //New Client
        _messageServiceMock.Setup(r => r.ProcessClient(It.IsAny<MessageRequest>())).Returns(true);

        _messageServiceMock.Setup(r => r.IsClientEnabled(It.IsAny<string>())).Returns(true);


        // Act
        await _grpcService.RequestMessage(_requestStreamMock.Object, _responseStreamMock.Object, _serverCallContextMock.Object);


        //Assert
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Message Splitter: Process registered process with ID: test")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!
            ),
            Times.Once
        );
        _messageServiceMock.Verify(ms => ms.ProcessMessageAndSendResponse(It.IsAny<MessageQueueRequest>(), messageRequest, _responseStreamMock.Object), Times.Never);
    }

    [Fact]
    public async Task GrpcMessageService_Handles_InActive_Client_Correctly()
    {
        // Arrange
        _requestStreamMock.SetupSequence(r => r.MoveNext(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        var messageRequest = new MessageRequest { Id = "test" };

        _requestStreamMock.Setup(r => r.Current).Returns(messageRequest);

        _messageServiceMock.Setup(r => r.IsApplicationEnabled()).Returns(true);

        _messageServiceMock.Setup(r => r.ProcessClient(It.IsAny<MessageRequest>())).Returns(false);

        //InActive Client
        _messageServiceMock.Setup(r => r.IsClientEnabled(It.IsAny<string>())).Returns(false);


        // Act
        try
        {
            await _grpcService.RequestMessage(_requestStreamMock.Object, _responseStreamMock.Object, _serverCallContextMock.Object);
        }


        //Assert
        catch (RpcException e)
        {
            e.StatusCode.Should().Be(StatusCode.Cancelled);
            e.Message.Should().Contain("Processor is not enabled.");
        }

        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Processor is not enabled. Skipping request ID: test")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!
            ),
            Times.Once
        );
        _messageServiceMock.Verify(ms => ms.ProcessMessageAndSendResponse(It.IsAny<MessageQueueRequest>(), messageRequest, _responseStreamMock.Object), Times.Never);
    }

}