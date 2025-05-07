namespace MootechPic.API.DTOs.Auth
{
    public class TokenRequestDto
    {
        public string Token { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }
}
