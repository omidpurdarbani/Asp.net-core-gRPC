using FluentAssertions;
using ManagementSystem.Controllers;
using ManagementSystem.DTOs;
using ManagementSystem.Persistence.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Message.Management.Tests
{
    public class HealthCheckControllerTests
    {
        private readonly Mock<IHealthCheckService> _mockHealthCheckService;
        private readonly HealthCheckController _controller;

        public HealthCheckControllerTests()
        {
            _mockHealthCheckService = new Mock<IHealthCheckService>();
            _controller = new HealthCheckController(_mockHealthCheckService.Object);
        }

        [Fact]
        public void HealthCheck_Returns_Ok_Result()
        {
            // Arrange
            var request = new HealthCheckRequest();
            var expectedResponse = new HealthCheckResponse
            {
                IsEnabled = true,
                NumberOfActiveClients = new Random().Next(0, 5),
                ExpirationTime = DateTime.Now.AddMinutes(10)
            };

            _mockHealthCheckService.Setup(service => service.CheckHealth(request)).Returns(expectedResponse);

            // Act
            var result = _controller.HealthCheck(request);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult?.Value.Should().NotBeNull();
        }

        [Fact]
        public void HealthCheck_Calls_CheckHealth_Once()
        {
            // Arrange
            var request = new HealthCheckRequest();

            // Act
            var result = _controller.HealthCheck(request);

            // Assert
            _mockHealthCheckService.Verify(service => service.CheckHealth(request), Times.Once);
        }

    }
}