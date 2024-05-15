using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using patchikatcha_backend.Data;
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
        private readonly AuthDbContext authDbContext;

        public ProductController(HttpClient client, IConfiguration configuration, IMemoryCache memoryCache, AuthDbContext authDbContext)
        {
            this.client = client;
            this.configuration = configuration;
            this.memoryCache = memoryCache;
            this.authDbContext = authDbContext;
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
        public async Task<IActionResult> GrabCategoryProducts(string categoryName)
        {
            if (memoryCache.TryGetValue(categoryName, out ProductCategoryDto products))
            {
                return Ok(products);
            }

            var apiKey = configuration["PRINTIFY_API"];
            var shopId = configuration["PRINTIFY_SHOP_ID"];

            client.DefaultRequestHeaders.Add("Authorization", apiKey);

            string url = $"https://api.printify.com/v1/shops/{shopId}/products.json";

            HttpResponseMessage response = await client.GetAsync(url);

            string jsonResponse = await response.Content.ReadAsStringAsync();

            ProductCategoryDto productCategory = JsonSerializer.Deserialize<ProductCategoryDto>(jsonResponse);

            var findProducts = productCategory.Data.Where(product => product.Tags.Contains($"{categoryName}")).ToList();
            var jsonContent = JsonSerializer.Serialize(findProducts);

            memoryCache.Set(categoryName, jsonContent, TimeSpan.FromSeconds(60));

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

        [HttpGet]
        [Route("grab-best-sellers")]
        public async Task<IActionResult> GrabBestSellers()
        {
            var findBestSellers = await authDbContext.Products.OrderByDescending(product => product.Purchases).ToListAsync();

            return Ok(findBestSellers);
        }
    }
}
