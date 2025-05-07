namespace MootechPic.API.DTOs.Products
{
    using System.ComponentModel.DataAnnotations;
    public class UpdateProductDto : CreateProductDto
    {
        [Required]
        public Guid Id { get; set; }
    }
}
