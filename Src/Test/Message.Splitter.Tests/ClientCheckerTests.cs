using Message.Processor.Services;
using Message.Splitter.Store;
using Microsoft.Extensions.Logging;
using Moq;

namespace Message.Splitter.Tests;

public class ClientCheckerTests
{
    [Fact]
    public async Task ClientChecker_DisablesInactiveClients()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ClientChecker>>();
        var clientChecker = new ClientChecker(loggerMock.Object);
        ApplicationStore.ProcessClientsList.Add(new ProcessClients
        {
            Id = "client1",
            IsEnabled = true,
            LastTransactionTime = DateTime.Now.AddMinutes(-10)
        });

        // Act
        var cancellationTokenSource = new CancellationTokenSource();
        await clientChecker.StartAsync(cancellationTokenSource.Token);

        // Assert
        await Task.Delay(65000, cancellationTokenSource.Token);
        Assert.False(ApplicationStore.ProcessClientsList[0].IsEnabled);
    }
}
