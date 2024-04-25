using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using patchikatcha_backend.Data;
using patchikatcha_backend.DTO;
using patchikatcha_backend.Models;
using System.Globalization;

namespace patchikatcha_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly AuthDbContext authDbContext;

        public ReviewController(UserManager<ApplicationUser> userManager, AuthDbContext authDbContext)
        {
            this.userManager = userManager;
            this.authDbContext = authDbContext;
        }

        [HttpGet]
        [Route("grab-3-reviews")]
        public async Task<IActionResult> Grab3Reviews(string productId)
        {
            var reviews = authDbContext.Reviews.Where(review => review.ProductId == productId).Take(3).Select(review => new
            {
                review.Id,
                review.Title,
                review.Comment,
                review.Rating,
                review.CreatedAt,
                Username = review.ApplicationUser.UserName,
            }).ToList();

            if (reviews != null)
            {
                return Ok(reviews);
            }

            return BadRequest("No reviews for this product");
        }

        [HttpPost]
        [Route("create-review")]
        public async Task<IActionResult> CreateReview(ReviewDto review)
        {

            var createReview = new Review
            {
                ProductId = review.ProductId,
                Title = review.Title,
                ApplicationUserId = review.ApplicationUserId,
                Comment = review.Comment,
                Rating = review.Rating,
                CreatedAt = review.CreatedAt,
            };

            await authDbContext.Reviews.AddAsync(createReview);
            await authDbContext.SaveChangesAsync();

            return Ok("review created");
        }
    }
}
