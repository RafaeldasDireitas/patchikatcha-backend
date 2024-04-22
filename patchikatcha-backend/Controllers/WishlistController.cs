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

        [HttpPut]
        [Route("create-wishlist")]
        public async Task<IActionResult> CreateWishlist(string userId, WishlistDto wishlistDto)
        {
            var findUser = await userManager.FindByIdAsync(userId);

            if (findUser != null)
            {
                var newWishlist = new Wishlist
                {
                    ApplicationUserId = userId,
                    Title = wishlistDto.Title,
                    Price = wishlistDto.Price,
                    Image = wishlistDto.Image,
                    ProductId = wishlistDto.ProductId
                };

                await authDbContext.Wishlists.AddAsync(newWishlist);
                await authDbContext.SaveChangesAsync();

                return Ok("Wishlist created");
            }

            return BadRequest("There was an error");
        }
    }
}
