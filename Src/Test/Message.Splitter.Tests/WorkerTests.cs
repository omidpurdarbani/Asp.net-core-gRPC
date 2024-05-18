using Message.Processor.Persistence.Interfaces;
using Message.Splitter.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Message.Splitter.Tests;

public class WorkerTests
{
    [Fact]
    public async Task Worker_StartsAndRunsCorrectly()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<Worker>>();
        var grpcLoggerMock = new Mock<ILogger<GrpcMessageService>>();
        var message = new Mock<IMessageService>();
        var grpcServiceMock = new GrpcMessageService(message.Object, grpcLoggerMock.Object);
        var worker = new Worker(grpcServiceMock, loggerMock.Object);

        // Act
        var cancellationTokenSource = new CancellationTokenSource();
        var executeTask = worker.StartAsync(cancellationTokenSource.Token);

        // Assert
        Assert.NotNull(executeTask);
        await cancellationTokenSource.CancelAsync();
        await executeTask;
    }
}