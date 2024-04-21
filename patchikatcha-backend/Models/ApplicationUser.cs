using Microsoft.AspNetCore.Identity;

namespace patchikatcha_backend.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? UserCountry { get; set; }
        public ICollection<Cart>? Cart { get; set; }
    }
}
