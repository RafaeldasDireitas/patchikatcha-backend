using System.ComponentModel.DataAnnotations;

namespace patchikatcha_backend.DTO
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string ProductId { get; set; }
        public required string ApplicationUserId { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreatedAt { get; set; }
    }
}
