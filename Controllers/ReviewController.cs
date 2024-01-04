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
            var review = _context.Reviews;
            return Ok(review);
        }


        [HttpGet("read")]
        public ActionResult Read(int product_id)
        {
			var reviewsQuery = from r in _context.Reviews
						  join c in _context.Customers on r.CustomerEmail equals c.Email
						  where r.ProductId == product_id
						  orderby r.CreatedAt descending
						  select new
						  {
                            id = r.Id,
                            customer_email = r.CustomerEmail,
                            name = c.Name,
                            rating = r.Rating,
                            content = r.Content,
                            admin_reply = r.AdminReply,
                            created_at = r.CreatedAt,
                            updated_at = r.UpdatedAt
						  };

            var reviews = reviewsQuery.ToList();

			if (reviews.Count == 0) 
            {
                return Ok(new
                {
                    status = 404,
                    message = "Reviews for this product do not exist"
                });
            }

            return Ok(new
            {
                review = reviews 
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
