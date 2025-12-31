using IteraClient.Models;
using IteraClient.Services;
using IteraClient.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace IteraClient.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        public IConfigurationRoot _configuration;
        public AuthConfig _authConfig;
        public EndPoints _endPoints;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;

            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _authConfig = new AuthConfig();
            _configuration.GetSection("AuthConfig").Bind(_authConfig);

            _endPoints = new EndPoints();
            _configuration.GetSection("EndPoints").Bind(_endPoints);
        }

        [HttpGet("GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet("Autentication")]
        public async Task<IActionResult> Auth()
        {
            var iteraAuthService = new IteraAuthService(_authConfig, _endPoints);

            var accessToken = await iteraAuthService.GetAccessTokenAsync();

            return Ok(accessToken);
        }

        [HttpGet("GetStatus")]
        public async Task<IActionResult> GetStatus()
        {
            var iteraAuthService = new IteraAuthService(_authConfig, _endPoints);

            using var authorizedClient = await iteraAuthService.CreateAuthorizedClientAsync();

            return Ok(await authorizedClient.GetIteraStatusAsync(Guid.Parse("490d3ec0-fe70-4e40-b286-09246f5e6d5d")));
        }

        [HttpGet("GetExport")]
        public async Task<IActionResult> GetExport()
        {
            var iteraAuthService = new IteraAuthService(_authConfig, _endPoints);

            using var authorizedClient = await iteraAuthService.CreateAuthorizedClientAsync();

            return Ok(await authorizedClient.GetExportJsonAsync(34413970000130));
        }

        
    }
}
