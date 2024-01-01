using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TechShopBackendDotnet.Models;
using System.Security.Cryptography.Xml;
using TechShopBackendDotnet.Token;
using static TechShopBackendDotnet.Controllers.OrderController;
using TechShopBackendDotnet.OtherModels;
using System.Text.Json;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using static Microsoft.AspNetCore.Razor.Language.TagHelperMetadata;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static TechShopBackendDotnet.Controllers.DiscountController;

namespace TechShopBackendDotnet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly TechShopContext _context;
        private readonly AppSettings _appSettings;
        private readonly HttpClient _httpClient;
		private readonly EmailService _emailService;

		public OrderController(TechShopContext context, AppSettings appSettings, HttpClient httpClient, EmailService emailService)
		{
			_context = context;
			_appSettings = appSettings;
			_httpClient = httpClient;
			_emailService = emailService;
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
                    CustomerEmail = null,
                    Name = orderData.infor.Name,
                    Address = orderData.infor.Address,
                    Ward = orderData.infor.Ward,
                    District = orderData.infor.District,
                    City = orderData.infor.City,
                    Phone = orderData.infor.Phone,
                    DiscountId = (discountQuery == null) ? null : discountQuery.Id,
                    ShippingFee = orderData.infor.Shipping_fee,
                    TotalPrice = orderData.infor.Total_price,
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

                string tbody = "";
                int orderId = 0;

                foreach (var orderDetailData in orderData.product)
                {
                    var nameProduct = (from p in _context.Products
                                       where p.Id == orderDetailData.ProductId
                                       select p.Name).FirstOrDefault();
                    orderId = order.Id;

					OrderDetail orderDetail = new OrderDetail
                    {
                        OrderId = order.Id, 
                        ProductId = orderDetailData.ProductId,
                        Color = orderDetailData.Color,
                        Quantity = orderDetailData.Quantity,
                        Price = orderDetailData.Price
                    };
                    var totalPrice = orderDetailData.Price * orderDetailData.Quantity;

					var tbody_data = @"<tr>
							            <td> {0} </td>
							            <td> {1} </td>
							            <td> {2} </td>
							            <td> {3} </td>
							            <td> {4} VNĐ</td>
							            <td> {5} VNĐ</td>
						               </tr> ";
                    tbody_data = string.Format(
                        tbody_data, orderDetailData.ProductId, nameProduct, orderDetailData.Color,
                        orderDetailData.Quantity, orderDetailData.Price.ToString("#,##0"),
						totalPrice.ToString("#,##0"));

                    tbody += tbody_data + " ";

					var updateProductQuantities = _context.ProductQuantities
                            .FirstOrDefault(pq => pq.ProductId == orderDetailData.ProductId && pq.Color == orderDetailData.Color);

                    updateProductQuantities.Quantity -= orderDetailData.Quantity;
                    updateProductQuantities.Sold += orderDetailData.Quantity;

                    _context.OrderDetails.Add(orderDetail);
                }
                _context.SaveChanges();
                try
                {
                    OrderInputModel data = new OrderInputModel();
                    data = orderData.infor;
                    string body = BodySendSuccessEmail(data.Name, orderId, data.Address, data.Ward, data.District, data.City, data.Phone, data.Shipping_fee, data.Discount_code, data.Total_price, data.Note, data.Delivery_type, data.Payment_type, tbody);
                    Console.WriteLine(body);
					_emailService.SendEmail(orderData.infor.Email, "Xác nhận đơn hàng từ TechShop", body);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex);
                }

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
                    ShippingFee = orderData.infor.Shipping_fee,
                    TotalPrice = orderData.infor.Total_price,
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

        public class ShippingFeeModel
        {
            public string province_name { get; set; }
            public string district_name { get; set; }   
            public string ward_name { get; set; }
        }

        [HttpPost("shipping_fee")]
        public async Task<IActionResult> CalculateShippingFee([FromBody] ShippingFeeModel shippingFeeModel)
        {
            try
            {
                int provinceID = await GetProvinceID(shippingFeeModel.province_name);

                int districtID = await GetDistrictID(provinceID, shippingFeeModel.district_name);

                string wardCode = await GetWardID(districtID, shippingFeeModel.ward_name);

                int price = await CalculateFee(districtID, wardCode);


                if (price > 0)
                {
                    return Ok(new
                    {
                        status = 200,
                        data = new
                        {
                            total_fee = price
                        }
                    });

                }
                else
                {
                    return Ok(new { status = 113, message = "Can't calculate fee" });
                }
            }catch(Exception ex)
            {
                return Ok(new { status = 113, message = "Can't calculate fee" });
            }
            
        }

        [HttpPost("get_provinceid")]
        public async Task<int> GetProvinceID(string provinceName)
        {
            provinceName = provinceName.ToLower();
            string urlProvince = "https://online-gateway.ghn.vn/shiip/public-api/master-data/province";

            using var client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Token", "0a8e7e91-8da4-11ee-a59f-a260851ba65c");

            HttpResponseMessage response = await client.GetAsync(urlProvince);

            Console.WriteLine(response);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();

                var responseData = JsonSerializer.Deserialize<PostProvinceResponse>(responseContent);
                
                foreach (DataProvince item in responseData.data)
                {
                    List<string> lowerCaseList = item.NameExtension.Select(item => item.ToLower()).ToList();
                    if (lowerCaseList.Contains(provinceName) || item.ProvinceName.ToLower() == provinceName)
                    {
                        return item.ProvinceID;
                    }
                }
            }

            return -1;
        }

        [HttpPost("get_districid")]
        public async Task<int> GetDistrictID(int provinceId, string districtName)
        {
            districtName = districtName.ToLower();
            string urlProvince = "https://online-gateway.ghn.vn/shiip/public-api/master-data/district";

            using var client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Token", "0a8e7e91-8da4-11ee-a59f-a260851ba65c");

            var postData = new { province_id = provinceId }; 

            var json = JsonSerializer.Serialize(postData);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(urlProvince, content);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();

                var responseData = JsonSerializer.Deserialize<PostDistrictResponse>(responseContent);
                foreach (DataDistrict item in responseData.data)
                {
                    List<string> lowerCaseList = item.NameExtension.Select(item => item.ToLower()).ToList();
                    if (lowerCaseList.Contains(districtName) || item.DistrictName.ToLower() == districtName)
                    {
                        return item.DistrictID;
                    }
                }
            }

            return -1;
        }

        [HttpPost("get_wardid")]
        public async Task<string> GetWardID(int districtId, string wardName)
        {
            wardName = wardName.ToLower();

            string urlProvince = "https://online-gateway.ghn.vn/shiip/public-api/master-data/ward";

            using var client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Token", "0a8e7e91-8da4-11ee-a59f-a260851ba65c");

            var postData = new { district_id = districtId };

            var json = JsonSerializer.Serialize(postData);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(urlProvince, content);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();

                var responseData = JsonSerializer.Deserialize<PostWardResponse>(responseContent);
                foreach (DataWard item in responseData.data)
                {
                    List<string> lowerCaseList = item.NameExtension.Select(item => item.ToLower()).ToList();
                    if (lowerCaseList.Contains(wardName) || item.WardName.ToLower() == wardName)
                    {
                        return item.WardCode;
                    }
                }
            }

            return "none";
        }

        [HttpPost("get_serviceid")]
        public async Task<Dictionary<string, int>> GetServiceID(int to_district_id)
        {
            var shop_id = 4007360;
            var from_district_id = 3695;
            var service_id = -1;
            var service_type_id = -1;

            string urlService = "https://online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/available-services";

            using var client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Token", "0a8e7e91-8da4-11ee-a59f-a260851ba65c");

            var postData = new
            {
                shop_id = shop_id,
                from_district = from_district_id,
                to_district = to_district_id,
            };

            var json = JsonSerializer.Serialize(postData);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(urlService, content);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();

                var responseData = JsonSerializer.Deserialize<PostServiceResponse>(responseContent);

                service_id = responseData.data[0].service_id;
                service_type_id = responseData.data[0].service_type_id;

                var serviceDict = new Dictionary<string, int>
                                {
                                    { "service_id", service_id },
                                    { "service_type_id", service_type_id }
                                };

                return serviceDict;
            }

            return new Dictionary<string, int>
                    {
                        { "service_id", -1 },
                        { "service_type_id", -1 }
                    };
        }

        [HttpPost("calculatefee")]
        public async Task<int> CalculateFee(int to_district_id, string to_ward_code)
        {
            var total_fee = -1;
            var from_district_id = 3695;
            var from_ward_code = "90737";
            var service = GetServiceID(to_district_id).Result;


            string urlCalculate = "https://online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/fee";

            using var client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Token", "0a8e7e91-8da4-11ee-a59f-a260851ba65c");

            var postData = new {
                from_district_id = from_district_id,
                from_ward_code = from_ward_code,
                service_id = service["service_id"],
                service_type_id = service["service_type_id"],
                to_district_id = to_district_id,
                to_ward_code = to_ward_code,
                height = 0,
                length = 0,
                weight = 1000,
                width = 0,
                insurance_value = 300000,
                cod_failed_amount = 20000,
                coupon = ""
            };

            var json = JsonSerializer.Serialize(postData);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(urlCalculate, content);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();

                var responseData = JsonSerializer.Deserialize<PostCalculateResponse>(responseContent);
                return responseData.data.total;
            }

            return -1;
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
            public OrderInputModel infor { get; set; }
            public List<OrderDetailInput> product { get; set; }
        }

        public class OrderDetailInput
        {
            public int ProductId { get; set; }
            public string Color { get; set; }
            public int Quantity { get; set; }
            public double Price { get; set; }
        }

        public class OrderInputModel
        {
            public string? Email { get; set; }

            public string Name { get; set; }

            public string Address { get; set; }

            public string Ward { get; set; }

            public string District { get; set; }

            public string City { get; set; }

            public string Phone { get; set; }
			public int Shipping_fee { get; set; }

			public string? Discount_code { get; set; }


            public double Total_price { get; set; }

            public string? Note { get; set; }

            public string Delivery_type { get; set; }
            public string Payment_type { get; set; }
        }

		private string BodySendSuccessEmail(string Name, int orderId, string Address, string Ward, string District, string City, string Phone, int Shipping_fee, string Discount_code, double Total_price, string Note, string Delivery_type, string Payment_type, string body)
		{
			string htmlString = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <title>Xác nhận đơn hàng</title>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        line-height: 1.6;
                        margin: 0;
                        padding: 0;
                        background-color: #f4f4f4;
                    }}
                    .container {{
                        width: 80%;
                        margin: 0 auto;
                        background-color: #fff;
                        padding: 20px;
                        box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                    }}
                    h2 {{
                        text-align: center;
                        color: #333;
                    }}
                    img {{
                        display: block;
                        margin: 0 auto;
                        width: 100px;
                    }}
                    ul {{
                        list-style: none;
                        padding: 0;
                    }}
                    table {{
                        width: 100%;
                        border-collapse: collapse;
                        margin-top: 20px;
                        color: black;
                    }}
                    table, th, td {{
                        border: 1px solid #ddd;
                        text-align: center;
                    }}
                    th, td {{
                        padding: 8px;
                        text-align: left;
                    }}
                    th {{
                        background-color: #f2f2f2;
                    }}
                    b {{
                        color: #3366cc;
                    }}
                    footer {{
                        margin-top: 20px;
                        text-align: center;
                        color: #777;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div style=""text-align: center;"">
                        <img alt="""" src=""https://img.upanh.tv/2023/11/30/techShopLogo.jpg"" style=""object-fit: cover; border-radius: 50%; display: block; outline: none; text-decoration: none; height: 50px; width: 50px; font-size: 13px;"" class=""CToWUd"" data-bit=""iit"">
                    </div>   
                    <p>Xin chào, <b>{Name}</b></p>
                    <p>Cảm ơn bạn đã mua hàng tại TechShop.</p>
                    <ul>
                        <li>Đơn hàng: <b>#{orderId}</b></li>
                        <li>Địa chỉ giao hàng: <b>{Address}, {Ward}, {District}, {City}</b></li>
                        <li>Số điện thoại liên lạc: <b>{Phone}</b></li>
                        <li>Phí vận chuyển: <b>{Shipping_fee} VNĐ</b></li>
                        <li>Mã giảm giá: <b>{Discount_code}</b></li>
                        <li>Tổng giá trị đơn hàng: <b>{Total_price:N0} VNĐ</b></li>
                        <li>Ghi chú: <b>{Note}</b></li>
                        <li>Hình thức giao hàng: <b>{Delivery_type}</b></li>
                        <li>Hình thức thanh toán: <b>{Payment_type}</b></li>
                    </ul>
                    <table border='1' cellpadding='6' cellspacing='0'>
                        <thead>
                            <tr>
                                <th>Mã sản phẩm</th>
                                <th>Tên sản phẩm</th>
                                <th>Màu sắc</th>
                                <th>Số lượng</th>
                                <th>Giá bán</th>
                                <th>Thành tiền</th>
                            </tr>
                        </thead>
                        <tbody>
                            {body}
                        </tbody>
                    </table>
                    <footer>
                      <p>Chúng tôi sẽ giao hàng đến địa chỉ của bạn trong vòng 3-5 ngày làm việc.</p>
                      <p>Nếu có bất kỳ thắc mắc, vui lòng liên hệ với chúng tôi qua email hoặc điện thoại.</p>
                      <p>Trân trọng,</p>
                      <p>TechShop</p>
                    </footer>
                </div>
            </body>
            </html>";


			return htmlString;
		}

	}
}

