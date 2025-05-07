using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MootechPic.API.Data;
using Microsoft.EntityFrameworkCore;


namespace MootechPic.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var now = DateTime.UtcNow;
            var last14Days = now.AddDays(-13).Date;

            var productCount = await _context.Products.CountAsync();
            var sparePartCount = await _context.SpareParts.CountAsync();
            var pendingRequestCount = await _context.Requests
                .CountAsync(r => r.Status == "Pending");
            var orderCount = await _context.Orders.CountAsync();

            var ordersPerDay = await _context.Orders
                .Where(o => o.CreatedAt >= last14Days)
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var requestsPerDay = await _context.Requests
                .Where(r => r.CreatedAt >= last14Days)
                .GroupBy(r => r.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            return Ok(new
            {
                productCount,
                sparePartCount,
                pendingRequestCount,
                orderCount,
                ordersPerDay,
                requestsPerDay
            });
        }
    }

}
