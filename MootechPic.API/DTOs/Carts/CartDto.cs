using MootechPic.API.DTOs.CartItems;

namespace MootechPic.API.DTOs.Carts
{
    public class CartDto
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
    }
}
