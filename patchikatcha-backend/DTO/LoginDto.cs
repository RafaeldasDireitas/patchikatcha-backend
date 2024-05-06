using System.ComponentModel.DataAnnotations;

namespace patchikatcha_backend.DTO
{
    public class LoginDto
    {
        [DataType(DataType.EmailAddress)]
        public required string Email { get; set; }
        [DataType(DataType.Password)]
        public required string Password { get; set; }
        public required string ApiKey { get; set; }
    }
}
