using FluentAssertions;
using ManagementSystem.Controllers;
using ManagementSystem.DTOs;
using ManagementSystem.Persistence.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Message.Tests
{
    public class MessageManagementTests
    {
        private readonly Mock<IHealthCheckService> _mockHealthCheckService;
        private readonly HealthCheckController _controller;

        public MessageManagementTests()
        {
            _mockHealthCheckService = new Mock<IHealthCheckService>();
            _controller = new HealthCheckController(_mockHealthCheckService.Object);
        }

        [Fact]
        public void HealthCheck_ReturnsOkResult_WithHealthCheckResponse()
        {
            // Arrange
            var request = new HealthCheckRequest();
            var expectedResponse = new HealthCheckResponse
            {
                IsEnabled = true,
                NumberOfActiveClients = new Random().Next(1, 20),
                ExpirationTime = DateTime.Now.AddMinutes(10)
            };

            _mockHealthCheckService.Setup(service => service.CheckHealth(request)).Returns(expectedResponse);

            // Act
            var result = _controller.HealthCheck(request);

            // Assert
            var okResult = (result.Result as OkObjectResult)!;
            okResult.Should().NotBeNull();

            okResult.Value.Should().BeOfType<HealthCheckResponse>()
                .Which.ExpirationTime.Should()
                .BeCloseTo(DateTime.Now.AddMinutes(10), TimeSpan.FromSeconds(1));

            okResult.Value.Should().BeOfType<HealthCheckResponse>()
                .Which.NumberOfActiveClients.Should()
                .BeInRange(1, 20);
        }

        [Fact]
        public void HealthCheck_CallsCheckHealth_OnHealthCheckService()
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