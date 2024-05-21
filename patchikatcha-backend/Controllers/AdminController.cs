using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using patchikatcha_backend.Data;
using patchikatcha_backend.DTO;
using patchikatcha_backend.Models;
using System.Text;
using System.Text.Json;

namespace patchikatcha_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly HttpClient client;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration configuration;
        public readonly IMemoryCache memoryCache;
        private readonly AuthDbContext authDbContext;

        public AdminController(HttpClient client, UserManager<ApplicationUser> userManager, IConfiguration configuration, IMemoryCache memoryCache, AuthDbContext authDbContext)
        {
            this.client = client;
            this.userManager = userManager;
            this.configuration = configuration;
            this.memoryCache = memoryCache;
            this.authDbContext = authDbContext;
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

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("grab-user-reviews")]
        public async Task<IActionResult> GrabOrder(string userId)
        {
            var findUser = await userManager.FindByIdAsync(userId);

            if (findUser == null)
            {
                return BadRequest("No user found");
            }

            var userReviews = findUser.Review.Where(review => review.ApplicationUserId == userId).ToList();

            return Ok(userReviews);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("create-product-in-db")]
        public async Task<IActionResult> CreateProductInDb(DbProductDto dbProductDto)
        {
            var findProduct = await authDbContext.Products.FirstOrDefaultAsync(product => product.ProductId == dbProductDto.ProductId);

            if (findProduct != null)
            {
                return BadRequest(new { message = "Product already exists" });
            }

            var newProduct = new Product
            {
                Title = dbProductDto.Title,
                ProductId = dbProductDto.ProductId,
                Tags = dbProductDto.Tags,
                Price = dbProductDto.Price,
                Purchases = dbProductDto.Purchases,
                Image = dbProductDto.Image
            };

            await authDbContext.Products.AddAsync(newProduct);
            await authDbContext.SaveChangesAsync();

            return Ok("");
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        [Route("change-product-tag")]
        public async Task<IActionResult> ChangeProductTag(string productId, [FromBody] string tag)
        {
            var apiKey = configuration["PRINTIFY_API"];
            var shopId = configuration["PRINTIFY_SHOP_ID"];

            client.DefaultRequestHeaders.Add("Authorization", apiKey);

            var url = $"https://api.printify.com/v1/shops/{shopId}/products/{productId}.json";

            var productTag = new
            {
                tags = new[] { tag }
            };

            var jsonDataBody = JsonSerializer.Serialize(productTag);
            var content = new StringContent(jsonDataBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PutAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return Ok(new { message = "Product tag was updated" });
            }

            return BadRequest(new { message = "There was an error updating product tag" });
        }
    }
}
