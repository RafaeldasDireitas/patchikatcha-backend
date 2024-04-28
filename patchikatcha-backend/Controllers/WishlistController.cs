using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using patchikatcha_backend.Data;
using patchikatcha_backend.DTO;
using patchikatcha_backend.Models;

namespace patchikatcha_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WishlistController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly AuthDbContext authDbContext;

        public WishlistController(UserManager<ApplicationUser> userManager, AuthDbContext authDbContext)
        {
            this.userManager = userManager;
            this.authDbContext = authDbContext;
        }

        [HttpGet]
        [Authorize]
        [Route("grab-wishlist")]
        public async Task<IActionResult> GrabWishlist(string userId)
        {
            var findUser = await userManager.FindByIdAsync(userId);

            if (findUser != null)
            {
                var wishlistList = authDbContext.Wishlists.Where(wishlist => wishlist.ApplicationUserId == userId).ToList();

                return Ok(wishlistList);

            }

            return BadRequest("There was an error");
        }

        [HttpGet]
        [Authorize]
        [Route("is-wishlisted")]
        public async Task<IActionResult> IsWishlisted(string userId, string productId)
        {
            var findUser = await userManager.FindByIdAsync(userId);

            if (findUser != null)
            {
                var findInDb = authDbContext.Wishlists.FirstOrDefault(product => product.ApplicationUserId == userId && product.ProductId == productId);

                if (findInDb != null)
                {
                    return Ok("Product wishlisted");
                } else
                {
                    return BadRequest("No product found");
                }
            }

            return BadRequest("No user found");
        }

        [HttpPost]
        [Authorize]
        [Route("create-wishlist")]
        public async Task<IActionResult> CreateWishlist(string userId, WishlistDto wishlist)
        {
            var findUser = await userManager.FindByIdAsync(userId);

            if (findUser != null)
            {
                var newWishlist = new Wishlist
                {
                    ApplicationUserId = userId,
                    Title = wishlist.Title,
                    Price = wishlist.Price,
                    Image = wishlist.Image,
                    ProductId = wishlist.ProductId
                };

                await authDbContext.Wishlists.AddAsync(newWishlist);
                await authDbContext.SaveChangesAsync();

                return Ok("Wishlist created");
            }

            return BadRequest("There was an error");
        }

        [HttpDelete]
        [Authorize]
        [Route("remove-wishlist")]
        public async Task<IActionResult> RemoveWishlist(string userId, string productId)
        {
            var findUser = await userManager.FindByIdAsync(userId);

            if (findUser != null)
            {
                var findProduct = authDbContext.Wishlists.FirstOrDefault(product => product.ApplicationUserId == userId && product.ProductId == productId);

                if (findProduct != null)
                {
                    authDbContext.Wishlists.Remove(findProduct);
                    await authDbContext.SaveChangesAsync();

                    return Ok("Wishlist deleted");
                } else
                {
                    return BadRequest("No product found");
                }
            } 

            return BadRequest("There was an error");
        }
    }
}
