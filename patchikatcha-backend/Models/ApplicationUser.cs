using Microsoft.AspNetCore.Identity;

namespace patchikatcha_backend.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? UserCountry { get; set; }
        public string? UserCurrency { get; set; }
        public ICollection<Cart>? Cart { get; set; }
        public ICollection<Wishlist>? Wishlist { get; set; }
        public ICollection<Review>? Review { get; set; }
    }
}
