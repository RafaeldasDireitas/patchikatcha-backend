using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using patchikatcha_backend.Data;
using patchikatcha_backend.DTO;

namespace patchikatcha_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly HttpClient client;
        private readonly IConfiguration configuration;
        private readonly PatchiContext patchiContext;

        public OrderController(HttpClient client, IConfiguration configuration, PatchiContext patchiContext)
        {
            this.client = client;
            this.configuration = configuration;
            this.patchiContext = patchiContext;
        }

        [HttpGet]
        [Route("grab-orders-id")]
        public async Task<IActionResult> GrabOrdersId(string userEmail)
        {
            var findOrders = patchiContext.Orders.Where(email => email.UserEmail == userEmail).ToArray();

            if (findOrders == null)
            {
                return BadRequest("No orders found");
            }

            var idList = new List<GrabUserOrdersDto>();

            foreach (var item in findOrders)
            {
                idList.Add(new GrabUserOrdersDto { OrderId = item.OrderId});
            }

            return Ok(idList);
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
