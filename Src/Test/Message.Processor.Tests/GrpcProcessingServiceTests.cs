using Grpc.Core;
using GrpcMessage;
using Message.Processor.Persistence.Interfaces;
using Message.Processor.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Message.Processor.Tests
{
    public class GrpcProcessingServiceTests
    {
        private readonly Mock<IProcessorService> _messageServiceMock;
        private readonly Mock<ILogger<GrpcProcessingService>> _loggerMock;
        private readonly GrpcProcessingService _grpcProcessingService;

        public GrpcProcessingServiceTests()
        {
            _messageServiceMock = new Mock<IProcessorService>();
            _loggerMock = new Mock<ILogger<GrpcProcessingService>>();
            _grpcProcessingService = new GrpcProcessingService(_messageServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task ProcessMessage_ProcessesRequest()
        {
            // Arrange
            var requestStreamMock = new Mock<IAsyncStreamReader<MessageQueueRequest>>();
            var responseStreamMock = new Mock<IServerStreamWriter<ProcessResponse>>();
            var contextMock = new Mock<ServerCallContext>();

            var request = new MessageQueueRequest { Id = "test-id", Message = "test-message" };
            requestStreamMock.SetupSequence(r => r.MoveNext(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            requestStreamMock.SetupGet(r => r.Current).Returns(request);

            var response = new ProcessResponse { Id = "test-id", IsValid = true };
            _messageServiceMock.Setup(m => m.ProcessMessage(request)).Returns(response);

            // Act
            await _grpcProcessingService.ProcessMessage(requestStreamMock.Object, responseStreamMock.Object, contextMock.Object);

            // Assert
            _messageServiceMock.Verify(m => m.ProcessMessage(request), Times.Once);
        }
    }
}
