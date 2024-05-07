namespace patchikatcha_backend.Models
{
    public class LineItem
    {
        public int Id { get; set; }
        public string ProductId { get; set; }
        public int VariantId { get; set; }
        public int Quantity { get; set; }
        public Guid OrderId { get; set; }
        public Order Order { get; set; }
    }
}