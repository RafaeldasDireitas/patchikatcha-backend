using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using patchikatcha_backend.Data;
using patchikatcha_backend.DTO;
using patchikatcha_backend.Models;

namespace patchikatcha_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly AuthDbContext authDbContext;

        public CartController(AuthDbContext authDbContext)
        {
            this.authDbContext = authDbContext;
        }

        [HttpPost]
        [Route("create-cart")]
        public async Task<IActionResult> CreateCart(string userId, [FromBody] CartDto cart)
        {
                var cartItem = new Cart
                {
                    ApplicationUserId = userId,
                    Name = cart.Name,
                    Description = cart.Description,
                    BasePrice = cart.BasePrice,
                    Price = cart.Price,
                    PriceId = cart.PriceId,
                    Image = cart.Image,
                    Quantity = cart.Quantity,
                    Size = (int)cart.Size,
                    Color = (int)cart.Color,
                    ProductId = cart.ProductId,
                    VariantId = cart.VariantId,
                    FirstItem = cart.FirstItem,
                    AdditionalItems = cart.AdditionalItems,
                };

                await authDbContext.AddAsync(cartItem);
                await authDbContext.SaveChangesAsync();

            return Ok("");
        }
    }
}
