using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace patchikatcha_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly HttpClient client;
        private readonly IConfiguration configuration;

        public OrderController(HttpClient client, IConfiguration configuration)
        {
            this.client = client;
            this.configuration = configuration;
        }

        [HttpGet]
        [Route("grab-user-orders")]
        public async Task<IActionResult> GrabUserOrders(string orderId)
        {
            var apiKey = configuration["PRINTIFY_API"];
            var shopId = configuration["PRINTIFY_SHOP_ID"];

            client.DefaultRequestHeaders.Add("Authorization", apiKey);

            var url = $"https://api.printify.com/v1/shops/{shopId}/orders/{orderId}.json";

            HttpResponseMessage response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest("No order found");
            }

            var data = await response.Content.ReadAsStringAsync();

            return Ok(data);
        }
    }
}
