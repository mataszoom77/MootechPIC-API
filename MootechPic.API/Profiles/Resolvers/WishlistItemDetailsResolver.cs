using AutoMapper;
using MootechPic.API.DTOs.Wishlist;
using MootechPic.API.Models;
using MootechPic.API.Data;
using Microsoft.EntityFrameworkCore;

namespace MootechPic.API.Profiles.Resolvers
{
    public class WishlistItemDetailsResolver : IValueResolver<WishlistItem, WishlistItemDto, string?>
    {
        private readonly AppDbContext _context;

        public WishlistItemDetailsResolver(AppDbContext context)
        {
            _context = context;
        }

        public string? Resolve(WishlistItem source, WishlistItemDto destination, string? destMember, ResolutionContext context)
        {
            if (source.ItemType == "Product")
            {
                var product = _context.Products
                    .AsNoTracking()
                    .FirstOrDefault(p => p.Id == source.ItemId);
                return product?.Name;
            }
            else if (source.ItemType == "SparePart")
            {
                var sparePart = _context.SpareParts
                    .AsNoTracking()
                    .FirstOrDefault(sp => sp.Id == source.ItemId);
                return sparePart?.Name;
            }

            return null;
        }
    }

    public class WishlistItemImageResolver : IValueResolver<WishlistItem, WishlistItemDto, string?>
    {
        private readonly AppDbContext _context;

        public WishlistItemImageResolver(AppDbContext context)
        {
            _context = context;
        }

        public string? Resolve(WishlistItem source, WishlistItemDto destination, string? destMember, ResolutionContext context)
        {
            if (source.ItemType == "Product")
            {
                var product = _context.Products
                    .Include(p => p.ProductImages) // ✅ Include images!
                    .AsNoTracking()
                    .FirstOrDefault(p => p.Id == source.ItemId);
                return product?.ProductImages?.FirstOrDefault()?.Url;
            }
            else if (source.ItemType == "SparePart")
            {
                var sparePart = _context.SpareParts
                    .Include(sp => sp.SparePartImages) // ✅ Include images!
                    .AsNoTracking()
                    .FirstOrDefault(sp => sp.Id == source.ItemId);
                return sparePart?.SparePartImages?.FirstOrDefault()?.Url;
            }

            return null;
        }
    }
}
