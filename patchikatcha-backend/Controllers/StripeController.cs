﻿using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Reflection.Metadata;
using System.Text;
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
using Stripe.Climate;

namespace patchikatcha_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StripeController : ControllerBase
    {
        private readonly HttpClient client;
        private readonly IConfiguration configuration;

        public StripeController(HttpClient client, IConfiguration configuration)
        {

            StripeConfiguration.ApiKey = "sk_test_51Onkz6Lwv2BbZpNwYDF8RzBVcmiQAZ59EeoWeBEYD3WJTRmhakFtyUR1tAJcCp4Vrr9mKhxzJARNA0rEPyfyofWV00cISXaGE8";
            this.client = client;
            this.configuration = configuration;
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
                LineItems = new List<SessionLineItemOptions>(),
                Metadata = new Dictionary<string, string>(),
                Mode = "payment",
                ReturnUrl = domain,
                BillingAddressCollection = "required",
                ShippingAddressCollection = new SessionShippingAddressCollectionOptions
                {
                    AllowedCountries = new List<string> { "US", "CA", "PT"}
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
                Console.WriteLine(item.ProductId);
                Console.WriteLine(item.VariantId);

                options.LineItems.Add(new SessionLineItemOptions
                {
                    Price = item.PriceId,
                    Quantity = item.Quantity,

                });

                var metadataKey = item.ProductId;

               options.Metadata.Add(metadataKey, item.VariantId.ToString());
            }

            foreach (var item in options.Metadata)
            {
                Console.WriteLine(item.Key);
                Console.WriteLine(item.Value);
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
            var apiKey = configuration["PRINTIFY_API"];
            var shopId = configuration["PRINTIFY_SHOP_ID"];

            client.DefaultRequestHeaders.Add("Authorization", apiKey);

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
                        var session = eventData.Object as Session;
                        string sessionId = session.Id;

                            // Make an additional API call to retrieve the expanded Checkout Session object
                        var options = new SessionGetOptions { Expand = new List<string> { "line_items" } };
                        var service = new SessionService();
                        var checkoutSession = service.Get(sessionId, options);

                            // Access the line_items property from the expanded Checkout Session object
                        var lineItems = checkoutSession.LineItems.Data;
                        var shippingDetails = checkoutSession.ShippingDetails;
                        var metaData = checkoutSession.Metadata;

                        var printifyOrder = new PrintifyOrderCreateDto()
                        {
                            external_id = Guid.NewGuid().ToString(),
                            label = "testOrder123",
                            line_items = new List<line_items>(),
                            shipping_method = 1,
                            is_printify_express = false,
                            send_shipping_notification = false,
                            address_to = new address_to()
                            {
                                first_name = shippingDetails.Name,
                                last_name = shippingDetails.Name,
                                email = checkoutSession.CustomerEmail,
                                phone = "",
                                country = shippingDetails.Address.Country,
                                region = "",
                                address1 = shippingDetails.Address.Line1,
                                address2 = shippingDetails.Address.Line2,
                                city = shippingDetails.Address.State,
                                zip = shippingDetails.Address.PostalCode
                            }

                        };

                        var metaDataEnumerator = metaData.GetEnumerator();
                        foreach (var item in lineItems)
                        {
                            // Move to the next metadata entry
                            metaDataEnumerator.MoveNext();

                            // Get the metadata key-value pair for the current line item
                            var kv = metaDataEnumerator.Current;

                            var lineItem = new line_items()
                            {
                                product_id = kv.Key,
                                variant_id = Convert.ToInt32(kv.Value),
                                quantity = (int)item.Quantity,
                            };

                            printifyOrder.line_items.Add(lineItem);
                        }

                        var url = $"https://api.printify.com/v1/shops/{shopId}/orders.json";
                        var jsonOrder = JsonSerializer.Serialize(printifyOrder);
                        var content = new StringContent(jsonOrder, Encoding.UTF8, "application/json");
                        HttpResponseMessage response = await client.PostAsync(url, content);

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