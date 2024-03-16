using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using patchikatcha_backend.DTO;
using patchikatcha_backend.Repositories;
using System.Text.Json;
using System.Web;

namespace patchikatcha_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IToken tokenRepository;

        public AuthController(UserManager<IdentityUser> userManager, IToken tokenRepository)
        {
            this.userManager = userManager;
            this.tokenRepository = tokenRepository;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {

            var identityUser = new IdentityUser
            {
                Email = registerDto.Email,
                UserName = registerDto.Email,
            };

            var identityResult = await userManager.CreateAsync(identityUser, registerDto.Password);

            if (!identityResult.Succeeded)
            {
                return BadRequest();
            }

            await userManager.AddToRolesAsync(identityUser, ["User"]);

            return Ok("User registered");
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
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
        public async Task<IActionResult> ChangePassword([FromBody] string userEmail, string token, string password)
        {
            var findUser = await userManager.FindByEmailAsync(userEmail);

            if (findUser == null)
            {
                return BadRequest("No user found");
            }

            var changePassword = await userManager.ResetPasswordAsync(findUser, token, password);

            if (changePassword == null)
            {
                return BadRequest("Unable to change password");
            }


            return Ok(changePassword);
        }

    }
}

