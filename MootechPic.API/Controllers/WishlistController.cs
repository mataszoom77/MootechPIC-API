using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MootechPic.API.Data;
using MootechPic.API.DTOs.Wishlist;
using MootechPic.API.Models;
using AutoMapper;
using System.Security.Claims;

namespace MootechPic.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WishlistController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public WishlistController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Wishlist
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WishlistItemDto>>> GetWishlist()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var wishlist = await _context.WishlistItems
                .Where(w => w.UserId.ToString() == userId)
                .ToListAsync();

            var result = new List<WishlistItemDto>();

            foreach (var item in wishlist)
            {
                if (item.ItemType == "Product")
                {
                    var product = await _context.Products
                        .Include(p => p.ProductImages)
                        .FirstOrDefaultAsync(p => p.Id == item.ItemId);

                    if (product != null)
                    {
                        result.Add(new WishlistItemDto
                        {
                            Id = item.Id,
                            ItemType = "Product",
                            ItemId = product.Id,
                            Name = product.Name,
                            Description = product.Description,
                            Price = product.Price,
                            ImageUrl = product.ProductImages?.FirstOrDefault()?.Url,
                            ImageUrls = product.ProductImages?.Select(pi => pi.Url).ToList()
                        });
                    }
                }
                else if (item.ItemType == "SparePart")
                {
                    var sparePart = await _context.SpareParts
                        .Include(sp => sp.SparePartImages)
                        .FirstOrDefaultAsync(sp => sp.Id == item.ItemId);

                    if (sparePart != null)
                    {
                        result.Add(new WishlistItemDto
                        {
                            Id = item.Id,
                            ItemType = "SparePart",
                            ItemId = sparePart.Id,
                            Name = sparePart.Name,
                            Description = sparePart.Description,
                            Price = sparePart.Price,
                            ImageUrl = sparePart.SparePartImages?.FirstOrDefault()?.Url,
                            ImageUrls = sparePart.SparePartImages?.Select(spi => spi.Url).ToList()
                        });
                    }
                }
            }

            return Ok(result);
        }

        // POST: api/Wishlist
        [HttpPost]
        public async Task<IActionResult> AddToWishlist([FromBody] CreateWishlistItemDto dto)
        {
            var userId = GetUserId();

            var exists = await _context.WishlistItems.AnyAsync(w =>
                w.UserId == userId && w.ItemId == dto.ItemId && w.ItemType == dto.ItemType);

            if (exists)
                return BadRequest("Item already in wishlist.");

            var wishlistItem = _mapper.Map<WishlistItem>(dto);
            wishlistItem.Id = Guid.NewGuid();
            wishlistItem.UserId = userId;

            _context.WishlistItems.Add(wishlistItem);
            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<WishlistItemDto>(wishlistItem));
        }

        // DELETE: api/Wishlist/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFromWishlist(Guid id)
        {
            var userId = GetUserId();
            var item = await _context.WishlistItems.FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

            if (item == null)
                return NotFound();

            _context.WishlistItems.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }
    }
}
