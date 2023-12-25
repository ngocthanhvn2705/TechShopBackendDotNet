using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechShopBackendDotnet.Models;
using System.Runtime.InteropServices.JavaScript;

namespace TechShopBackendDotnet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscountController : ControllerBase
    {
        private readonly TechShopContext _context;

        public DiscountController(TechShopContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult GetDiscount()
        {
            var discount = _context.Discounts;
            return Ok(discount);
        }

        [HttpGet("{id}")]
        public ActionResult GetDiscount(int id)
        {
            var discount = _context.Discounts.Find(id);
            if (discount == null)
            {
                return NotFound();
            }
            return Ok(discount);
        }


        [HttpGet("Code={code}")]
        public ActionResult GetDiscount(string code)
        {
            var discount = _context.Discounts.Where(d => d.Code == code).FirstOrDefault();
            if (discount == null)
            {
                return NotFound();
            }
            return Ok(discount);
        }

        [HttpGet("GetLastId")]
        public ActionResult GetLastId()
        {
            var discount = _context.Discounts.OrderByDescending(d => d.Id).FirstOrDefault();
            if (discount == null)
            {
                return NotFound();
            }
            return Ok(discount.Id);
        }

        public class ValidateModel
        {
            public string code { get; set; }
            public double total_price { get; set; }
        }


        [HttpPost("validate")]
        public ActionResult Validate([FromBody] ValidateModel validateModel)
        {
            var status = 114;
            var message = "";

            var discount = _context.Discounts.Where(d => d.Code == validateModel.code).FirstOrDefault();
            if (discount == null)
            {
                message = "Discount does not exist";
            }

            if (discount.Status == "disabled")
            {
                message = "Discount code is not available";
            }

            if (discount.Quantity < 1)
            {
                message = "Discount code is not available";
            }

            if (discount.Status == "disabled")
            {
                message = "Discount code is not available";
            }

            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

            if ( discount.EndDate < today || discount.Status == "expired")
            {
                message = $"Discount code has expired, expired at {discount.EndDate}";
            }

            if (discount.StartDate > today )
            {
                message = $"Discount code has not started yet, it begins on {discount.StartDate}";
            }

            if (validateModel.total_price < discount.MinApply)
            {
                message = $"Order has not reached the minimum value";
                
            }
            if (message != "")
            {
                return Ok(new
                {
                    status = status,
                    message = message
                });
            }

            var discount_value = (discount.Type == "percent") ? validateModel.total_price * discount.Value : discount.Value;
            var total_discount = (discount.MaxSpeed != -1) ? ((discount_value > discount.MaxSpeed) ? discount.MaxSpeed : discount_value) : discount_value;
            return Ok(new
            {
                status = 200,
                data = new
                {
                    id = discount.Id,
                    discount_value = total_discount
                }
            });
            
        }







    }
}
