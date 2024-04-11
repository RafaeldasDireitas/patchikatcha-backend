using Microsoft.AspNetCore.Identity;

namespace patchikatcha_backend.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? UserCountry { get; set; }
        public Cart? Cart { get; set; }
    }
}
