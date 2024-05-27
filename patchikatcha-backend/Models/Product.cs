namespace patchikatcha_backend.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string ProductId { get; set; }
        public string Title { get; set; }
        public string Tag { get; set; }
        public string CategoryTag { get; set; }
        public int Price { get; set; }
        public string Image { get; set; }
        public string SecondImage { get; set; }
        public int Purchases { get; set; }
    }
}
