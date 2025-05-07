using System;

namespace MootechPic.API.Models
{
    public class CartItem
    {
        public Guid Id { get; set; }
        public Guid CartId { get; set; }
        public Cart? Cart { get; set; }

        public string? ItemType { get; set; } // "PRODUCT" or "SPARE_PART"
        public Guid ItemId { get; set; }     // Refers to Product.Id or SparePart.Id
        public int Quantity { get; set; }
    }
}
