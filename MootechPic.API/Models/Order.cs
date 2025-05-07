using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MootechPic.API.Models
{
    public class Order
    {
        [Key]
        public Guid Id { get; set; }

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

        // Totals
        public decimal Subtotal { get; set; }
        public decimal Taxes { get; set; }
        public decimal Total { get; set; }

        public string Status { get; set; } = "pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
