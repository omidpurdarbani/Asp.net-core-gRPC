using ManagementSystem.DTOs;
using ManagementSystem.Persistence.Interfaces;

namespace ManagementSystem.Persistence.Services
{
    public class HealthCheckService : IHealthCheckService
    {
        private readonly Random _random;

        public HealthCheckService()
        {
            _random = new Random();
        }

        public HealthCheckResponse CheckHealth(HealthCheckRequest request)
        {
            return new HealthCheckResponse
            {
                IsEnabled = true,
                NumberOfActiveClients = _random.Next(0, 6),
                ExpirationTime = DateTime.UtcNow.AddMinutes(10)
            };
        }
    }
}
