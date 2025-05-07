using MootechPic.API.DTOs.AdminResponse;
using MootechPic.API.DTOs.Users;

namespace MootechPic.API.DTOs.Requests
{
    public class RequestDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<Uri> ImageUrls { get; set; } = new();
        public string? Status { get; set; } // e.g. Pending, Answered, Cancelled
        public DateTime CreatedAt { get; set; }
        public List<AdminResponseDto> Responses { get; set; } = new();

        public UserDto? User { get; set; }
    }

}
