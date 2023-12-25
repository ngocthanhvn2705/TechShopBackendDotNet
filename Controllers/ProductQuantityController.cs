using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TechShopBackendDotnet.Models;

namespace TechShopBackendDotnet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductQuantityController : ControllerBase
    {
        private readonly TechShopContext _context;

        public ProductQuantityController(TechShopContext context)
        {
            _context = context;
        }

 

    }
}
