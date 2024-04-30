﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using patchikatcha_backend.DTO;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace patchikatcha_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly HttpClient client;
        private readonly IConfiguration configuration;
        public readonly IMemoryCache memoryCache;

        public ProductController(HttpClient client, IConfiguration configuration, IMemoryCache memoryCache)
        {
            this.client = client;
            this.configuration = configuration;
            this.memoryCache = memoryCache;
        }

        // GET: ProductController
        [HttpGet]
        [ResponseCache(Duration = 60)]
        [Route("new-products")]
        public async Task<ActionResult> GetNewProducts()
        {
            var apiKey = configuration["PRINTIFY_API"];
            var shopId = configuration["PRINTIFY_SHOP_ID"];
            int limit = 8;

            client.DefaultRequestHeaders.Add("Authorization", apiKey);

            string url = $"https://api.printify.com/v1/shops/{shopId}/products.json?limit={limit}";

            HttpResponseMessage response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest();
            }

            string jsonResponse = await response.Content.ReadAsStringAsync();

            return Ok(jsonResponse);
        }

        [HttpGet]
        [Route("grab-category-products")]
        public async Task<IActionResult> GrabCategoryProducts(int limit, int pageNumber, string productTag)
        {
            if (memoryCache.TryGetValue(productTag, out ProductCategoryDto products))
            {
                return Ok(products);
            }

            var apiKey = configuration["PRINTIFY_API"];
            var shopId = configuration["PRINTIFY_SHOP_ID"];

            client.DefaultRequestHeaders.Add("Authorization", apiKey);

            string url = $"https://api.printify.com/v1/shops/{shopId}/products.json?limit={limit}&page={pageNumber}";

            HttpResponseMessage response = await client.GetAsync(url);

            string jsonResponse = await response.Content.ReadAsStringAsync();

            ProductCategoryDto productCategory = JsonSerializer.Deserialize<ProductCategoryDto>(jsonResponse);

            var findProducts = productCategory.Data.Where(product => product.Tags.Contains($"{productTag}")).ToList();
            var jsonContent = JsonSerializer.Serialize(findProducts);

            memoryCache.Set(productTag, jsonContent, TimeSpan.FromSeconds(60));

            return Ok(jsonContent);

        }

        [HttpGet]
        [Route("grab-all-products")]
        public async Task<IActionResult> GrabAllProducts(int limit)
        {

            var apiKey = configuration["PRINTIFY_API"];
            var shopId = configuration["PRINTIFY_SHOP_ID"];

            client.DefaultRequestHeaders.Add("Authorization", apiKey);

            string url = $"https://api.printify.com/v1/shops/{shopId}/products.json?limit={limit}";

            HttpResponseMessage response = await client.GetAsync(url);

            string jsonResponse = await response.Content.ReadAsStringAsync();

            return Ok(jsonResponse);
        }

        [HttpGet]
        [Route("grab-product")]
        public async Task<IActionResult> GrabProduct(string productId)
        {
            if (memoryCache.TryGetValue(productId, out string cachedResponse))
            {
                return Ok(cachedResponse);
            }

            var apiKey = configuration["PRINTIFY_API"];
            var shopId = configuration["PRINTIFY_SHOP_ID"];

            client.DefaultRequestHeaders.Add("Authorization", apiKey);

            string url = $"https://api.printify.com/v1/shops/{shopId}/products/{productId}.json";

            HttpResponseMessage response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest("Product not found.");
            }

            string responseData = await response.Content.ReadAsStringAsync();
            JsonDocument jsonProduct = JsonDocument.Parse(responseData);

            memoryCache.Set(productId, jsonProduct, TimeSpan.FromMinutes(1));

            return Ok(jsonProduct);
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
