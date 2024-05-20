using Grpc.Core;
using GrpcMessage;
using Message.Processor.Persistence.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace Message.Processor.Tests
{
    public class ProcessorTests
    {
        private readonly Mock<ILogger<Processor>> _loggerMock;
        private readonly Mock<IProcessorService> _messageServiceMock;
        private readonly Processor _processor;

        public ProcessorTests()
        {
            _loggerMock = new Mock<ILogger<Processor>>();
            _messageServiceMock = new Mock<IProcessorService>();
            _processor = new Processor(_loggerMock.Object, _messageServiceMock.Object);
        }

        [Fact]
        public async Task StartTask_CreatesInitialRequest()
        {
            // Arrange
            var instanceId = "test-instance";
            var initialRequest = new MessageRequest { Id = instanceId, Type = "RegexEngine" };
            _messageServiceMock.Setup(m => m.InitialRequest(instanceId)).ReturnsAsync(initialRequest);


            // Act
            await _processor.StartTask(instanceId);


            // Assert
            _messageServiceMock.Verify(m => m.InitialRequest(instanceId), Times.Once);
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Message Processor[{instanceId}]: Created")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task StartTask_HandlesRpcException()
        {
            // Arrange
            var instanceId = "test-instance2";
            _messageServiceMock.Setup(m => m.ProcessMessage(It.IsAny<MessageQueueRequest>())).Throws(new RpcException(new Status(StatusCode.Cancelled, "RPC Error")));

            // Act
            await _processor.StartTask(instanceId);
            await Task.Delay(2000);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("RPC Error")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!
                ),
                Times.AtLeastOnce
            );
        }
    }
}
