using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using Message.Splitter.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Message.Processor.Services;

public class HealthChecker : BackgroundService
{
    private readonly ILogger<HealthChecker> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly Guid _id;
    private bool _active;

    public HealthChecker(ILogger<HealthChecker> logger, IConfiguration configuration, HttpClient httpClient)
    {
        _logger = logger;
        _active = true;
        _configuration = configuration;
        _httpClient = httpClient;
        _id = GenerateGuid();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Health Checker has started...");

        int period = 30;

        using PeriodicTimer timer = new(TimeSpan.FromSeconds(period));

        while (_active && !stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                var healthCheckInfo = new HealthCheckInfoDTO
                {
                    Id = _id.ToString(),
                    SystemTime = DateTime.UtcNow,
                    NumberOfConnectedClients = 5 //todo
                };

                var managementUrl = _configuration["ManagementUrl"] ?? "";
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
                _logger.LogError($"Failed to execute HealthChecker with exception message: {ex.Message}");
            }
        }
    }

    private async Task<HttpResponseMessage> SendHealthCheckAsync(string url, HealthCheckInfoDTO healthCheckInfo)
    {
        return await _httpClient.PostAsJsonAsync(url, healthCheckInfo);
    }

    private void DeActiveSystem()
    {
        _active = false;
    }

    private async Task RetryHealthCheckAsync(string url, HealthCheckInfoDTO healthCheckInfo)
    {
        for (var i = 0; i < 5; i++)
        {
            await Task.Delay(10000);

            var response = await SendHealthCheckAsync(url, healthCheckInfo);
            if (!response.IsSuccessStatusCode) continue;

            var managementResponse = await response.Content.ReadFromJsonAsync<ManagementResponseDTO>();
            if (managementResponse == null) continue;

            HandleManagementResponse(managementResponse);
            return;
        }

        // Disable the service
        _logger.LogError("Failed to execute HealthChecker after 5 retries.");
        DeActiveSystem();
    }

    private void HandleManagementResponse(ManagementResponseDTO response)
    {
        if (!response.IsEnabled)
        {
            // Disable message processing
            _logger.LogInformation("Disabling message processing as per management response.");
            DeActiveSystem();

        }

        if (response.ExpirationTime < DateTime.UtcNow)
        {
            // Expiration time has passed, disable the service
            _logger.LogInformation("Service has expired as per management response.");
            DeActiveSystem();

        }

        // Handle other fields as necessary

        _logger.LogInformation("Health Checker has been executed...");
    }

    private static Guid GenerateGuid()
    {
        var macAddress = GetMacAddress()!;
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(macAddress));
        return new Guid(hash);
    }
    private static string? GetMacAddress()
    {
        return NetworkInterface
            .GetAllNetworkInterfaces()
            .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .Select(nic => nic.GetPhysicalAddress().ToString())
            .FirstOrDefault();
    }
}
