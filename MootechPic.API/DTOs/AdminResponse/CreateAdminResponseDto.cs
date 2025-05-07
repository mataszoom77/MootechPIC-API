namespace MootechPic.API.DTOs.AdminResponse
{
    public class CreateAdminResponseDto
    {
        public string? Description { get; set; }
        public List<AdminResponseAttachmentDto>? Attachments { get; set; }
    }
}
