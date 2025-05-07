namespace MootechPic.API.DTOs.Products
{
    using System.ComponentModel.DataAnnotations;

    public class CreateProductDto
    {
        [Required]
        public string? Name { get; set; }

        public string? Description { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        public List<string> ImageUrls { get; set; } = new List<string>();

        [Required]
        public Guid CategoryId { get; set; }
    }
}
