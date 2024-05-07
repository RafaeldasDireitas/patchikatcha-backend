namespace patchikatcha_backend.Models
{
    public class Order
    {
        public Guid Id { get; set; }
        public string? OrderId { get; set; }
        public string UserEmail { get; set; }
        public string ExternalId { get; set; }
        public ICollection<LineItem>? LineItems { get; set; }
        public string Label { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Phone { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }
        public string Address1 { get; set; }
        public string? Address2 { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
    }
}