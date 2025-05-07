using MootechPic.API.DTOs.Users;

namespace MootechPic.API.DTOs.Auth
{
    public class TokenResponseDto
    {
        public string Token { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public UserDto User { get; set; } = null!;
    }
}
