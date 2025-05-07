namespace MootechPic.API.DTOs.Requests
{
    using System.ComponentModel.DataAnnotations;

    public class CreateRequestDto
    {

        [Required]
        public string? Name { get; set; }

        public string? Description { get; set; } 
        
        [Required]
        [MinLength(1)]
        public List<string> ImageUrls { get; set; } = new();
    }

}
