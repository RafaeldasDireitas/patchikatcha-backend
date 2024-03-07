namespace patchikatcha_backend.DTO
{
    public class PrintifyOrderCreateDto
    {
        public string external_id { get; set; }
        public string label { get; set; }
        public List<line_items> line_items { get; set; }
        public int shipping_method { get; set; }
        public bool is_printify_express { get; set; }
        public bool send_shipping_notification {  get; set; }
        public address_to address_to { get; set; }

        
    }

    public class line_items
    {
        public string product_id { get; set; }
        public int variant_id { get; set; }
        public int quantity { get; set; }
    }

    public class address_to
    {
        public string first_name {  get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string country { get; set; }
        public string region { get; set; }
        public string address1 {  get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string zip { get; set; }
    }
}
