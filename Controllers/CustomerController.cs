using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TechShopBackendDotnet.Models;
using TechShopBackendDotnet.Token;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using static TechShopBackendDotnet.Controllers.CustomerController;
using TechShopBackendDotnet.OtherModels;
using Humanizer;
using static TechShopBackendDotnet.Controllers.DiscountController;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NuGet.Common;

namespace TechShopBackendDotnet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly TechShopContext _context;
        private readonly AppSettings _appSettings;
        private readonly EmailService _emailService;

        public CustomerController(TechShopContext context, AppSettings appSettings, EmailService emailService)
        {
            _context = context;
            _appSettings = appSettings;
            _emailService = emailService;
        }

        [HttpGet("GetCustomer")]
        public ActionResult GetCustomer()
        {
            var customers = _context.Customers
                .Select(c => new
                {
                    Email = c.Email,
                    Name = c.Name,
                    Phone = c.Phone,
                    Gender = c.Gender,
                    Birthday = c.Birthday,
                    Address = c.Address,
                    Ward = c.Ward,
                    District = c.District,
                    City = c.City,
                    Status = c.Status,
                    Image = c.Image
                })
                .ToList();

            return Ok(customers);
        }


        [HttpGet("read")]
        public ActionResult ReadCustomer()
        {
            var jwt = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var email = GetEmailFromToken(jwt);

            if (email == null)
            {
                return Ok(new { status = 300, message = "Invalid token" });
            }

            var customerQuery = (from c in _context.Customers
                                where c.Email == email
                                select new
                                {
                                    email = c.Email,
                                    name = c.Name,
                                    phone = c.Phone,
                                    gender = c.Gender,
                                    birthday = c.Birthday,
                                    address = c.Address,
                                    ward = c.Ward,
                                    district = c.District,
                                    city = c.City,
                                    image = c.Image,
                                    status = c.Status,
                                }).FirstOrDefault();

            var result = new
            {
                status = 200,
                data = customerQuery
            };

            return Ok(result);
        }



        [HttpPost("signup")]
        public ActionResult CreateCustomer(Customer customer)
        {
            var existingCustomer = _context.Customers.Find(customer.Email);

            if (existingCustomer == null)
            {
                customer.Password = PasswordHasher.HashPassword(customer.Password);

                _context.Customers.Add(customer);
                _context.SaveChanges();

                return CreatedAtAction(nameof(GetCustomer), new { email = customer.Email }, new { status = "200", message = "User add Successfully" });
            }
            else
            {
                return Conflict(new
                {
                    status = "407",
                    message = "Email already Exists"
                });
            }
        }

        public class UpdateModel
        {
            public string? Name { get; set; }
            public string? old_password { get; set; }
            public string? new_password { get; set; }
            public string? Phone { get; set; }
            public string? Gender { get; set; }
            public string? Address { get; set; }
            public string? Ward { get; set; }
            public string? District { get; set; }
            public string? City { get; set; }

        }

        [HttpPut("update")]
        public ActionResult UpdateCustomer([FromBody] UpdateModel updateModel)
        {
            var jwt = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var email = GetEmailFromToken(jwt);

            if (email == "Expired token")
            {
                return Ok(new
                {
                    status = 300,
                    message = email
                });
            }

            var existingCustomer = _context.Customers.FirstOrDefault(c => c.Email == email);

            if (existingCustomer == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(updateModel.Name))
            {
                existingCustomer.Name = updateModel.Name;
            }

            if (!string.IsNullOrEmpty(updateModel.old_password) && !string.IsNullOrEmpty(updateModel.new_password))
            {
                if (PasswordHasher.VerifyPassword(updateModel.old_password, existingCustomer.Password))
                {
                    existingCustomer.Password = PasswordHasher.HashPassword(updateModel.new_password);
                }
                else
                {
                    return Ok(new
                    {
                        status = 115,
                        message = "Old password incorrect"
                    });
                }
                
            }

            if (!string.IsNullOrEmpty(updateModel.Phone))
            {
                existingCustomer.Phone = updateModel.Phone;
            }

            if (!string.IsNullOrEmpty(updateModel.Gender))
            {
                existingCustomer.Gender = updateModel.Gender;
            }

            if (!string.IsNullOrEmpty(updateModel.Address))
            {
                existingCustomer.Address = updateModel.Address;
            }

            if (!string.IsNullOrEmpty(updateModel.Ward))
            {
                existingCustomer.Ward = updateModel.Ward;
            }

            if (!string.IsNullOrEmpty(updateModel.District))
            {
                existingCustomer.District = updateModel.District;
            }

            if (!string.IsNullOrEmpty(updateModel.City))
            {
                existingCustomer.City = updateModel.City;
            }

            _context.SaveChanges();
            return Ok(new
            {
                status =200,
                message = "Updated successfully."
            });
        }


        public class LoginModel
        {
            public string Email { get; set; }
            public string? Password { get; set; }
        }


        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel loginModel)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.Email == loginModel.Email);


            if (customer == null || !PasswordHasher.VerifyPassword(loginModel.Password, customer.Password))
            {
                return Ok(new {status =108, message = "Email or Password is incorrect." });
            }

            if (customer.Status != "active")
            {
                return Ok(new
                {
                    status = 109,
                    message = "User is not activated. Please contact to admin."
                });
            }


            var token = GenerateToken(customer);

            return Ok(new
            {
                status = 200,
                jwt = token,
                message = "Login Successfully",
                data = new
                {
                    email = customer.Email,
                    name = customer.Name,
                    phone = customer.Phone,
                    gender = customer.Gender,
                    birthday = customer.Birthday,
                    address = customer.Address,
                    ward = customer.Ward,
                    district = customer.District,
                    city = customer.City,
                    image = customer.Image,
                    status = customer.Status
                }
            });
        }

        public class ValidateModell
        {
            public string Email { get; set; }
            public int? Key { get; set; }
        }

        [HttpPost("send_validate_email")]
        public IActionResult SendValidateEmail([FromBody] ValidateModell validateModel)
        {
            try
            {
                var emailQuery = _context.Customers.FirstOrDefault(c => c.Email == validateModel.Email);
                if (emailQuery != null)
                {
                    return Ok(new { status = 111, message = "Email already exists" });
                }

                Random random = new Random();
                var key = random.Next(100000, 999999);
                var keyHash = PasswordHasher.HashPassword(key.ToString());
                DateTime expiresAt = DateTime.Now.AddMinutes(30);


                _emailService.SendEmail(validateModel.Email, "Verify email from techshop", BodyValidate(key));

                var validateQuery = _context.CustomerValidateEmails.FirstOrDefault(cv => cv.Email == validateModel.Email);

                if (validateQuery != null)
                {
                    validateQuery.ValidateKeyHash = keyHash;
                    validateQuery.ValidateKeyExpiresAt = expiresAt;

                }
                else
                {
                    var newValidate = new CustomerValidateEmail
                    {
                        Email = validateModel.Email,
                        ValidateKeyHash = keyHash,
                        ValidateKeyExpiresAt = expiresAt
                    };
                    _context.CustomerValidateEmails.Add(newValidate);
                }
                _context.SaveChanges();
                return Ok(new { status = 200, message = "Message sent, please check your inbox." });
            }catch (Exception ex)
            {
                return Ok( new { status = 110, message = ex.Message});
            }
        }


        [HttpPost("check_validate_email")]
        public IActionResult CheckValidateEmail([FromBody] ValidateModell validateModel)
        {
            try
            {
                var key = validateModel.Key;
                var keyHash = PasswordHasher.HashPassword(key.ToString());

                var validateQuery = _context.CustomerValidateEmails
                                    .FirstOrDefault(cv => cv.Email == validateModel.Email && cv.ValidateKeyHash == keyHash);

                if (validateQuery != null)
                {
                    if (validateQuery.ValidateKeyExpiresAt <= DateTime.Now)
                    {
                        return Ok(new { status = 303, message = "Key has expired" });
                    }

                    return Ok(new { statu = 200, message = "Email authentication successful" });
                }
                else
                {
                    return Ok(new { StatusCode = 303, message = "Key incorrect" });
                }
            }
            catch (Exception ex)
            {
                return Ok(new { status = 110, message = ex.Message });
            }
        }


        public class ResetModell
        {
            public string? Email { get; set; }
            public string? Token { get; set; }
            public string? Password { get; set; }
        }

        [HttpPost("send_reset_password")]
        public IActionResult SendResetPassword([FromBody] ResetModell resetModell)
        {

            Guid guid = Guid.NewGuid();
            string token = guid.ToString("N");

            string token_hash = PasswordHasher.HashPassword(token);
            DateTime expiresAt = DateTime.Now.AddMinutes(30);

            var resetTokenQuery = _context.Customers.FirstOrDefault(c => c.Email == resetModell.Email);

            if (resetTokenQuery != null)
            {
                _emailService.SendEmail(resetModell.Email, "Password Reset from Techshop", BodyResetPass(token) );

                resetTokenQuery.ResetTokenHash = token_hash;
                resetTokenQuery.ResetTokenExpiresAt = expiresAt;
                _context.SaveChanges();

                return Ok(new { status = 200, message = "Message sent, please check your inbox." });

            }
            else
            {
                return Ok(new { status = 111, message = "Email is not registered" });
            }

        }

        [HttpPost("validate_token")]
        public IActionResult ValidateToken ([FromBody] ResetModell resetModell) 
        {
            var token = resetModell.Token;
            string token_hash = PasswordHasher.HashPassword(token);

            var tokenQuery = _context.Customers.FirstOrDefault(c => c.ResetTokenHash == token_hash && c.Email == resetModell.Email);

            if (tokenQuery != null)
            {
                if (tokenQuery.ResetTokenExpiresAt <= DateTime.Now)
                {
                    return Ok(new { status = 303, message = "Token has expired" });
                }

                return Ok(new { status = 200, message = "Valid Tokens" });
            }
            else
            {
                return Ok(new { status = 404, message = "Invalid Token" });
            }
        }

        [HttpPost("reset_password")]
        public IActionResult ResetPassword([FromBody] ResetModell resetModell)
        {
            var token = resetModell.Token;
            string token_hash = PasswordHasher.HashPassword(token);

            var password = resetModell.Password;
            string password_hash = PasswordHasher.HashPassword(password);

            var tokenQuery = _context.Customers.FirstOrDefault(c => c.ResetTokenHash == token_hash && c.Email == resetModell.Email);


            if (tokenQuery != null)
            {
                if (tokenQuery.ResetTokenExpiresAt <= DateTime.Now)
                {
                    return Ok(new { status = 303, message = "Token has expired" });
                }

                tokenQuery.ResetTokenExpiresAt = null;
                tokenQuery.ResetTokenHash = null;
                tokenQuery.Password = password_hash;
                _context.SaveChanges();

                return Ok(new { status = 200, message = "Change New Password Successful" });
            }
            else
            {
                return Ok(new { status = 404, message = "Invalid Token" });
            }

        }



        private string GenerateToken(Customer customer)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var secretKeyBytes = Encoding.ASCII.GetBytes(_appSettings.SecretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("email", customer.Email),
                    new Claim("name", customer.Name),
                }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKeyBytes), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);

            return jwtTokenHandler.WriteToken(token);
        }

        private string GetEmailFromToken(string token)
        {
            var tokenReader = new OtherModels.TokenReader(_appSettings.SecretKey);
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

        private string BodyValidate(int key)
        {
            var body = @"
                <div style=""max-width:525px;margin:0 auto;text-align:center;padding:0 4px 16px"">
                    <tr>
                        <td style=""font-size:0px;padding:0px;word-break:break-word"" align=""left"">
                            <table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""border-collapse:collapse;border-spacing:0px"">
                                <tbody>
                                    <tr>
                                        <img alt="""" height=""48"" src=""https://img.upanh.tv/2023/11/30/techShopLogo.jpg"" style=""object-fit:contain;border:0;border-radius:10px;display:block;outline:none;text-decoration:none;height:48px;width:100%;font-size:13px"" width=""48"" class=""CToWUd"" data-bit=""iit"">
                                    </tr>
                                </tbody>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td style=""font-size:0px;padding:0px;word-break:break-word"" align=""center"">
                            <div style=""font-family:system-ui,Segoe UI,sans-serif;font-size:15px;line-height:1.6;text-align:center;color:#333333"">Your Techshop verification code is:
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td style=""font-size:0px;padding:0px;word-break:break-word"" align=""center"">
                            <div style=""font-family:system-ui,Segoe UI,sans-serif;font-size:19px;font-weight:700;line-height:1.6;text-align:center;color:#333333"">" + key + @"
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td style=""font-size:0px;padding:0px;word-break:break-word"" align=""center"">
                            <div style=""font-family:system-ui,Segoe UI,sans-serif;font-size:15px;line-height:1.6;text-align:center;color:#333333"">Don't share this code with anyone; our employees will never ask for the code.
                            </div>
                        </td>
                    </tr>
                </div>
            ";

            return body;
        }

        private string BodyResetPass(string token)
        {
            var body = @"
                <!DOCTYPE html>
                <html>
                <head>
                    <title>Password Reset</title>
                </head>
                <body>
                <div style=""max-width:525px;margin:0 auto;text-align:center;padding:0 4px 16px"">
                    <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"">
                        <tr>
                            <td align=""center"" valign=""top"" bgcolor=""#ffffff"" style=""padding: 27px 20px 0 15px; font-family: Helvetica, Arial, sans-serif; height: 100%!important;"">
                                <p style=""text-align: left; margin: 0;"">
                                    <img src=""https://img.upanh.tv/2023/11/30/techShopLogo.jpg"" width=""70"" height=""auto"" alt=""TechShop Logo"" title="""" style=""width: 70px; height: auto; border: 0; line-height: 100%; outline: none; text-decoration: none;"" class=""CToWUd"">
                                </p>
                            </td>
                        </tr>
                        <tr>
                            <td align=""left"" valign=""top"" bgcolor=""#ffffff"" style=""padding: 40px 20px; color: #353740; text-align: left; line-height: 1.5; font-family: Helvetica, Arial, sans-serif;"">
                                <h1 style=""color: #202123; font-size: 32px; margin: 0 0 20px;"">Password Reset</h1>
                                <p>We received a request to reset your TechShop passwor. Click ""Reset Password"" to create a new password. Please set a new password immediately.</p>
                                <p style=""margin: 24px 0 0; text-align: left;"">
                                    <a href=""https://techshopui.vercel.app/forget/reset?token=" + token + @""" style=""display: inline-block; text-decoration: none; background: #10a37f; border-radius: 3px; color: white; font-family: Helvetica, sans-serif; font-size: 16px; line-height: 24px; font-weight: 400; padding: 12px 20px 11px; margin: 0px;"" target=""_blank"">
                                Reset Password
                                    </a>
                                </p>
                            </td>
                        </tr>
                        <tr>
                            <td align=""left"" valign=""top"" bgcolor=""#ffffff"" style=""padding: 0 20px 20px; color: #6e6e80; font-size: 13px; line-height: 1.4; text-align: left; background: #ffffff;"">
                                <p>If you are having any issues with your account, please contact us at <a href=""mailto:support@techshop.com"" target=""_blank"">support@techshop.com</a></p>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>
                </div>
            ";

            return body;
        }


    }
}


