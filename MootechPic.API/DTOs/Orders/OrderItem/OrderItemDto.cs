namespace MootechPic.API.DTOs.Orders.OrderItem
{
    public class OrderItemDto
    {
        public Guid Id { get; set; }
        public Guid? ProductId { get; set; }
        public Guid? SparePartId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
