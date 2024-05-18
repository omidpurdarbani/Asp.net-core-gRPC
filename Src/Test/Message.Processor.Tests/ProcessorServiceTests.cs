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
            Assert.NotNull(result);
            Assert.Equal(instanceId, result.Id);
            Assert.Equal("RegexEngine", result.Type);
        }

        [Fact]
        public async Task RequestMessage_ReturnsCorrectMessageRequest()
        {
            // Arrange
            var instanceId = "test-instance";

            // Act
            var result = await _messageService.RequestMessage(instanceId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(instanceId, result.Id);
            Assert.Equal("RegexEngine", result.Type);
        }

        [Fact]
        public void ProcessMessage_ReturnsCorrectProcessResponse()
        {
            // Arrange
            var request = new MessageQueueRequest { Id = "test-id", Message = "test-message", AdditionalFields = { { "key", "value" } } };

            // Act
            var result = _messageService.ProcessMessage(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test-id", result.Id);
            Assert.Equal("RegexEngine", result.Engine);
            Assert.Equal(12, result.MessageLength);
            Assert.True(result.IsValid);
            Assert.Single(result.AdditionalFields);
        }
    }
}
