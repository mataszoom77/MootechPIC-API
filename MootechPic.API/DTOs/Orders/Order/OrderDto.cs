using MootechPic.API.DTOs.Orders.OrderItem;

namespace MootechPic.API.DTOs.Orders.Order
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public string ShipName { get; set; }
        public string ShipAddress { get; set; }
        public string ShipCity { get; set; }
        public string ShipPincode { get; set; }
        public string ShipCountry { get; set; }
        public DateTime CreatedAt { get; set; }


        public string DeliveryMethod { get; set; }
        public decimal DeliveryCost { get; set; }

        public string PaymentMethod { get; set; }

        public string BizName { get; set; }
        public string BizAddress { get; set; }
        public string BizVatNumber { get; set; }

        public decimal Subtotal { get; set; }
        public decimal Taxes { get; set; }
        public decimal Total { get; set; }

        public string Status { get; set; }

        public List<OrderItemDto> Items { get; set; }
    }
}
