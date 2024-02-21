using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace patchikatcha_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly HttpClient client;

        public ProductController(HttpClient client)
        {
            this.client = client;
        }

        // GET: ProductController
        [HttpGet]
        [Route("new-products")]
        public async Task<ActionResult> GetNewProducts()
        {
            client.DefaultRequestHeaders.Add("Authorization", "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiJ9.eyJhdWQiOiIzN2Q0YmQzMDM1ZmUxMWU5YTgwM2FiN2VlYjNjY2M5NyIsImp0aSI6IjcxNmRjNzdmZTMwZTFiZTAyZTdkZDMwZDRiZWU0YzQ2ZmVjMDlkNTU4ZDYwOWE2MWI4MTI1OWExZjg0NDBiZThlMTY1NTA3OTg3MGE3MDVmIiwiaWF0IjoxNzA4MzQwNjY3LjgwNDY1OCwibmJmIjoxNzA4MzQwNjY3LjgwNDY2LCJleHAiOjE3Mzk5NjMwNjcuNzk0MzY4LCJzdWIiOiIxNzE0MDc5MCIsInNjb3BlcyI6WyJzaG9wcy5tYW5hZ2UiLCJzaG9wcy5yZWFkIiwiY2F0YWxvZy5yZWFkIiwib3JkZXJzLnJlYWQiLCJvcmRlcnMud3JpdGUiLCJwcm9kdWN0cy5yZWFkIiwicHJvZHVjdHMud3JpdGUiLCJ3ZWJob29rcy5yZWFkIiwid2ViaG9va3Mud3JpdGUiLCJ1cGxvYWRzLnJlYWQiLCJ1cGxvYWRzLndyaXRlIiwicHJpbnRfcHJvdmlkZXJzLnJlYWQiXX0.AHXC4NtWwKIhRY3cAuW_kdnCJ1tHARlgMhL4Km1fcifnA1PlYSWlGjn7eEASseS9eEd38mJOBkqEKc7wMHg");

            string url = "https://api.printify.com/v1/shops/14479257/products.json";

            HttpResponseMessage response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest();
            }

            string jsonResponse = await response.Content.ReadAsStringAsync();
            JsonDocument json = JsonDocument.Parse(jsonResponse);
            JsonElement data = json.RootElement.GetProperty("data");
            JsonElement[] slicedData = data.EnumerateArray().Take(3).ToArray();

            return Ok(slicedData);
        }

        [HttpPost]
        [Route("publish-product")]
        public async Task<ActionResult> PublishProduct()
        {
            return Ok("");
        }
    }
}
