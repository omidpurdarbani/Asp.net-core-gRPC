using ManagementSystem.DTOs;

namespace ManagementSystem.Persistence.Interfaces
{
    public interface IHealthCheckService
    {
        public HealthCheckResponse CheckHealth(HealthCheckRequest request);
    }
}
