namespace MootechPic.API.Models
{
    public class TokenResponse
    {
        public string Token { get; set; }        // your JWT
        public string RefreshToken { get; set; } // long‐lived random token
    }
}
