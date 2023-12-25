using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechShopBackendDotnet.Models;

namespace TechShopBackendDotnet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParameterAdapterController : ControllerBase
    {
        private readonly TechShopContext _context;

        public ParameterAdapterController(TechShopContext context)
        {
            _context = context;
        }

 


    }
}
