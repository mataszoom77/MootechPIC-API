namespace MootechPic.API.DTOs.Categories
{
    using System.ComponentModel.DataAnnotations;
    public class UpdateCategoryDto : CreateCategoryDto
    {
        [Required]
        public Guid Id { get; set; }
    }
}
