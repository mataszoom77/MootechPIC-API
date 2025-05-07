namespace MootechPic.API.DTOs.Products
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public List<string> ImageUrls { get; set; } = new();

        public Guid CategoryId { get; set; }
        public string? CategoryName { get; set; } // optional for display
    }

}
