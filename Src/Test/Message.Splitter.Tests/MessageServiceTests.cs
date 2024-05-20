using System.Diagnostics;
using FluentAssertions;
using GrpcMessage;
using Message.Processor.Persistence.Services;
using Message.Splitter.Store;
using Microsoft.Extensions.Logging;
using Moq;

namespace Message.Splitter.Tests;

public class MessageServiceTests
{
    private readonly Mock<ILogger<MessageService>> _loggerMock;
    private readonly Mock<MessageProcessor.MessageProcessorClient> _clientMock;

    private readonly MessageService _messageService;

    public MessageServiceTests()
    {
        _loggerMock = new Mock<ILogger<MessageService>>();
        _clientMock = new Mock<MessageProcessor.MessageProcessorClient>();

        _messageService = new MessageService(_loggerMock.Object, _clientMock.Object);
    }

    [Fact]
    public async Task MessageService_Handles_GetMessageFromQueue_Correctly()
    {
        // Arrange
        var timer = new Stopwatch();


        // Act
        timer.Start();
        var res = await _messageService.GetMessageFromQueue();
        timer.Stop();


        // Assert
        timer.Elapsed.Should().BeCloseTo(TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(100));
        res.Should().NotBeNull();
        res.Id.Should().NotBeNull();
        res.Sender.Should().NotBeNull();
        res.Message.Should().NotBeNull();
        res.AdditionalFields.Should().NotBeNull();
        res.AdditionalFields.Count.Should().BeGreaterThan(1);
    }

    [Fact]
    public async Task MessageService_IsApplicationEnabled_Works_Correctly()
    {
        // Arrange
        ApplicationStore.IsEnabled = true;
        ApplicationStore.ExpirationTime = DateTime.Now.AddMinutes(1);


        // Act
        var res = _messageService.IsApplicationEnabled();


        // Assert
        res.Should().BeTrue();
    }

    [Fact]
    public async Task MessageService_IsApplicationEnabled_Works_Correctly_When_Application_Is_Not_Enabled()
    {
        // Arrange
        ApplicationStore.IsEnabled = false;
        ApplicationStore.ExpirationTime = DateTime.Now.AddMinutes(1);


        // Act
        var res = _messageService.IsApplicationEnabled();


        // Assert
        res.Should().BeFalse();
    }

    [Fact]
    public async Task MessageService_IsApplicationEnabled_Works_Correctly_When_ExpirationTime_Is_Expired()
    {
        // Arrange
        ApplicationStore.IsEnabled = true;
        ApplicationStore.ExpirationTime = DateTime.Now.AddMinutes(-1);


        // Act
        var res = _messageService.IsApplicationEnabled();


        // Assert
        res.Should().BeFalse();
    }

    [Fact]
    public async Task MessageService_IsClientEnabled_Works_Correctly()
    {
        // Arrange
        ApplicationStore.ProcessClientsList.Add(new ProcessClients
        {
            Id = "client1",
            IsEnabled = true,
            LastTransactionTime = DateTime.Now
        });


        // Act
        var res = _messageService.IsClientEnabled("client1");


        // Assert
        res.Should().BeTrue();
    }

    [Fact]
    public async Task MessageService_ProcessClient_Registers_New_Clients_Correctly()
    {
        // Arrange
        var client = new MessageRequest
        {
            Id = "new",
            Type = ""
        };


        // Act
        var newClient = _messageService.ProcessClient(client);


        // Assert
        newClient.Should().BeTrue();
    }

    [Fact]
    public async Task MessageService_ProcessClient_Works_Correctly()
    {
        // Arrange
        ApplicationStore.NumberOfMaximumActiveClients = 2;
        ApplicationStore.ProcessClientsList.Add(new ProcessClients
        {
            Id = "client1",
            IsEnabled = false,
            LastTransactionTime = DateTime.Now
        });
        ApplicationStore.ProcessClientsList.Add(new ProcessClients
        {
            Id = "client2",
            IsEnabled = true,
            LastTransactionTime = DateTime.Now.AddMinutes(-6)
        });
        ApplicationStore.ProcessClientsList.Add(new ProcessClients
        {
            Id = "client3",
            IsEnabled = true,
            LastTransactionTime = DateTime.Now.AddMinutes(-9)
        });
        var client = new MessageRequest
        {
            Id = "client1",
            Type = ""
        };


        // Act
        var newClient = _messageService.ProcessClient(client);


        // Assert
        newClient.Should().BeFalse();
        ApplicationStore.ProcessClientsList[0].IsEnabled.Should().BeTrue();
    }
}