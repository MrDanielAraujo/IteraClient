using IteraClient.Models;
using IteraClient.Services;
using IteraClient.Utils;
using Microsoft.AspNetCore.Mvc;

namespace IteraClient.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IteraController : ControllerBase
    {
        private readonly IteraAuthService _iteraAuthService;
        
        public IteraController()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var authConfig = new AuthConfig();
            configuration.GetSection("AuthConfig").Bind(authConfig);

            var endPoints = new EndPoints();
            configuration.GetSection("EndPoints").Bind(endPoints);

            _iteraAuthService = new IteraAuthService(authConfig, endPoints);
        }

        [HttpGet("GetAccessToken")]
        public async Task<IActionResult> Auth()
            => Ok(await _iteraAuthService.GetAccessTokenAsync());
        

        [HttpGet("GetStatus")]
        public async Task<IActionResult> GetStatus()
            => Ok(await _iteraAuthService.GetIteraStatusAsync(Guid.Parse("490d3ec0-fe70-4e40-b286-09246f5e6d5d")));
        

        [HttpGet("GetExport")]
        public async Task<IActionResult> GetExport()
            => Ok(await _iteraAuthService.GetExportJsonAsync(34413970000130));
        
        [HttpGet("GetDePara")]
        public async Task<IActionResult> GetDePara()
            => Ok(await _iteraAuthService.GetIteraDeParaAsync(Guid.Parse("490d3ec0-fe70-4e40-b286-09246f5e6d5d")));
    }
}
