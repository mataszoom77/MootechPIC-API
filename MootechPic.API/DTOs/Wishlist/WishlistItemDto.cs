namespace MootechPic.API.DTOs.Wishlist
{
    public class WishlistItemDto
    {
        public Guid Id { get; set; }
        public string ItemType { get; set; } = null!; 
        public Guid ItemId { get; set; }

        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal? Price { get; set; }

        public string? ImageUrl { get; set; } 
        public List<string>? ImageUrls { get; set; }
    }
}
