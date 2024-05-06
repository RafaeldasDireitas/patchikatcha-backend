using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Reflection.Metadata;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using patchikatcha_backend.Data;
using patchikatcha_backend.DTO;
using patchikatcha_backend.Models;
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
        private readonly AuthDbContext authDbContext;
        private readonly UserManager<ApplicationUser> userManager;

        public StripeController(HttpClient client, IConfiguration configuration, AuthDbContext authDbContext, UserManager<ApplicationUser> userManager)
        {

            StripeConfiguration.ApiKey = "sk_test_51Onkz6Lwv2BbZpNwYDF8RzBVcmiQAZ59EeoWeBEYD3WJTRmhakFtyUR1tAJcCp4Vrr9mKhxzJARNA0rEPyfyofWV00cISXaGE8";
            this.client = client;
            this.configuration = configuration;
            this.authDbContext = authDbContext;
            this.userManager = userManager;
        }

        [HttpPost]
        [Authorize]
        [Route("create-checkout-session")]
        public async Task<ActionResult> Create(string userId)
        {
            int shippingRate = 0;
            var findUser = await userManager.FindByIdAsync(userId);

            if (findUser != null)
            {
                var userCart = await authDbContext.Carts.Where(product => product.ApplicationUserId == userId).ToListAsync();

                foreach (var cart in userCart)
                {
                    if (cart.Quantity == 1)
                    {
                        shippingRate = shippingRate + cart.FirstItem;
                    }

                    if (cart.Quantity > 1)
                    {
                        shippingRate = shippingRate + cart.FirstItem;
                        for (int i = 0; i < cart.Quantity - 1; i++)
                        {
                            shippingRate = shippingRate + cart.AdditionalItems;
                        }
                    }
                }

                var domain = "http://localhost:3000/checkout/order-successful";

                var customerService = new CustomerService();
                var existingCustomers = customerService.ListAutoPaging(new CustomerListOptions { Email = findUser.Email });

                Customer customer;

                if (existingCustomers.Any())
                {
                    // Use existing customer if found
                    customer = existingCustomers.FirstOrDefault();
                }
                else
                {
                    // Create new customer in Stripe
                    var customerOptions = new CustomerCreateOptions
                    {
                        Email = findUser.Email,
                    };
                    customer = await customerService.CreateAsync(customerOptions);
                }

                var options = new SessionCreateOptions
                {
                    Customer = customer.Id,
                    UiMode = "embedded",
                    LineItems = new List<SessionLineItemOptions>(),
                    Metadata = new Dictionary<string, string>(),
                    Mode = "payment",
                    AllowPromotionCodes = true,
                    ReturnUrl = domain,
                    Locale = "auto",
                    BillingAddressCollection = "required",
                    ShippingAddressCollection = new SessionShippingAddressCollectionOptions
                    {
                        AllowedCountries = new List<string> { $"{findUser.UserCountry}" },
                    },
                    AutomaticTax = new SessionAutomaticTaxOptions
                    {
                        Enabled = true
                    },
                    CustomerUpdate = new SessionCustomerUpdateOptions
                    {
                        Shipping = "auto",
                    },
                    ShippingOptions = new List<SessionShippingOptionOptions>
                {
                    new SessionShippingOptionOptions
                    {
                        ShippingRateData = new SessionShippingOptionShippingRateDataOptions
                        {
                            Type = "fixed_amount",
                            FixedAmount = new SessionShippingOptionShippingRateDataFixedAmountOptions {
                                Amount = shippingRate,
                                Currency = findUser.UserCurrency,
                            },
                            DisplayName = "Shipping",
                            DeliveryEstimate = new SessionShippingOptionShippingRateDataDeliveryEstimateOptions
                            {
                                Minimum = new SessionShippingOptionShippingRateDataDeliveryEstimateMinimumOptions
                                {
                                    Unit = "business_day",
                                    Value = 2
                                },
                                Maximum = new SessionShippingOptionShippingRateDataDeliveryEstimateMaximumOptions
                                {
                                    Unit = "business_day",
                                    Value = 10
                                },
                            }
                        },
                    },
                }
                };

                foreach (var item in userCart)
                {
                    string uniqueGuid = Guid.NewGuid().ToString();
                    string firstTenChars = uniqueGuid.Substring(0, 10);

                    options.LineItems.Add(new SessionLineItemOptions
                    {
                        Price = item.PriceId,
                        Quantity = item.Quantity,

                    });

                    var metadataKey = item.VariantId.ToString() + "_" + firstTenChars;

                    options.Metadata.Add(metadataKey, item.PrintProviderId.ToString());
                }

                var service = new SessionService();
                Session session = service.Create(options);
                string clientSecret = session.ClientSecret;
                string clientId = session.Id;

                var clientData = new
                {
                    clientSecret = clientSecret,
                    clientId = clientId
                };

                var clientJson = JsonSerializer.Serialize(clientData);

                return Ok(clientJson);
            }

            return BadRequest("No user found");
            
        }

        [HttpGet]
        [Route("session-status")]
        public ActionResult SessionStatus([FromQuery] string sessionId)
        {
            var sessionService = new SessionService();
            Session session = sessionService.Get(sessionId);

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
                customer_email = session.CustomerDetails.Email,
                shipping_address = session.CustomerDetails.Address
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
                        var userEmail = checkoutSession.CustomerEmail;
                        string fullName = shippingDetails.Name;
                        string firstName;
                        string lastName;

                        fullName = fullName.Trim();
                        string[] nameArray = fullName.Split(' ');
                        firstName = nameArray[0];
                        lastName = nameArray.Length == 1 ? firstName : nameArray[^1];

                        //var printifyOrderList = new List<PrintifyOrderCreateDto>();

                        //var sortedMetadata = metaData.Reverse();
                        //var metaDataEnumerator = sortedMetadata.GetEnumerator();
                        //foreach (var item in lineItems)
                        //{
                        //    // Move to the next metadata entry
                        //    metaDataEnumerator.MoveNext();

                        //    // Get the metadata key-value pair for the current line item
                        //    var kv = metaDataEnumerator.Current;
                        //    var printProviderId = kv.Value;

                        //    var variantId = Convert.ToInt32(kv.Key.Split("_")[0]);

                        //    // Try to find an existing printify order with the same print provider ID
                        //    var existingPrintifyOrder = printifyOrderList.FirstOrDefault(order => order.print_provider_id == printProviderId);

                        //    if (existingPrintifyOrder != null)
                        //    {
                        //        // If an existing printify order is found, add the line item to it
                        //        var lineItem = new line_items()
                        //        {
                        //            product_id = item.Price.LookupKey,
                        //            variant_id = variantId,
                        //            quantity = (int)item.Quantity,
                        //        };

                        //        existingPrintifyOrder.line_items.Add(lineItem);
                        //    }
                        //    else
                        //    {
                        //        string label1 = Guid.NewGuid().ToString().Substring(0, 3);
                        //        string label2 = Guid.NewGuid().ToString().Substring(0, 2);
                        //        string labelName = label1 + label2;

                        //        // If no existing printify order is found, create a new one and add the line item to it
                        //        var newPrintifyOrder = new PrintifyOrderCreateDto
                        //        {
                        //            external_id = Guid.NewGuid().ToString(),
                        //            label = "Order-" + labelName, //add 2 guids and grab only 3 chars of each
                        //            line_items = new List<line_items>(),
                        //            shipping_method = 1,
                        //            is_printify_express = false,
                        //            send_shipping_notification = false,
                        //            address_to = new address_to()
                        //            {
                        //                first_name = firstName,
                        //                last_name = lastName,
                        //                email = checkoutSession.CustomerEmail,
                        //                phone = "",
                        //                country = shippingDetails.Address.Country,
                        //                region = shippingDetails.Address.City,
                        //                address1 = shippingDetails.Address.Line1,
                        //                address2 = shippingDetails.Address.Line2,
                        //                city = shippingDetails.Address.City,
                        //                zip = shippingDetails.Address.PostalCode
                        //            },
                        //            print_provider_id = printProviderId
                        //        };

                        //        var lineItem = new line_items()
                        //        {
                        //            product_id = item.Price.LookupKey,
                        //            variant_id = variantId,
                        //            quantity = (int)item.Quantity,
                        //        };

                        //        newPrintifyOrder.line_items.Add(lineItem);

                        //        // Add the new printify order to the list
                        //        printifyOrderList.Add(newPrintifyOrder);
                        //    }
                        //}

                        string label1 = Guid.NewGuid().ToString().Substring(0, 3);
                        string label2 = Guid.NewGuid().ToString().Substring(0, 2);
                        string labelName = label1 + label2;

                        var printifyOrder = new PrintifyOrderCreateDto()
                        {
                            external_id = Guid.NewGuid().ToString(),
                            label = "Order-" + labelName, //add 2 guids and grab only 3 chars of each
                            line_items = new List<line_items>(),
                            shipping_method = 1,
                            is_printify_express = false,
                            is_economy_shipping = false,
                            send_shipping_notification = false,
                            address_to = new address_to()
                            {
                                first_name = firstName,
                                last_name = lastName,
                                email = checkoutSession.CustomerEmail,
                                phone = "",
                                country = shippingDetails.Address.Country,
                                region = shippingDetails.Address.City,
                                address1 = shippingDetails.Address.Line1,
                                address2 = shippingDetails.Address.Line2,
                                city = shippingDetails.Address.City,
                                zip = shippingDetails.Address.PostalCode
                            }
                        };

                        var sortedMetadata = metaData.Reverse();
                        var metaDataEnumerator = sortedMetadata.GetEnumerator();
                        foreach (var item in lineItems)
                        {
                            // Move to the next metadata entry
                            metaDataEnumerator.MoveNext();

                            // Get the metadata key-value pair for the current line item
                            var kv = metaDataEnumerator.Current;

                            var variantId = Convert.ToInt32(kv.Key.Split("_")[0]);

                            var lineItem = new line_items()
                            {
                                product_id = item.Price.LookupKey,
                                variant_id = variantId,
                                quantity = (int)item.Quantity,
                            };

                            printifyOrder.line_items.Add(lineItem);

                        }

                        //foreach (var order in printifyOrderList)
                        //{
                            var jsonOrder = JsonSerializer.Serialize(printifyOrder);
                            var content = new StringContent(jsonOrder, Encoding.UTF8, "application/json");
                            var url = $"https://api.printify.com/v1/shops/{shopId}/orders.json";

                            HttpResponseMessage response = await client.PostAsync(url, content);

                            if (response.IsSuccessStatusCode)
                            {
                                string responseContent = await response.Content.ReadAsStringAsync();
                                JsonDocument doc = JsonDocument.Parse(responseContent);
                                JsonElement root = doc.RootElement;
                                string orderId = root.GetProperty("id").GetString();

                                var newOrder = new Models.Order()
                                {
                                    OrderId = orderId,
                                    UserEmail = userEmail,
                                };

                                await authDbContext.AddAsync(newOrder);
                                await authDbContext.SaveChangesAsync();
                            }
                            else
                            {
                                // Log the response content when a 500 error occurs
                                Console.WriteLine($"Failed to create order. Status code: {response.StatusCode}");
                                string responseContent = await response.Content.ReadAsStringAsync();
                                Console.WriteLine($"Response content: {responseContent}");
                            }
                        //}

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