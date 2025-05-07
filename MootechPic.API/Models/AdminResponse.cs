namespace MootechPic.API.Models
{
    public class AdminResponse
    {
        public Guid Id { get; set; }
        public Guid RequestId { get; set; }
        public Request Request { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public ICollection<AdminResponseAttachment> Attachments { get; set; }
            = new List<AdminResponseAttachment>();
    }
}
