namespace MootechPic.API.DTOs.SpareParts
{
    using System.ComponentModel.DataAnnotations;

    public class CreateSparePartDto
    {
        [Required]
        public string? Name { get; set; }

        public string? Description { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        public List<string> ImageUrls { get; set; } = new List<string>();
    }

}
