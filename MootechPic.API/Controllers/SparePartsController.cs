using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MootechPic.API.Data;
using MootechPic.API.Models;
using MootechPic.API.DTOs.SpareParts;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using MootechPic.API.DTOs.Products;

namespace MootechPic.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SparePartsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public SparePartsController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        #region GET

        // GET: api/SpareParts
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<SparePartDto>>> GetSpareParts()
        {
            var spareParts = await _context.SpareParts
                .Include(sp => sp.SparePartImages)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<SparePartDto>>(spareParts));
        }

        // GET: api/SpareParts/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<SparePartDto>> GetSparePart(Guid id)
        {
            var sparePart = await _context.SpareParts
                .Include(sp => sp.SparePartImages)
                .FirstOrDefaultAsync(sp => sp.Id == id);

            if (sparePart == null)
                return NotFound();

            // Sort images before mapping
            sparePart.SparePartImages = sparePart.SparePartImages
                .OrderBy(i => i.DisplayOrder)
                .ToList();

            return Ok(_mapper.Map<SparePartDto>(sparePart));
        }


        // GET: api/SpareParts/{id}/products
        [HttpGet("{id}/products")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductsForSparePart(Guid id)
        {
            var sparePart = await _context.SpareParts
                .Include(sp => sp.ProductSpareParts!)
                    .ThenInclude(psp => psp.Product)
                        .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(sp => sp.Id == id);

            if (sparePart == null)
                return NotFound();

            var productsDto = sparePart.ProductSpareParts!
                .Select(psp => new {
                    psp.Product!.Id,
                    psp.Product.Name,
                    psp.Product.Description,
                    psp.Product.Price,
                    ImageUrls = psp.Product.ProductImages
                        .OrderBy(i => i.DisplayOrder) // optional
                        .Select(i => i.Url)
                        .ToList()
                })
                .ToList();

            return Ok(productsDto);
        }


        // GET: api/SpareParts/{id}/images
        [HttpGet("{id}/images")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<string>>> GetSparePartImages(Guid id)
        {
            var sparePart = await _context.SpareParts
                .Include(sp => sp.SparePartImages)
                .FirstOrDefaultAsync(sp => sp.Id == id);

            if (sparePart == null)
                return NotFound();

            var urls = sparePart.SparePartImages.Select(i => i.Url).ToList();
            return Ok(urls);
        }

        #endregion

        #region POST / PUT

        // POST: api/SpareParts
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SparePartDto>> PostSparePart([FromBody] CreateSparePartDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var sparePart = _mapper.Map<SparePart>(dto);
            sparePart.Id = Guid.NewGuid();

            sparePart.SparePartImages = dto.ImageUrls.Select(url => new SparePartImage
            {
                Id = Guid.NewGuid(),
                SparePartId = sparePart.Id,
                Url = url
            }).ToList();

            _context.SpareParts.Add(sparePart);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSparePart), new { id = sparePart.Id }, _mapper.Map<SparePartDto>(sparePart));
        }

        // PUT: api/SpareParts/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutSparePart(Guid id, [FromBody] UpdateSparePartDto dto)
        {
            if (id != dto.Id)
                return BadRequest("ID mismatch.");

            var sparePart = await _context.SpareParts
                .Include(sp => sp.SparePartImages)
                .FirstOrDefaultAsync(sp => sp.Id == id);

            if (sparePart == null)
                return NotFound();

            _mapper.Map(dto, sparePart);

            var submittedUrls = dto.ImageUrls.ToList();

            // Remove old
            var toRemove = sparePart.SparePartImages
                .Where(i => !submittedUrls.Contains(i.Url))
                .ToList();
            _context.SparePartImages.RemoveRange(toRemove);

            // Keep + Add
            var remaining = sparePart.SparePartImages
                .Where(i => submittedUrls.Contains(i.Url))
                .ToList();

            var existingUrls = remaining.Select(i => i.Url).ToHashSet();
            var toAdd = submittedUrls
                .Where(url => !existingUrls.Contains(url))
                .Select(url => new SparePartImage
                {
                    Id = Guid.NewGuid(),
                    SparePartId = id,
                    Url = url
                }).ToList();

            remaining.AddRange(toAdd);

            // Apply order
            for (int i = 0; i < submittedUrls.Count; i++)
            {
                var img = remaining.FirstOrDefault(x => x.Url == submittedUrls[i]);
                if (img != null)
                    img.DisplayOrder = i;
            }

            sparePart.SparePartImages = remaining;


            await _context.SaveChangesAsync();
            return NoContent();
        }
        [HttpPost("{id}/products")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddProductToSparePart(Guid id, [FromBody] LinkProductDto dto)
        {
            var sparePart = await _context.SpareParts.FindAsync(id);
            var product = await _context.Products.FindAsync(dto.ProductId);

            if (sparePart == null || product == null)
                return NotFound();

            var alreadyLinked = await _context.ProductSpareParts
                .AnyAsync(x => x.ProductId == dto.ProductId && x.SparePartId == id);

            if (!alreadyLinked)
            {
                _context.ProductSpareParts.Add(new ProductSparePart
                {
                    ProductId = dto.ProductId,
                    SparePartId = id
                });
                await _context.SaveChangesAsync();
            }

            return Ok();
        }



        [HttpPost("{id}/images")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddSparePartImage(Guid id, [FromBody] string imageUrl)
        {
            var sparePart = await _context.SpareParts.FindAsync(id);
            if (sparePart == null)
                return NotFound();

            var image = new SparePartImage
            {
                Id = Guid.NewGuid(),
                SparePartId = id,
                Url = imageUrl
            };

            _context.SparePartImages.Add(image);
            await _context.SaveChangesAsync();
            return Ok();
        }

        #endregion

        #region DELETE

        // DELETE: api/SpareParts/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSparePart(Guid id)
        {
            var sparePart = await _context.SpareParts.FindAsync(id);
            if (sparePart == null)
                return NotFound();

            _context.SpareParts.Remove(sparePart);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{sparePartId}/products/{productId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveProductFromSparePart(Guid sparePartId, Guid productId)
        {
            var link = await _context.ProductSpareParts
                .FirstOrDefaultAsync(psp => psp.ProductId == productId && psp.SparePartId == sparePartId);

            if (link == null)
                return NotFound();

            _context.ProductSpareParts.Remove(link);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        #endregion
    }
}
