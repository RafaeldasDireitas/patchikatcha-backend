using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using patchikatcha_backend.Data;
using patchikatcha_backend.DTO;
using patchikatcha_backend.Models;
using Stripe.Climate;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace patchikatcha_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly AuthDbContext authDbContext;
        private readonly HttpClient client;
        private readonly IConfiguration configuration;

        public CartController(UserManager<ApplicationUser> userManager, AuthDbContext authDbContext, HttpClient client, IConfiguration configuration)
        {
            this.authDbContext = authDbContext;
            this.client = client;
            this.configuration = configuration;
            this.userManager = userManager;

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
                    BlueprintId = cart.BlueprintId,
                    PrintProviderId = cart.PrintProviderId,
                    UserCountry = cart.UserCountry,
                    Currency = cart.Currency
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

        [HttpPut]
        [Route("update-cart-shipping")]
        public async Task<IActionResult> UpdateCartShipping(string userId, CartBlueprintDto[] cartBlueprint)
        {
            var apiKey = configuration["PRINTIFY_API"];
            var shopId = configuration["PRINTIFY_SHOP_ID"];

            client.DefaultRequestHeaders.Add("Authorization", apiKey);

            var findUser = await userManager.FindByIdAsync(userId);

            if (findUser != null)
            {
                var profileList = new List<ProfilesDto>();

                foreach (var item in cartBlueprint)
                {
                    var url = $"https://localhost:7065/api/Blueprint/get-blueprint/{item.BlueprintId}/{item.PrintProviderId}";

                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();

                        var deserializedData = JsonSerializer.Deserialize<BlueprintDto>(data);

                        var findCountry = deserializedData.Profiles.FirstOrDefault(profile => profile.countries.Contains(item.UserCountryCode));

                        var findItem = authDbContext.Carts.Where(cart => cart.BlueprintId == item.BlueprintId && cart.PrintProviderId == item.PrintProviderId && cart.ApplicationUserId == userId);

                        if (findItem != null)
                        {
                            foreach (var product in findItem)
                            {
                                product.FirstItem = findCountry.first_item.cost;
                                product.AdditionalItems = findCountry.additional_items.cost;
                                product.UserCountry = item.UserCountryCode;

                                var newProfile = new ProfilesDto
                                {
                                    variant_ids = findCountry.variant_ids,
                                    first_item = new first_item
                                    {
                                        cost = findCountry.first_item.cost,
                                        currency = findCountry.first_item.currency
                                    },
                                    additional_items = new additional_items
                                    {
                                        cost = findCountry.additional_items.cost,
                                        currency = findCountry.additional_items.currency
                                    },
                                    countries = findCountry.countries
                                };

                                profileList.Add(newProfile);
                            }

                            await authDbContext.SaveChangesAsync();

                            return Ok(profileList);
                        } else
                        {
                            return BadRequest("There was an error");
                        }
                    }
                    else
                    {
                        return BadRequest("Something went wrong, try again");
                    }
                }

                return Ok("Shipping rate changed");
            }

            return BadRequest("No user was found");
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
