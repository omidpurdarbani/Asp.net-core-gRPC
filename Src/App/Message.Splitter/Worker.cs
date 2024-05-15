using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MessageDispatcher.Services
{
    public class Worker : BackgroundService
    {
        private readonly MessageService _messageService;
        private readonly ILogger<Worker> _logger;

        public Worker(MessageService messageService, ILogger<Worker> logger)
        {
            _messageService = messageService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Message Splitter started.");
            await _messageService.StartProcessingMessages();

        }
    }
}