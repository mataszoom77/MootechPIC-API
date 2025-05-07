namespace MootechPic.API.DTOs.Wishlist
{
    public class CreateWishlistItemDto
    {
        public Guid ItemId { get; set; }
        public string ItemType { get; set; } = null!;
    }
}
