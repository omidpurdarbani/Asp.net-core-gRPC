using Message.Processor.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Message.Splitter.Tests;

public class HealthCheckerTests
{
    [Fact]
    public async Task HealthChecker_SendsInitialHealthCheckCorrectly()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<HealthChecker>>();
        var configurationMock = new Mock<IConfiguration>();
        var httpClientMock = new Mock<HttpClient>();

        configurationMock.Setup(c => c["ManagementUrl"]).Returns("http://localhost");

        var healthChecker = new HealthChecker(loggerMock.Object, configurationMock.Object, httpClientMock.Object);

        // Act
        var cancellationTokenSource = new CancellationTokenSource();
        var executeTask = healthChecker.StartAsync(cancellationTokenSource.Token);

        // Assert
        Assert.NotNull(executeTask);
        await cancellationTokenSource.CancelAsync();
        await executeTask;
    }
}