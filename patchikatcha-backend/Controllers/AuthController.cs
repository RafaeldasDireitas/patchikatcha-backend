using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using patchikatcha_backend.Data;
using patchikatcha_backend.DTO;
using patchikatcha_backend.Models;
using patchikatcha_backend.Repositories;
using System.Text;
using System.Text.Json;
using System.Web;

namespace patchikatcha_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IToken tokenRepository;
        private readonly AuthDbContext authDbContext;
        private readonly HttpClient client;

        public AuthController(UserManager<ApplicationUser> userManager, IToken tokenRepository, AuthDbContext authDbContext, HttpClient client)
        {
            this.userManager = userManager;
            this.tokenRepository = tokenRepository;
            this.authDbContext = authDbContext;
            this.client = client;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {

            var applicationUser = new ApplicationUser
            {
                Email = registerDto.Email,
                UserName = registerDto.Username,
                UserCurrency = "EUR",
            };

            var identityResult = await userManager.CreateAsync(applicationUser, registerDto.Password);

            if (!identityResult.Succeeded)
            {
                return BadRequest();
            }

            await userManager.AddToRolesAsync(applicationUser, ["User"]);

            return Ok("User registered");
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var googleRequest = new
            {
                secret = "6Ld74NIpAAAAAMe_24uYeE85Gj3Fqys7lRVeIV8a",
                response = loginDto.ApiKey
            };

            var jsonRequest = JsonSerializer.Serialize(googleRequest);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/x-www-form-urlencoded");
            var url = $"https://www.google.com/recaptcha/api/siteverify?secret=6Ld74NIpAAAAAMe_24uYeE85Gj3Fqys7lRVeIV8a&response={loginDto.ApiKey}";

            HttpResponseMessage response = await client.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            var user = await userManager.FindByEmailAsync(loginDto.Email);

            if (user == null)
            {
                return BadRequest("No user found.");
            }

            var password = await userManager.CheckPasswordAsync(user, loginDto.Password);

            if (!password)
            {
                return BadRequest("Password doesn't match.");
            }

            var roles = await userManager.GetRolesAsync(user);

            if (roles == null)
            {
                return BadRequest("No roles");
            }

            var jwtToken = tokenRepository.CreateJTWToken(user, roles.ToList());

            var tokenResponse = new LoginTokenResponse
            {
                jwtToken = jwtToken,
                userId = user.Id,
                verificationResponse = responseBody
            };

            return Ok(tokenResponse);
        }

        [HttpGet]
        [Route("verify-user-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> VerifyUserRole(string email)
        {

            var userEmail = await userManager.FindByEmailAsync(email);

            var isAdmin = await userManager.IsInRoleAsync(userEmail, "Admin");

            if (!isAdmin)
            {
                return BadRequest("You don't have access to this page.");
            }


            return Ok("You're verified as Admin");
        }

        [HttpGet]
        [Route("grab-email-token")]
        public async Task<IActionResult> GrabEmailToken(string email)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return BadRequest("No user found");
            }

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

            if (token == null)
            {
                return BadRequest("No token was generated");
            }

            return Content(token, "text/plain");
        }

        [HttpPut]
        [Route("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return BadRequest("No user found");
            }

            var confirmEmail = await userManager.ConfirmEmailAsync(user, token);

            if (confirmEmail == null)
            {
                return BadRequest("Email wasn't confirmed");
            }

            if (confirmEmail.Succeeded)
            {
                return Ok("Email verified");
            } else
            {
                return BadRequest("Email confirmation failed");
            }
        }

        [HttpPut]
        [Authorize]
        [Route("update-user-country")]
        public async Task<IActionResult> UpdateUserCountry(string userId, [FromBody] string newCountry)
        {
            var user = await userManager.FindByIdAsync(userId);

            user.UserCountry = newCountry;

            await userManager.UpdateAsync(user);
            await authDbContext.SaveChangesAsync();

            return Ok("Saved");
        }

        [HttpPut]
        [Route("change-user-email")]
        public async Task<IActionResult> Test(string userEmail, string newEmail)
        {

            var user = await userManager.FindByEmailAsync(userEmail);

            user.Email = newEmail;
            user.NormalizedEmail = newEmail.ToUpper();
            user.UserName = newEmail;
            user.NormalizedUserName = newEmail.ToUpper();

            var orders = authDbContext.Orders.Where(order => order.UserEmail == userEmail).ToList();

            foreach (var order in orders)
            {
                order.UserEmail = newEmail;
            }

            await authDbContext.SaveChangesAsync();

            return Ok("Your email was changed");
        }

        [HttpGet]
        [Route("is-email-confirmed")]
        public async Task<IActionResult> IsEmailConfirmed(string email)
        {
            var userEmail = await userManager.FindByEmailAsync(email);

            if (userEmail == null)
            {
                return BadRequest("No email was found");
            }

            var confirmEmail = await userManager.IsEmailConfirmedAsync(userEmail);

            if (!confirmEmail)
            {
                return BadRequest("Your email isn't confirmed!");
            }

            return Ok("Email is confirmed");
        }

        [HttpGet]
        [Route("grab-password-token")]
        public async Task<IActionResult> GrabPasswordToken(string userEmail)
        {
            var findUser = await userManager.FindByEmailAsync(userEmail);

            if (findUser == null)
            {
                return BadRequest("No user found");
            }

            var passwordToken = await userManager.GeneratePasswordResetTokenAsync(findUser);


            return Ok(passwordToken);
        }

        [HttpPut]
        [Route("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] PasswordChangerDto passwordChanger)
        {

            var findUser = await userManager.FindByEmailAsync(passwordChanger.userEmail);

            if (findUser == null)
            {
                return BadRequest("No user found");
            }

            var changePassword = await userManager.ResetPasswordAsync(findUser, passwordChanger.token, passwordChanger.password);

            if (changePassword == null)
            {
                return BadRequest("Unable to change password");
            }

            return Ok(changePassword);
        }

        [HttpPut]
        [Route("activate-twoFactor-authentication")]
        public async Task<IActionResult> ActivateTwoFactorAuthentication(string userEmail)
        {
            var user = await userManager.FindByEmailAsync(userEmail);

            return Ok();
        }
    }
}

