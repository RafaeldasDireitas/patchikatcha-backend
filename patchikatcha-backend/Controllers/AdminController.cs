using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using System.Text.Json;

namespace patchikatcha_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly HttpClient client;
        private readonly IConfiguration configuration;
        public readonly IMemoryCache memoryCache;

        public AdminController(HttpClient client, IConfiguration configuration, IMemoryCache memoryCache)
        {
            this.client = client;
            this.configuration = configuration;
            this.memoryCache = memoryCache;
        }

        [HttpPost]
        [Route("publish-product")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> PublishProduct([FromBody] string productId)
        {
            var apiKey = configuration["PRINTIFY_API"];
            var shopId = configuration["PRINTIFY_SHOP_ID"];

            client.DefaultRequestHeaders.Add("Authorization", apiKey);

            var url = $"https://api.printify.com/v1/shops/{shopId}/products/{productId}/publishing_succeeded.json";

            var dataBody = new
            {
                external = new
                {
                    id = productId,
                    handle = "http://localhost:3000",
                }
            };

            var jsonDataBody = JsonSerializer.Serialize(dataBody);
            var content = new StringContent(jsonDataBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest("Product wasn't published");
            }

            return Ok("Product published");
        }
    }
}
