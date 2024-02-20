using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using patchikatcha_backend.DTO;
using patchikatcha_backend.Repositories;

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

            await userManager.AddToRolesAsync(identityUser, registerDto.Roles);

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
    }
}

