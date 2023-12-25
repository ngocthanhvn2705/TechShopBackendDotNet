using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechShopBackendDotnet.Models;
using TechShopBackendDotnet.OtherModels;
using TechShopBackendDotnet.Token;

namespace TechShopBackendDotnet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly TechShopContext _context;
        private readonly AppSettings _appSettings;

        public ReviewController(TechShopContext context, AppSettings appSettings)
        {
            _context = context;
            _appSettings = appSettings;
        }

        [HttpGet]
        public ActionResult Get()
        {
            var reviews = _context.Reviews;
            return Ok(reviews);
        }


        [HttpGet("read")]
        public ActionResult Read(int product_id)
        {
            var reviews = _context.Reviews
                            .Where(r => r.ProductId == product_id)
                            .ToList();

            if (reviews.Count == 0) // Kiểm tra xem danh sách đánh giá có rỗng không
            {
                return Ok(new
                {
                    status = 404,
                    message = "Reviews for this product do not exist"
                });
            }

            return Ok(new
            {
                reviews = reviews // Trả về danh sách đánh giá nếu có
            });
        }


        public class ReviewModel
        {
            public int ProductId { get; set; }
            public int Rating { get; set; }
            public string Content { get; set; }

        }

        [HttpPost("add")]
        public ActionResult Add([FromBody] ReviewModel reviewData)
        {
            try
            {
                var jwt = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var email = GetEmailFromToken(jwt);

                if (email == null)
                {
                    return StatusCode(401, "Unauthorized");
                }

                var newReview = new Review
                {
                    ProductId = reviewData.ProductId,
                    Rating = reviewData.Rating,
                    Content = reviewData.Content,
                    CustomerEmail = email,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Reviews.Add(newReview);
                _context.SaveChanges();

                return Ok(new
                {
                    status = 200,
                    message = "Review added successfully"
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    status = 300,
                    message = ex.Message
                });
            }
        }



        private string GetEmailFromToken(string token)
        {
            var tokenReader = new TokenReader(_appSettings.SecretKey);
            var principal = tokenReader.GetPrincipalFromToken(token);

            if (principal == null)
            {
                return null;
            }

            var emailClaim = principal.FindFirst("email");

            if (emailClaim == null)
            {
                return null;
            }

            return emailClaim.Value;
        }

    }

}
