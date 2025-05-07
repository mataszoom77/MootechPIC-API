using MootechPic.API.DTOs.Orders.OrderItem;

namespace MootechPic.API.DTOs.Orders.Order
{
    public class CreateOrderDto
    {
        public Guid UserId { get; set; }

        // Shipping
        public string ShipName { get; set; }
        public string ShipAddress { get; set; }
        public string ShipCity { get; set; }
        public string ShipPincode { get; set; }
        public string ShipCountry { get; set; }

        // Delivery
        public string DeliveryMethod { get; set; }
        public decimal DeliveryCost { get; set; }

        // Payment
        public string PaymentMethod { get; set; }

        // Business (optional)
        public string BizName { get; set; }
        public string BizAddress { get; set; }
        public string BizVatNumber { get; set; }

        // Line items
        public List<CreateOrderItemDto> Items { get; set; }
    }
}
