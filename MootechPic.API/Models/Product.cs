using System;
using System.Collections.Generic;

namespace MootechPic.API.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

        public Guid CategoryId { get; set; }
        public Category? Category { get; set; }

        public ICollection<ProductSparePart>? ProductSpareParts { get; set; }
    }
}
