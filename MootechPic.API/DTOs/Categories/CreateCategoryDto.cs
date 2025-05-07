namespace MootechPic.API.DTOs.Categories
{
    using System.ComponentModel.DataAnnotations;

    public class CreateCategoryDto
    {
        [Required]
        public string? Name { get; set; }
    }
}
