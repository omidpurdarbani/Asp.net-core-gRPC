using FluentAssertions;
using GrpcMessage;
using Message.Processor.Persistence.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Message.Processor.Tests
{
    public class ProcessorServiceTests
    {
        private readonly Mock<ILogger<ProcessorService>> _loggerMock;
        private readonly ProcessorService _messageService;

        public ProcessorServiceTests()
        {
            _loggerMock = new Mock<ILogger<ProcessorService>>();
            _messageService = new ProcessorService(_loggerMock.Object);
        }

        [Fact]
        public async Task InitialRequest_ReturnsCorrectMessageRequest()
        {
            // Arrange
            var instanceId = "test-instance";


            // Act
            var result = await _messageService.InitialRequest(instanceId);


            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(instanceId);
            result.Type.Should().Be("RegexEngine");
        }

        [Fact]
        public async Task RequestMessage_ReturnsCorrectMessageRequest()
        {
            // Arrange
            var instanceId = "test-instance";

            // Act
            var result = await _messageService.RequestMessage(instanceId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(instanceId);
            result.Type.Should().Be("RegexEngine");
        }

        [Fact]
        public void ProcessMessage_ReturnsCorrectProcessResponse()
        {
            // Arrange
            var request = new MessageQueueRequest
            {
                Id = "test-id",
                Message = "test-message",
                AdditionalFields =
                {
                    { "HasNumbers", @"\d" },
                    { "HasLetters", @"[a-zA-Z]" }
                }
            };


            // Act
            var result = _messageService.ProcessMessage(request);


            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be("test-id");
            result.Engine.Should().Be("RegexEngine");
            result.MessageLength.Should().Be(12);
            result.IsValid.Should().BeTrue();
            result.AdditionalFields.First().Value.Should().BeFalse();
            result.AdditionalFields.Last().Value.Should().BeTrue();
        }
    }
}
