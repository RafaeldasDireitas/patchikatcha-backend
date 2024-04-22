using patchikatcha_backend.Models;

namespace patchikatcha_backend.DTO
{
    public class WishlistDto
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; }
        public string Title { get; set; }
        public float Price { get; set; }
        public string Image { get; set; }
        public string ProductId { get; set; }
    }
}
