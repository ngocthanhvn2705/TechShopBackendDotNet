using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TechShopBackendDotnet.Models;
using System.Security.Cryptography.Xml;
using TechShopBackendDotnet.Token;
using static TechShopBackendDotnet.Controllers.OrderController;
using TechShopBackendDotnet.OtherModels;

namespace TechShopBackendDotnet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly TechShopContext _context;
        private readonly AppSettings _appSettings;

        public OrderController(TechShopContext context, AppSettings appSettings)
        {
            _context = context;
            _appSettings = appSettings;
        }

      
        public class ReadGuestModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Phone { get; set; }
        }

        [HttpPost("read_guest")]
        public IActionResult ReadGuest(ReadGuestModel readGuestModel)
        {
            try
            {

                var orders = _context.Orders
                    .Where(o => o.Id == readGuestModel.Id && o.Name == readGuestModel.Name && o.Phone == readGuestModel.Phone)
                    .OrderByDescending(o => o.OrderDate)
                    .ToList();

                var result = new List<object>();

                foreach (var order in orders)
                {
                    var orderDetails = _context.OrderDetails
                                    .Where(od => od.OrderId == order.Id)
                                    .Join(
                                        _context.Products,
                                        od => od.ProductId,
                                        p => p.Id,
                                        (od, p) => new
                                        {
                                            id = od.Id,
                                            order_id = od.OrderId,
                                            product_id = od.ProductId,
                                            name = p.Name,
                                            color = od.Color,
                                            quantity = od.Quantity,
                                            price = od.Price,
                                            image = p.Image
                                        })
                                    .ToList();

                    var orderInfo = new
                    {
                        id = order.Id,
                        name = order.Name,
                        address = order.Address,
                        ward = order.Ward,
                        district = order.District,
                        city = order.City,
                        phone = order.Phone,
                        discount_id = order.DiscountId,
                        shipping_fee = order.ShippingFee,
                        total_price = order.TotalPrice,
                        order_date = order.OrderDate,
                        canceled_date = order.CanceledDate,
                        completed_date = order.CompletedDate,
                        delivery_type = order.DeliveryType,
                        payment_type = order.PaymentType,
                        status = order.Status,
                        order_detail = orderDetails
                    };

                    result.Add(orderInfo);
                }

                return Ok(new { orders = result });
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

        [HttpGet("read")]
        public IActionResult Read()
        {
            try
            {
                var jwt = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var email = GetEmailFromToken(jwt);

                var orders = _context.Orders
                    .Where(o => o.CustomerEmail == email)
                    .OrderByDescending(o => o.OrderDate)
                    .ToList();

                var result = new List<object>();

                foreach (var order in orders)
                {
                    var orderDetails = _context.OrderDetails
                                    .Where(od => od.OrderId == order.Id)
                                    .Join(
                                        _context.Products,
                                        od => od.ProductId,
                                        p => p.Id,
                                        (od, p) => new
                                        {
                                            id = od.Id,
                                            order_id = od.OrderId,
                                            product_id = od.ProductId,
                                            name = p.Name,
                                            color = od.Color,
                                            quantity = od.Quantity,
                                            price = od.Price,
                                            image = p.Image 
                                        })
                                    .ToList();

                    var orderInfo = new
                    {
                        id = order.Id,
                        name = order.Name,
                        address = order.Address,
                        ward = order.Ward,
                        district = order.District,
                        city = order.City,
                        phone = order.Phone,
                        discount_id = order.DiscountId,
                        shipping_fee = order.ShippingFee,
                        total_price = order.TotalPrice,
                        order_date = order.OrderDate,
                        canceled_date = order.CanceledDate,
                        completed_date = order.CompletedDate,
                        delivery_type = order.DeliveryType,
                        payment_type = order.PaymentType,
                        status = order.Status,
                        order_detail = orderDetails
                    };

                    result.Add(orderInfo);
                }

                return Ok(new { orders = result });
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

        
        [HttpPut("order_guest")]
        public IActionResult OrderGuest(OrderModel orderData)
        {

            try
            {
                if (orderData.product != null)
                {
                    foreach (var orderDetailData in orderData.product)
                    {
                        var checkProduct = _context.ProductQuantities
                            .FirstOrDefault(pq => pq.ProductId == orderDetailData.ProductId && pq.Color == orderDetailData.Color);
                        if (checkProduct.Quantity < orderDetailData.Quantity)
                        {
                            return Ok(new
                            {
                                status = 405,
                                message = $"ID: {checkProduct.ProductId}, Color: {checkProduct.Color}. Ordered not enough quantity {orderDetailData.Quantity} "
                            });
                        }
                    }
                }

                DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
                var discountQuery = _context.Discounts.FirstOrDefault(d => d.Code == orderData.infor.Discount_code);

                Order order = new Order
                {
                    CustomerEmail = orderData.infor.Email,
                    Name = orderData.infor.Name,
                    Address = orderData.infor.Address,
                    Ward = orderData.infor.Ward,
                    District = orderData.infor.District,
                    City = orderData.infor.City,
                    Phone = orderData.infor.Phone,
                    DiscountId = (discountQuery == null) ? null : discountQuery.Id,
                    ShippingFee = orderData.infor.ShippingFee,
                    TotalPrice = orderData.infor.TotalPrice,
                    Note = orderData.infor.Note,
                    OrderDate = today,
                    DeliveryType = orderData.infor.Delivery_type,
                    PaymentType = orderData.infor.Payment_type,
                    Status = "Processing"
                };

                _context.Orders.Add(order);
                if (discountQuery != null)
                {
                    discountQuery.Quantity -= 1;
                }
                _context.SaveChanges();


                foreach (var orderDetailData in orderData.product)
                {
                    OrderDetail orderDetail = new OrderDetail
                    {
                        OrderId = order.Id, // Sử dụng Id mới tạo của Order
                        ProductId = orderDetailData.ProductId,
                        Color = orderDetailData.Color,
                        Quantity = orderDetailData.Quantity,
                        Price = orderDetailData.Price
                    };

                    var updateProductQuantities = _context.ProductQuantities
                            .FirstOrDefault(pq => pq.ProductId == orderDetailData.ProductId && pq.Color == orderDetailData.Color);

                    updateProductQuantities.Quantity -= orderDetailData.Quantity;
                    updateProductQuantities.Sold += orderDetailData.Quantity;

                    _context.OrderDetails.Add(orderDetail);
                }
                _context.SaveChanges();

                return Ok(new
                {
                    status = 200,
                    message = "Order Successfully"
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

        [HttpPut("order")]
        public IActionResult Order(OrderModel orderData)
        {
            try
            {
                var jwt = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var email = GetEmailFromToken(jwt);


                if (orderData.product != null)
                {
                    foreach (var orderDetailData in orderData.product)
                    {
                        var checkProduct = _context.ProductQuantities
                            .FirstOrDefault(pq => pq.ProductId == orderDetailData.ProductId && pq.Color == orderDetailData.Color);
                        if (checkProduct.Quantity < orderDetailData.Quantity)
                        {
                            return Ok(new
                            {
                                status = 405,
                                message = $"ID: {checkProduct.ProductId}, Color: {checkProduct.Color}. Ordered not enough quantity {orderDetailData.Quantity} "
                            });
                        }
                    }
                }

                DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
                var discountQuery = _context.Discounts.FirstOrDefault(d => d.Code == orderData.infor.Discount_code);

                Order order = new Order
                {
                    CustomerEmail = email,
                    Name = orderData.infor.Name,
                    Address = orderData.infor.Address,
                    Ward = orderData.infor.Ward,
                    District = orderData.infor.District,
                    City = orderData.infor.City,
                    Phone = orderData.infor.Phone,
                    DiscountId = (discountQuery == null) ? null : discountQuery.Id,
                    ShippingFee = orderData.infor.ShippingFee,
                    TotalPrice = orderData.infor.TotalPrice,
                    Note = orderData.infor.Note,
                    OrderDate = today,
                    DeliveryType = orderData.infor.Delivery_type,
                    PaymentType = orderData.infor.Payment_type,
                    Status = "Processing"
                };

                _context.Orders.Add(order);
                if (discountQuery != null)
                {
                    discountQuery.Quantity -= 1;
                }
                _context.SaveChanges();


                foreach (var orderDetailData in orderData.product)
                {
                    OrderDetail orderDetail = new OrderDetail
                    {
                        OrderId = order.Id, // Sử dụng Id mới tạo của Order
                        ProductId = orderDetailData.ProductId,
                        Color = orderDetailData.Color,
                        Quantity = orderDetailData.Quantity,
                        Price = orderDetailData.Price
                    };

                    var updateProductQuantities = _context.ProductQuantities
                            .FirstOrDefault(pq => pq.ProductId == orderDetailData.ProductId && pq.Color == orderDetailData.Color);

                    updateProductQuantities.Quantity -= orderDetailData.Quantity;
                    updateProductQuantities.Sold += orderDetailData.Quantity;
                    _context.OrderDetails.Add(orderDetail);
                }
                _context.SaveChanges();

                return Ok(new
                {
                    status = 200,
                    message = "Order Successfully"
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


        public class CancelModel
        {
            public int id { get; set; }
        }

        [HttpPut("cancel")]
        public IActionResult Cancel([FromBody] CancelModel cancelModel)
        {
            try
            {
                var jwt = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var email = GetEmailFromToken(jwt);

                var orderCancel = _context.Orders.FirstOrDefault(o => o.Id == cancelModel.id && o.CustomerEmail == email);

                if (orderCancel == null)
                {
                    return Ok(new { status = 404, message = "Order does not exist" });
                }

                if (orderCancel.Status != "Processing")
                {
                    return Ok(new { status = 406, message = $"Cancel unsuccessfully because status is {orderCancel.Status}" });
                }

                var orderDetailsCancel = _context.OrderDetails
                                        .Where(od => od.OrderId == orderCancel.Id)
                                        .ToList();

                foreach (var orderDetail in orderDetailsCancel)
                {
                    var productQuantitiesReturn = _context.ProductQuantities
                                                  .FirstOrDefault(pq => pq.ProductId == orderDetail.ProductId && pq.Color == orderDetail.Color);

                    productQuantitiesReturn.Quantity += orderDetail.Quantity;
                    productQuantitiesReturn.Sold -= orderDetail.Quantity;

                    _context.SaveChanges();
                }

                orderCancel.Status = "Cancelled";
                orderCancel.CanceledDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);

                _context.SaveChanges();

                return Ok(new {status = 200, message = "Cancel successfully" });


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


        [HttpPut("cancel_guest")]
        public IActionResult CancelGuest([FromBody] CancelModel cancelModel)

        {
            try
            {

                var orderCancel = _context.Orders.FirstOrDefault(o => o.Id == cancelModel.id);

                if (orderCancel == null)
                {
                    return Ok(new { status = 404, message = "Order does not exist" });
                }

                if (orderCancel.Status != "Processing")
                {
                    return Ok(new { status = 406, message = $"Cancel unsuccessfully because status is {orderCancel.Status}" });
                }

                var orderDetailsCancel = _context.OrderDetails
                                        .Where(od => od.OrderId == orderCancel.Id)
                                        .ToList();

                foreach (var orderDetail in orderDetailsCancel)
                {
                    var productQuantitiesReturn = _context.ProductQuantities
                                                  .FirstOrDefault(pq => pq.ProductId == orderDetail.ProductId && pq.Color == orderDetail.Color);

                    productQuantitiesReturn.Quantity += orderDetail.Quantity;
                    productQuantitiesReturn.Sold -= orderDetail.Quantity;

                    _context.SaveChanges();
                }

                orderCancel.Status = "Cancelled";
                orderCancel.CanceledDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);

                _context.SaveChanges();

                return Ok(new { status = 200, message = "Cancel successfully" });


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

        public class OrderModel
        {
            public OrderInput infor { get; set; }
            public List<OrderDetail> product { get; set; }
        }


        public class OrderInput
        {
            public int Id { get; set; }

            public string? Email { get; set; }

            public string Name { get; set; }

            public string Address { get; set; }

            public string Ward { get; set; }

            public string District { get; set; }

            public string City { get; set; }


            public string Phone { get; set; }

            public string? Discount_code { get; set; }

            public double ShippingFee { get; set; }

            public double TotalPrice { get; set; }

            public string? Note { get; set; }


            public string Delivery_type { get; set; }
            public string Payment_type { get; set; }
        }
    }
}
