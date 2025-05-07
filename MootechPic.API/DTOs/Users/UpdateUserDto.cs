namespace MootechPic.API.DTOs.Users
{
    using System.ComponentModel.DataAnnotations;
    public class UpdateUserDto : CreateUserDto
    {
        [Required]
        public Guid Id { get; set; }
    }
}
