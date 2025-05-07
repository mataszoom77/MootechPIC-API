namespace MootechPic.API.DTOs.Users
{
    using System.ComponentModel.DataAnnotations;

    public class CreateUserDto
    {
        [Required]
        public string? Name { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? PasswordHash { get; set; }

        [Required]
        public string? Role { get; set; } 
    }
}
