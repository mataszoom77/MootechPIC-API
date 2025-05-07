﻿using System;
using System.Collections.Generic;

namespace MootechPic.API.Models
{
    public class Category
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }

        public ICollection<Product>? Products { get; set; }
    }
}
