using Microsoft.AspNetCore.Authorization;
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
        [Route("grab-product-reviews")]
        public async Task<IActionResult> GrabProductReviews(string productId, int limit, int page)
        {
            int skipReviews = limit * page;

            var reviews = authDbContext.Reviews.Where(review => review.ProductId == productId);

            var reviewsCount = await reviews.CountAsync();

            var grabReviews = await reviews.OrderByDescending(review => review.Id).Skip(skipReviews).Take(limit).Select(review => new
            {
                review.Id,
                review.Title,
                review.Comment,
                review.Rating,
                review.CreatedAt,
                Username = review.ApplicationUser.UserName,
            }).ToListAsync();

            if (reviews != null)
            {
                return Ok(new { reviews = grabReviews, reviewsCount =  reviewsCount});
            }

            return BadRequest("No reviews for this product");
        }

        [HttpGet]
        [Route("grab-user-reviews")]
        public async Task<IActionResult> GrabUserReviews(string userId)
        {
            var findUser = await userManager.FindByIdAsync(userId);

            if (findUser != null)
            {
                var userReviews = await authDbContext.Reviews.Where(review => review.ApplicationUserId == userId).Select(review => new
                {
                    review.Id,
                    review.Title,
                    review.ProductImage,
                    review.Comment,
                    review.Rating,
                    review.CreatedAt,
                    Username = review.ApplicationUser.UserName,
                }).ToListAsync();

                return Ok(userReviews);
            }

            return BadRequest("User has no reviews");

        }

        [HttpPost]
        [Authorize]
        [Route("create-review")]
        public async Task<IActionResult> CreateReview(ReviewDto review)
        {
            var findReview = authDbContext.Reviews.FirstOrDefault(reviews => reviews.Title == review.Title);

            if (findReview != null)
            {
                return BadRequest(new { message = "Review title already exists"});
            }

            var createReview = new Review
            {
                ProductId = review.ProductId,
                ProductTitle = review.ProductTitle,
                ProductImage = review.ProductImage,
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

        [HttpDelete]
        [Authorize]
        [Route("delete-review")]
        public async Task<IActionResult> DeleteReview(string userId, int Id)
        {
            var findUser = await userManager.FindByIdAsync(userId);

            if (findUser == null)
            {
                return BadRequest(new { message = "No user found" });
            }

            var findReview = await authDbContext.Reviews.FirstOrDefaultAsync(review =>  review.ApplicationUserId == userId && review.Id == Id);

            if (findReview == null)
            {
                return BadRequest(new { message = "No review found" });
            }

            authDbContext.Reviews.Remove(findReview);
            await authDbContext.SaveChangesAsync();

            return Ok(new { message = "Review removed" });
        }
    }
}
