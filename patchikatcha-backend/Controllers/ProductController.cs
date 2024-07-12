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
        [ResponseCache(Duration = 10800)]
        [Route("new-products")]
        public async Task<ActionResult> GetNewProducts()
        {
            int limit = 8;

            var findNewProducts = await authDbContext.Products.OrderByDescending(product => product.Id).Take(limit).ToListAsync();

            return Ok(findNewProducts);
        }

        [HttpGet]
        [Route("grab-product-id")]
        public async Task<IActionResult> GrabProductId(string productTitle)
        {
            var findProduct = await authDbContext.Products.FirstOrDefaultAsync(product => product.Title == productTitle);

            if (findProduct == null)
            {
                return BadRequest("No product found");
            }

            return Ok(findProduct.ProductId);
        }

        [HttpGet]
        [Route("grab-category-products")]
        public async Task<IActionResult> GrabCategoryProducts(string categoryName, int page)
        {
            if (memoryCache.TryGetValue(categoryName, out ProductCategoryDto products))
            {
                return Ok(products);
            }

            var totalProducts = await authDbContext.Products.Where(product => product.Tag == categoryName || product.CategoryTag == categoryName).CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalProducts / 6);

            var findCategoryProducts = await authDbContext.Products.Where(product => product.Tag == categoryName || product.CategoryTag == categoryName).Skip(page * 6).Take(6).ToListAsync();

            memoryCache.Set(categoryName, findCategoryProducts, TimeSpan.FromHours(3));

            return Ok(new { categoryProducts = findCategoryProducts, totalPages = totalPages });

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

            memoryCache.Set(productId, jsonProduct, TimeSpan.FromHours(3));

            return Ok(jsonProduct);
        }

        [HttpGet]
        [ResponseCache(Duration = 10800)]
        [Route("grab-best-sellers")]
        public async Task<IActionResult> GrabBestSellers()
        {
            var findBestSellers = await authDbContext.Products.OrderByDescending(product => product.Purchases).Take(2).ToListAsync();

            return Ok(findBestSellers);
        }

        [HttpGet]
        [Route("recommended-products")]
        public async Task<IActionResult> RecommendedProducts(string tag)
        {
            if (memoryCache.TryGetValue(tag, out string cachedResponse))
            {
                return Ok(cachedResponse);
            }

            var findRecommendedProducts = await authDbContext.Products.Where(product => product.Tag == tag).OrderBy(x => Guid.NewGuid()).Take(4).ToListAsync();

            memoryCache.Set(tag, findRecommendedProducts, TimeSpan.FromMinutes(10));

            return Ok(findRecommendedProducts);
        }
    }
}
