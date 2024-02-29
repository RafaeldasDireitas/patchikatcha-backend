using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stripe;
using Stripe.Checkout;

public class StripeOptions
{
    public string option { get; set; }
}

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
        public ActionResult Create()
        {
            var domain = "http://localhost:3000";
            var options = new SessionCreateOptions
            {
                UiMode = "embedded",
                LineItems = new List<SessionLineItemOptions>
                {
                  new SessionLineItemOptions
                  {
                    // Provide the exact Price ID (for example, pr_1234) of the product you want to sell
                    Price = "price_1OpAbDLwv2BbZpNwoLtI9FU6",
                    Quantity = 1,
                  },
                },
                Mode = "payment",
                ReturnUrl = domain
            };

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
    }
}