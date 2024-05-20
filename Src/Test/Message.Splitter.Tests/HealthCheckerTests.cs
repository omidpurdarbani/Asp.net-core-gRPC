using System.Net;
using System.Text.Json;
using Message.Processor.Services;
using Message.Splitter.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace Message.Splitter.Tests
{
    public class HealthCheckerTests
    {
        private readonly Mock<ILogger<HealthChecker>> _loggerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly HealthChecker _healthChecker;

        public HealthCheckerTests()
        {
            _loggerMock = new Mock<ILogger<HealthChecker>>();
            _configurationMock = new Mock<IConfiguration>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _healthChecker = new HealthChecker(_loggerMock.Object, _configurationMock.Object, _httpClient);
        }

        [Fact]
        public async Task HealthChecker_Sends_Initial_HealthCheckCorrectly()
        {
            // Arrange
            _configurationMock.Setup(c => c["ManagementUrl"]).Returns("https://localhost:7265/api/module/health");

            var responseContent = new ManagementResponseDTO
            {
                ExpirationTime = DateTime.Now.AddMinutes(10),
                IsEnabled = true,
                NumberOfActiveClients = 3
            };
            var jsonResponse = JsonSerializer.Serialize(responseContent);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            // Act
            var cancellationTokenSource = new CancellationTokenSource();
            await _healthChecker.StartAsync(cancellationTokenSource.Token);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Health Checker has started...")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!
                ),
                Times.Once
            );
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Health Checker has been executed successfully...")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task HealthChecker_Disables_System_Correctly()
        {
            // Arrange
            _configurationMock.Setup(c => c["ManagementUrl"]).Returns("https://localhost:7265/api/module/health");

            var responseContent = new ManagementResponseDTO
            {
                ExpirationTime = DateTime.Now.AddMinutes(10),
                IsEnabled = false,
                NumberOfActiveClients = 3
            };
            var jsonResponse = JsonSerializer.Serialize(responseContent);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            // Act
            var cancellationTokenSource = new CancellationTokenSource();
            await _healthChecker.StartAsync(cancellationTokenSource.Token);

            // Assert

            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Critical),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Health Checker is disabling system...")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!
                ),
                Times.Once
            );
        }
    }

}
