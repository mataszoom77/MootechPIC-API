using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MootechPic.API.Data;
using MootechPic.API.Models;
using MootechPic.API.DTOs.CartItems;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MootechPic.API.DTOs.Carts;

namespace MootechPic.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CartItemsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public CartItemsController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/CartItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CartItemDto>>> GetCartItems()
        {
            var items = await _context.CartItems.ToListAsync();
            return Ok(_mapper.Map<IEnumerable<CartItemDto>>(items));
        }

        // GET: api/CartItems/5
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CartItemDto>>> GetUserCartItems()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == Guid.Parse(userId));

            if (cart == null)
                return Ok(new List<CartItemDto>());  // Empty cart

            var items = await _context.CartItems
                .Where(ci => ci.CartId == cart.Id)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<CartItemDto>>(items));
        }


        [HttpPost("add")]
        public async Task<ActionResult<CartDto>> AddToCart([FromBody] CreateCartItemDto dto)
        {
            if (dto.Quantity <= 0)
                return BadRequest("Quantity must be at least 1.");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == Guid.Parse(userId));

            if (cart == null)
            {
                cart = new Cart
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.Parse(userId),
                    CreatedAt = DateTime.UtcNow
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var existingItem = await _context.CartItems.FirstOrDefaultAsync(ci =>
                ci.CartId == cart.Id &&
                ci.ItemId == dto.ItemId &&
                ci.ItemType == dto.ItemType);

            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
                if (existingItem.Quantity < 1)
                    existingItem.Quantity = 1;

                _context.CartItems.Update(existingItem);
            }
            else
            {
                var cartItem = new CartItem
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,
                    ItemId = dto.ItemId,
                    ItemType = dto.ItemType,
                    Quantity = dto.Quantity
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            // 🔹 Reload the updated cart with fresh data
            var updatedCart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == cart.Id);
            Console.WriteLine($"CartItems Count: {updatedCart.CartItems.Count}");
            var cartDto = _mapper.Map<CartDto>(updatedCart);
            return Ok(cartDto);
        }





        // PUT: api/CartItems/5
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> PutCartItem(Guid id, [FromBody] UpdateCartItemDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem == null)
                return NotFound();

            // verify ownership
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == cartItem.CartId
                                        && c.UserId == Guid.Parse(userId));
            if (cart == null)
                return Forbid();

            // update quantity
            cartItem.Quantity = Math.Max(1, dto.Quantity);

            await _context.SaveChangesAsync();

            var updatedCart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == cart.Id);

            var cartDto = _mapper.Map<CartDto>(updatedCart);
            return Ok(cartDto);
        }



        // DELETE: api/CartItems/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<CartDto>> DeleteCartItem(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem == null)
                return NotFound("Cart item not found.");

            // Ensure the item belongs to the current user's cart
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == cartItem.CartId && c.UserId == Guid.Parse(userId));

            if (cart == null)
                return Forbid("You don't have permission to modify this cart.");

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            // Reload updated cart
            var updatedCart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == cart.Id);

            var cartDto = _mapper.Map<CartDto>(updatedCart);
            return Ok(cartDto);
        }

    }
}
