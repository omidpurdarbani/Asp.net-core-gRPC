using Message.Splitter.Store;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Message.Processor.Services;

public class ClientChecker : BackgroundService
{
    private readonly ILogger<ClientChecker> _logger;
    private readonly int _period;
    private bool _active => ApplicationStore.IsEnabled;

    public ClientChecker(ILogger<ClientChecker> logger)
    {
        _logger = logger;
        _period = 1;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Client Checker has started...");
        #region Priodic Request

        using PeriodicTimer timer = new(TimeSpan.FromMinutes(_period));

        while (_active && !stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            foreach (var clients in ApplicationStore.ProcessClientsList.Where(p => p.IsEnabled && p.LastTransactionTime.AddMinutes(5) <= DateTime.Now))
            {
                clients.IsEnabled = false;
            }
        }

        #endregion
    }
}
