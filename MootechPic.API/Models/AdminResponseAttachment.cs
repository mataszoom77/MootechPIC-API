namespace MootechPic.API.Models
{
    public class AdminResponseAttachment
    {
        public Guid Id { get; set; }
        public Guid AdminResponseId { get; set; }
        public AdminResponse AdminResponse { get; set; } = null!;

        // reuse existing Product and SparePart tables
        public string ItemType { get; set; } = null!;     // "Product" or "SparePart"
        public Guid ItemId { get; set; }
    }
}
