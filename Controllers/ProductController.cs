using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechShopBackendDotnet.Models;

namespace TechShopBackendDotnet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly TechShopContext _context;
        public ProductController(TechShopContext context)
        {
            _context = context;
        }

        [HttpGet("read")]
        public ActionResult Read()
        {
            var allProductQuery = from p in _context.Products
                                  select new
                                  {
                                      id = p.Id,
                                      name = p.Name,
                                      price = p.Price,
                                      description = p.Description,
                                      category = p.Category,
                                      brand = p.Brand,
                                      pre_discount = p.PreDiscount,
                                      discount_percent = p.DiscountPercent,
                                      image = p.Image
                                  };

            var allProduct = allProductQuery.ToList();

            var result = new
            {
                product = allProduct
            };

            return Ok(result);
        }


        [HttpGet("show_color")]
        public ActionResult ShowColor(int id)
        {
            var colorQuery = from p in _context.Products
                             join pq in _context.ProductQuantities on p.Id equals pq.ProductId
                             where p.Id == id
                             select pq.Color;

            var colors = colorQuery.ToList();

            var result = new
            {
                color = colors
            };

            return Ok(result);
        }

        [HttpGet("show_by_category")]
        public ActionResult ShowByCategory(string name_category)
        {
            var productQuery = from p in _context.Products
                               join c in _context.Categories on p.Category equals c.Id
                               where c.Name == name_category
                               select new
                               {
                                   p.Id,
                                   p.Name,
                                   p.Price,
                                   p.Description,
                                   p.Image,
                                   p.Category,
                                   p.Brand,
                                   p.PreDiscount,
                                   p.DiscountPercent
                               };

            var products = productQuery.ToList();

            var result = new
            {
                product = products 
            };

            return Ok(result);
        }
        [HttpGet("show_by_category_and_brand")]
        public ActionResult ShowByCategoryAndBrand(string categoryName = null, string brand = null)
        {
            var productQuery = from p in _context.Products
                               join c in _context.Categories on p.Category equals c.Id
                               where (string.IsNullOrEmpty(categoryName) || c.Name == categoryName) &&
                                     (string.IsNullOrEmpty(brand) || p.Brand == brand)
                               select new
                               {
                                   p.Id,
                                   p.Name,
                                   p.Price,
                                   p.Description,
                                   p.Image,
                                   p.Category,
                                   p.Brand,
                                   p.PreDiscount,
                                   p.DiscountPercent
                               };

            var products = productQuery.ToList();

            var result = new
            {
                product = products
            };

            return Ok(result);
        }

        [HttpGet("search")]
        public ActionResult Search(string key = null)
        {
            var normalizedKeyword = key?.Replace(" ", "").ToLower(); 

            var productQuery = from p in _context.Products
                               join c in _context.Categories on p.Category equals c.Id
                               let normalizedName = p.Name.Replace(" ", "").ToLower()
                               let normalizedBrand = p.Brand.Replace(" ", "").ToLower()
                               let normalizedCategory = c.Name.Replace(" ", "").ToLower()
                               where (string.IsNullOrEmpty(normalizedKeyword) ||
                                      normalizedName.Contains(normalizedKeyword) ||
                                      normalizedBrand.Contains(normalizedKeyword) ||
                                      normalizedCategory.Contains(normalizedKeyword))
                               orderby c.Id == 1 ? 0 :
                                       c.Id == 2 ? 1 :
                                       c.Id == 3 ? 2 :
                                       c.Id == 4 ? 3 :
                                       4, normalizedName 
                               select new
                               {
                                   id =  p.Id,
                                   name = p.Name,
                                   price = p.Price,
                                   description = p.Description,
								   image = p.Image,
								   category = p.Category,
								   brand = p.Brand,
								   pre_discount = p.PreDiscount,
								   discount_percent = p.DiscountPercent
                               };


            var product = productQuery.ToList();

            var result = new
            {
                product = product
            };

            return Ok(result);
        }

        [HttpGet("show_all_brand_by_category")]
        public ActionResult ShowAllBrandByCategory(string categoryName)
        {
            var productQuery = (from p in _context.Products
                                join c in _context.Categories on p.Category equals c.Id
                                where c.Name == categoryName
                                select p.Brand).Distinct().ToList();

            var result = productQuery.Select(brand => new { brand = brand }).ToList();

            return Ok(new { brand = result });
        }

        [HttpGet("get_param")]
        public ActionResult GetParam(int id)
        {
            var productQuery = (from p in _context.Products
                                join c in _context.Categories on p.Category equals c.Id
                                where p.Id == id
                                select c.Name).FirstOrDefault();

            switch (productQuery)
            {
                case "phone":
                    var phoneParametersQuery = from pp in _context.ParameterPhones
                                               where pp.ProductId == id
                                               select new
                                               {
                                                   product_id = pp.ProductId,
                                                   screen = pp.Screen,
                                                   operating_system = pp.OperatingSystem,
                                                   back_camera = pp.BackCamera,
                                                   front_camera = pp.FrontCamera,
                                                   chip = pp.Chip,
                                                   ram = pp.Ram,
                                                   rom = pp.Rom,
                                                   sim = pp.Sim,
                                                   battery_charger = pp.BatteryCharger
                                               };

                    if (phoneParametersQuery.Any())
                    {
                        var responseData = new
                        {
                            status = 200,
                            data = phoneParametersQuery.ToList()
                        };

                        return Ok(responseData);
                    }
                    else
                    {
                        return NotFound();
                    }

                case "cable":
                    var cablePrameterQuery = from c in _context.ParameterCables
                                             where c.ProductId == id
                                             select new
                                             {
                                                 product_id = c.ProductId,
                                                 madein = c.Madein,
                                                 brandof = c.Brandof,
                                                 brand = c.Brand,
                                                 tech = c.Tech,
                                                 function = c.Function,
                                                 input = c.Input,
                                                 output = c.Output,
                                                 length = c.Length,
                                                 maximum = c.Maximum
                                             };

                    if (cablePrameterQuery.Any())
                    {
                        var responseData = new
                        {
                            status = 200,
                            data = cablePrameterQuery.ToList()
                        };

                        return Ok(responseData);
                    }
                    else
                    {
                        return NotFound();
                    }

                case "adapter":
                    var adapterPrameterQuery = from c in _context.ParameterAdapters
                                             where c.ProductId == id
                                             select new
                                             {
                                                 product_id = c.ProductId,
                                                 madein = c.Madein,
                                                 brandof = c.Brandof,
                                                 brand = c.Brand,
                                                 model = c.Model,
                                                 function = c.Function,
                                                 input = c.Input,
                                                 output = c.Output,
                                                 maximum = c.Maximum,
                                                 size = c.Size,
                                                 tech = c.Tech
                                             };

                    if (adapterPrameterQuery.Any())
                    {
                        var responseData = new
                        {
                            status = 200,
                            data = adapterPrameterQuery.ToList()
                        };

                        return Ok(responseData);
                    }
                    else
                    {
                        return NotFound();
                    }

                case "backupcharger":
                    var backupchargerPrameterQuery = from c in _context.ParameterBackupchargers
                                                   where c.ProductId == id
                                                   select new
                                                   {
                                                       product_id = c.ProductId,
                                                       madein = c.Madein,
                                                       brandof = c.Brandof,
                                                       brand = c.Brand,
                                                       efficiency = c.Efficiency,
                                                       capacity = c.Capacity,
                                                       time_full_charge = c.Timefullcharge,
                                                       input = c.Input,
                                                       output = c.Output,
                                                       core = c.Core,
                                                       tech = c.Tech,
                                                       size = c.Size,
                                                       weight = c.Weight
                                                       
                                                   };

                    if (backupchargerPrameterQuery.Any())
                    {
                        var responseData = new
                        {
                            status = 200,
                            data = backupchargerPrameterQuery.ToList()
                        };

                        return Ok(responseData);
                    }
                    else
                    {
                        return NotFound();
                    }

                default:
                    return NotFound(); 
            }

        }



    }
}
