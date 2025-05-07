using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MootechPic.API.Data;
using MootechPic.API.DTOs.AdminResponse;
using MootechPic.API.DTOs.Requests;
using MootechPic.API.Models;

namespace MootechPic.API.Controllers
{
    [Route("api/requests/{requestId}/responses")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminResponsesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public AdminResponsesController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // POST: api/requests/{requestId}/responses
        [HttpPost]
        public async Task<ActionResult<AdminResponseDto>> PostResponse(
            Guid requestId,
            [FromBody] CreateAdminResponseDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verify the request exists
            if (!await _context.Requests.AnyAsync(r => r.Id == requestId))
                return NotFound($"Request {requestId} not found.");

            var response = new AdminResponse
            {
                Id = Guid.NewGuid(),
                RequestId = requestId,
                Description = dto.Description!,
                CreatedAt = DateTime.UtcNow
            };

            // Map attachments
            foreach (var att in dto.Attachments ?? Enumerable.Empty<AdminResponseAttachmentDto>())
            {
                response.Attachments.Add(new AdminResponseAttachment
                {
                    Id = Guid.NewGuid(),
                    ItemType = att.ItemType,
                    ItemId = att.ItemId
                });
            }

            _context.AdminResponses.Add(response);
            await _context.SaveChangesAsync();

            // Eager-load attachments so the DTO is fully populated
            await _context.Entry(response)
                          .Collection(r => r.Attachments)
                          .LoadAsync();

            var resultDto = _mapper.Map<AdminResponseDto>(response);
            return CreatedAtAction(nameof(GetResponses),
                                   new { requestId },
                                   resultDto);
        }

        // GET: api/requests/{requestId}/responses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdminResponseDto>>> GetResponses(Guid requestId)
        {
            var responses = await _context.AdminResponses
                .Where(r => r.RequestId == requestId)
                .Include(r => r.Attachments)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<AdminResponseDto>>(responses));
        }
    }
}
