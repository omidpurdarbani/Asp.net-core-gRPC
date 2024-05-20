using Message.Processor.Persistence.Interfaces;
using Message.Processor.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Message.Splitter.Tests
{
    public class WorkerTests
    {
        private readonly Mock<IProcessorService> _grpcProcessorService;
        private readonly Mock<ILogger<GrpcProcessingService>> _grpcLogger;
        private readonly GrpcProcessingService _grpcService;

        private readonly Mock<Processor.Processor> _processorMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger<Worker>> _loggerMock;
        private readonly Worker _worker;

        public WorkerTests()
        {
            _grpcProcessorService = new Mock<IProcessorService>();
            _grpcLogger = new Mock<ILogger<GrpcProcessingService>>();
            _grpcService = new GrpcProcessingService(_grpcProcessorService.Object, _grpcLogger.Object);

            _processorMock = new Mock<Processor.Processor>(Mock.Of<ILogger<Processor.Processor>>(), Mock.Of<IProcessorService>());
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<Worker>>();
            _worker = new Worker(_configurationMock.Object, _grpcService, _processorMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Worker_Starts_Correctly()
        {
            // Arrange
            _configurationMock.Setup(c => c["NumberOfInstances"]).Returns("6");


            // Act
            var cancellationTokenSource = new CancellationTokenSource();
            await _worker.StartAsync(cancellationTokenSource.Token);


            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Message Processor has started...")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!
                ),
                Times.Once
            );
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Creating Message Processor")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!
                ),
                Times.Exactly(6)
            );
        }
    }
}
