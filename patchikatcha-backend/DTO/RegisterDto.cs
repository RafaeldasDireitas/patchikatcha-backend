using System.ComponentModel.DataAnnotations;

namespace patchikatcha_backend.DTO
{
    public class RegisterDto
    {
        [DataType((DataType.EmailAddress))]
        public required string Email { get; set; }

        [DataType(DataType.Password)]
        public required string Password { get; set; }

        public required string[] Roles { get; set; }
    }
}
