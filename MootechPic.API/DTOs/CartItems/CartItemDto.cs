namespace MootechPic.API.DTOs.CartItems
{
    public class CartItemDto
    {
        public Guid Id { get; set; }
        public Guid CartId { get; set; }
        public string? ItemType { get; set; }
        public Guid ItemId { get; set; }
        public int Quantity { get; set; }

        // Nested fields for display
        public string? ItemName { get; set; }
        public string? ItemImageUrl { get; set; }
        public decimal? ItemPrice { get; set; }
    }


}
