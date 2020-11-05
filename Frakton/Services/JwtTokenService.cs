using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Frakton.Models.Responses;
using Microsoft.IdentityModel.Tokens;

namespace Frakton.Services
{
    public class JwtTokenService : ITokenService
    {
        public string GenerateToken(string userId, string secretKey)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", userId) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        //public AuthenticationResponse RefreshToken(string token, string refreshToken, string secretKey)
        //{
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    SecurityToken validatedToken;
        //    var key = Encoding.ASCII.GetBytes(secretKey);
        //    var pricipal = tokenHandler.ValidateToken(token,
        //        new TokenValidationParameters
        //        {
        //            ValidateIssuerSigningKey = true,
        //            IssuerSigningKey = new SymmetricSecurityKey(key),
        //            ValidateIssuer = false,
        //            ValidateAudience = false,
        //            ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
        //        }, out validatedToken);
        //    var jwtToken = validatedToken as JwtSecurityToken;
        //    if (jwtToken == null || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        //    {
        //        throw new SecurityTokenException("Invalid token passed!");
        //    }

        //    var userName = pricipal.Identity.Name;

        //    if (refreshToken != jWTAuthenticationManager.UsersRefreshTokens[userName])
        //    {
        //        throw new SecurityTokenException("Invalid token passed!");
        //    }

        //    return tokenHandler.WriteToken(token);
        //}
    }
}