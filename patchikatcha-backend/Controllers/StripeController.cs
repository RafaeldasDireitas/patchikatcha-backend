using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using patchikatcha_backend.DTO;
using Stripe;
using Stripe.Checkout;

namespace patchikatcha_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StripeController : ControllerBase
    {
        public StripeController()
        {

            StripeConfiguration.ApiKey = "sk_test_51Onkz6Lwv2BbZpNwYDF8RzBVcmiQAZ59EeoWeBEYD3WJTRmhakFtyUR1tAJcCp4Vrr9mKhxzJARNA0rEPyfyofWV00cISXaGE8";

        }

        [HttpPost]
        [Route("create-checkout-session")]
        public ActionResult Create(string userEmail, [FromBody] CartDto[] checkoutObject)
        {
            var domain = "http://localhost:3000/checkout/order-successful";
            var options = new SessionCreateOptions
            {
                CustomerEmail = userEmail,
                
                UiMode = "embedded",
                LineItems = new List<SessionLineItemOptions>
                {
                    
                },
                Mode = "payment",
                ReturnUrl = domain,
                BillingAddressCollection = "required",
                ShippingAddressCollection = new SessionShippingAddressCollectionOptions
                {
                    AllowedCountries = new List<string> { "US", "CA"}
                },
                ShippingOptions = new List<SessionShippingOptionOptions>
                {
                    new SessionShippingOptionOptions
                    {
                        ShippingRate = "shr_1Or81ZLwv2BbZpNwAZArxHdb",
                    }
                },

            };

            foreach (var item in checkoutObject)
            {
                options.LineItems.Add(new SessionLineItemOptions
                {
                    Price = item.PriceId,
                    Quantity = item.Quantity,

                });
            }


            var service = new SessionService();
            Session session = service.Create(options);
            string clientSecret = session.ClientSecret;

            var clientJson = JsonSerializer.Serialize(clientSecret);

            return Content(clientJson, "application/json");
        }

        [HttpGet]
        [Route("session-status")]
        public ActionResult SessionStatus([FromQuery] string sessionId)
        {
            var sessionSerivce = new SessionService();
            Session session = sessionSerivce.Get(sessionId);

            // Check if session is null
            if (session == null)
            {
                return NotFound("Session not found.");
            }

            // Check if CustomerDetails is null
            if (session.CustomerDetails == null)
            {
                return BadRequest("Customer details not available.");
            }

            var sessionJson = new
            {
                status = session.Status,
                customer_email = session.CustomerDetails.Email
            };

            var jsonObject = JsonSerializer.Serialize(sessionJson);

            return Ok(jsonObject);
        }

        [HttpGet]
        [Route("grab-price-id")]
        public IActionResult GrabPriceId(string productId)
        {
           var priceService = new PriceService();

            var options = new PriceListOptions
            {
                LookupKeys = new List<string> { productId }
            };

            var productPrice = priceService.List(options);
            var priceId = productPrice.First().Id;

            if (productPrice == null)
            {
                return BadRequest("No product found");
            }

            return Ok(priceId);
        }

        [HttpPost]
        [Route("webhook-payment-completed")]
        public async Task<IActionResult> WebhookPaymentCollected()
        {
            const string endpointSecret = "whsec_34670d831291649de50799b6d4a8d94cb1a4610fcc6f211436c25a53e2ad1947";

            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"], endpointSecret);

                // Handle the event
                if (stripeEvent.Type == Events.CheckoutSessionCompleted)
                {
                    if (stripeEvent.Data is EventData eventData)
                    {
                        // Check if eventData.Object is of type Stripe.Checkout.Session
                        if (eventData.Object is Session session)
                        {
                            // Access the id property of the session
                            string sessionId = session.Id;

                            // Make an additional API call to retrieve the expanded Checkout Session object
                            var options = new SessionGetOptions { Expand = new List<string> { "line_items" } };
                            var service = new SessionService();
                            var checkoutSession = service.Get(sessionId, options);

                            // Access the line_items property from the expanded Checkout Session object
                            var lineItems = checkoutSession.LineItems.Data;

                            foreach (var item in lineItems)
                            {
                                Console.WriteLine($"Description: {item.Object} {item.Description}, Quantity: {item.Quantity}, ProductId: {item.Id}");
                            }

                        }
                    }
                }
                // ... handle other event types
                else
                {
                    Console.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
                }

                return Ok();
            }
            catch (StripeException e)
            {
                return BadRequest();
            }
        }

    }
}