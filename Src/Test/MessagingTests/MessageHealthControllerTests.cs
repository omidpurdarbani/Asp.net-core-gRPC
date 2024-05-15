using ManagementSystem.Controllers;
using ManagementSystem.DTOs;
using ManagementSystem.Persistence.Services;

namespace MessageManagementSystem.Tests
{
    public class MessageHealthControllerTests
    {

        [Fact]
        public void HealthCheck_ReturnsCorrectResponse()
        {
            // Arrange
            var controller = new HealthCheckController(new HealthCheckService());
            var request = new HealthCheckRequest
            {
                Id = Guid.NewGuid().ToString(),
                SystemTime = DateTime.UtcNow,
                NumberOfConnectedClients = 5
            };

            // Act
            var response = controller.HealthCheck(request).Value;

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsEnabled);
            Assert.InRange(response.NumberOfActiveClients, 0, request.NumberOfConnectedClients);
            Assert.Equal(DateTime.UtcNow.AddMinutes(10).ToString("O"), response.ExpirationTime.ToString("O"));
        }
    }
}