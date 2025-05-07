using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MootechPic.API.Data;
using MootechPic.API.Models;
using MootechPic.API.DTOs.Products;
using MootechPic.API.DTOs.SpareParts;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using NuGet.Packaging;

namespace MootechPic.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ProductsController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        #region GET

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<ProductDto>>(products));
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages.OrderBy(img => img.DisplayOrder)) // ✅ key fix
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            return Ok(_mapper.Map<ProductDto>(product));
        }


        [HttpGet("{id}/spareparts")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<SparePartDto>>> GetSparePartsForProduct(Guid id)
        {
            var product = await _context.Products
                .Include(p => p.ProductSpareParts!)
                    .ThenInclude(psp => psp.SparePart!)
                        .ThenInclude(sp => sp.SparePartImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            var spareParts = product.ProductSpareParts!
                .Select(psp => psp.SparePart!)
                .Select(sp => new
                {
                    sp.Id,
                    sp.Name,
                    sp.Description,
                    sp.Price,
                    ImageUrls = sp.SparePartImages
                        .OrderBy(i => i.DisplayOrder)
                        .Select(i => i.Url)
                        .ToList()
                })
                .ToList();

            return Ok(spareParts);
        }


        [HttpGet("{id}/images")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<string>>> GetProductImages(Guid id)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            var urls = product.ProductImages.Select(img => img.Url).ToList();
            return Ok(urls);
        }

        #endregion

        #region POST / PUT

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductDto>> PostProduct([FromBody] CreateProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = _mapper.Map<Product>(dto);
            product.Id = Guid.NewGuid();

            // Attach images
            product.ProductImages = dto.ImageUrls.Select(url => new ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Url = url
            }).ToList();

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var result = _mapper.Map<ProductDto>(product);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutProduct(Guid id, [FromBody] UpdateProductDto dto)
        {
            if (id != dto.Id)
                return BadRequest("ID mismatch.");

            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            _mapper.Map(dto, product);

            var submittedUrls = dto.ImageUrls.ToList();

            // Remove existing images not present in new list
            var imagesToRemove = product.ProductImages
                .Where(pi => !submittedUrls.Contains(pi.Url))
                .ToList();

            _context.ProductImages.RemoveRange(imagesToRemove);

            // Keep existing images that are still in the list
            var remainingImages = product.ProductImages
                .Where(pi => submittedUrls.Contains(pi.Url))
                .ToList();

            // Add missing images from client that aren't in the current collection
            var existingUrls = remainingImages.Select(i => i.Url).ToHashSet();

            var imagesToAdd = submittedUrls
                .Where(url => !existingUrls.Contains(url))
                .Select(url => new ProductImage
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    Url = url
                })
                .ToList();

            remainingImages.AddRange(imagesToAdd);

            // Set correct order
            for (int i = 0; i < submittedUrls.Count; i++)
            {
                var img = remainingImages.FirstOrDefault(x => x.Url == submittedUrls[i]);
                if (img != null)
                    img.DisplayOrder = i;
            }

            product.ProductImages = remainingImages;

            await _context.SaveChangesAsync();
            return NoContent();
        }




        [HttpPost("{id}/images")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddProductImage(Guid id, [FromBody] string imageUrl)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            _context.ProductImages.Add(new ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = id,
                Url = imageUrl
            });

            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpPost("{id}/spareparts")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddSparePartToProduct(Guid id, [FromBody] LinkSparePartDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            var sparePart = await _context.SpareParts.FindAsync(dto.SparePartId);

            if (product == null || sparePart == null)
                return NotFound();

            var exists = await _context.ProductSpareParts
                .AnyAsync(x => x.ProductId == id && x.SparePartId == dto.SparePartId);

            if (!exists)
            {
                _context.ProductSpareParts.Add(new ProductSparePart
                {
                    ProductId = id,
                    SparePartId = dto.SparePartId
                });

                await _context.SaveChangesAsync();
            }

            return Ok();
        }


        #endregion

        #region DELETE

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpDelete("{productId}/spareparts/{sparePartId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveSparePartFromProduct(Guid productId, Guid sparePartId)
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
