using System;

namespace MootechPic.API.Models
{
    public class ProductImage
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public string Url { get; set; } = null!;
        public int DisplayOrder { get; set; }

    }
}
