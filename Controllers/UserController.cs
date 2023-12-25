using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TechShopBackendDotnet.Models;
using TechShopBackendDotnet.Token;
using System.IdentityModel.Tokens.Jwt;

namespace TechShopBackendDotnet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly TechShopContext _context;
        private readonly AppSettings _appSettings;

        public UserController(TechShopContext context, AppSettings appSettings)
        {
            _context = context;
            _appSettings = appSettings;
        }

        
    }
}
