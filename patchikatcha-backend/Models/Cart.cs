using System.Text.Json.Serialization;

namespace patchikatcha_backend.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; }
        [JsonIgnore]
        public ApplicationUser ApplicationUser {  get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public int Size { get; set; }
        public int Color { get; set; }
        public string Image { get; set; }
        public int BasePrice { get; set; }
        public int Price { get; set; }
        public string PriceId { get; set; }
        public string ProductId { get; set; }
        public int VariantId { get; set; }
        public int FirstItem { get; set; }
        public int AdditionalItems { get; set; }
        public int BlueprintId { get; set; }
        public int PrintProviderId { get; set; }
    }

}