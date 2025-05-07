using System;
using System.Collections.Generic;

namespace MootechPic.API.Models
{
    public class SparePart
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public ICollection<SparePartImage> SparePartImages { get; set; } = new List<SparePartImage>();

        public ICollection<ProductSparePart>? ProductSpareParts { get; set; }
    }
}
