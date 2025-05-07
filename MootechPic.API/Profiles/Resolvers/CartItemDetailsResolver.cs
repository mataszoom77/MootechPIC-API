using AutoMapper;
using MootechPic.API.Data;
using MootechPic.API.Models;
using MootechPic.API.DTOs.CartItems;
using Microsoft.EntityFrameworkCore;

namespace MootechPic.API.Profiles.Resolvers
{
    public class CartItemDetailsResolver : IMappingAction<CartItem, CartItemDto>
    {
        private readonly AppDbContext _context;

        public CartItemDetailsResolver(AppDbContext context)
        {
            _context = context;
        }

        public void Process(CartItem source, CartItemDto destination, ResolutionContext context)
        {
            switch (source.ItemType)
            {
                case "Product":
                    var product = _context.Products
                        .Include(p => p.ProductImages) // ✅ include images
                        .AsNoTracking()
                        .FirstOrDefault(p => p.Id == source.ItemId);

                    if (product != null)
                    {
                        destination.ItemName = product.Name;
                        destination.ItemImageUrl = product.ProductImages?.FirstOrDefault()?.Url; // ✅ first image
                        destination.ItemPrice = product.Price;
                    }
                    break;

                case "SparePart":
                    var sparePart = _context.SpareParts
                        .Include(sp => sp.SparePartImages) // ✅ include images
                        .AsNoTracking()
                        .FirstOrDefault(sp => sp.Id == source.ItemId);

                    if (sparePart != null)
                    {
                        destination.ItemName = sparePart.Name;
                        destination.ItemImageUrl = sparePart.SparePartImages?.FirstOrDefault()?.Url; // ✅ first image
                        destination.ItemPrice = sparePart.Price;
                    }
                    break;
            }
        }
    }
}
