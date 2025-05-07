using System;

namespace MootechPic.API.Models
{
    public class ProductSparePart
    {
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public Guid SparePartId { get; set; }
        public SparePart? SparePart { get; set; }
    }
}
