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
            var findCartItem = authDbContext.Carts.FirstOrDefault(cartInDb => cartInDb.Name == cart.Name && cartInDb.Size == cart.Size && cartInDb.Color == cart.Color);

            if (findCartItem == null)
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

                await authDbContext.Carts.AddAsync(cartItem);
                await authDbContext.SaveChangesAsync();

                return Ok("Cart item created");
            }

            return BadRequest("There was an error, try again");
        }

        [HttpPut]
        [Route("update-cart")]
        public async Task<IActionResult> UpdateCart(string userId, [FromBody] CartDto cart)
        {
            var findCartItem = authDbContext.Carts.FirstOrDefault(cartInDb => cartInDb.ApplicationUserId == userId && cartInDb.Name == cart.Name && cartInDb.Size == cart.Size && cartInDb.Color == cart.Color);

            if (findCartItem != null)
            {
                findCartItem.Quantity = cart.Quantity;

                await authDbContext.SaveChangesAsync();

                return Ok("Cart changed");
            }

            return BadRequest("There was an error");
        }

        [HttpDelete]
        [Route("remove-cart")]
        public async Task<IActionResult> RemoveCart(string userId, [FromBody] CartDto cart)
        {
            var findCartItem = authDbContext.Carts.FirstOrDefault(cartInDb => cartInDb.ApplicationUserId == userId && cartInDb.Name == cart.Name && cartInDb.Size == cart.Size && cartInDb.Color == cart.Color);

            if (findCartItem != null)
            {
                authDbContext.Carts.Remove(findCartItem);

                await authDbContext.SaveChangesAsync();

                return Ok("Cart removed");
            }

            return BadRequest("There was an error");
        }
    }
}
