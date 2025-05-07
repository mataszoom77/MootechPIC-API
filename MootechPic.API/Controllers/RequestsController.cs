using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MootechPic.API.Data;
using MootechPic.API.Models;
using MootechPic.API.DTOs.Requests;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MootechPic.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public RequestsController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Requests
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<RequestDto>>> GetRequests()
        {
            // pull the userId from the JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Forbid();
            var userId = Guid.Parse(userIdClaim);

            // check role
            var isAdmin = User.IsInRole("Admin");

            // base query
            var query = _context.Requests
                .Include(r => r.Images)
                .Include(r => r.Responses)
                    .ThenInclude(ar => ar.Attachments)
                .AsQueryable();

            // if not admin, filter to only the current user's
            if (!isAdmin)
                query = query.Where(r => r.UserId == userId);

            var list = await query.ToListAsync();
            return Ok(_mapper.Map<IEnumerable<RequestDto>>(list));
        }


        // GET: api/Requests/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<RequestDto>> GetRequest(Guid id)
        {
            var request = await _context.Requests
                .Include(r => r.User)
                .Include(r => r.Images)
                .Include(r => r.Responses)
                    .ThenInclude(ar => ar.Attachments)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound();

            return Ok(_mapper.Map<RequestDto>(request));
        }

        // POST: api/Requests
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<RequestDto>> PostRequest(
            [FromBody] CreateRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 1) Get the user ID from the JWT (NameIdentifier claim)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Forbid();

            var userId = Guid.Parse(userIdClaim);

            // 2) Map the rest of the DTO and override UserId
            var request = _mapper.Map<Request>(dto);
            request.Id = Guid.NewGuid();
            request.UserId = userId;
            request.Status = "Pending";
            request.CreatedAt = DateTime.UtcNow;

            // 3) Add images
            foreach (var url in dto.ImageUrls)
            {
                request.Images.Add(new RequestImage
                {
                    Id = Guid.NewGuid(),
                    RequestId = request.Id,
                    Url = url
                });
            }

            // 4) Save and return
            _context.Requests.Add(request);
            await _context.SaveChangesAsync();

            var result = _mapper.Map<RequestDto>(request);
            return CreatedAtAction(nameof(GetRequest),
                                   new { id = request.Id }, result);
        }

        // PUT: api/Requests/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutRequest(Guid id, [FromBody] UpdateRequestDto dto)
        {
            if (id != dto.Id)
                return BadRequest("ID mismatch.");

            var request = await _context.Requests.FindAsync(id);
            if (request == null)
                return NotFound();

            // Only status is updated here; admin responses go in AdminResponsesController
            request.Status = dto.Status!;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Requests/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteRequest(Guid id)
        {
            var request = await _context.Requests.FindAsync(id);
            if (request == null)
                return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Only admins or the request owner may delete
            if (userRole != "Admin" && request.UserId.ToString() != userId)
                return Forbid("You can only delete your own requests.");

            _context.Requests.Remove(request);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
