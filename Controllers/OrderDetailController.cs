using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TechShopBackendDotnet.Models;

namespace TechShopBackendDotnet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderDetailController : ControllerBase
    {
        private readonly TechShopContext _context;

        public OrderDetailController(TechShopContext context)
        {
            _context = context;
        }

    }
}
