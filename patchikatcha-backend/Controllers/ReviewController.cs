using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        [HttpPost]
        [Route("create-review")]
        public async Task<IActionResult> CreateReview(ReviewDto review)
        {

            var createReview = new Review
            {
                ProductId = review.ProductId,
                Username = review.Username,
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
