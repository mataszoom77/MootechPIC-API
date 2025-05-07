namespace MootechPic.API.DTOs.CartItems
{
    using System.ComponentModel.DataAnnotations;

    public class CreateCartItemDto
    {
        [Required]
        public string ItemType { get; set; } = null!;  // "Product" or "SparePart"

        [Required]
        public Guid ItemId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
