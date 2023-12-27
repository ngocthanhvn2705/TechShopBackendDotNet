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
    public class CartController : ControllerBase
    {
        private readonly TechShopContext _context;
        private readonly AppSettings _appSettings;

        public CartController(TechShopContext context, AppSettings appSettings)
        {
            _context = context;
            _appSettings = appSettings;
        }


        [HttpGet("read")]
        public ActionResult Read()
        {
            try
            {

                var jwt = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var email = GetEmailFromToken(jwt);

                var cartItem = from c in _context.Carts
                           join cd in _context.CartDetails on c.Id equals cd.CartId
                           join p in _context.Products on cd.ProductId equals p.Id
                           join i in _context.ImageDetails on new { cd.ProductId, cd.Color } equals new { i.ProductId, i.Color }
                           join pq in _context.ProductQuantities on
                           new { cd.ProductId, cd.Color } equals new { pq.ProductId, pq.Color }
                           where c.CustomerEmail == email && i.Ordinal == -1 
                           select new {
                               id = cd.ProductId,
                               name = p.Name,
                               price = p.Price,
                               category = p.Category,
                               brand = p.Brand,
                               pre_discount = p.PreDiscount,
                               discount_percent = p.DiscountPercent,
                               color = cd.Color,
                               quantity = cd.Quantity,
                               stock = pq.Quantity,
                               image = i.Image
                           };
                return Ok(new
                {
                    product_cart = cartItem.ToList()
                });

            }
            catch(Exception ex)
            {
                return Ok(new
                {
                    status = 300,
                    message = ex.Message
                });
            }

            
        }

        public class CartDetailModel
        {
            public int product_id { get; set; }
            public int Quantity { get; set; }
            public string Color { get; set; }
        }

        [HttpPut("add")]
        public ActionResult Add(CartDetailModel cd)
        {
            try
            {

                var jwt = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var email = GetEmailFromToken(jwt);

                var checkCart = _context.Carts.FirstOrDefault(c => c.CustomerEmail == email);

                if (checkCart == null) 
                {
                    var cart = new Cart
                    {
                        CustomerEmail = email,
                        CreatedAt = DateTime.Now
                    };
                    _context.Carts.Add(cart);
                    _context.SaveChanges();
                }
                else
                {
                    checkCart.UpdatedAt = DateTime.Now;
                    _context.SaveChanges();
                }

                var cartToAdd = _context.Carts.FirstOrDefault(c => c.CustomerEmail == email);

                var checkCartDetail = (from c in _context.Carts
                                          join cds in _context.CartDetails on c.Id equals cds.CartId
                                          where c.CustomerEmail == email && cds.Color == cd.Color && cds.ProductId == cd.product_id
                                          select cds).FirstOrDefault();

                if (checkCartDetail == null)
                {

                    var cartDetail = new CartDetail
                    {
                        CartId = cartToAdd.Id,
                        ProductId = cd.product_id,
                        Color = cd.Color,
                        Quantity = cd.Quantity
                    };

                    _context.Add(cartDetail);
                    _context.SaveChanges();

                   
                }
                else
                {
                    checkCartDetail.Quantity += cd.Quantity;
                    _context.SaveChanges();

                }

                return Ok(new
                {
                    status = 200,
                    message = "Add successful"
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



        [HttpPut("update_quantity")]
        public ActionResult UpdateQuantity(CartDetailModel cd)
        {
            try
            {

                var jwt = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var email = GetEmailFromToken(jwt);

                var cartToUpdate = _context.Carts.FirstOrDefault(c => c.CustomerEmail == email);

                var cartDetailToUpdate = (from c in _context.Carts
                                  join cds in _context.CartDetails on c.Id equals cds.CartId
                                  where c.CustomerEmail == email && cds.Color == cd.Color && cds.ProductId == cd.product_id
                                  select cds).FirstOrDefault();

                if (cartDetailToUpdate != null && cartToUpdate != null)
                {
                    cartToUpdate.UpdatedAt = DateTime.Now;

                    cartDetailToUpdate.Quantity = cd.Quantity; 

                    _context.SaveChanges(); 

                    return Ok(new
                    {
                        status = 200,
                        message = "Update successful"
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = 404,
                        message = "Not Found Cart or Cart Detail"
                    });
                }

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

        [HttpDelete("delete")]
        public ActionResult DeleteCartDetail(CartDetailModel cd)
        {
            try
            {

                var jwt = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var email = GetEmailFromToken(jwt);

                var cartToDelete = _context.Carts.FirstOrDefault(c => c.CustomerEmail == email);

                var cartDetailToDelete = (from c in _context.Carts
                                    join cds in _context.CartDetails on c.Id equals cds.CartId
                                    where c.CustomerEmail == email && cds.Color == cd.Color && cds.ProductId == cd.product_id
                                    select cds).FirstOrDefault();

                if (cartDetailToDelete != null && cartToDelete != null)
                {
                    cartToDelete.UpdatedAt = DateTime.Now;

                    _context.CartDetails.Remove(cartDetailToDelete);

                    _context.SaveChanges();

                    return Ok(new
                    {
                        status = 200,
                        message = "Delete successful"
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = 404,
                        message = "Not Found Cart Detail"
                    });
                }

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

        [HttpDelete("deleteAll")]
        public ActionResult DeleteAllCartDetail()
        {
            try
            {

                var jwt = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var email = GetEmailFromToken(jwt);

                var cartToDeleteAll = _context.Carts.FirstOrDefault(c => c.CustomerEmail == email);

                var cartDetailToDeleteAll = (from c in _context.Carts
                                          join cds in _context.CartDetails on c.Id equals cds.CartId
                                          where c.CustomerEmail == email
                                          select cds).ToList();

                if (cartDetailToDeleteAll.Any() && cartToDeleteAll != null)
                {
                    cartToDeleteAll.UpdatedAt = DateTime.Now;

                    _context.CartDetails.RemoveRange(cartDetailToDeleteAll);

                    _context.SaveChanges();

                    return Ok(new
                    {
                        status = 200,
                        message = "Delete successful"
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = 404,
                        message = "Not Found Cart Detail"
                    });
                }

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
