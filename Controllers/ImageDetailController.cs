using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechShopBackendDotnet.Models;

namespace TechShopBackendDotnet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageDetailController : ControllerBase
    {
        private readonly TechShopContext _context;

        public ImageDetailController(TechShopContext context)
        {
            _context = context;
        }

        [HttpGet("show_by_productid")]
        public ActionResult GetImageByProductId2(int productid)
        {
            var imageDetail = _context.ImageDetails.Where(p => p.ProductId == productid);
            if (imageDetail == null)
            {
                return NotFound();
            }

            var result = new
            {
                image = imageDetail
            };

            return Ok(result); 
        }

        [HttpGet("show_by_productid_ver2")]
        public ActionResult GetImageByProductIdVer2(int productid)
        {
            var colorQuery = (from image in _context.ImageDetails
                              where image.ProductId == productid
                              select image.Color).Distinct();

            var colors = colorQuery.ToList();
            var image_array = new Dictionary<string, Dictionary<string, List<string>>>();

            foreach (var color in colors)
            {
                var imagesByColor = _context.ImageDetails
                    .Where(p => p.ProductId == productid && p.Color == color)
                    .OrderBy(img => img.Ordinal)
                    .ToList();

                var colorImages = new Dictionary<string, List<string>>();
                var images = new List<string>();
                var thumbnail = new List<string>();

                foreach (var img in imagesByColor)
                {
                    var base64Image = Convert.ToBase64String(img.Image);
                    if (img.Ordinal == -1)
                    {
                        thumbnail.Add(base64Image);
                    }
                    else
                    {
                        images.Add(base64Image);
                    }
                }

                colorImages["images"] = images;
                colorImages["thumbnail"] = thumbnail;
                image_array[color] = colorImages;
            }

            var result = new
            {
                productId = productid.ToString(),
                color = image_array
            };

            return Ok(result);
        }


    }
}
