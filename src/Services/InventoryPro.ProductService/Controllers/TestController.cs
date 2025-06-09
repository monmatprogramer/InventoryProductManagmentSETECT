using Microsoft.AspNetCore.Mvc;

namespace InventoryPro.ProductService.Controllers
    {
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
        {
        [HttpGet]
        public IActionResult Get()
            {
            return Ok(new { message = "Product Service is running!", timestamp = DateTime.Now });
            }
        }
    }   