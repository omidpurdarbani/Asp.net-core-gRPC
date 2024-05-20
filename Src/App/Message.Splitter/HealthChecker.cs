using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using Message.Splitter.Helper;
using Message.Splitter.Models;
using Message.Splitter.Store;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("Message.Splitter.Tests")]

namespace Message.Processor.Services;

public class HealthChecker : BackgroundService
{
    private readonly ILogger<HealthChecker> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly Guid _id;
    private bool _active => ApplicationStore.IsEnabled;
    private readonly int _period;


    public HealthChecker(ILogger<HealthChecker> logger, IConfiguration configuration, HttpClient httpClient)
    {
        _logger = logger;
        _period = 30;
        _configuration = configuration;
        _httpClient = httpClient;
        _id = Tools.GenerateGuid();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Health Checker has started...");

        #region Initial Request

        var healthCheckInfo = new HealthCheckInfoDTO
        {
            Id = _id.ToString(),
            SystemTime = DateTime.UtcNow,
            NumberOfConnectedClients = ApplicationStore.ProcessClientsList.Count
        };
        var managementUrl = _configuration["ManagementUrl"] ?? "";

        try
        {
            var response = await SendHealthCheckAsync(managementUrl, healthCheckInfo);
            if (response.IsSuccessStatusCode)
            {
                var managementResponse = await response.Content.ReadFromJsonAsync<ManagementResponseDTO>(cancellationToken: stoppingToken);
                if (managementResponse != null)
                {
                    HandleManagementResponse(managementResponse);
                }
            }
            else
            {
                await RetryHealthCheckAsync(managementUrl, healthCheckInfo);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to execute HealthChecker with exception message: {ex.Message}", ex.Message);
            await RetryHealthCheckAsync(managementUrl, healthCheckInfo);
        }


        #endregion

        #region Priodic Request

        using PeriodicTimer timer = new(TimeSpan.FromSeconds(_period));

        while (_active && !stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            healthCheckInfo = new HealthCheckInfoDTO
            {
                Id = _id.ToString(),
                SystemTime = DateTime.UtcNow,
                NumberOfConnectedClients = ApplicationStore.ProcessClientsList.Count
            };
            try
            {
                var response = await SendHealthCheckAsync(managementUrl, healthCheckInfo);
                if (response.IsSuccessStatusCode)
                {
                    var managementResponse = await response.Content.ReadFromJsonAsync<ManagementResponseDTO>(cancellationToken: stoppingToken);
                    if (managementResponse != null)
                    {
                        HandleManagementResponse(managementResponse);
                    }
                }
                else
                {
                    await RetryHealthCheckAsync(managementUrl, healthCheckInfo);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to execute HealthChecker with exception message: {ex.Message}", ex.Message);
                await RetryHealthCheckAsync(managementUrl, healthCheckInfo);
            }
        }

        #endregion
    }

    private async Task<HttpResponseMessage> SendHealthCheckAsync(string url, HealthCheckInfoDTO healthCheckInfo)
    {
        return await _httpClient.PostAsJsonAsync(url, healthCheckInfo);
    }

    private void DeActivateSystem()
    {
        ApplicationStore.IsEnabled = false;
    }

    private async Task RetryHealthCheckAsync(string url, HealthCheckInfoDTO healthCheckInfo)
    {
        for (var i = 0; i < 5; i++)
        {
            await Task.Delay(10000);

            try
            {
                var response = await SendHealthCheckAsync(url, healthCheckInfo);
                if (!response.IsSuccessStatusCode) continue;

                var managementResponse = await response.Content.ReadFromJsonAsync<ManagementResponseDTO>();
                if (managementResponse == null) continue;

                HandleManagementResponse(managementResponse);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to execute HealthChecker with exception message: {ex.Message}", ex.Message);
            }
        }

        // Disable the service
        _logger.LogError("Failed to execute HealthChecker after 5 retries.");
        _logger.LogError("Health Checker is disabling system...");
        DeActivateSystem();
    }

    private void HandleManagementResponse(ManagementResponseDTO response)
    {
        if (!response.IsEnabled)
        {
            // Disable message splitting
            _logger.LogCritical("Health Checker is disabling system...");
        }

        ApplicationStore.IsEnabled = response.IsEnabled;
        ApplicationStore.ExpirationTime = response.ExpirationTime;
        ApplicationStore.NumberOfMaximumActiveClients = response.NumberOfActiveClients;

        _logger.LogInformation("Health Checker has been executed successfully...");
    }

}
