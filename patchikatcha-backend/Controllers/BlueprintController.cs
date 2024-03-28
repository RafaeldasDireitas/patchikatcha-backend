using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using static System.Net.WebRequestMethods;

namespace patchikatcha_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlueprintController : ControllerBase
    {
        private readonly HttpClient client;
        private readonly IConfiguration configuration;
        public BlueprintController(HttpClient client, IConfiguration configuration)
        {
            this.configuration = configuration;
            this.client = client;
        }

        [HttpGet]
        [Route("get-blueprint/{blueprintId}/{printProvider}")]
        public async Task<IActionResult> GetBluePrint(string blueprintId, string printProvider)
        {
            var apiKey = configuration["PRINTIFY_API"];
            var shopId = configuration["PRINTIFY_SHOP_ID"];

            client.DefaultRequestHeaders.Add("Authorization", apiKey);

            string url = $"https://api.printify.com/v1/catalog/blueprints/{blueprintId}/print_providers/{printProvider}/shipping.json";

            HttpResponseMessage response = await client.GetAsync(url);

            string responseJson = await response.Content.ReadAsStringAsync();

            return Ok(responseJson);
        }
    }
}
