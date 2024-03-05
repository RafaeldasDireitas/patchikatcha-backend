using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            var domain = "http://localhost:3000";
            var options = new SessionCreateOptions
            {
                CustomerEmail = userEmail,
                UiMode = "embedded",
                LineItems = new List<SessionLineItemOptions>
                {
                   
                },
                Mode = "payment",
                ReturnUrl = domain
            };

            foreach (var item in checkoutObject)
            {
               options.LineItems.Add(new SessionLineItemOptions
               {
                  Price = item.PriceId,
                   Quantity = item.Quantity
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
    }
}