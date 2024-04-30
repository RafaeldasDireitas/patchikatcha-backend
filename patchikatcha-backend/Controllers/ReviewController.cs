﻿using Microsoft.AspNetCore.Authorization;
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

            var reviews = authDbContext.Reviews.Where(review => review.ProductId == productId).OrderByDescending(review => review.Id).Skip(skipReviews).Take(limit).Select(review => new
            {
                review.Id,
                review.Title,
                review.Comment,
                review.Rating,
                review.CreatedAt,
                Username = review.ApplicationUser.UserName,
            }).ToList();

            var reviewsCount = authDbContext.Reviews.Where(review => review.ProductId == productId).Count();

            if (reviews != null)
            {
                return Ok(new { reviews = reviews, reviewsCount =  reviewsCount});
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
                var userReviews = authDbContext.Reviews.Where(review => review.ApplicationUserId == userId).ToList();

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

            if (findUser != null)
            {
                var findReview = await authDbContext.Reviews.FirstOrDefaultAsync(review =>  review.ApplicationUserId == userId && review.Id == Id);

                if (findReview != null)
                {
                    authDbContext.Reviews.Remove(findReview);
                    await authDbContext.SaveChangesAsync();

                    return Ok("Review removed");
                }

                return BadRequest("No review found");
            }

            return BadRequest("No user found");
        }
    }
}
