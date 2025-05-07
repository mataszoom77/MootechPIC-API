using System;

namespace MootechPic.API.Models
{
    public class SparePartImage
    {
        public Guid Id { get; set; }
        public Guid SparePartId { get; set; }
        public SparePart SparePart { get; set; } = null!;
        public string Url { get; set; } = null!;
        public int DisplayOrder { get; set; }
    }
}
