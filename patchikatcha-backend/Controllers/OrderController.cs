﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using patchikatcha_backend.Data;
using patchikatcha_backend.DTO;
using System.Text;
using System.Text.Json;

namespace patchikatcha_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly HttpClient client;
        private readonly IConfiguration configuration;
        private readonly AuthDbContext authDbContext;
        private readonly IMemoryCache memoryCache;

        public OrderController(HttpClient client, IConfiguration configuration, IMemoryCache memoryCache, AuthDbContext authDbContext)
        {
            this.client = client;
            this.configuration = configuration;
            this.authDbContext = authDbContext;
            this.memoryCache = memoryCache;
        }

        [HttpGet]
        [Authorize]
        [Route("grab-orders-id")]
        public async Task<IActionResult> GrabOrdersId(string userEmail)
        {
            if (memoryCache.TryGetValue(userEmail, out List<GrabUserOrdersDto> cachedResponse))
            {
                return Ok(cachedResponse);
            }

            var findOrders = authDbContext.Orders.Where(orders => orders.UserEmail == userEmail).ToArray();

            if (findOrders == null)
            {
                return BadRequest("No orders found");
            }

            var idList = new List<GrabUserOrdersDto>();

            foreach (var item in findOrders)
            {
                idList.Add(new GrabUserOrdersDto { OrderId = item.OrderId });
            }

            memoryCache.Set(userEmail, idList, TimeSpan.FromMinutes(30));

            return Ok(idList);
        }

        [HttpGet]
        [Authorize]
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

        [HttpPost]
        [Route("create-user-order")]
        public async Task<IActionResult> CreateUserOrder([FromBody] PrintifyOrderCreateDto printifyOrder)
        {
            var apiKey = configuration["PRINTIFY_API"];
            var shopId = configuration["PRINTIFY_SHOP_ID"];

            client.DefaultRequestHeaders.Add("Authorization", apiKey);

            var jsonOrder = JsonSerializer.Serialize(printifyOrder);
            var content = new StringContent(jsonOrder, Encoding.UTF8, "application/json");
            var url = $"https://api.printify.com/v1/shops/{shopId}/orders.json";

            HttpResponseMessage response = await client.PostAsync(url, content);
            var responseContente = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                JsonDocument doc = JsonDocument.Parse(responseContent);
                JsonElement root = doc.RootElement;
                string orderId = root.GetProperty("id").GetString();

                var newOrder = new Models.Order()
                {
                    OrderId = orderId,
                    UserEmail = printifyOrder.address_to.email,
                };

                await authDbContext.AddAsync(newOrder);
                await authDbContext.SaveChangesAsync();
            }

            return Ok(responseContente);
        }
    }
}
