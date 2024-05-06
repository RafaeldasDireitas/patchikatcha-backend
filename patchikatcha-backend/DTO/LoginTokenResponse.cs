namespace patchikatcha_backend.DTO
{
    public class LoginTokenResponse
    {
        public string jwtToken { get; set; }
        public string userId { get; set; }
        public string verificationResponse { get; set; }
    }
}
