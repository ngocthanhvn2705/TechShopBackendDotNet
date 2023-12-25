using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechShopBackendDotnet.Models;

namespace TechShopBackendDotnet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartDetailController : ControllerBase
    {
        private readonly TechShopContext _context;

        public CartDetailController(TechShopContext context)
        {
            _context = context;
        }

       
    }
}
