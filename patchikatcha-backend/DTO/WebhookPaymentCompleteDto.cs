using Stripe;
using Stripe.Checkout;

namespace patchikatcha_backend.DTO
{
    public class WebhookPaymentCompleteDto
    {
        public List<LineItem> lineItems { get; set; }
        public SessionShippingDetails shippingDetails { get; set; }
        public Dictionary<string, string> metaData { get; set; }
        public string? userEmail { get; set; }
        public string fullName { get; set; }
    }
}
