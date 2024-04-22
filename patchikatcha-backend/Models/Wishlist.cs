using System.Text.Json.Serialization;

namespace patchikatcha_backend.Models
{
    public class Wishlist
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; }
        [JsonIgnore]
        public ApplicationUser ApplicationUser { get; set; }
        public string Title { get; set; }
        public float Price { get; set; }
        public string Image { get; set; }
        public string ProductId { get; set; }
    }
}
