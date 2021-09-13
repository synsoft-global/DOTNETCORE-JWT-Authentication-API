using System;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using NetCoreDAL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace NetCoreAPI.Middlewares
{
    public class UseJWTValidator
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configure;
        public UseJWTValidator(RequestDelegate next, IConfiguration configure)
        {
            _next = next;
            _configure = configure;
        }

        public async Task Invoke(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
                await ValidateJWT(context, token);

            await _next(context);
        }

        /// <summary>
        /// This method is used for validate json web token
        /// </summary>
        /// <param name="context"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task ValidateJWT(HttpContext context, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configure.GetSection("JWTSecret").ToString());
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userDetails = (jwtToken.Claims.First(x => x.Type == "user").Value).ToString();

                // attach user details to context on successful jwt validation
                context.Items["User"] = JsonConvert.DeserializeObject<User>(userDetails);
                context.Items["IsExpired"] = false;
            }
            catch (Exception ex)
            {
                context.Items["IsExpired"] = true;
                // do nothing if jwt validation fails
                // account is not attached to context so request won't have access to secure routes
            }
        }
    }
}
