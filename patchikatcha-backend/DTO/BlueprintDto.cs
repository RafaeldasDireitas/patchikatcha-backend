using System.Text.Json.Serialization;

namespace patchikatcha_backend.DTO
{
    public class BlueprintDto
    {
        public HandlingTime handling_time { get; set; }

        [JsonPropertyName("profiles")]
        public Profiles[] Profiles { get; set; }

    }

    public class HandlingTime
    {
        public int value { get; set; }
        public string unit { get; set; }
    }

    public class Profiles
    {
        public int[] variant_ids { get; set; }
        public FirstItem first_item { get; set; }
        public AdditionalItems additional_items { get; set; }
        public string[] countries { get; set; }
    }

    public class FirstItem
    {
        public int cost { get; set; }
        public string currency { get; set; }
    }

    public class AdditionalItems
    {
        public int cost { get; set; }
        public string currency { get; set; }
    }
}
