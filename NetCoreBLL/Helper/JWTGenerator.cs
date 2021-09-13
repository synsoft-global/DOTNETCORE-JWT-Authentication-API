using NetCoreBLL.Interface;
using NetCoreDAL.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NetCoreBLL.Helper
{
    public class JWTGenerator : IJWTGenerator
    {
        private readonly IConfiguration _configure;
        public JWTGenerator(IConfiguration configure)
        {
            _configure = configure;
        }

        /// <summary>
        /// This method is used for generate json web token 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<Object> GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configure.GetSection("JWTSecret").ToString());

            //Create a List of Claims, Keep claims name short            
            var SessionClaims = new List<Claim>();
            SessionClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            SessionClaims.Add(new Claim("user", JsonConvert.SerializeObject(user)));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(SessionClaims),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = (JwtSecurityToken)tokenHandler.CreateToken(tokenDescriptor);
            var jwtTokenResponse = new
            {
                access_token = tokenHandler.WriteToken(token),
                token_type = "Bearer",
                expires_in = token.Claims.Count() > 0 ? Convert.ToInt64(token.Claims.FirstOrDefault(c => c.Type == "exp").Value) : 0
            };
            return jwtTokenResponse;
        }
    }
}
