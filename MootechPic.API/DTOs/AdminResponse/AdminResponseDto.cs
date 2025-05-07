namespace MootechPic.API.DTOs.AdminResponse
{
    public class AdminResponseDto
    {
        public Guid Id { get; set; }
        public Guid RequestId { get; set; }
        public string Description { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public List<AdminResponseAttachmentDto> Attachments { get; set; }
            = new();
    }

    public class AdminResponseAttachmentDto
    {
        public string ItemType { get; set; } = null!;
        public Guid ItemId { get; set; }
    }
}
