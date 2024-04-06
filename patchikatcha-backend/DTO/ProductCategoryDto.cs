using System.Text.Json.Serialization;

namespace patchikatcha_backend.DTO
{
    public class ProductCategoryDto
    {
        [JsonPropertyName("current_page")]
        public int CurrentPage { get; set; }
        [JsonPropertyName("data")]
        public Data[] Data { get; set; }
    }

    public class Data
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("tags")]
        public string[] Tags { get; set; }
        [JsonPropertyName("options")]
        public Options[] Options { get; set; }
        [JsonPropertyName("variants")]
        public Variants[] Variants { get; set; }
        [JsonPropertyName("images")]
        public Images[] Images { get; set; }
        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; }
        [JsonPropertyName("updated_at")]
        public string UpdatedAt { get; set; }
        [JsonPropertyName("visible")]
        public bool Visible { get; set; }
        [JsonPropertyName("is_locked")]
        public bool IsLocked { get; set; }
        [JsonPropertyName("external")]
        public External External { get; set; }
        [JsonPropertyName("blueprint_id")]
        public int BlueprintId { get; set; }
        [JsonPropertyName("user_id")]
        public int UserId { get; set; }
        [JsonPropertyName("shop_id")]
        public int ShopId { get; set; }
        [JsonPropertyName("print_provider_id")]
        public int PrintProviderId { get; set; }
        [JsonPropertyName("print_areas")]
        public PrintAreas[] PrintAreas { get; set; }
        [JsonPropertyName("print_details")]
        public string[] PrintDetails { get; set; }
        [JsonPropertyName("is_printify_express_eligible")]
        public bool IsPrintifyExpressEligible { get; set; }
        [JsonPropertyName("is_printify_express_enabled")]
        public bool IsPrintifyExpressEnabled { get; set; }
        [JsonPropertyName("is_economy_shipping_eligible")]
        public bool IsEconomyShippingEligible { get; set; }
        [JsonPropertyName("is_economy_shipping_enabled")]
        public bool IsEconomyShippingEnabled { get; set; }

    }

    public class PrintAreas
    {
        public int[] VariantIds { get; set; }
        public Placeholders[] Placeholders { get; set; }
    }

    public class Placeholders
    {
        public string Position { get; set; }
        public class Images
        {
            public string Id { get; set;  }
            public string Name { get; set; }
            public string Type { get; set; }
            public int Height { get; set; }
            public int Width { get; set; }
            public float X { get; set; }
            public float Y { get; set; }
            public int Scale { get; set; }
            public int Angle { get; set; }
        }
    }

    public class External
    {
        public string Id { get; set; }
        public string Handle { get; set; }
    }

    public class Options
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("values")]
        public Values[] Values { get; set; }
    }

    public class Values
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("colors")]
        public string[] Colors { get; set; }
    }

    public class Variants
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("sku")]
        public string Sku { get; set; }
        [JsonPropertyName("cost")]
        public int Cost { get; set; }
        [JsonPropertyName("price")]
        public int Price { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("grams")]
        public int Grams { get; set; }
        [JsonPropertyName("is_enabled")]
        public bool IsEnabled { get; set; }
        [JsonPropertyName("is_default")]
        public bool IsDefault { get; set; }
        [JsonPropertyName("is_available")]
        public bool IsAvailable { get; set; }
        [JsonPropertyName("is_printify_express_eligible")]
        public bool IsPrintifyExpressEligible { get; set; }
        public int[] Options { get; set; }
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
    }

    public class Images
    {
        [JsonPropertyName("src")]
        public string Src { get; set; }
        [JsonPropertyName("variant_ids")]
        public int[] VariantIds { get; set; }
        public string Position { get; set; }
        [JsonPropertyName("is_default")]
        public bool IsDefault { get; set; }
        [JsonPropertyName("is_selected_for_publishing")]
        public bool IsSelectedForPublishing { get; set; }
        [JsonPropertyName("orders")]
        public string Order { get; set; }
    }
}
