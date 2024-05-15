namespace ManagementSystem.Controllers;

using ManagementSystem.DTOs;
using ManagementSystem.Persistence.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/module/health")]
public class HealthCheckController : ControllerBase
{
    private readonly IHealthCheckService _healthCheckService;

    public HealthCheckController(IHealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    [HttpPost]
    public ActionResult<HealthCheckResponse> HealthCheck([FromBody] HealthCheckRequest request)
    {
        var response = _healthCheckService.CheckHealth(request);
        return Ok(response);
    }
}