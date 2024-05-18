using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace patchikatcha_backend.Models
{
    public class Review
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string ProductId { get; set; }
        public string ProductTitle { get; set; }
        public string ProductImage { get; set; }
        public string ApplicationUserId { get; set; }
        [JsonIgnore]
        public ApplicationUser ApplicationUser { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreatedAt { get; set; }

    }
}
