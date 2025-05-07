namespace MootechPic.API.DTOs.Requests
{
    using System.ComponentModel.DataAnnotations;
    public class UpdateRequestDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string? Status { get; set; }

        public string? AdminResponse { get; set; }

        public DateTime? AnsweredAt { get; set; }
    }

}
