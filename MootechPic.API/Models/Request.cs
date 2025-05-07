using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MootechPic.API.Models
{
    public class Request
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public User User { get; set; } = null!;
        public string? Name { get; set; }
        public string? Description { get; set; }

        // ImageUrl removed entirely

        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        // Now explicitly a List<T>
        public ICollection<RequestImage> Images { get; set; }
            = new List<RequestImage>();
        public ICollection<AdminResponse> Responses { get; set; }
            = new List<AdminResponse>();
    }

}
