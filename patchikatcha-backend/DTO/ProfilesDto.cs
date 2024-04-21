namespace patchikatcha_backend.DTO
{
    public class ProfilesDto
    {
        public int[] variant_ids { get; set; }
        public first_item first_item { get; set; }
        public additional_items additional_items { get; set; }
        public string[] countries { get; set; }
    }

    public class first_item
    {
        public int cost { get; set; }
        public string currency { get; set; }
    }

    public class additional_items
    {
        public int cost { get; set; }
        public string currency { get; set; }
    }
}
