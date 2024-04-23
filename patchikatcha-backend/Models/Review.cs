using System.ComponentModel.DataAnnotations;

namespace patchikatcha_backend.Models
{
    public class Review
    {
        public int Id { get; set; }
        public required string ProductId { get; set; }
        public required string Username { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreatedAt { get; set; }

    }
}
