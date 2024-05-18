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
        var grpcServiceMock = new Mock<GrpcMessageService>(null, loggerMock.Object);
        var worker = new Worker(grpcServiceMock.Object, loggerMock.Object);

        // Act
        var cancellationTokenSource = new CancellationTokenSource();
        var executeTask = worker.StartAsync(cancellationTokenSource.Token);

        // Assert
        Assert.NotNull(executeTask);
        cancellationTokenSource.Cancel();
        await executeTask;
    }
}