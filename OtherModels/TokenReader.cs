using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace TechShopBackendDotnet.OtherModels
{
    public class TokenReader
    {
        private readonly string _secretKey;

        public TokenReader(string secretKey)
        {
            _secretKey = secretKey;
        }

        public ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                if (validatedToken is JwtSecurityToken jwtSecurityToken && jwtSecurityToken.Claims != null)
                {
                    return new ClaimsPrincipal(new ClaimsIdentity(jwtSecurityToken.Claims));
                }
            }
            catch (Exception)
            {
                // Token validation failed
                return null;
            }

            return null;
        }
    }
}
