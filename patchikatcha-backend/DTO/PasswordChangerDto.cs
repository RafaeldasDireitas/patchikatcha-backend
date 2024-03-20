namespace patchikatcha_backend.DTO
{
    public class PasswordChangerDto
    {
        public string userEmail { get; set; }
        public string token { get; set; }
        public string password { get; set; }
    }
}
