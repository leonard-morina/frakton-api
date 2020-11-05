using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Frakton.Models;
using Microsoft.IdentityModel.Tokens;

namespace Frakton.Services
{
    public class JwtTokenService : ITokenService
    {
        public static Dictionary<string, string> RefreshTokenDict;

        public JwtTokenService()
        {
            RefreshTokenDict = new Dictionary<string, string>();
        }

        public JwtToken GenerateToken(string userId, string secretKey)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {new Claim("id", userId)}),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var createdToken = tokenHandler.CreateToken(tokenDescriptor);
            var token = tokenHandler.WriteToken(createdToken);

            var jwtToken = new JwtToken
            {
                RefreshToken = GenerateRefreshToken(),
                Token = token
            };
            return jwtToken;
        }

        public string GenerateRefreshToken(int size = 32)
        {
            var refreshToken = new byte[size];
            using var rand = RandomNumberGenerator.Create();
            rand.GetBytes(refreshToken);
            return Convert.ToBase64String(refreshToken);
        }

        public void SaveRefreshToken(UserRefreshToken userRefreshToken)
        {
            if (RefreshTokenDict.ContainsKey(userRefreshToken.Email))
            {
                RefreshTokenDict[userRefreshToken.Email] = userRefreshToken.RefreshToken;
            }
            else
            {
                RefreshTokenDict.Add(userRefreshToken.Email, userRefreshToken.RefreshToken);
            }
        }

        public bool IsRefreshTokenValid(string username, string refreshToken)
        {
            RefreshTokenDict.TryGetValue(username, out var refToken);
            return !string.IsNullOrEmpty(refToken) && refToken.Equals(refreshToken);
        }
    }
}