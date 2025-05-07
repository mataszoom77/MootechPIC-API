namespace MootechPic.API.DTOs.SpareParts
{
    using System.ComponentModel.DataAnnotations;
    public class UpdateSparePartDto : CreateSparePartDto
    {
        [Required]
        public Guid Id { get; set; }
    }
}
