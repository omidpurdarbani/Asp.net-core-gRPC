using Message.Processor.Persistence.Interfaces;
using Message.Splitter.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Message.Splitter.Tests
{
    public class WorkerTests
    {
        private readonly Mock<IMessageService> _grpcMessageService;
        private readonly Mock<ILogger<GrpcMessageService>> _grpcLogger;
        private readonly GrpcMessageService _grpcService;

        private readonly Mock<ILogger<Worker>> _loggerMock;
        private readonly Worker _worker;

        public WorkerTests()
        {
            _grpcMessageService = new Mock<IMessageService>();
            _grpcLogger = new Mock<ILogger<GrpcMessageService>>();
            _grpcService = new GrpcMessageService(_grpcMessageService.Object, _grpcLogger.Object);

            _loggerMock = new Mock<ILogger<Worker>>();
            _worker = new Worker(_grpcService, _loggerMock.Object);
        }

        [Fact]
        public async Task Worker_Starts_Correctly()
        {
            // Arrange


            // Act
            var cancellationTokenSource = new CancellationTokenSource();
            await _worker.StartAsync(cancellationTokenSource.Token);


            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Message Splitter has started...")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!
                ),
                Times.Once
            );
        }
    }
}
