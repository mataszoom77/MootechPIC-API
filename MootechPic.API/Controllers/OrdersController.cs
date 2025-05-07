using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MootechPic.API.Data;
using MootechPic.API.DTOs;
using MootechPic.API.DTOs.Orders.Order;
using MootechPic.API.Models;

namespace MootechPic.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]  // require authentication
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public OrdersController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        // GET /api/orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll()
        {
            // pull current user info
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Forbid();

            var isAdmin = User.IsInRole("Admin");
            var query = _db.Orders
                           .Include(o => o.OrderItems)
                           .AsQueryable();

            if (!isAdmin)
            {
                var currentUserId = Guid.Parse(userIdClaim);
                query = query.Where(o => o.UserId == currentUserId);
            }

            var list = await query.ToListAsync();
            return Ok(_mapper.Map<List<OrderDto>>(list));
        }

        // GET /api/orders/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<OrderDto>> GetById(Guid id)
        {
            var order = await _db.Orders
                                 .Include(o => o.OrderItems)
                                 .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Forbid();

            var isAdmin = User.IsInRole("Admin");
            var currentUserId = Guid.Parse(userIdClaim);

            if (!isAdmin && order.UserId != currentUserId)
                return Forbid();

            return Ok(_mapper.Map<OrderDto>(order));
        }

        // POST /api/orders
        [HttpPost]
        public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderDto dto)
        {
            // enforce server‐side userId
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Forbid();
            var currentUserId = Guid.Parse(userIdClaim);

            var order = _mapper.Map<Order>(dto);
            order.Id = Guid.NewGuid();
            order.UserId = currentUserId;            // override any client input
            order.CreatedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            // compute line totals & totals are done by AutoMapper profile
            await _db.Orders.AddAsync(order);
            await _db.SaveChangesAsync();

            var result = _mapper.Map<OrderDto>(order);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, result);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] string newStatus)
        {
            var order = await _db.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

    }
}
