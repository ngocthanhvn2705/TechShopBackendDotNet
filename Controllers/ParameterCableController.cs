using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechShopBackendDotnet.Models;

namespace TechShopBackendDotnet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParameterCableController : ControllerBase
    {
        private readonly TechShopContext _context;

        public ParameterCableController(TechShopContext context)
        {
            _context = context;
        }

        
    }
}
