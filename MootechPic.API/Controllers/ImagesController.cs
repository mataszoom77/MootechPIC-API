using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MootechPic.API.Data;
using MootechPic.API.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.EntityFrameworkCore;

namespace MootechPic.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly AppDbContext _context;
        private readonly Cloudinary _cloudinary;

        public ImagesController(IWebHostEnvironment env, AppDbContext context, Cloudinary cloudinary)
        {
            _env = env;
            _context = context;
            _cloudinary = cloudinary;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadToCloud(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file provided.");

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                Folder = "mootechpic" // optional folder in Cloudinary
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
                return BadRequest(uploadResult.Error.Message);

            return Ok(new { url = uploadResult.SecureUrl.ToString() });
        }

        [HttpPost("upload-multiple")]
        [Authorize(Roles = "Admin")]
        [RequestSizeLimit(50_000_000)] // Optional: Increase max upload size
        public async Task<IActionResult> UploadMultiple(List<IFormFile> files, [FromQuery] Guid itemId, [FromQuery] string itemType)
        {
            if (files == null || files.Count == 0)
                return BadRequest("No files uploaded.");

            if (itemType != "Product" && itemType != "SparePart")
                return BadRequest("Invalid item type.");

            var uploadedUrls = new List<string>();

            foreach (var file in files)
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream()),
                    Folder = "mootechpic"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                    return BadRequest(uploadResult.Error.Message);

                uploadedUrls.Add(uploadResult.SecureUrl.ToString());

                // Save to DB
                if (itemType == "Product")
                {
                    _context.ProductImages.Add(new ProductImage
                    {
                        Id = Guid.NewGuid(),
                        ProductId = itemId,
                        Url = uploadResult.SecureUrl.ToString()
                    });
                }
                else if (itemType == "SparePart")
                {
                    _context.SparePartImages.Add(new SparePartImage
                    {
                        Id = Guid.NewGuid(),
                        SparePartId = itemId,
                        Url = uploadResult.SecureUrl.ToString()
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(uploadedUrls);
        }
        [HttpDelete("{id}/images")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveProductImage(Guid id, [FromBody] string imageUrl)
        {
            var image = await _context.ProductImages
                .FirstOrDefaultAsync(i => i.ProductId == id && i.Url == imageUrl);

            if (image == null)
                return NotFound();

            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();

            return NoContent();
        }



    }
}
