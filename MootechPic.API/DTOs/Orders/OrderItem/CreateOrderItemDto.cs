namespace MootechPic.API.DTOs.Orders.OrderItem
{
    public class CreateOrderItemDto
    {
        public Guid? ProductId { get; set; }
        public Guid? SparePartId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
