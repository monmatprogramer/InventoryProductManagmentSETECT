using Microsoft.AspNetCore.Mvc;

namespace InventoryPro.AuthService.Controllers
    {
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
        {
        [HttpGet]
        public IActionResult Get()
            {
            return Ok("AuthService is running!");
            }
        }
    }