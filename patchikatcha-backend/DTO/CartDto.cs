namespace patchikatcha_backend.DTO
{
    public class CartDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public float Price { get; set; }
        public string PriceId { get; set; }
        public string Image { get; set; }
        public int Quantity { get; set; }
        public int? Size { get; set; }
        public int? Color { get; set; }
        public string ProductId { get; set; }
        public int VariantId { get; set; }
        public string Country { get; set; }
    }
}
