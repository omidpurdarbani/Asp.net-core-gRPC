using FluentAssertions;
using Message.Processor.Services;
using Message.Splitter.Store;
using Microsoft.Extensions.Logging;
using Moq;

namespace Message.Splitter.Tests
{
    public class ClientCheckerTests
    {
        private readonly Mock<ILogger<ClientChecker>> _loggerMock;
        private readonly ClientChecker _clientChecker;

        public ClientCheckerTests()
        {
            _loggerMock = new Mock<ILogger<ClientChecker>>();
            _clientChecker = new ClientChecker(_loggerMock.Object, 2);
        }

        [Fact]
        public async Task ClientChecker_Disables_Inactive_Users_Correctly()
        {
            // Arrange
            ApplicationStore.ProcessClientsList.Add(new ProcessClients
            {
                Id = "client1",
                IsEnabled = true,
                LastTransactionTime = DateTime.Now.AddMinutes(-10)
            });


            // Act
            var cancellationTokenSource = new CancellationTokenSource();
            await _clientChecker.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(2500);


            // Assert
            ApplicationStore.ProcessClientsList[0].IsEnabled.Should().BeFalse();

            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Disabled client with id: client1")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!
                ),
                Times.Once
            );
        }
    }

}
