using System;
using System.Collections.Generic;

namespace MootechPic.API.Models
{
    public class Cart
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<CartItem>? CartItems { get; set; }
    }
}
