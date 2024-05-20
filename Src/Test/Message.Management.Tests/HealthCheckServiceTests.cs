using FluentAssertions;
using ManagementSystem.DTOs;
using ManagementSystem.Persistence.Services;

namespace Message.Management.Tests
{
    public class HealthCheckServiceTests
    {
        private readonly HealthCheckService _checkService;

        public HealthCheckServiceTests()
        {
            _checkService = new HealthCheckService();
        }

        [Fact]
        public void CheckHealth_Returns_Expected_Result()
        {
            // Arrange
            var request = new HealthCheckRequest();


            // Act
            var result = _checkService.CheckHealth(request);


            // Assert
            result.Should().NotBeNull();
            result.IsEnabled.Should().BeTrue();
            result.ExpirationTime.Should().BeCloseTo(DateTime.Now.AddMinutes(10), TimeSpan.FromSeconds(1));
            result.NumberOfActiveClients.Should().BeInRange(0, 5);
        }
    }
}